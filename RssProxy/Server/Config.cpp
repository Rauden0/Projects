#include "Config.h"

#include <QFile>
#include <QSettings>

static QString iniSavePath(const QString& iniPath)
{
    if (iniPath.endsWith(QStringLiteral(".ini"), Qt::CaseInsensitive)
        || iniPath.endsWith(QStringLiteral(".conf"), Qt::CaseInsensitive))
        return iniPath;
    return iniPath + QStringLiteral(".ini");
}

static QString iniLoadPath(const QString& iniPath)
{
    const QString savedPath = iniSavePath(iniPath);
    if (QFile::exists(savedPath))
        return savedPath;
    if (QFile::exists(iniPath))
        return iniPath;
    return savedPath;
}

bool Config::isValid(QString* error) const
{
    if (port == 0) {
        if (error)
            *error = QStringLiteral("server port must be between 1 and 65535");
        return false;
    }
    if (schedulerIntervalMs < 1'000) {
        if (error)
            *error = QStringLiteral("scheduler interval must be at least 1000 ms");
        return false;
    }
    if (maxConcurrentFetches < 1) {
        if (error)
            *error = QStringLiteral("max concurrent fetches must be at least 1");
        return false;
    }
    if (fetchTimeoutMs < 1'000) {
        if (error)
            *error = QStringLiteral("fetch timeout must be at least 1000 ms");
        return false;
    }
    if (maxResponseBytes < 1024) {
        if (error)
            *error = QStringLiteral("max response size must be at least 1024 bytes");
        return false;
    }
    if (host == QStringLiteral("0.0.0.0") && apiKey.isEmpty()) {
        if (error)
            *error = QStringLiteral(
                "binding to 0.0.0.0 requires security/apiKey to be configured");
        return false;
    }
    return true;
}

Config Config::load(const QString& iniPath)
{
    QSettings s(iniLoadPath(iniPath), QSettings::IniFormat);
    Config c;
    c.host   = s.value(QStringLiteral("server/host"),   c.host).toString();
    c.port   = static_cast<quint16>(s.value(QStringLiteral("server/port"), c.port).toUInt());
    c.dbPath = s.value(QStringLiteral("database/path"), c.dbPath).toString();

    c.apiKey = s.value(QStringLiteral("security/apiKey"), c.apiKey).toString();

    c.schedulerIntervalMs  = s.value(QStringLiteral("scheduler/intervalMs"),
                                       c.schedulerIntervalMs).toInt();
    c.maxConcurrentFetches = s.value(QStringLiteral("scheduler/maxConcurrent"),
                                     c.maxConcurrentFetches).toInt();
    c.fetchTimeoutMs       = s.value(QStringLiteral("fetch/timeoutMs"),
                                       c.fetchTimeoutMs).toInt();
    c.maxResponseBytes     = s.value(QStringLiteral("fetch/maxResponseBytes"),
                                     c.maxResponseBytes).toInt();
    c.allowLocalhost       = s.value(QStringLiteral("fetch/allowLocalhost"),
                                     c.allowLocalhost).toBool();
    return c;
}

void Config::save(const QString& iniPath) const
{
    QSettings s(iniSavePath(iniPath), QSettings::IniFormat);
    s.setValue(QStringLiteral("server/host"),   host);
    s.setValue(QStringLiteral("server/port"),   port);
    s.setValue(QStringLiteral("database/path"), dbPath);

    s.setValue(QStringLiteral("security/apiKey"), apiKey);

    s.setValue(QStringLiteral("scheduler/intervalMs"),  schedulerIntervalMs);
    s.setValue(QStringLiteral("scheduler/maxConcurrent"), maxConcurrentFetches);
    s.setValue(QStringLiteral("fetch/timeoutMs"),       fetchTimeoutMs);
    s.setValue(QStringLiteral("fetch/maxResponseBytes"), maxResponseBytes);
    s.setValue(QStringLiteral("fetch/allowLocalhost"),  allowLocalhost);
    s.sync();
}
