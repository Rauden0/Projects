#include "Client.h"
#include <QCommandLineOption>
#include <QCommandLineParser>
#include <QCoreApplication>

int main(int argc, char* argv[])
{
    QCoreApplication app(argc, argv);
    QCoreApplication::setApplicationName(QStringLiteral("RssProxy Client"));
    QCoreApplication::setApplicationVersion(QStringLiteral("2.0"));

    QCommandLineParser parser;
    parser.setApplicationDescription(QStringLiteral("RSS proxy command-line client"));
    parser.addHelpOption();
    parser.addVersionOption();
    parser.addOption({
        {QStringLiteral("H"), QStringLiteral("host")},
        QStringLiteral("Server host"),
        QStringLiteral("host"),
        QStringLiteral("127.0.0.1"),
    });
    parser.addOption({
        {QStringLiteral("p"), QStringLiteral("port")},
        QStringLiteral("Server port"),
        QStringLiteral("port"),
        QStringLiteral("8080"),
    });
    parser.addOption({
        {QStringLiteral("k"), QStringLiteral("api-key")},
        QStringLiteral("API key (X-Api-Key header)"),
        QStringLiteral("key"),
    });
    parser.process(app);

    const QString  host = parser.value(QStringLiteral("host"));
    const quint16  port = static_cast<quint16>(parser.value(QStringLiteral("port")).toUInt());
    const QString  apiKey = parser.value(QStringLiteral("api-key"));

    new Client(host, port, apiKey, &app);
    return app.exec();
}
