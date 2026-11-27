import asyncio
import hashlib
import logging
import math
import re
from functools import lru_cache
from typing import Protocol

from jobmatcher import metrics
from jobmatcher.caching import LruCache, normalize_text
from jobmatcher.config import get_settings


class Embedder(Protocol):
    dim: int
    model_name: str

    async def embed_documents(self, texts: list[str]) -> list[list[float]]: ...

    async def embed_query(self, text: str) -> list[float]: ...


class E5Embedder:
    """multilingual-e5 expects 'passage: ' / 'query: ' prefixes; vectors are L2-normalized
    so cosine distance in pgvector behaves as expected."""

    def __init__(self, model_name: str, dim: int) -> None:
        from sentence_transformers import SentenceTransformer  # optional dependency

        self._model = SentenceTransformer(model_name)
        model_dim = self._model.get_sentence_embedding_dimension()
        if model_dim != dim:
            raise ValueError(
                f"EMBEDDING_MODEL={model_name} produces {model_dim}-dimensional vectors, "
                f"but EMBEDDING_DIM is {dim}"
            )
        self.dim = dim
        self.model_name = model_name

    def _encode(self, texts: list[str]) -> list[list[float]]:
        vectors = self._model.encode(texts, normalize_embeddings=True, batch_size=16)
        return [v.tolist() for v in vectors]

    async def embed_documents(self, texts: list[str]) -> list[list[float]]:
        return await asyncio.to_thread(self._encode, [f"passage: {t}" for t in texts])

    async def embed_query(self, text: str) -> list[float]:
        [vector] = await asyncio.to_thread(self._encode, [f"query: {text}"])
        return vector


class HashingEmbedder:
    """Deterministic bag-of-words embedding. No semantics, but stable and dependency-free:
    used in tests and as a fallback when the real model is not installed."""

    def __init__(self, dim: int) -> None:
        self.dim = dim
        self.model_name = f"hashing-{dim}"

    def _embed(self, text: str) -> list[float]:
        vector = [0.0] * self.dim
        for token in re.findall(r"\w+", text.lower()):
            digest = hashlib.blake2b(token.encode(), digest_size=8).digest()
            index = int.from_bytes(digest[:4], "little") % self.dim
            vector[index] += 1.0 if digest[4] % 2 else -1.0
        norm = math.sqrt(sum(x * x for x in vector)) or 1.0
        return [x / norm for x in vector]

    async def embed_documents(self, texts: list[str]) -> list[list[float]]:
        return [self._embed(t) for t in texts]

    async def embed_query(self, text: str) -> list[float]:
        return self._embed(text)


class CachedEmbedder:
    """Remembers query vectors by their whitespace-normalised text: the same searches repeat,
    and encoding one costs a model forward pass on the CPU. Documents pass through."""

    def __init__(self, inner: Embedder, max_entries: int) -> None:
        self.inner = inner
        self.dim = inner.dim
        self.model_name = inner.model_name
        self._vectors: LruCache[str, list[float]] = LruCache(max_entries)

    async def embed_documents(self, texts: list[str]) -> list[list[float]]:
        return await self.inner.embed_documents(texts)

    async def embed_query(self, text: str) -> list[float]:
        key = normalize_text(text)
        vector = self._vectors.get(key)
        metrics.recorder.cache_lookup("query_embedding", vector is not None)
        if vector is None:
            vector = await self.inner.embed_query(key)
            self._vectors.put(key, vector)
        return list(vector)


def _model(model_name: str, dim: int) -> Embedder:
    try:
        return E5Embedder(model_name, dim)
    except ImportError:
        logging.getLogger(__name__).warning(
            "sentence-transformers not installed, using HashingEmbedder (no semantic search)"
        )
        return HashingEmbedder(dim)


@lru_cache
def get_embedder() -> Embedder:
    settings = get_settings()
    embedder = _model(settings.embedding_model, settings.embedding_dim)
    if settings.query_embedding_cache_size == 0:
        return embedder
    return CachedEmbedder(embedder, settings.query_embedding_cache_size)
