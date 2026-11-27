import math
import sys
from types import ModuleType

import pytest

from jobmatcher.search.embeddings import CachedEmbedder, E5Embedder, HashingEmbedder


@pytest.mark.asyncio
async def test_hashing_embedder_is_normalized_and_deterministic() -> None:
    embedder = HashingEmbedder(dim=64)
    [a, b] = await embedder.embed_documents(["Python backend", "python  BACKEND"])
    assert a == b
    assert math.isclose(sum(x * x for x in a), 1.0)


@pytest.mark.asyncio
async def test_hashing_embedder_similarity_follows_overlap() -> None:
    embedder = HashingEmbedder(dim=256)
    query = await embedder.embed_query("python postgresql rabbitmq")
    close, far = await embedder.embed_documents(
        ["python developer postgresql rabbitmq docker", "marketing manager sales"]
    )

    def cosine(u: list[float], v: list[float]) -> float:
        return sum(x * y for x, y in zip(u, v, strict=True))

    assert cosine(query, close) > cosine(query, far)


class FakeSentenceTransformer:
    def __init__(self, model_name: str) -> None:
        self.model_name = model_name

    def get_sentence_embedding_dimension(self) -> int:
        return 768


@pytest.fixture
def fake_sentence_transformers(monkeypatch: pytest.MonkeyPatch) -> None:
    module = ModuleType("sentence_transformers")
    module.SentenceTransformer = FakeSentenceTransformer  # type: ignore[attr-defined]
    monkeypatch.setitem(sys.modules, "sentence_transformers", module)


def test_e5_embedder_rejects_a_model_with_another_dimension(
    fake_sentence_transformers: None,
) -> None:
    with pytest.raises(ValueError, match="768-dimensional"):
        E5Embedder("intfloat/multilingual-e5-base", 384)
    assert E5Embedder("intfloat/multilingual-e5-base", 768).dim == 768


class CountingEmbedder(HashingEmbedder):
    def __init__(self) -> None:
        super().__init__(dim=16)
        self.queries: list[str] = []

    async def embed_query(self, text: str) -> list[float]:
        self.queries.append(text)
        return await super().embed_query(text)


async def test_cached_embedder_encodes_a_repeated_query_once() -> None:
    inner = CountingEmbedder()
    embedder = CachedEmbedder(inner, max_entries=2)
    first = await embedder.embed_query("python  Brno ")
    again = await embedder.embed_query("python Brno")
    assert first == again
    assert inner.queries == ["python Brno"]
    assert (embedder.dim, embedder.model_name) == (16, "hashing-16")


async def test_cached_embedder_evicts_the_least_recently_used_query() -> None:
    inner = CountingEmbedder()
    embedder = CachedEmbedder(inner, max_entries=2)
    for query in ("a", "b", "a", "c", "a", "b"):
        await embedder.embed_query(query)
    assert inner.queries == ["a", "b", "c", "b"]


async def test_cached_embedder_passes_documents_through() -> None:
    inner = CountingEmbedder()
    embedder = CachedEmbedder(inner, max_entries=2)
    assert await embedder.embed_documents(["x"]) == await inner.embed_documents(["x"])
    assert inner.queries == []
