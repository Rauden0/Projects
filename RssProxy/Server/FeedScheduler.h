#pragma once

#include "Database.h"
#include <QObject>
#include <QQueue>
#include <QSet>
#include <QTimer>

class FeedScheduler : public QObject
{
    Q_OBJECT
public:
    explicit FeedScheduler(Database* db, int checkIntervalMs = 10'000,
                           int maxConcurrent = 8, QObject* parent = nullptr);

    void start();
    void stop();

    // Trigger an immediate fetch (e.g. right after a feed is added)
    void scheduleFetch(const QString& url);

    int activeFetchCount() const { return m_inProgress.size(); }
    int pendingFetchCount() const { return m_queued.size(); }

public slots:
    void onFeedFetched(const QString& url, const QByteArray& data, bool success);

signals:
    void fetchRequested(const QString& url);

private slots:
    void check();

private:
    void dispatch(const QString& url);
    void drainQueue();

    Database*     m_db;
    QTimer        m_timer;
    int           m_maxConcurrent;
    QSet<QString> m_inProgress;
    QSet<QString> m_queued;
    QQueue<QString> m_pending;
};
