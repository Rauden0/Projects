# RssProxy

Enterprise-oriented RSS caching proxy built with **Qt 6** and **C++17**. The server polls subscribed feeds on configurable intervals, stores responses in SQLite, and exposes a JSON HTTP API. A lightweight CLI client is included for local administration.

## Architecture

```
Client (REPL) ──HTTP──► HttpServer ──► Database (SQLite)
                              │
                              └──► FeedScheduler ──► FeedFetcher ──► upstream RSS
```

| Component | Responsibility |
|-----------|----------------|
| `HttpServer` | REST API, optional API-key auth, health probes |
| `Database` | Feed metadata + cached XML payloads |
| `FeedScheduler` | Interval polling with concurrency limits |
| `FeedFetcher` | HTTP client with timeouts and SSRF checks |
| `UrlValidator` | Blocks private/loopback targets by default |

## Quick start

### Prerequisites

- CMake 3.16+
- MSVC 2022 (Windows) or compatible C++17 toolchain
- Qt 6.x with modules: Core, Network, HttpServer, Sql, Test

See [SETUP.md](SETUP.md) for Qt installation. A local kit may be placed at `Tests/6.8.0/msvc2022_64` for development.

### Build (monorepo)

```powershell
cmake -B build
cmake --build build --config Release
ctest --test-dir build -C Release --output-on-failure
```

### Run

```powershell
$env:PATH = "Tests\6.8.0\msvc2022_64\bin;$env:PATH"
Server\build\Release\Server.exe -c Server\config.ini
```

In another terminal:

```powershell
Client\build\Release\Client.exe -H 127.0.0.1 -p 8080
```

## Configuration (`config.ini`)

| Section | Key | Default | Description |
|---------|-----|---------|-------------|
| `server` | `host` | `127.0.0.1` | Bind address |
| `server` | `port` | `8080` | Listen port |
| `database` | `path` | `rss_proxy.db` | SQLite file |
| `security` | `apiKey` | *(empty)* | API key; required when binding to `0.0.0.0` |
| `scheduler` | `intervalMs` | `10000` | Poll tick interval |
| `scheduler` | `maxConcurrent` | `8` | Max parallel fetches |
| `fetch` | `timeoutMs` | `30000` | Per-request timeout |
| `fetch` | `maxResponseBytes` | `5242880` | Max cached payload (5 MiB) |
| `fetch` | `allowLocalhost` | `false` | Allow loopback/private URLs |

Pass the API key to the client with `-k <key>` or send `Authorization: Bearer <key>` / `X-Api-Key: <key>`.

## API

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/health` | No | Liveness probe |
| GET | `/ready` | No | Readiness (DB + scheduler stats) |
| POST | `/feeds` | Yes* | Add feed |
| GET | `/feeds` | Yes* | List feeds (`?label=`) |
| DELETE | `/feeds` | Yes* | Remove feed (`?url=`) |
| GET | `/feeds/data` | Yes* | Cached XML (`?url=`) |
| GET | `/feeds/state` | Yes* | Feed status (`?url=`) |
| POST | `/quit` | Yes* | Graceful shutdown |

\*Auth is enforced only when `security/apiKey` is set.

Full examples: [Server/README.md](Server/README.md).

## Security notes

- **Default bind is localhost** — safe for development.
- **Set `apiKey`** before exposing the service on a network interface.
- **SSRF protection** rejects private IP ranges and `localhost` unless `allowLocalhost=true`.
- **Response size cap** prevents unbounded memory use from malicious feeds.

## Logging

Qt logging categories:

| Category | Usage |
|----------|-------|
| `rssproxy` | Application lifecycle |
| `rssproxy.http` | HTTP access log |
| `rssproxy.fetch` | Upstream fetch events |

Enable with `QT_LOGGING_RULES="rssproxy.*=true"`.

## Project layout

```
RssProxy/
├── CMakeLists.txt      # Top-level monorepo build
├── Server/             # rssproxy_core library + Server executable
├── Client/             # CLI client
├── Tests/              # Qt Test suites (CTest)
├── SETUP.md
└── check-dependencies.ps1
```

## License

MIT — see [LICENSE](LICENSE).
