import gzip

import pytest

from jobmatcher import messaging


def test_gzip_body_is_decoded() -> None:
    assert messaging.decode_body(gzip.compress(b"{}"), "gzip") == b"{}"


def test_identity_body_is_returned_unchanged() -> None:
    assert messaging.decode_body(b"{}", None) == b"{}"


def test_gzip_bomb_is_rejected() -> None:
    bomb = gzip.compress(b"\0" * (messaging.MAX_DECODED_BYTES + 1))
    with pytest.raises(messaging.UndecodableBody):
        messaging.decode_body(bomb, "gzip")


def test_body_at_the_limit_is_accepted() -> None:
    body = b"a" * messaging.MAX_DECODED_BYTES
    assert messaging.decode_body(gzip.compress(body), "gzip") == body


def test_truncated_gzip_is_rejected() -> None:
    with pytest.raises(messaging.UndecodableBody):
        messaging.decode_body(gzip.compress(b"x" * 1000)[:-10], "gzip")


def test_unknown_encoding_is_rejected() -> None:
    with pytest.raises(messaging.UndecodableBody):
        messaging.decode_body(b"x", "br")
