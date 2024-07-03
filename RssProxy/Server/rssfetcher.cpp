#include "rssfetcher.h"

RSSReader::RSSReader(Database* database)
{
    networkManager = new QNetworkAccessManager();
    this->database = database;
}

void RSSReader::SetDatabase(Database* database) { this->database = database; }

void RSSReader::fetchRssFeed(const QString& url)
{
    QNetworkRequest request;
    request.setUrl(QUrl(url));
    QNetworkReply* reply = networkManager->get(request);
    reply->setProperty("url", QVariant(url));
    connect(reply, &QNetworkReply::finished,
        [this, reply, url]() { onNetworkReply(reply); });
}
void RSSReader::onNetworkReply(QNetworkReply* reply)
{
    if (reply->error() == QNetworkReply::NoError) {
        QByteArray responseData = reply->readAll();
        QXmlStreamReader xml(responseData);
        QVariant urlVariant = reply->property("url");
        QString url = urlVariant.isValid() ? urlVariant.toString() : QString();
        database->SaveRssData(responseData, url, xml.hasError());
    } else {
        QVariant urlVariant = reply->property("url");
        QString url = urlVariant.isValid() ? urlVariant.toString() : QString();
        database->SaveRssData(nullptr, url, true);
    }
    reply->deleteLater();
}
