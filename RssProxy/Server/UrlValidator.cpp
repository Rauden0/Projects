#include "UrlValidator.h"

#include <QHostAddress>
#include <QUrl>

namespace {

bool isBlockedAddress(const QHostAddress& addr, bool allowLocalhost)
{
  if (!addr.isNull() && addr.protocol() == QAbstractSocket::IPv4Protocol) {
    const quint32 ip = addr.toIPv4Address();
    const auto inRange = [ip](quint8 a, quint8 b, quint8 cStart, quint8 cEnd) {
      return ((ip >> 24) & 0xFF) == a && ((ip >> 16) & 0xFF) == b
             && ((ip >> 8) & 0xFF) >= cStart && ((ip >> 8) & 0xFF) <= cEnd;
    };

    if (inRange(10, 0, 0, 255))
      return true;
    if (inRange(172, 16, 0, 31))
      return true;
    if (inRange(192, 168, 0, 255))
      return true;
    if (!allowLocalhost && inRange(127, 0, 0, 255))
      return true;
    if (inRange(169, 254, 0, 255))
      return true;
    if (((ip >> 24) & 0xFF) == 0)
      return true;
  }

  if (!allowLocalhost) {
    if (addr.isLoopback() || addr.isLinkLocal())
      return true;
  }

  return false;
}

}

UrlValidator::Result UrlValidator::validate(const QString& url, bool allowLocalhost)
{
  const QUrl parsed(url);
  if (!parsed.isValid() || parsed.isEmpty())
    return {false, QStringLiteral("URL is not valid")};

  const QString scheme = parsed.scheme().toLower();
  if (scheme != QStringLiteral("http") && scheme != QStringLiteral("https"))
    return {false, QStringLiteral("Only http and https URLs are allowed")};

  if (parsed.host().isEmpty())
    return {false, QStringLiteral("URL host is required")};

  const QString host = parsed.host().toLower();
  if (!allowLocalhost) {
    if (host == QStringLiteral("localhost")
        || host.endsWith(QStringLiteral(".localhost"))
        || host.endsWith(QStringLiteral(".local")))
      return {false, QStringLiteral("Local or reserved hostnames are not allowed")};
  }

  const QHostAddress direct(host);
  if (!direct.isNull() && isBlockedAddress(direct, allowLocalhost))
    return {false, QStringLiteral("URL resolves to a blocked address range")};

  return {true, {}};
}
