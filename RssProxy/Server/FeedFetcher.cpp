#include "FeedFetcher.h"

#include "UrlValidator.h"

#include <QLoggingCategory>
#include <QNetworkReply>
#include <QNetworkRequest>

Q_LOGGING_CATEGORY(lcFetch, "rssproxy.fetch")

FeedFetcher::FeedFetcher(int fetchTimeoutMs, int maxResponseBytes, bool allowLocalhost,
                         QObject* parent)
    : QObject(parent)
    , m_fetchTimeoutMs(fetchTimeoutMs)
    , m_maxResponseBytes(maxResponseBytes)
    , m_allowLocalhost(allowLocalhost)
{}

void FeedFetcher::fetch(const QString& url)
{
    const auto validation = UrlValidator::validate(url, m_allowLocalhost);
    if (!validation.ok) {
        qCWarning(lcFetch) << "Rejected URL" << url << ":" << validation.error;
        emit feedFetched(url, {}, false);
        return;
    }

    QNetworkRequest req{QUrl(url)};
    req.setRawHeader("User-Agent", "RssProxy/2.0");
    req.setTransferTimeout(m_fetchTimeoutMs);

    auto* reply = m_nam.get(req);

    connect(reply, &QNetworkReply::finished, this, [this, reply, url]() {
        const bool networkOk = reply->error() == QNetworkReply::NoError;
        QByteArray data;
        bool success = false;

        if (networkOk) {
            data = reply->readAll();
            if (data.size() > m_maxResponseBytes) {
                qCWarning(lcFetch) << "Response too large for" << url
                                   << "(" << data.size() << "bytes)";
                data.clear();
            } else {
                success = true;
            }
        } else {
            qCWarning(lcFetch) << "Failed for" << url << "-" << reply->errorString();
        }

        emit feedFetched(url, success ? data : QByteArray{}, success);
        reply->deleteLater();
    });
}
