#include "Database.h"
#include <QDateTime>
#include <QSqlError>
#include <QSqlQuery>
#include <atomic>

static QString uniqueConnName()
{
    static std::atomic<int> counter{0};
    return QStringLiteral("rss_proxy_%1").arg(counter.fetch_add(1));
}

Database::Database(const QString& dbPath, QObject* parent)
    : QObject(parent), m_dbPath(dbPath), m_connName(uniqueConnName())
{}

Database::~Database()
{
    m_db.close();
    m_db = QSqlDatabase();
    QSqlDatabase::removeDatabase(m_connName);
}

bool Database::open()
{
    m_db = QSqlDatabase::addDatabase(QStringLiteral("QSQLITE"), m_connName);
    m_db.setDatabaseName(m_dbPath);
    if (!m_db.open()) {
        qCritical("Database::open: %s", qPrintable(m_db.lastError().text()));
        return false;
    }
    return createSchema();
}

bool Database::createSchema()
{
    QSqlQuery q(m_db);

    const auto exec = [&](const char* sql) -> bool {
        if (!q.exec(sql)) {
            qCritical("Database::createSchema: %s", qPrintable(q.lastError().text()));
            return false;
        }
        return true;
    };

    return exec("PRAGMA foreign_keys = ON") &&
           exec(R"sql(
               CREATE TABLE IF NOT EXISTS feeds (
                   id          INTEGER PRIMARY KEY AUTOINCREMENT,
                   url         TEXT    NOT NULL UNIQUE,
                   labels      TEXT    DEFAULT '',
                   interval    INTEGER NOT NULL DEFAULT 3600,
                   last_update INTEGER NOT NULL DEFAULT 0,
                   last_error  INTEGER NOT NULL DEFAULT 0
               )
           )sql") &&
           exec(R"sql(
               CREATE TABLE IF NOT EXISTS feed_data (
                   feed_id    INTEGER PRIMARY KEY
                              REFERENCES feeds(id) ON DELETE CASCADE,
                   content    BLOB    NOT NULL DEFAULT '',
                   updated_at INTEGER NOT NULL DEFAULT 0
               )
           )sql");
}

static FeedInfo rowToFeed(const QSqlQuery& q)
{
    const QVariant labelsValue = q.value(QStringLiteral("labels"));
    const QStringList labels = labelsValue.isNull()
        ? QStringList{}
        : labelsValue.toString().split(u',', Qt::SkipEmptyParts);

    return FeedInfo{
        q.value(QStringLiteral("id")).toLongLong(),
        q.value(QStringLiteral("url")).toString(),
        labels,
        q.value(QStringLiteral("interval")).toLongLong(),
        q.value(QStringLiteral("last_update")).toLongLong(),
        q.value(QStringLiteral("last_error")).toBool(),
    };
}

bool Database::addFeed(const QString& url, const QStringList& labels, qint64 intervalSecs)
{
    QSqlQuery q(m_db);
    q.prepare(QStringLiteral(
        "INSERT INTO feeds (url, labels, interval) VALUES (:url, :labels, :interval)"));
    q.bindValue(QStringLiteral(":url"),      url);
    // Qt/SQLite maps empty QString to NULL; bind UTF-8 bytes to store "" reliably.
    q.bindValue(QStringLiteral(":labels"), labels.join(u',').toUtf8());
    q.bindValue(QStringLiteral(":interval"), intervalSecs);
    if (!q.exec()) {
        qWarning("Database::addFeed: %s", qPrintable(q.lastError().text()));
        return false;
    }
    return true;
}

bool Database::removeFeed(const QString& url)
{
    QSqlQuery q(m_db);
    q.prepare(QStringLiteral("DELETE FROM feeds WHERE url = :url"));
    q.bindValue(QStringLiteral(":url"), url);
    if (!q.exec()) {
        qWarning("Database::removeFeed: %s", qPrintable(q.lastError().text()));
        return false;
    }
    return true;
}

std::optional<FeedInfo> Database::feedByUrl(const QString& url) const
{
    QSqlQuery q(m_db);
    q.prepare(QStringLiteral("SELECT * FROM feeds WHERE url = :url LIMIT 1"));
    q.bindValue(QStringLiteral(":url"), url);
    if (q.exec() && q.next())
        return rowToFeed(q);
    return std::nullopt;
}

QList<FeedInfo> Database::allFeeds() const
{
    QSqlQuery q(m_db);
    if (!q.exec(QStringLiteral("SELECT * FROM feeds ORDER BY id"))) {
        qWarning("Database::allFeeds: %s", qPrintable(q.lastError().text()));
        return {};
    }
    QList<FeedInfo> list;
    while (q.next())
        list.append(rowToFeed(q));
    return list;
}

QList<FeedInfo> Database::feedsByLabel(const QString& label) const
{
    QSqlQuery q(m_db);
    q.prepare(QStringLiteral(
        "SELECT * FROM feeds WHERE ',' || labels || ',' LIKE :pat ORDER BY id"));
    q.bindValue(QStringLiteral(":pat"), QLatin1String("%,") + label + QLatin1String(",%"));
    QList<FeedInfo> list;
    if (!q.exec()) {
        qWarning("Database::feedsByLabel: %s", qPrintable(q.lastError().text()));
        return list;
    }
    while (q.next())
            list.append(rowToFeed(q));
    return list;
}

bool Database::updateFeedData(const QString& url, const QByteArray& data, bool error)
{
    const qint64 now = QDateTime::currentSecsSinceEpoch();
    QSqlQuery q(m_db);

    q.prepare(QStringLiteral(
        "UPDATE feeds SET last_update = :now, last_error = :err WHERE url = :url"));
    q.bindValue(QStringLiteral(":now"), now);
    q.bindValue(QStringLiteral(":err"), error ? 1 : 0);
    q.bindValue(QStringLiteral(":url"), url);
    if (!q.exec()) {
        qWarning("Database::updateFeedData (feeds): %s", qPrintable(q.lastError().text()));
        return false;
    }

    if (!error) {
        q.prepare(QStringLiteral(R"sql(
            INSERT INTO feed_data (feed_id, content, updated_at)
            SELECT id, :content, :now FROM feeds WHERE url = :url
            ON CONFLICT(feed_id) DO UPDATE
                SET content    = excluded.content,
                    updated_at = excluded.updated_at
        )sql"));
        q.bindValue(QStringLiteral(":content"), data);
        q.bindValue(QStringLiteral(":now"),     now);
        q.bindValue(QStringLiteral(":url"),     url);
        if (!q.exec()) {
            qWarning("Database::updateFeedData (feed_data): %s", qPrintable(q.lastError().text()));
            return false;
        }
    }
    return true;
}

std::optional<QByteArray> Database::feedData(const QString& url) const
{
    QSqlQuery q(m_db);
    q.prepare(QStringLiteral(R"sql(
        SELECT fd.content
        FROM   feed_data fd
        JOIN   feeds f ON f.id = fd.feed_id
        WHERE  f.url = :url
        LIMIT  1
    )sql"));
    q.bindValue(QStringLiteral(":url"), url);
    if (q.exec() && q.next())
        return q.value(0).toByteArray();
    return std::nullopt;
}
