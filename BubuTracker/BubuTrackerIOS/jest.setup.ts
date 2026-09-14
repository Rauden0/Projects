// react-test-renderer needs this flag so React knows updates made inside
// @testing-library/react-native's render()/waitFor() are wrapped in act();
// without it every state update from App.tsx's async effects logs a spurious
// "not configured to support act(...)" warning even though the tests pass.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
(globalThis as any).IS_REACT_ACT_ENVIRONMENT = true;
