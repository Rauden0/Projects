#pragma once

#include <QString>

class UrlValidator
{
public:
  struct Result {
    bool    ok = false;
    QString error;
  };

  static Result validate(const QString& url, bool allowLocalhost = false);
};
