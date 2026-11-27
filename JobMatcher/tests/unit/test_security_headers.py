import base64
import hashlib
import re

from fastapi.testclient import TestClient

from jobmatcher.api.main import CDN_SCRIPTS, create_app


def _csp(header: str) -> dict[str, list[str]]:
    directives = (d.split() for d in header.split(";") if d.strip())
    return {d[0]: d[1:] for d in directives}


def test_page_csp_allows_only_its_own_inline_code_and_the_pinned_scripts() -> None:
    response = TestClient(create_app()).get("/")
    csp = _csp(response.headers["content-security-policy"])
    inline = re.findall(r"<script>(.*?)</script>", response.text, flags=re.DOTALL)
    digest = base64.b64encode(hashlib.sha256(inline[0].encode()).digest()).decode()

    assert csp["default-src"] == ["'none'"]
    assert csp["script-src"] == [*CDN_SCRIPTS, f"'sha256-{digest}'"]
    assert csp["connect-src"] == ["'self'"]
    assert csp["frame-ancestors"] == ["'none'"]
    for url in CDN_SCRIPTS:
        assert f'src="{url}"' in response.text


def test_api_responses_carry_security_headers() -> None:
    response = TestClient(create_app()).get("/healthz")
    assert response.headers["x-content-type-options"] == "nosniff"
    assert response.headers["referrer-policy"] == "no-referrer"
    assert response.headers["x-frame-options"] == "DENY"
    assert _csp(response.headers["content-security-policy"])["default-src"] == ["'none'"]


def test_swagger_ui_keeps_working_without_the_api_csp() -> None:
    response = TestClient(create_app()).get("/docs")
    assert "content-security-policy" not in response.headers
    assert response.headers["x-content-type-options"] == "nosniff"
