//
// Created by werti on 14.12.2023.
//

#ifndef QT_CMAKE_HTTPSERVER_HTTPSERVER_H
#define QT_CMAKE_HTTPSERVER_HTTPSERVER_H

#include "rssfetcher.h"
#include <QCoreApplication>
#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QNetworkRequest>
#include <QUrl>
#include <QXmlStreamReader>
#include <QtHttpServer/QtHttpServer>

// Forward declaration of Database and RSSReader classes to avoid circular
// dependencies
class Database;
class RSSReader;

class HttpServer : public QObject {
    Q_OBJECT

private:
    QHttpServer* server; // Pointer to the HTTP server instance
    Database* database; // Pointer to the database instance
    QString address; // Server address
    qintptr port; // Server port
    RSSReader* reader; // Pointer to the RSSReader instance

public:
    // Constructor with parameters for server address, port, database path, and
    // optional parent
    HttpServer(QString address, qintptr port, QString databasePath,
        QObject* parent = nullptr);

    // Destructor
    ~HttpServer();

    // Handler function for the "AddFeed" HTTP request
    QHttpServerResponse AddFeed(const QHttpServerRequest& req);

    // Handler function for the "GetFeed" HTTP request
    QHttpServerResponse GetFeed(const QHttpServerRequest& req);

    // Handler function for the "FeedState" HTTP request
    QHttpServerResponse FeedState(const QHttpServerRequest& req);

    // Handler function for the "ListFeed" HTTP request
    QHttpServerResponse ListFeed(const QHttpServerRequest& req);

signals:
    // Signal to request stopping the server
    void requestStop();

public slots:
    // Slot function to start listening for incoming requests
    void startListening();

    // Slot function to stop the server
    void stop();

private:
    // Function to process incoming HTTP requests
    QByteArray processRequest(const QByteArray& requestData);
};

#endif // QT_CMAKE_HTTPSERVER_HTTPSERVER_H
