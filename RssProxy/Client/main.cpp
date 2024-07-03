#include "httpclient.h"
#include <QCoreApplication>

int main(int argc, char* argv[])
{
    QCoreApplication a(argc, argv);
    HttpClient* client = new HttpClient();
    QObject::connect(&a, &QCoreApplication::aboutToQuit, [&client]() { delete client; });
    return a.exec();
}
