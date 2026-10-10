"""Thin wrapper over the LLM provider so the rest of the code (and tests) never import the SDK."""

from typing import Any, Protocol

from jobmatcher.config import get_settings


class LLMClient(Protocol):
    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str: ...

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 2048,
    ) -> Any: ...


class AnthropicClient:
    def __init__(self, api_key: str, model: str) -> None:
        from anthropic import AsyncAnthropic  # optional dependency

        self._client = AsyncAnthropic(api_key=api_key)
        self.model = model

    async def complete(self, *, system: str, user: str, max_tokens: int = 1024) -> str:
        response = await self._client.messages.create(
            model=self.model,
            max_tokens=max_tokens,
            system=system,
            messages=[{"role": "user", "content": user}],
        )
        return "".join(block.text for block in response.content if block.type == "text")

    async def create_message(
        self,
        *,
        system: str,
        messages: list[dict[str, Any]],
        tools: list[dict[str, Any]],
        tool_choice: dict[str, str] | None = None,
        max_tokens: int = 2048,
    ) -> Any:
        return await self._client.messages.create(
            model=self.model,
            max_tokens=max_tokens,
            system=system,
            messages=messages,
            tools=tools,
            tool_choice=tool_choice or {"type": "auto"},
        )


def get_llm() -> LLMClient | None:
    settings = get_settings()
    if not settings.llm_api_key:
        return None
    return AnthropicClient(settings.llm_api_key, settings.llm_model)
