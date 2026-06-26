#pragma once

#include <QNetworkAccessManager>
#include <QObject>

class FeedFetcher : public QObject
{
    Q_OBJECT
public:
    explicit FeedFetcher(int fetchTimeoutMs = 30'000,
                         int maxResponseBytes = 5 * 1024 * 1024,
                         bool allowLocalhost = false,
                         QObject* parent = nullptr);

public slots:
    void fetch(const QString& url);

signals:
    void feedFetched(const QString& url, const QByteArray& data, bool success);

private:
    QNetworkAccessManager m_nam;
    int                   m_fetchTimeoutMs;
    int                   m_maxResponseBytes;
    bool                  m_allowLocalhost;
};
