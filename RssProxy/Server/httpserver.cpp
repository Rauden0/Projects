#include "HttpServer.h"

#include "UrlValidator.h"

#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QLoggingCategory>
#include <QUrlQuery>

Q_LOGGING_CATEGORY(lcHttp, "rssproxy.http")

QHttpServerResponse HttpServer::jsonOk(const QJsonValue& payload)
{
    if (payload.isArray())
        return QHttpServerResponse(payload.toArray(), QHttpServerResponse::StatusCode::Ok);
    return QHttpServerResponse(payload.toObject(), QHttpServerResponse::StatusCode::Ok);
}

QHttpServerResponse HttpServer::jsonError(const QString& msg,
                                          QHttpServerResponse::StatusCode code)
{
    return QHttpServerResponse(QJsonObject{{QStringLiteral("error"), msg}}, code);
}

static QJsonObject feedToJson(const FeedInfo& f)
{
    QJsonArray labels;
    for (const auto& l : f.labels)
        labels.append(l);
    return QJsonObject{
        {QStringLiteral("url"),        f.url},
        {QStringLiteral("labels"),     labels},
        {QStringLiteral("interval"),   f.interval},
        {QStringLiteral("lastUpdate"), f.lastUpdate},
        {QStringLiteral("lastError"),  f.lastError},
    };
}

bool HttpServer::isAuthorized(const QHttpServerRequest& req) const
{
    if (m_apiKey.isEmpty())
        return true;

    QString token;
    const QByteArray authHdr = req.value(QByteArray("Authorization"));
    if (authHdr.startsWith("Bearer "))
        token = QString::fromUtf8(authHdr.mid(7)).trimmed();

    if (token.isEmpty()) {
        const QByteArray apiKeyHdr = req.value(QByteArray("X-Api-Key"));
        if (!apiKeyHdr.isEmpty())
            token = QString::fromUtf8(apiKeyHdr).trimmed();
    }

    return token == m_apiKey;
}


QHttpServerResponse HttpServer::handleHealth(const QHttpServerRequest&)
{
    return jsonOk(QJsonObject{{QStringLiteral("status"), QStringLiteral("ok")}});
}

QHttpServerResponse HttpServer::handleReady(const QHttpServerRequest&)
{
    if (!m_db->isOpen()) {
        return jsonError(QStringLiteral("Database not ready"),
                         QHttpServerResponse::StatusCode::ServiceUnavailable);
    }
    return jsonOk(QJsonObject{
        {QStringLiteral("status"), QStringLiteral("ready")},
        {QStringLiteral("activeFetches"), m_scheduler->activeFetchCount()},
        {QStringLiteral("queuedFetches"), m_scheduler->pendingFetchCount()},
    });
}

QHttpServerResponse HttpServer::handleAddFeed(const QHttpServerRequest& req)
{
    if (!isAuthorized(req))
        return jsonError(QStringLiteral("Unauthorized"),
                         QHttpServerResponse::StatusCode::Unauthorized);

    const auto doc = QJsonDocument::fromJson(req.body());
    if (!doc.isObject())
        return jsonError(QStringLiteral("Expected JSON object body"));

    const QJsonObject body = doc.object();
    const QString url = body.value(QStringLiteral("url")).toString().trimmed();
    if (url.isEmpty())
        return jsonError(QStringLiteral("'url' is required"));

    const auto validation = UrlValidator::validate(url, m_allowLocalhost);
    if (!validation.ok)
        return jsonError(validation.error);

    const qint64 interval = body.value(QStringLiteral("interval")).toInteger(3600);
    if (interval <= 0)
        return jsonError(QStringLiteral("'interval' must be > 0"));

    QStringList labels;
    for (const auto& v : body.value(QStringLiteral("labels")).toArray())
        labels.append(v.toString());

    if (m_db->feedByUrl(url))
        return jsonError(QStringLiteral("Feed already exists"),
                         QHttpServerResponse::StatusCode::Conflict);

    if (!m_db->addFeed(url, labels, interval))
        return jsonError(QStringLiteral("Database error"),
                         QHttpServerResponse::StatusCode::InternalServerError);

    m_scheduler->scheduleFetch(url);

    return QHttpServerResponse(
        QJsonObject{{QStringLiteral("message"), QStringLiteral("Feed added")}},
        QHttpServerResponse::StatusCode::Created);
}

QHttpServerResponse HttpServer::handleListFeeds(const QHttpServerRequest& req)
{
    if (!isAuthorized(req))
        return jsonError(QStringLiteral("Unauthorized"),
                         QHttpServerResponse::StatusCode::Unauthorized);

    const QString label = QUrlQuery(req.url()).queryItemValue(QStringLiteral("label"));
    const QList<FeedInfo> feeds =
        label.isEmpty() ? m_db->allFeeds() : m_db->feedsByLabel(label);

    QJsonArray arr;
    for (const auto& f : feeds)
        arr.append(feedToJson(f));
    return jsonOk(arr);
}

QHttpServerResponse HttpServer::handleGetData(const QHttpServerRequest& req)
{
    if (!isAuthorized(req))
        return jsonError(QStringLiteral("Unauthorized"),
                         QHttpServerResponse::StatusCode::Unauthorized);

    const QString url = QUrlQuery(req.url()).queryItemValue(QStringLiteral("url"));
    if (url.isEmpty())
        return jsonError(QStringLiteral("'url' query param is required"));

    if (!m_db->feedByUrl(url))
        return jsonError(QStringLiteral("Feed not found"),
                         QHttpServerResponse::StatusCode::NotFound);

    const auto data = m_db->feedData(url);
    if (!data)
        return jsonError(QStringLiteral("No data yet – feed may still be fetching"),
                         QHttpServerResponse::StatusCode::NotFound);

    return QHttpServerResponse(
        QByteArrayLiteral("application/xml"),
        *data,
        QHttpServerResponse::StatusCode::Ok);
}

QHttpServerResponse HttpServer::handleGetState(const QHttpServerRequest& req)
{
    if (!isAuthorized(req))
        return jsonError(QStringLiteral("Unauthorized"),
                         QHttpServerResponse::StatusCode::Unauthorized);

    const QString url = QUrlQuery(req.url()).queryItemValue(QStringLiteral("url"));
    if (url.isEmpty())
        return jsonError(QStringLiteral("'url' query param is required"));

    const auto feed = m_db->feedByUrl(url);
    if (!feed)
        return jsonError(QStringLiteral("Feed not found"),
                         QHttpServerResponse::StatusCode::NotFound);

    const auto data = m_db->feedData(url);
    return jsonOk(QJsonObject{
        {QStringLiteral("url"),        feed->url},
        {QStringLiteral("interval"),   feed->interval},
        {QStringLiteral("lastUpdate"), feed->lastUpdate},
        {QStringLiteral("lastError"),  feed->lastError},
        {QStringLiteral("hasData"),    data.has_value()},
        {QStringLiteral("dataSize"),   data ? static_cast<qint64>(data->size()) : 0LL},
    });
}

QHttpServerResponse HttpServer::handleRemoveFeed(const QHttpServerRequest& req)
{
    if (!isAuthorized(req))
        return jsonError(QStringLiteral("Unauthorized"),
                         QHttpServerResponse::StatusCode::Unauthorized);

    const QString url = QUrlQuery(req.url()).queryItemValue(QStringLiteral("url"));
    if (url.isEmpty())
        return jsonError(QStringLiteral("'url' query param is required"));

    if (!m_db->feedByUrl(url))
        return jsonError(QStringLiteral("Feed not found"),
                         QHttpServerResponse::StatusCode::NotFound);

    if (!m_db->removeFeed(url))
        return jsonError(QStringLiteral("Database error"),
                         QHttpServerResponse::StatusCode::InternalServerError);

    return jsonOk(QJsonObject{{QStringLiteral("message"), QStringLiteral("Feed removed")}});
}

QHttpServerResponse HttpServer::handleQuit(const QHttpServerRequest& req)
{
    if (!isAuthorized(req))
        return jsonError(QStringLiteral("Unauthorized"),
                         QHttpServerResponse::StatusCode::Unauthorized);

    qCInfo(lcHttp) << "Shutdown requested via /quit";
    QMetaObject::invokeMethod(this, [this]() { emit stopRequested(); }, Qt::QueuedConnection);
    return jsonOk(QJsonObject{{QStringLiteral("message"), QStringLiteral("Shutting down")}});
}


void HttpServer::setupRoutes()
{
    using Method = QHttpServerRequest::Method;

    m_server.route(QStringLiteral("/health"), Method::Get,
        [this](const QHttpServerRequest& req) { return handleHealth(req); });

    m_server.route(QStringLiteral("/ready"), Method::Get,
        [this](const QHttpServerRequest& req) { return handleReady(req); });

    m_server.route(QStringLiteral("/feeds"), Method::Post,
        [this](const QHttpServerRequest& req) { return handleAddFeed(req); });

    m_server.route(QStringLiteral("/feeds"), Method::Get,
        [this](const QHttpServerRequest& req) { return handleListFeeds(req); });

    m_server.route(QStringLiteral("/feeds"), Method::Delete,
        [this](const QHttpServerRequest& req) { return handleRemoveFeed(req); });

    m_server.route(QStringLiteral("/feeds/data"), Method::Get,
        [this](const QHttpServerRequest& req) { return handleGetData(req); });

    m_server.route(QStringLiteral("/feeds/state"), Method::Get,
        [this](const QHttpServerRequest& req) { return handleGetState(req); });

    m_server.route(QStringLiteral("/quit"), Method::Post,
        [this](const QHttpServerRequest& req) { return handleQuit(req); });

    m_server.addAfterRequestHandler(this,
        [](const QHttpServerRequest& request, QHttpServerResponse& response) {
            qCInfo(lcHttp).nospace()
                << request.method() << ' ' << request.url().path()
                << " -> " << static_cast<int>(response.statusCode());
        });
}

HttpServer::HttpServer(const QString& host, quint16 port,
                       Database* db, FeedScheduler* scheduler,
                       const QString& apiKey, bool allowLocalhost,
                       QObject* parent)
    : QObject(parent)
    , m_host(host)
    , m_port(port)
    , m_apiKey(apiKey)
    , m_allowLocalhost(allowLocalhost)
    , m_db(db)
    , m_scheduler(scheduler)
{
    setupRoutes();
}

bool HttpServer::listen()
{
    m_tcpServer = new QTcpServer(this);
    if (!m_tcpServer->listen(QHostAddress(m_host), m_port)) {
        qCCritical(lcHttp) << "Failed to listen on" << m_host << ":" << m_port
                           << "-" << m_tcpServer->errorString();
        return false;
    }
    if (!m_server.bind(m_tcpServer)) {
        qCCritical(lcHttp) << "Failed to bind QHttpServer to TCP server";
        return false;
    }
    m_boundPort = m_tcpServer->serverPort();
    qCInfo(lcHttp) << "Listening on" << m_host << ":" << m_boundPort
                   << (m_apiKey.isEmpty() ? "(auth disabled)" : "(auth enabled)");
    return true;
}
