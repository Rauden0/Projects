#pragma once

#include <QList>
#include <QObject>
#include <QSqlDatabase>
#include <QString>
#include <QStringList>
#include <optional>

struct FeedInfo {
    qint64      id;
    QString     url;
    QStringList labels;
    qint64      interval; 
    qint64      lastUpdate;
    bool        lastError;
};

class Database : public QObject
{
    Q_OBJECT
public:
    explicit Database(const QString& dbPath, QObject* parent = nullptr);
    ~Database() override;

    bool open();
    bool isOpen() const { return m_db.isOpen(); }

    bool                    addFeed(const QString& url, const QStringList& labels, qint64 intervalSecs);
    bool                    removeFeed(const QString& url);
    std::optional<FeedInfo> feedByUrl(const QString& url) const;
    QList<FeedInfo>         allFeeds() const;
    QList<FeedInfo>         feedsByLabel(const QString& label) const;

    bool                      updateFeedData(const QString& url, const QByteArray& data, bool error);
    std::optional<QByteArray> feedData(const QString& url) const;

private:
    bool createSchema();

    QSqlDatabase m_db;
    QString      m_dbPath;
    QString      m_connName;
};
