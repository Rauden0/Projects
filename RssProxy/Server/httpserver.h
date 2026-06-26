#pragma once

#include "Database.h"
#include "FeedScheduler.h"
#include <QObject>
#include <QTcpServer>
#include <QtHttpServer/QtHttpServer>

class HttpServer : public QObject
{
    Q_OBJECT
public:
    HttpServer(const QString& host, quint16 port,
               Database* db, FeedScheduler* scheduler,
               const QString& apiKey = {},
               bool allowLocalhost = false,
               QObject* parent = nullptr);

    bool    listen();
    quint16 boundPort() const { return m_boundPort; }

signals:
    void stopRequested();

private:
    void setupRoutes();

    bool isAuthorized(const QHttpServerRequest& req) const;

    QHttpServerResponse handleHealth    (const QHttpServerRequest& req);
    QHttpServerResponse handleReady     (const QHttpServerRequest& req);
    QHttpServerResponse handleAddFeed   (const QHttpServerRequest& req);
    QHttpServerResponse handleListFeeds (const QHttpServerRequest& req);
    QHttpServerResponse handleGetData   (const QHttpServerRequest& req);
    QHttpServerResponse handleGetState  (const QHttpServerRequest& req);
    QHttpServerResponse handleRemoveFeed(const QHttpServerRequest& req);
    QHttpServerResponse handleQuit      (const QHttpServerRequest& req);

    static QHttpServerResponse jsonOk(const QJsonValue& payload);
    static QHttpServerResponse jsonError(
        const QString& msg,
        QHttpServerResponse::StatusCode code = QHttpServerResponse::StatusCode::BadRequest);

    QHttpServer    m_server;
    QTcpServer*    m_tcpServer = nullptr;
    QString        m_host;
    quint16        m_port;
    quint16        m_boundPort = 0;
    QString        m_apiKey;
    bool           m_allowLocalhost = false;
    Database*      m_db;
    FeedScheduler* m_scheduler;
};
