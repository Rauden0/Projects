#include "../Server/Database.h"
#include "../Server/FeedFetcher.h"
#include "../Server/FeedScheduler.h"
#include "../Server/HttpServer.h"
#include <QEventLoop>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QNetworkRequest>
#include <QtTest>

// ── HTTP helpers ──────────────────────────────────────────────────────────────

struct Response {
    int        status;
    QByteArray body;
    QJsonDocument json() const { return QJsonDocument::fromJson(body); }
};

class Http
{
public:
    explicit Http(quint16 port)
        : m_base(QStringLiteral("http://127.0.0.1:%1").arg(port)) {}

    Response post(const QString& path, const QByteArray& body = {})
    {
        QNetworkRequest req(QUrl(m_base + path));
        req.setHeader(QNetworkRequest::ContentTypeHeader, QStringLiteral("application/json"));
        return exec(m_nam.post(req, body));
    }

    Response get(const QString& path, const QString& query = {})
    {
        const QString url = query.isEmpty() ? m_base + path
                                            : m_base + path + u'?' + query;
        return exec(m_nam.get(QNetworkRequest(QUrl(url))));
    }

    Response del(const QString& path, const QString& query = {})
    {
        const QString url = query.isEmpty() ? m_base + path
                                            : m_base + path + u'?' + query;
        return exec(m_nam.deleteResource(QNetworkRequest(QUrl(url))));
    }

private:
    Response exec(QNetworkReply* reply)
    {
        QEventLoop loop;
        QObject::connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
        loop.exec();
        const int status = reply->attribute(
            QNetworkRequest::HttpStatusCodeAttribute).toInt();
        const QByteArray body = reply->readAll();
        reply->deleteLater();
        return {status, body};
    }

    QNetworkAccessManager m_nam;
    QString               m_base;
};

// ── Test class ────────────────────────────────────────────────────────────────

class tst_HttpServer : public QObject
{
    Q_OBJECT

private:
    Database*      m_db        = nullptr;
    FeedScheduler* m_scheduler = nullptr;
    HttpServer*    m_server    = nullptr;
    Http*          m_http      = nullptr;

    static constexpr char kUrl[]  = "https://example.com/rss";
    static constexpr char kUrl2[] = "https://other.com/rss";

    static QByteArray addBody(const char* url, qint64 interval,
                               QStringList labels = {})
    {
        QJsonArray arr;
        for (auto& l : labels) arr.append(l);
        return QJsonDocument(QJsonObject{
            {QStringLiteral("url"),      QLatin1String(url)},
            {QStringLiteral("interval"), interval},
            {QStringLiteral("labels"),   arr},
        }).toJson();
    }

private slots:

    void initTestCase()
    {
        m_db        = new Database(QStringLiteral(":memory:"), this);
        QVERIFY(m_db->open());
        m_scheduler = new FeedScheduler(m_db, 60'000, 8, this);
        m_server    = new HttpServer(QStringLiteral("127.0.0.1"), 0,
                                     m_db, m_scheduler, {}, false, this);
        QVERIFY(m_server->listen());
        m_http = new Http(m_server->boundPort());
    }

    void cleanupTestCase()
    {
        delete m_http;
    }

    void cleanup()
    {
        // Remove all feeds between tests so each test starts clean
        for (const auto& f : m_db->allFeeds())
            m_db->removeFeed(f.url);
    }

    // ── POST /feeds ──────────────────────────────────────────────────────────

    void addFeed_valid_returns201()
    {
        auto r = m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 3600));
        QCOMPARE(r.status, 201);
        QVERIFY(!r.json().isNull());
    }

    void addFeed_duplicate_returns409()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 60));
        auto r = m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 60));
        QCOMPARE(r.status, 409);
    }

    void addFeed_missingUrl_returns400()
    {
        const QByteArray body = QJsonDocument(QJsonObject{
            {QStringLiteral("interval"), 60}
        }).toJson();
        auto r = m_http->post(QStringLiteral("/feeds"), body);
        QCOMPARE(r.status, 400);
    }

    void addFeed_invalidInterval_returns400()
    {
        const QByteArray body = QJsonDocument(QJsonObject{
            {QStringLiteral("url"), QLatin1String(kUrl)},
            {QStringLiteral("interval"), -1},
        }).toJson();
        auto r = m_http->post(QStringLiteral("/feeds"), body);
        QCOMPARE(r.status, 400);
    }

    void addFeed_notJson_returns400()
    {
        auto r = m_http->post(QStringLiteral("/feeds"), QByteArray("not json at all"));
        QCOMPARE(r.status, 400);
    }

    // ── GET /feeds ────────────────────────────────────────────────────────────

    void listFeeds_empty_returnsEmptyArray()
    {
        auto r = m_http->get(QStringLiteral("/feeds"));
        QCOMPARE(r.status, 200);
        QVERIFY(r.json().isArray());
        QVERIFY(r.json().array().isEmpty());
    }

    void health_returnsOk()
    {
        auto r = m_http->get(QStringLiteral("/health"));
        QCOMPARE(r.status, 200);
        QCOMPARE(r.json().object().value(QStringLiteral("status")).toString(),
                 QStringLiteral("ok"));
    }

    void ready_returnsReady()
    {
        auto r = m_http->get(QStringLiteral("/ready"));
        QCOMPARE(r.status, 200);
        QCOMPARE(r.json().object().value(QStringLiteral("status")).toString(),
                 QStringLiteral("ready"));
    }

    void listFeeds_afterAdd_returnsFeeds()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 60));
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl2, 60));
        auto r = m_http->get(QStringLiteral("/feeds"));
        QCOMPARE(r.status, 200);
        QCOMPARE(r.json().array().size(), 2);
    }

    void listFeeds_byLabel_filters()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl,  60, {QStringLiteral("tech")}));
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl2, 60, {QStringLiteral("news")}));

        auto r = m_http->get(QStringLiteral("/feeds"), QStringLiteral("label=tech"));
        QCOMPARE(r.status, 200);
        QCOMPARE(r.json().array().size(), 1);
        QCOMPARE(r.json().array()[0].toObject()
                     .value(QStringLiteral("url")).toString(),
                 QLatin1String(kUrl));
    }

    void listFeeds_byLabel_noMatch_returnsEmptyArray()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 60, {QStringLiteral("tech")}));
        auto r = m_http->get(QStringLiteral("/feeds"), QStringLiteral("label=sport"));
        QCOMPARE(r.status, 200);
        QVERIFY(r.json().array().isEmpty());
    }

    // ── GET /feeds/state ──────────────────────────────────────────────────────

    void getState_notFound_returns404()
    {
        auto r = m_http->get(QStringLiteral("/feeds/state"),
                              QStringLiteral("url=https://nobody.com/rss"));
        QCOMPARE(r.status, 404);
    }

    void getState_missingParam_returns400()
    {
        auto r = m_http->get(QStringLiteral("/feeds/state"));
        QCOMPARE(r.status, 400);
    }

    void getState_afterAdd_returnsInfo()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 1800));
        auto r = m_http->get(QStringLiteral("/feeds/state"),
                              QStringLiteral("url=") + QLatin1String(kUrl));
        QCOMPARE(r.status, 200);
        const QJsonObject obj = r.json().object();
        QVERIFY(obj.contains(QStringLiteral("url")));
        QVERIFY(obj.contains(QStringLiteral("interval")));
        QVERIFY(obj.contains(QStringLiteral("hasData")));
        QCOMPARE(obj.value(QStringLiteral("interval")).toInteger(), qint64(1800));
        QCOMPARE(obj.value(QStringLiteral("hasData")).toBool(), false);
    }

    // ── GET /feeds/data ───────────────────────────────────────────────────────

    void getData_noDataYet_returns404()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 60));
        auto r = m_http->get(QStringLiteral("/feeds/data"),
                              QStringLiteral("url=") + QLatin1String(kUrl));
        QCOMPARE(r.status, 404);
    }

    void getData_afterUpdate_returnsXml()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 60));
        const QByteArray xml = QByteArray("<rss><channel><title>Test</title></channel></rss>");
        m_db->updateFeedData(QLatin1String(kUrl), xml, false);

        auto r = m_http->get(QStringLiteral("/feeds/data"),
                              QStringLiteral("url=") + QLatin1String(kUrl));
        QCOMPARE(r.status, 200);
        QCOMPARE(r.body, xml);
    }

    void getData_notFound_returns404()
    {
        auto r = m_http->get(QStringLiteral("/feeds/data"),
                              QStringLiteral("url=https://nobody.com/rss"));
        QCOMPARE(r.status, 404);
    }

    // ── DELETE /feeds ─────────────────────────────────────────────────────────

    void removeFeed_valid_returns200()
    {
        m_http->post(QStringLiteral("/feeds"), addBody(kUrl, 60));
        auto r = m_http->del(QStringLiteral("/feeds"),
                              QStringLiteral("url=") + QLatin1String(kUrl));
        QCOMPARE(r.status, 200);
        // Feed is gone
        auto list = m_http->get(QStringLiteral("/feeds"));
        QVERIFY(list.json().array().isEmpty());
    }

    void removeFeed_notFound_returns404()
    {
        auto r = m_http->del(QStringLiteral("/feeds"),
                              QStringLiteral("url=https://nobody.com/rss"));
        QCOMPARE(r.status, 404);
    }

    void removeFeed_missingParam_returns400()
    {
        auto r = m_http->del(QStringLiteral("/feeds"));
        QCOMPARE(r.status, 400);
    }
};

QTEST_MAIN(tst_HttpServer)
#include "tst_HttpServer.moc"
