#include "servercontroller.h"

ServerController::ServerController(const QString& addr, qintptr port,
    QString databasePath, QObject* parent)
    : QObject { parent }
{
    server = new HttpServer(addr, port, databasePath);
    server_thread = new QThread();

    server->moveToThread(server_thread);

    connect(this, &ServerController::serve, server, &HttpServer::startListening);
    connect(server, &HttpServer::requestStop, this,
        &ServerController::requestEnd);

    server_thread->start();
}

ServerController::~ServerController() { delete server; }
void ServerController::startListening() { emit serve(); }

void ServerController::requestEnd() { emit endServer(); }
