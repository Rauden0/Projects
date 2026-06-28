#include "Client.h"

#include <QCoreApplication>
#include <QEventLoop>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QNetworkReply>
#include <QNetworkRequest>
#include <QTextStream>
#include <QTimer>
#include <QUrlQuery>

Client::Client(const QString& host, quint16 port, const QString& apiKey, QObject* parent)
    : QObject(parent)
    , m_baseUrl(QStringLiteral("http://%1:%2").arg(host).arg(port))
    , m_apiKey(apiKey)
    , m_in(stdin)
    , m_out(stdout)
{
    QTimer::singleShot(0, this, &Client::readCommand);
}


static QByteArray syncExec(QNetworkReply* reply)
{
    QEventLoop loop;
    QObject::connect(reply, &QNetworkReply::finished, &loop, &QEventLoop::quit);
    loop.exec();
    const QByteArray data = reply->readAll();
    if (reply->error() != QNetworkReply::NoError)
        qWarning("Network error: %s", qPrintable(reply->errorString()));
    reply->deleteLater();
    return data;
}

void Client::applyAuth(QNetworkRequest& req) const
{
    if (!m_apiKey.isEmpty())
        req.setRawHeader("X-Api-Key", m_apiKey.toUtf8());
}

QString Client::buildUrl(const QString& path, const QUrlQuery& query) const
{
    QUrl url(m_baseUrl + path);
    if (!query.isEmpty())
        url.setQuery(query);
    return url.toString();
}

QByteArray Client::post(const QString& path, const QByteArray& json)
{
    QNetworkRequest req(QUrl(m_baseUrl + path));
    req.setHeader(QNetworkRequest::ContentTypeHeader, QStringLiteral("application/json"));
    applyAuth(req);
    return syncExec(m_nam.post(req, json));
}

QByteArray Client::get(const QString& path, const QUrlQuery& query)
{
    QNetworkRequest req(buildUrl(path, query));
    applyAuth(req);
    return syncExec(m_nam.get(req));
}

QByteArray Client::del(const QString& path, const QUrlQuery& query)
{
    QNetworkRequest req(buildUrl(path, query));
    applyAuth(req);
    return syncExec(m_nam.deleteResource(req));
}


void Client::prettyPrint(const QByteArray& body)
{
    const auto doc = QJsonDocument::fromJson(body);
    if (!doc.isNull())
        m_out << doc.toJson(QJsonDocument::Indented);
    else
        m_out << body << '\n';
    m_out.flush();
}

void Client::printHelp()
{
    m_out << R"(Commands:
  add <url> <interval_secs> [label ...]   Add a feed
  list [label]                            List feeds (optionally filtered)
  get <url>                               Print raw XML of a feed
  state <url>                             Show feed status
  remove <url>                            Remove a feed
  quit                                    Shutdown server
  help                                    Show this message
)" << Qt::endl;
}


void Client::cmdAdd(const QStringList& args)
{
    if (args.size() < 2) {
        m_out << "Usage: add <url> <interval_secs> [label ...]\n";
        return;
    }
    const QString url      = args[0];
    const qint64  interval = args[1].toLongLong();
    if (interval <= 0) { m_out << "interval must be > 0\n"; return; }

    QJsonArray labels;
    for (int i = 2; i < args.size(); ++i)
        labels.append(args[i]);

    const QJsonObject body{
        {QStringLiteral("url"),      url},
        {QStringLiteral("interval"), interval},
        {QStringLiteral("labels"),   labels},
    };
    prettyPrint(post(QStringLiteral("/feeds"), QJsonDocument(body).toJson()));
}

void Client::cmdList(const QStringList& args)
{
    QUrlQuery query;
    if (!args.isEmpty())
        query.addQueryItem(QStringLiteral("label"), args[0]);
    prettyPrint(get(QStringLiteral("/feeds"), query));
}

void Client::cmdGet(const QStringList& args)
{
    if (args.isEmpty()) { m_out << "Usage: get <url>\n"; return; }
    QUrlQuery query;
    query.addQueryItem(QStringLiteral("url"), args[0]);
    const QByteArray data = get(QStringLiteral("/feeds/data"), query);
    m_out << data << Qt::endl;
}

void Client::cmdState(const QStringList& args)
{
    if (args.isEmpty()) { m_out << "Usage: state <url>\n"; return; }
    QUrlQuery query;
    query.addQueryItem(QStringLiteral("url"), args[0]);
    prettyPrint(get(QStringLiteral("/feeds/state"), query));
}

void Client::cmdRemove(const QStringList& args)
{
    if (args.isEmpty()) { m_out << "Usage: remove <url>\n"; return; }
    QUrlQuery query;
    query.addQueryItem(QStringLiteral("url"), args[0]);
    prettyPrint(del(QStringLiteral("/feeds"), query));
}

void Client::cmdQuit()
{
    prettyPrint(post(QStringLiteral("/quit"), {}));
    QCoreApplication::quit();
}


void Client::readCommand()
{
    m_out << "> " << Qt::flush;
    QString line;
    if (!m_in.readLineInto(&line)) {
        QCoreApplication::quit();
        return;
    }
    const QStringList parts = line.trimmed().split(u' ', Qt::SkipEmptyParts);
    if (!parts.isEmpty()) {
        const QString cmd = parts[0].toLower();
        const QStringList args = parts.mid(1);

        if      (cmd == u"add")    cmdAdd(args);
        else if (cmd == u"list")   cmdList(args);
        else if (cmd == u"get")    cmdGet(args);
        else if (cmd == u"state")  cmdState(args);
        else if (cmd == u"remove") cmdRemove(args);
        else if (cmd == u"quit")   { cmdQuit(); return; }
        else if (cmd == u"help")   printHelp();
        else    m_out << "Unknown command. Type 'help'.\n" << Qt::flush;
    }

    QTimer::singleShot(0, this, &Client::readCommand);
}
