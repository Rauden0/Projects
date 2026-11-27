class JobMatcherError(Exception):
    """Base of every failure a caller is expected to handle; anything else is a bug."""


class NotFound(JobMatcherError):
    def __init__(self, resource: str, key: object) -> None:
        super().__init__(f"{resource} {key} not found")
        self.resource = resource
        self.key = key


class LLMUnavailable(JobMatcherError):
    """No LLM is configured for this purpose."""


class LLMError(JobMatcherError):
    """The provider call failed: network, timeout, error status or an unreadable response."""

    def __init__(self, message: str, status_code: int | None = None) -> None:
        super().__init__(message)
        self.status_code = status_code


class ExtractionError(JobMatcherError):
    """The LLM answered, but not with valid facts."""


class ToolError(JobMatcherError):
    """An agent tool rejected its input; the message is shown to the model."""


class BudgetExceeded(JobMatcherError):
    def __init__(self, purpose: str, spent_usd: float, limit_usd: float) -> None:
        super().__init__(f"{purpose} budget exhausted: ${spent_usd:.2f} of ${limit_usd:.2f}")
        self.purpose = purpose
        self.spent_usd = spent_usd
        self.limit_usd = limit_usd


class ClientLimited(JobMatcherError):
    def __init__(self, reason: str, retry_after_s: int) -> None:
        super().__init__(reason)
        self.reason = reason
        self.retry_after_s = retry_after_s
