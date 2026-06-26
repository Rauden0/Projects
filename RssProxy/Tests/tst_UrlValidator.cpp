#include "../Server/UrlValidator.h"
#include <QtTest>

class tst_UrlValidator : public QObject
{
    Q_OBJECT

private slots:
    void acceptsPublicHttps()
    {
        const auto r = UrlValidator::validate(QStringLiteral("https://example.com/rss"));
        QVERIFY(r.ok);
    }

    void rejectsInvalidUrl()
    {
        const auto r = UrlValidator::validate(QStringLiteral("not-a-url"));
        QVERIFY(!r.ok);
    }

    void rejectsNonHttpSchemes()
    {
        const auto r = UrlValidator::validate(QStringLiteral("file:///etc/passwd"));
        QVERIFY(!r.ok);
    }

    void rejectsLoopbackByDefault()
    {
        const auto r = UrlValidator::validate(QStringLiteral("http://127.0.0.1/feed"));
        QVERIFY(!r.ok);
    }

    void allowsLoopbackWhenConfigured()
    {
        const auto r = UrlValidator::validate(QStringLiteral("http://127.0.0.1/feed"), true);
        QVERIFY(r.ok);
    }

    void rejectsPrivateRanges()
    {
        const auto r = UrlValidator::validate(QStringLiteral("http://192.168.1.10/rss"));
        QVERIFY(!r.ok);
    }

    void rejectsLocalhostHostname()
    {
        const auto r = UrlValidator::validate(QStringLiteral("http://localhost/rss"));
        QVERIFY(!r.ok);
    }
};

QTEST_MAIN(tst_UrlValidator)
#include "tst_UrlValidator.moc"
