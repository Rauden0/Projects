#include "../Server/Database.h"
#include <QTemporaryFile>
#include <QtTest>

class tst_Database : public QObject
{
    Q_OBJECT

private:
    QString     m_dbPath;
    Database*   m_db = nullptr;

    void setup()
    {
        m_db = new Database(QStringLiteral(":memory:"), this);
        QVERIFY(m_db->open());
    }
    void teardown()
    {
        delete m_db;
        m_db = nullptr;
    }

private slots:


    void open_createsSchema()
    {
        setup();
        QVERIFY(m_db->allFeeds().isEmpty());
        teardown();
    }

    void addFeed_basic()
    {
        setup();
        QVERIFY(m_db->addFeed(QStringLiteral("https://example.com/rss"),
                              {QStringLiteral("tech")}, 3600));
        auto feeds = m_db->allFeeds();
        QCOMPARE(feeds.size(), 1);
        QCOMPARE(feeds[0].url, QStringLiteral("https://example.com/rss"));
        QCOMPARE(feeds[0].interval, qint64(3600));
        QCOMPARE(feeds[0].labels, QStringList{QStringLiteral("tech")});
        teardown();
    }

    void addFeed_duplicateUrl_fails()
    {
        setup();
        QVERIFY( m_db->addFeed(QStringLiteral("https://dupe.com/rss"), {}, 60));
        QVERIFY(!m_db->addFeed(QStringLiteral("https://dupe.com/rss"), {}, 60));
        teardown();
    }

    void addFeed_multipleLabels()
    {
        setup();
        const QStringList labels{QStringLiteral("a"), QStringLiteral("b"), QStringLiteral("c")};
        QVERIFY(m_db->addFeed(QStringLiteral("https://multi.com/rss"), labels, 120));
        auto f = m_db->feedByUrl(QStringLiteral("https://multi.com/rss"));
        QVERIFY(f.has_value());
        QCOMPARE(f->labels, labels);
        teardown();
    }

    void feedByUrl_notFound_returnsNullopt()
    {
        setup();
        QVERIFY(!m_db->feedByUrl(QStringLiteral("https://nobody.com/rss")).has_value());
        teardown();
    }

    void feedByUrl_found()
    {
        setup();
        m_db->addFeed(QStringLiteral("https://found.com/rss"), {}, 10);
        QVERIFY(m_db->feedByUrl(QStringLiteral("https://found.com/rss")).has_value());
        teardown();
    }


    void removeFeed_removesEntry()
    {
        setup();
        const QString url = QStringLiteral("https://rm.com/rss");
        m_db->addFeed(url, {}, 60);
        QVERIFY(m_db->feedByUrl(url).has_value());
        QVERIFY(m_db->removeFeed(url));
        QVERIFY(!m_db->feedByUrl(url).has_value());
        QVERIFY(m_db->allFeeds().isEmpty());
        teardown();
    }

    void removeFeed_cascadesData()
    {
        setup();
        const QString url = QStringLiteral("https://cascade.com/rss");
        m_db->addFeed(url, {}, 60);
        m_db->updateFeedData(url, QByteArray("<rss/>"), false);
        QVERIFY(m_db->feedData(url).has_value());

        m_db->removeFeed(url);
        QVERIFY(!m_db->feedData(url).has_value());
        teardown();
    }

    void feedsByLabel_filters()
    {
        setup();
        m_db->addFeed(QStringLiteral("https://tech.com/rss"),   {QStringLiteral("tech")}, 60);
        m_db->addFeed(QStringLiteral("https://news.com/rss"),   {QStringLiteral("news")}, 60);
        m_db->addFeed(QStringLiteral("https://both.com/rss"),
                      {QStringLiteral("tech"), QStringLiteral("news")}, 60);

        auto tech = m_db->feedsByLabel(QStringLiteral("tech"));
        QCOMPARE(tech.size(), 2);

        auto news = m_db->feedsByLabel(QStringLiteral("news"));
        QCOMPARE(news.size(), 2);

        auto empty = m_db->feedsByLabel(QStringLiteral("sport"));
        QVERIFY(empty.isEmpty());
        teardown();
    }


    void feedData_noDataYet_returnsNullopt()
    {
        setup();
        m_db->addFeed(QStringLiteral("https://nodata.com/rss"), {}, 60);
        QVERIFY(!m_db->feedData(QStringLiteral("https://nodata.com/rss")).has_value());
        teardown();
    }

    void updateFeedData_storesContent()
    {
        setup();
        const QString url = QStringLiteral("https://data.com/rss");
        m_db->addFeed(url, {}, 60);
        const QByteArray xml = QByteArray("<rss><channel/></rss>");
        QVERIFY(m_db->updateFeedData(url, xml, false));

        auto data = m_db->feedData(url);
        QVERIFY(data.has_value());
        QCOMPARE(*data, xml);
        teardown();
    }

    void updateFeedData_error_setsFlag()
    {
        setup();
        const QString url = QStringLiteral("https://err.com/rss");
        m_db->addFeed(url, {}, 60);
        m_db->updateFeedData(url, QByteArray("<rss/>"), false);
        QVERIFY(m_db->updateFeedData(url, {}, true));
        auto f = m_db->feedByUrl(url);
        QVERIFY(f.has_value());
        QVERIFY(f->lastError);
        QVERIFY(m_db->feedData(url).has_value());
        teardown();
    }

    void updateFeedData_updatesTimestamp()
    {
        setup();
        const QString url = QStringLiteral("https://ts.com/rss");
        m_db->addFeed(url, {}, 60);
        QCOMPARE(m_db->feedByUrl(url)->lastUpdate, qint64(0));
        m_db->updateFeedData(url, QByteArray("<rss/>"), false);
        QVERIFY(m_db->feedByUrl(url)->lastUpdate > 0);
        teardown();
    }
};

QTEST_MAIN(tst_Database)
#include "tst_Database.moc"
