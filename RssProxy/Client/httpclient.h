// httpclient.h
#ifndef HTTPCLIENT_H
#define HTTPCLIENT_H

#include <QNetworkAccessManager>
#include <QNetworkReply>
#include <QObject>

class HttpClient : public QObject {
    Q_OBJECT

public:
    // Constructor with optional parent parameter
    HttpClient(QObject* parent = nullptr);
    // Destructor
    ~HttpClient();
public slots:
    // Slot to perform client actions
    void performActions();

    // Slot function to process HTTP responses
    void processResponse(QNetworkReply* reply);

private:
    QNetworkAccessManager* manager; // Network access manager for handling HTTP requests
    QString currentAction; // Current client action

    // Private functions to handle specific client actions
    void handleAddAction(const QStringList& inputList);
    void handleGetAction(const QStringList& inputList);
    void handleQuitAction(const QStringList& inputList);
    void handleStateAction(const QStringList& inputList);
    void handleListAction(const QStringList& inputList);
};

#endif // HTTPCLIENT_H
