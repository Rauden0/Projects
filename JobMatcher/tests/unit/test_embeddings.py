import math

import pytest

from jobmatcher.search.embeddings import HashingEmbedder


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
