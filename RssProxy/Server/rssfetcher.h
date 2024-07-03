#ifndef QT_CMAKE_HTTPSERVER_RSSREADER_H
#define QT_CMAKE_HTTPSERVER_RSSREADER_H

#include "database.h"
#include <QCoreApplication>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QNetworkRequest>
#include <QUrl>
#include <QXmlStreamReader>

// Forward declaration of Database class to avoid circular dependencies
class Database;

class RSSReader : public QObject {
    Q_OBJECT

private:
    Database* database; // Pointer to the Database instance

public:
    // Constructor with optional parameter for Database instance
    RSSReader(Database* database = nullptr);

    // Function to set the Database instance
    void SetDatabase(Database* database);

    // Function to handle network replies
    void onNetworkReply(QNetworkReply* reply);

public slots:
    // Slot to handle fetch feeds
    void fetchRssFeed(const QString& url);

public:
    QNetworkAccessManager* networkManager; // Network access manager for handling HTTP requests
};

#endif // QT_CMAKE_HTTPSERVER_RSSREADER_H
