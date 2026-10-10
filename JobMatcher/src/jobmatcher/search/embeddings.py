import asyncio
import hashlib
import math
import re
from functools import lru_cache
from typing import Protocol

from jobmatcher.config import get_settings


class Embedder(Protocol):
    dim: int

    async def embed_documents(self, texts: list[str]) -> list[list[float]]: ...

    async def embed_query(self, text: str) -> list[float]: ...


class E5Embedder:
    """multilingual-e5 expects 'passage: ' / 'query: ' prefixes; vectors are L2-normalized
    so cosine distance in pgvector behaves as expected."""

    def __init__(self, model_name: str, dim: int) -> None:
        from sentence_transformers import SentenceTransformer  # optional dependency

        self._model = SentenceTransformer(model_name)
        self.dim = dim

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


@lru_cache
def get_embedder() -> Embedder:
    settings = get_settings()
    try:
        return E5Embedder(settings.embedding_model, settings.embedding_dim)
    except ImportError:
        return HashingEmbedder(settings.embedding_dim)
