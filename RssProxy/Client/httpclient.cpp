// httpclient.cpp
#include "httpclient.h"
#include <QDebug>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QTextStream>
#include <QThread>
#include <QtConcurrent>
#include <iostream>
HttpClient::HttpClient(QObject* parent)
    : QObject { parent }
{
    manager = new QNetworkAccessManager(this);
    connect(manager, &QNetworkAccessManager::finished, this, &HttpClient::processResponse);
    QMetaObject::invokeMethod(this, "performActions", Qt::QueuedConnection);
}
HttpClient::~HttpClient()
{
    delete manager;
}

void HttpClient::performActions()
{
    QString input;
    QTextStream input_stream(stdin);
    if (input_stream.readLineInto(&input, 0)) {
        QStringList inputList = input.split(' ');
        if (!inputList.isEmpty()) {
            currentAction = inputList.first();

            if (currentAction == "Add")
                handleAddAction(inputList.mid(1));
            else if (currentAction == "Get")
                handleGetAction(inputList.mid(1));
            else if (currentAction == "Quit")
                handleQuitAction(inputList.mid(1));
            else if (currentAction == "State")
                handleStateAction(inputList.mid(1));
            else if (currentAction == "List")
                handleListAction(inputList.mid(1));
            else
                qDebug() << "Unknown action: " << currentAction;
        }
    }
    QCoreApplication::processEvents();
    QMetaObject::invokeMethod(this, "performActions", Qt::QueuedConnection);
}
void PrintState(QNetworkReply* reply)
{
    QByteArray responseData = reply->readAll();
    QJsonDocument jsonDocument = QJsonDocument::fromJson(responseData);

    if (jsonDocument.isObject()) {
        QJsonObject jsonObject = jsonDocument.object();
        QDateTime dateTime = QDateTime::fromSecsSinceEpoch(jsonObject.value("Last update").toVariant().toLongLong());
        QString normalTimeLastUpdate = dateTime.toString("yyyy-MM-dd HH:mm:ss");
        bool errorAtLastUpdate = jsonObject.value("Error at last update").toBool();
        qint64 currentSize = jsonObject.value("Current size").toVariant().toLongLong();
        qDebug() << "Last Update:" << normalTimeLastUpdate;
        qDebug() << "Error at Last Update:" << errorAtLastUpdate;
        qDebug() << "Current Size:" << currentSize;
    } else {
        qDebug() << "Invalid JSON format in the response.";
    }
}
void PrintGet(QNetworkReply* reply)
{

    QByteArray responseData = reply->readAll();
    QJsonDocument jsonDocument = QJsonDocument::fromJson(responseData);
    if (jsonDocument.isObject()) {
        QJsonObject jsonObject = jsonDocument.object();
        if (jsonObject.contains("Feed Data")) {
            QString base64String = jsonObject.value("Feed Data").toString();
            qDebug() << "Base64 String:" << base64String;

            QByteArray decodedData = QByteArray::fromBase64(base64String.toUtf8());
            qDebug() << "Received Feed Data:" << decodedData;
        } else {
            qDebug() << "Feed Data not found in the response.";
        }
    } else {
        qDebug() << "Error in the HTTP request:" << reply->errorString();
    }
}
void PrintList(QNetworkReply* reply)
{
    QByteArray responseData = reply->readAll();
    QJsonDocument jsonDocument = QJsonDocument::fromJson(responseData);
    qDebug() << "Urls under managment";
    if (jsonDocument.isArray()) {
        QJsonArray jsonArray = jsonDocument.array();

        for (const auto& jsonFeed : jsonArray) {
            if (jsonFeed.isObject()) {
                QJsonObject feedObject = jsonFeed.toObject();
                if (feedObject.contains("url")) {
                    QString url = feedObject["url"].toString();
                    qDebug() << "URL: " << url;
                }
            }
        }
    }
}

void HttpClient::processResponse(QNetworkReply* reply)
{
    if (reply->error() == QNetworkReply::NoError) {
        if (currentAction == "State") {
            PrintState(reply);
        } else if (currentAction == "Get") {
            PrintGet(reply);
        } else if (currentAction == "Add") {
            qDebug() << "Succesfully Added";
        } else if (currentAction == "Quit") {
            qDebug() << "Succesfully Quitted";
        } else if (currentAction == "List") {
            PrintList(reply);
        }
    } else {
        qDebug() << "Error in the HTTP request:" << reply->errorString();
    }

    reply->deleteLater();
}

void HttpClient::handleAddAction(const QStringList& inputList)
{
    QUrl url("http://127.0.0.1:8080/AddFeed");
    QByteArray requestBody = inputList.join(' ').toUtf8();

    QNetworkRequest request(url);
    request.setHeader(QNetworkRequest::ContentTypeHeader, "application/x-www-form-urlencoded");

    QNetworkReply* reply = manager->post(request, requestBody);
    QEventLoop loop;
    connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
    loop.exec();
}

void HttpClient::handleGetAction(const QStringList& inputList)
{
    QUrl url("http://127.0.0.1:8080/GetFeed");
    QByteArray requestBody = inputList.join(' ').toUtf8();
    QNetworkRequest request(url);
    request.setHeader(QNetworkRequest::ContentTypeHeader, "application/x-www-form-urlencoded");

    QNetworkReply* reply = manager->post(request, requestBody);
    QEventLoop loop;
    connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
    loop.exec();
}

void HttpClient::handleQuitAction(const QStringList& inputList)
{
    QUrl url("http://127.0.0.1:8080/Quit");
    QByteArray requestBody = inputList.join(' ').toUtf8();

    QNetworkRequest request(url);
    request.setHeader(QNetworkRequest::ContentTypeHeader, "application/x-www-form-urlencoded");
    QNetworkReply* reply = manager->post(request, requestBody);
    QEventLoop loop;
    connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
    loop.exec();
}
void HttpClient::handleStateAction(const QStringList& inputList)
{
    QUrl url("http://127.0.0.1:8080/State");
    QByteArray requestBody = inputList.join(' ').toUtf8();

    QNetworkRequest request(url);
    request.setHeader(QNetworkRequest::ContentTypeHeader, "application/x-www-form-urlencoded");

    QNetworkReply* reply = manager->post(request, requestBody);
    QEventLoop loop;
    connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
    loop.exec();
}

void HttpClient::handleListAction(const QStringList& inputList)
{
    QUrl url("http://127.0.0.1:8080/List");
    QByteArray requestBody = inputList.join(' ').toUtf8();

    QNetworkRequest request(url);
    request.setHeader(QNetworkRequest::ContentTypeHeader, "application/x-www-form-urlencoded");

    QNetworkReply* reply = manager->post(request, requestBody);
    QEventLoop loop;
    connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
    loop.exec();
}
