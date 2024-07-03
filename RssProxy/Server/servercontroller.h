#ifndef SERVERCONTROLLER_H
#define SERVERCONTROLLER_H

#include "httpserver.h"
#include <QObject>
#include <QThread>

// Forward declaration of HttpServer class to avoid circular dependencies
class HttpServer;

class ServerController : public QObject {
    Q_OBJECT
    QThread* server_thread; // Thread for running the HTTP server
    HttpServer* server; // Pointer to the HttpServer instance

public:
    // Constructor with parameters for server address, port, database path, and
    // optional parent
    ServerController(const QString& address, qintptr port, QString databasePath,
        QObject* parent = nullptr);

    // Function to start listening for incoming requests
    void startListening();

    // Destructor
    ~ServerController();

public slots:
    // Slot function to request ending the server
    void requestEnd();

signals:
    // Signal to start serving requests
    void serve();

    // Signal to end the server
    void endServer();
};

#endif // SERVERCONTROLLER_H
