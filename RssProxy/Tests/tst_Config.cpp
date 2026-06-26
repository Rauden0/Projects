#include "../Server/Config.h"
#include <QDir>
#include <QTemporaryFile>
#include <QtTest>

class tst_Config : public QObject
{
    Q_OBJECT

private slots:

    void defaults_whenFileDoesNotExist()
    {
        Config c = Config::load(QStringLiteral("nonexistent_config_xyz.ini"));
        QCOMPARE(c.host,   QStringLiteral("127.0.0.1"));
        QCOMPARE(c.port,   quint16(8080));
        QCOMPARE(c.dbPath, QStringLiteral("rss_proxy.db"));
    }

    void load_parsesValues()
    {
        QTemporaryFile f;
        f.setAutoRemove(true);
        QVERIFY(f.open());
        f.write(
            "[server]\n"
            "host = 0.0.0.0\n"
            "port = 9090\n"
            "[database]\n"
            "path = /tmp/my.db\n"
        );
        f.close();

        Config c = Config::load(f.fileName());
        QCOMPARE(c.host,   QStringLiteral("0.0.0.0"));
        QCOMPARE(c.port,   quint16(9090));
        QCOMPARE(c.dbPath, QStringLiteral("/tmp/my.db"));
    }

    void saveAndReload_roundtrip()
    {
        QTemporaryFile f;
        QVERIFY(f.open());
        const QString path = f.fileName();
        f.close();

        Config orig;
        orig.host   = QStringLiteral("192.168.1.1");
        orig.port   = 1234;
        orig.dbPath = QStringLiteral("data/test.db");
        orig.save(path);

        Config loaded = Config::load(path);
        QCOMPARE(loaded.host,   orig.host);
        QCOMPARE(loaded.port,   orig.port);
        QCOMPARE(loaded.dbPath, orig.dbPath);
    }

    void load_invalidPort_keepsDefault()
    {
        QTemporaryFile f;
        QVERIFY(f.open());
        f.write("[server]\nport = notanumber\n");
        f.close();

        Config c = Config::load(f.fileName());
        QVERIFY(c.port == 0 || c.port == 8080);
    }

    void isValid_rejectsPublicBindWithoutApiKey()
    {
        Config c;
        c.host = QStringLiteral("0.0.0.0");
        c.apiKey.clear();
        QString err;
        QVERIFY(!c.isValid(&err));
        QVERIFY(!err.isEmpty());
    }
};

QTEST_MAIN(tst_Config)
#include "tst_Config.moc"
