#ifndef QT_CMAKE_HTTPSERVER_DATABASE_H
#define QT_CMAKE_HTTPSERVER_DATABASE_H

#include <QCoreApplication>
#include <QDebug>
#include <QDir>
#include <QFile>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QString>
#include <QThread>
#include <QtConcurrent>
#include <iostream>

// Forward declaration of RSSReader class to avoid circular dependencies
class RSSReader;

// Structure representing a feed
struct feed {
    std::vector<QString> label; // Labels associated with the feed
    QString url; // URL of the feed
    long interval; // Update interval for the feed
    qint64 lastupdate = 0; // Timestamp of the last update
    bool first = true; // Flag indicating if it's the first update
    QByteArray* data; // Pointer to the feed data
    bool being_updated; // Flag indicating if the feed is being updated
    bool error = false; // Flag indicating if an error occurred during the update
} typedef feed;

// Database class responsible for managing feeds
class Database : public QObject {
    Q_OBJECT
public:
    std::vector<feed*>* database; // Vector of feed pointers representing the database
    RSSReader* reader; // Pointer to the RSSReader
    bool stopFeedUpdater = true; // Flag to control the feed updater thread
    QString path = ""; // Path to the database file

public:
    // Constructor with optional parameters for RSSReader and database path
    Database(RSSReader* reader = nullptr, QString databasePath = "");

    // Destructor
    ~Database();

    // Save the database to a file
    void SaveToFile(QJsonArray& rssData);

    // Load data from the database file
    void LoadFile();

    // Load data from the database
    void LoadData();

    // Save the entire database
    void SaveDatabase();

    // Feed updater function
    void FeedUpdater();

    // Slot function called when RSS data is ready
    void SaveRssData(QByteArray rssData, QString url, bool success);
};

#endif // QT_CMAKE_HTTPSERVER_DATABASE_H
