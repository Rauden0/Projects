#pragma once

#include <QString>

// Validates feed URLs before persistence or outbound fetch (SSRF mitigation).
class UrlValidator
{
public:
  struct Result {
    bool    ok = false;
    QString error;
  };

  static Result validate(const QString& url, bool allowLocalhost = false);
};
