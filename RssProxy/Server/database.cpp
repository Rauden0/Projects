#include "database.h"
#include "rssfetcher.h"
bool DoesFileExist(const QString& filePath)
{
    QFile file(filePath);
    return file.exists();
}
bool DoesDirExists(const QString& path)
{
    QDir directory(path);
    if (!directory.exists()) {
        directory.mkpath(".");
    }
    return directory.exists();
}

void Database::FeedUpdater()
{
    while (stopFeedUpdater) {
        QDateTime currentDateTime = QDateTime::currentDateTime();
        qint64 epochTime = currentDateTime.toSecsSinceEpoch();
        for (int i = 0; i < database->size(); i++) {
            if (((epochTime - database->at(i)->lastupdate) > database->at(i)->interval) && !database->at(i)->being_updated) {
                database->at(i)->being_updated = true;
                QString url = database->at(i)->url;
                QMetaObject::invokeMethod(reader, "fetchRssFeed", Qt::QueuedConnection,
                    Q_ARG(QString, url));
            }
        }
    }
}
Database::Database(RSSReader* reader, QString databasePath)
{
    this->reader = reader;
    this->database = new std::vector<feed*>;
    this->path = databasePath;
    if (DoesDirExists(this->path)) {
        LoadData();
    }
    QtConcurrent::run([this]() { FeedUpdater(); });
}

void Database::LoadData()
{
    QDir folder(path);

    if (!folder.exists()) {
        qDebug() << "Database folder does not exist:" << path;
        return;
    }

    QStringList feedFiles = folder.entryList(QStringList() << "feed_*.txt", QDir::Files);
    for (const QString& feedFile : feedFiles) {
        QString filePath = folder.filePath(feedFile);
        QFile file(filePath);

        if (file.open(QIODevice::ReadOnly | QIODevice::Text)) {
            QTextStream in(&file);

            feed* loadedFeed = new feed;
            in >> loadedFeed->url >> loadedFeed->lastupdate >> loadedFeed->interval;
            QString labels;
            in >> labels;
            QStringList labelList = labels.split(" ", Qt::SkipEmptyParts);
            loadedFeed->label = std::vector<QString>();
            for (int i = 0; i < labelList.count(); i++) {
                loadedFeed->label.push_back(labelList[i]);
            }
            database->push_back(loadedFeed);

            file.close();
        } else {
            qDebug() << "Error opening file for reading:" << file.fileName();
        }
    }
    QStringList dataFiles = folder.entryList(QStringList() << "data_*.txt", QDir::Files);
    for (int i = 0; i < dataFiles.size(); ++i) {
        QString filePath = folder.filePath(dataFiles[i]);
        QFile file(filePath);

        if (file.open(QIODevice::ReadOnly | QIODevice::Text)) {
            QTextStream in(&file);
            QByteArray data = in.readAll().toUtf8();
            if (i < database->size()) {
                (*database)[i]->data = new QByteArray(data);
            } else {
                qDebug() << "Error: Mismatch between feed and data files";
            }
            file.close();
        } else {
            qDebug() << "Error opening data file for reading:" << file.fileName();
        }
    }
}

void Database::SaveDatabase()
{
    QDir folder(this->path);

    if (!folder.exists()) {
        if (!folder.mkpath(".")) {
            qDebug() << "Error creating folder:" << this->path;
            throw std::runtime_error("Cannot make database directory");
        }
    }

    for (int i = 0; i < database->size(); i++) {
        const feed& currentFeed = *database->at(i);
        QString FeedName = QString("%1/feed_%2.txt").arg(this->path).arg(i);
        QFile FeedFile(FeedName);
        if (FeedFile.open(QIODevice::WriteOnly | QIODevice::Text)) {
            QTextStream out(&FeedFile);
            QString joinedLabels;
            for (const QString& label : currentFeed.label) {
                joinedLabels += label + " ";
            }
            out << currentFeed.url << " " << currentFeed.lastupdate << " "
                << currentFeed.interval << " " << joinedLabels.trimmed();

            FeedFile.close();
        } else {
            qDebug() << "Error opening file for writing:" << FeedFile.fileName();
        }
        QString DataName = QString("%1/data_%2.txt").arg(this->path).arg(i);
        QFile DataFile(DataName);
        if (DataFile.open(QIODevice::WriteOnly | QIODevice::Text)) {
            QTextStream out(&DataFile);
            out << currentFeed.data->data();
            DataFile.close();
        } else {
            qDebug() << "Error opening file for writing:" << DataFile.fileName();
        }
    }
}

Database::~Database()
{
    int i = 0;

    stopFeedUpdater = false;
    for (; i < 5; i++) {
        try {
            SaveDatabase();
            break;
        } catch (std::exception& e) {
            std::cout << "Failed to save the database" << std::endl;
            std::cout << "Retrying" << std::endl;
        }
    }
    if (i == 5) {
        std::cout << "Completly failed to save the database proceed with caution"
                  << std::endl;
    }
    for (int i = 0; i < database->size(); i++) {
        delete database->at(i);
    }
    delete this->database;
}

void Database::SaveRssData(QByteArray rssData, QString url, bool error)
{
    for (int i = 0; i < database->size(); ++i) {
        const auto& currentFeed = (*database)[i];
        if (currentFeed->url == url) {
            if (error && currentFeed->first) {
                database->erase(database->begin() + i);
                return;
            }
            if (error) {
                database->at(i)->error = true;
                return;
            }
            database->at(i)->error = false;
            database->at(i)->being_updated = false;
            QDateTime currentDateTime = QDateTime::currentDateTime();
            qint64 epochTime = currentDateTime.toSecsSinceEpoch();
            currentFeed->data = new QByteArray(rssData);
            currentFeed->lastupdate = epochTime;
            currentFeed->first = false;
            break;
        }
    }
    SaveDatabase();
}
