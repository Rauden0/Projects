# RssProxy

RSS caching proxy. The **Server** periodically fetches RSS feeds and serves them via a JSON HTTP API. The **Client** is a tiny CLI to drive the server.

---

## Server

### Configuration (`config.ini`)

```ini
[server]
host = 127.0.0.1
port = 8080

[database]
path = rss_proxy.db
```

Pass a custom config with `--config /path/to/config.ini`.

### API

| Method | Path            | Query / Body                                   | Description            |
|--------|-----------------|------------------------------------------------|------------------------|
| POST   | `/feeds`        | JSON `{url, interval, labels[]}`               | Add a feed             |
| GET    | `/feeds`        | `?label=<label>` _(optional)_                  | List feeds             |
| DELETE | `/feeds`        | `?url=<url>`                                   | Remove a feed          |
| GET    | `/feeds/data`   | `?url=<url>`                                   | Raw XML content        |
| GET    | `/feeds/state`  | `?url=<url>`                                   | Feed status JSON       |
| POST   | `/quit`         | –                                              | Shutdown server        |

---

## Client

```
Usage: Client [--host <host>] [--port <port>]

Commands:
  add <url> <interval_secs> [label ...]   Add a feed
  list [label]                            List feeds
  get <url>                               Print raw XML
  state <url>                             Show feed status
  remove <url>                            Remove a feed
  quit                                    Shutdown server
  help                                    Show help
```

### Examples

```
> add https://www.nasa.gov/rss/dyn/lg_image_of_the_day.rss 3600 space nasa
> list space
> state https://www.nasa.gov/rss/dyn/lg_image_of_the_day.rss
> get https://www.nasa.gov/rss/dyn/lg_image_of_the_day.rss
> remove https://www.nasa.gov/rss/dyn/lg_image_of_the_day.rss
> quit
```

