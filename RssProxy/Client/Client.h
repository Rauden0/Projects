#pragma once

#include <QNetworkAccessManager>
#include <QObject>
#include <QString>
#include <QTextStream>
#include <QUrlQuery>

class Client : public QObject
{
    Q_OBJECT
public:
    explicit Client(const QString& host, quint16 port,
                    const QString& apiKey = {}, QObject* parent = nullptr);

private slots:
    void readCommand();

private:
    QByteArray post(const QString& path, const QByteArray& json);
    QByteArray get (const QString& path, const QUrlQuery& query = {});
    QByteArray del (const QString& path, const QUrlQuery& query = {});

    void applyAuth(QNetworkRequest& req) const;
    QString buildUrl(const QString& path, const QUrlQuery& query) const;

    void cmdAdd   (const QStringList& args);
    void cmdList  (const QStringList& args);
    void cmdGet   (const QStringList& args);
    void cmdState (const QStringList& args);
    void cmdRemove(const QStringList& args);
    void cmdQuit  ();

    void printHelp();
    void prettyPrint(const QByteArray& body);

    QNetworkAccessManager m_nam{this};
    QString               m_baseUrl;
    QString               m_apiKey;
    QTextStream           m_in;
    QTextStream           m_out;
};
