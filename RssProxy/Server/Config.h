#pragma once

#include <QString>

struct Config {
    QString host   = QStringLiteral("127.0.0.1");
    quint16 port   = 8080;
    QString dbPath = QStringLiteral("rss_proxy.db");

    QString apiKey;

    int schedulerIntervalMs = 10'000;
    int maxConcurrentFetches = 8;
    int fetchTimeoutMs       = 30'000;
    int maxResponseBytes     = 5 * 1024 * 1024; // 5 MiB

    bool allowLocalhost = false;

    bool isValid(QString* error = nullptr) const;

    static Config load(const QString& iniPath);
    void          save(const QString& iniPath) const;
};
