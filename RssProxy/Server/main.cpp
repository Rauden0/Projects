#include "Config.h"
#include "Database.h"
#include "FeedFetcher.h"
#include "FeedScheduler.h"
#include "HttpServer.h"
#include <QCommandLineOption>
#include <QCommandLineParser>
#include <QCoreApplication>
#include <QLoggingCategory>

Q_LOGGING_CATEGORY(lcApp, "rssproxy")

int main(int argc, char* argv[])
{
    QCoreApplication app(argc, argv);
    QCoreApplication::setApplicationName(QStringLiteral("RssProxy Server"));
    QCoreApplication::setApplicationVersion(QStringLiteral("2.0"));

    QCommandLineParser parser;
    parser.setApplicationDescription(QStringLiteral("RSS proxy caching server"));
    parser.addHelpOption();
    parser.addVersionOption();
    parser.addOption({
        {QStringLiteral("c"), QStringLiteral("config")},
        QStringLiteral("Path to config.ini"),
        QStringLiteral("file"),
        QStringLiteral("config.ini"),
    });
    parser.process(app);

    Config cfg = Config::load(parser.value(QStringLiteral("config")));
    QString configError;
    if (!cfg.isValid(&configError)) {
        qCCritical(lcApp) << "Invalid configuration:" << configError;
        return 1;
    }

    auto* db = new Database(cfg.dbPath, &app);
    auto* fetcher = new FeedFetcher(cfg.fetchTimeoutMs, cfg.maxResponseBytes,
                                     cfg.allowLocalhost, &app);
    auto* scheduler = new FeedScheduler(db, cfg.schedulerIntervalMs,
                                        cfg.maxConcurrentFetches, &app);
    auto* server = new HttpServer(cfg.host, cfg.port, db, scheduler,
                                  cfg.apiKey, cfg.allowLocalhost, &app);

    if (!db->open()) {
        qCCritical(lcApp) << "Failed to open database at" << cfg.dbPath;
        return 1;
    }

    QObject::connect(scheduler, &FeedScheduler::fetchRequested,
                     fetcher,   &FeedFetcher::fetch);
    QObject::connect(fetcher,   &FeedFetcher::feedFetched,
                     scheduler, &FeedScheduler::onFeedFetched);

    QObject::connect(server, &HttpServer::stopRequested, &app, [&]() {
        qCInfo(lcApp) << "Graceful shutdown initiated";
        scheduler->stop();
        QCoreApplication::quit();
    });

    if (!server->listen())
        return 1;

    scheduler->start();
    qCInfo(lcApp) << "RssProxy server started";
    return app.exec();
}
