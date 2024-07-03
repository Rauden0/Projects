#include "httpserver.h"
#include "QByteArray"

QString hello() { return "Hello world"; }
QHttpServerResponse HttpServer::AddFeed(const QHttpServerRequest& req)
{
    QStringList data = QString(req.body()).split(' ');
    if (data.length() < 3) {
        QJsonObject obj = {
            { "error", "Invalid number of arguments" },
            { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
        };
        QJsonDocument doc;
        doc.setObject(obj);
        return QHttpServerResponse(obj,
            QHttpServerResponse::StatusCode::BadRequest);
    }
    bool ok;
    data.at(data.length() - 1).toInt(&ok, 10);
    if (!ok) {
        QJsonObject obj = {
            { "error", "Invalid arguments" },
            { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
        };
        QJsonDocument doc;
        doc.setObject(obj);
        return QHttpServerResponse(obj,
            QHttpServerResponse::StatusCode::BadRequest);
    }
    for (int i = 0; i < database->database->size(); i++) {
        if (database->database->at(i)->url == data.at(data.length() - 2)) {
            QJsonObject obj = {
                { "error", "Duplicate located" },
                { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
            };
            QJsonDocument doc;
            doc.setObject(obj);
            return QHttpServerResponse(obj,
                QHttpServerResponse::StatusCode::BadRequest);
        }
    }
    feed* new_feed = new feed;
    for (int i = 0; i < data.length() - 2; i++) {
        new_feed->label.push_back(data[i]);
    }
    new_feed->url = data[data.length() - 2];
    new_feed->interval = data[data.length() - 1].toInt();
    database->database->push_back(new_feed);
    QJsonObject obj = { { "Success", "Sucessfully added" },
        { "Code", (int)QHttpServerResponse::StatusCode::Ok } };
    QJsonDocument doc;
    doc.setObject(obj);
    return QHttpServerResponse(obj, QHttpServerResponse::StatusCode::Ok);
}

QHttpServerResponse HttpServer::GetFeed(const QHttpServerRequest& req)
{

    QStringList data = QString(req.body()).split(' ');
    if (data.size() != 1) {
        QJsonObject reply = {
            { "error", "Invalid number of arguments" },
            { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
        };
        return QHttpServerResponse(reply,
            QHttpServerResponse::StatusCode::BadRequest);
    }
    for (int i = 0; i < database->database->size(); ++i) {
        const auto& currentFeed = (*database->database)[i];
        if (currentFeed->url == data.at(0)) {
            QByteArray base64Data = currentFeed->data->toBase64();
            QString base64String(base64Data);
            QJsonObject jsonObject {
                { "Feed Data", base64String },
            };
            QJsonObject reply = { jsonObject };
            return QHttpServerResponse(reply, QHttpServerResponse::StatusCode::Ok);
        }
    }
    QJsonObject reply = {
        { "error", "No feed with this url" },
        { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
    };
    return QHttpServerResponse(reply,
        QHttpServerResponse::StatusCode::BadRequest);
}

QHttpServerResponse HttpServer::FeedState(const QHttpServerRequest& req)
{
    QStringList data = QString(req.body()).split(' ');
    if (data.size() != 1) {
        QJsonObject reply = {
            { "error", "Invalid number of arguments" },
            { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
        };
        return QHttpServerResponse(reply,
            QHttpServerResponse::StatusCode::BadRequest);
    }
    for (int i = 0; i < database->database->size(); ++i) {
        const auto& currentFeed = (*database->database)[i];
        if (currentFeed->url == data.at(0)) {
            QJsonObject reply = { { "Last update", currentFeed->lastupdate },
                { "Error at last update", currentFeed->error },
                { "Current size", currentFeed->data->size() } };
            return QHttpServerResponse(reply, QHttpServerResponse::StatusCode::Ok);
        }
    }
    QJsonObject reply = {
        { "error", "No feed with this url" },
        { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
    };
    return QHttpServerResponse(reply,
        QHttpServerResponse::StatusCode::BadRequest);
}

QHttpServerResponse HttpServer::ListFeed(const QHttpServerRequest& req)
{
    QStringList data = QString(req.body()).split(' ');
    if (data.size() > 1) {
        QJsonObject reply = {
            { "error", "Invalid number of arguments" },
            { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
        };
        return QHttpServerResponse(reply,
            QHttpServerResponse::StatusCode::BadRequest);
    }
    QString replyData = "";
    QJsonArray jsonArray;

    for (const auto& feed : *database->database) {
        if (data.size() == 0)
        {
            QJsonObject jsonFeed;
            jsonFeed["url"] = feed->url;
            jsonArray.append(jsonFeed);
            continue;
        }
        if (std::find(feed->label.begin(), feed->label.end(), data[0]) != feed->label.end()) {
            QJsonObject jsonFeed;
            jsonFeed["url"] = feed->url;
            jsonArray.append(jsonFeed);
        }
    }
    if (jsonArray.empty()) {

        QJsonObject reply = {
            { "error", "No feed with this url" },
            { "errorCode", (int)QHttpServerResponse::StatusCode::BadRequest }
        };
        return QHttpServerResponse(reply,
            QHttpServerResponse::StatusCode::BadRequest);
    }
    return QHttpServerResponse(jsonArray, QHttpServerResponse::StatusCode::Ok);
}

HttpServer::HttpServer(QString adress, qintptr port, QString databasePath,
    QObject* parent)
    : QObject(parent)
    , address(adress)
    , port(port)
{
    server = new QHttpServer(this);
    server->route("/AddFeed", QHttpServerRequest::Method::Post,
        [this](const QHttpServerRequest& req) { return AddFeed(req); });
    server->route("/GetFeed", QHttpServerRequest::Method::Post,
        [&](const QHttpServerRequest& req) { return GetFeed(req); });
    server->route("/List", QHttpServerRequest::Method::Post,
        [&](const QHttpServerRequest& req) { return ListFeed(req); });
    server->route("/State", QHttpServerRequest::Method::Post,
        [&](const QHttpServerRequest& req) { return FeedState(req); });
    server->route("/Quit", [this]() {
        qDebug() << "Ending the server";
        QHttpServerResponse response(QHttpServerResponse::StatusCode::Ok);
        QMetaObject::invokeMethod(this, "requestStop", Qt::QueuedConnection);
        return response;
    });
    // Here is a hack so reader can access the database could not think of better
    // way sorry (not sure if this is very optimal)
    reader = new RSSReader(database);
    database = new Database(reader, databasePath);
    reader->SetDatabase(database);
}

HttpServer::~HttpServer()
{
    delete server;
    delete database;
    delete reader;
}

void HttpServer::startListening()
{
    if (server->listen(QHostAddress(address), port)) {
        qDebug() << "Server is listening on port " << port;
    } else {
        qDebug() << "Failed to start server!";
    }
    connect(this, &HttpServer::requestStop, this, &HttpServer::stop);
}

void HttpServer::stop() { thread()->quit(); }

QByteArray HttpServer::processRequest(const QByteArray& requestData)
{
    return requestData;
}
