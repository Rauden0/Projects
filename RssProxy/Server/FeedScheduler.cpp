#include "FeedScheduler.h"

#include <QDateTime>

FeedScheduler::FeedScheduler(Database* db, int checkIntervalMs, int maxConcurrent,
                           QObject* parent)
    : QObject(parent)
    , m_db(db)
    , m_maxConcurrent(maxConcurrent)
{
    m_timer.setInterval(checkIntervalMs);
    connect(&m_timer, &QTimer::timeout, this, &FeedScheduler::check);
}

void FeedScheduler::start() { m_timer.start(); }
void FeedScheduler::stop()  { m_timer.stop(); }

void FeedScheduler::dispatch(const QString& url)
{
    if (m_inProgress.contains(url) || m_queued.contains(url))
        return;

    if (m_inProgress.size() >= m_maxConcurrent) {
        m_queued.insert(url);
        m_pending.enqueue(url);
        return;
    }

    m_inProgress.insert(url);
    emit fetchRequested(url);
}

void FeedScheduler::drainQueue()
{
    while (m_inProgress.size() < m_maxConcurrent && !m_pending.isEmpty()) {
        const QString url = m_pending.dequeue();
        m_queued.remove(url);
        if (!m_inProgress.contains(url))
            dispatch(url);
    }
}

void FeedScheduler::scheduleFetch(const QString& url)
{
    dispatch(url);
}

void FeedScheduler::check()
{
    const qint64 now = QDateTime::currentSecsSinceEpoch();
    for (const FeedInfo& feed : m_db->allFeeds()) {
        if (!m_inProgress.contains(feed.url) && !m_queued.contains(feed.url)
            && (now - feed.lastUpdate) >= feed.interval)
        {
            dispatch(feed.url);
        }
    }
}

void FeedScheduler::onFeedFetched(const QString& url, const QByteArray& data, bool success)
{
    m_db->updateFeedData(url, data, !success);
    m_inProgress.remove(url);
    drainQueue();
}
