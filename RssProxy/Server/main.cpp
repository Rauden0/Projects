#include "servercontroller.h"
#include <QCoreApplication>
QString DatabasePath(QCoreApplication& a)
{
    QCommandLineParser parser;
    QCommandLineOption pathOption(QStringList() << "p"
                                                << "path",
        "Database path", "path");
    parser.addOption(pathOption);
    parser.process(a);

    QString databasePath;

    if (parser.isSet(pathOption)) {
        databasePath = parser.value(pathOption);
    } else {
        QSettings settings("Martin", "Server");
        databasePath = settings.value("databasePath").toString();
    }

    if (databasePath.isEmpty()) {
        databasePath = "database";
    }
    QSettings settings("Martin", "Server");
    settings.setValue("databasePath", databasePath);

    return databasePath;
}

int main(int argc, char* argv[])
{
    QCoreApplication a(argc, argv);
    QString path = DatabasePath(a);
    ServerController* controller = new ServerController("127.0.0.1", 8080, path);

    QObject::connect(controller, &ServerController::endServer, &a,
        &QCoreApplication::quit);
    controller->startListening();
    QObject::connect(&a, &QCoreApplication::aboutToQuit,
        [&controller]() { delete controller; });
    return a.exec();
}
