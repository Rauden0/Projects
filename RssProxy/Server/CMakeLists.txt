cmake_minimum_required(VERSION 3.16)

project(RssProxyServer LANGUAGES CXX)

if(NOT DEFINED CMAKE_PREFIX_PATH OR CMAKE_PREFIX_PATH STREQUAL "")
    set(_local_qt "${CMAKE_CURRENT_SOURCE_DIR}/../Tests/6.8.0/msvc2022_64")
    if(EXISTS "${_local_qt}/lib/cmake/Qt6/Qt6Config.cmake")
        list(PREPEND CMAKE_PREFIX_PATH "${_local_qt}")
    endif()
endif()

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

find_package(QT NAMES Qt6 Qt5 REQUIRED COMPONENTS Core Network HttpServer Sql)
find_package(Qt${QT_VERSION_MAJOR} REQUIRED COMPONENTS Core Network HttpServer Sql)

set(RSSPROXY_CORE_SOURCES
    Config.cpp
    Database.cpp
    FeedFetcher.cpp
    FeedScheduler.cpp
    HttpServer.cpp
    UrlValidator.cpp
)

set(RSSPROXY_CORE_HEADERS
    Config.h
    Database.h
    FeedFetcher.h
    FeedScheduler.h
    HttpServer.h
    UrlValidator.h
)

add_library(rssproxy_core STATIC
    ${RSSPROXY_CORE_SOURCES}
    ${RSSPROXY_CORE_HEADERS}
)

target_include_directories(rssproxy_core
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}
)

target_link_libraries(rssproxy_core
    PUBLIC
        Qt${QT_VERSION_MAJOR}::Core
        Qt${QT_VERSION_MAJOR}::Network
        Qt${QT_VERSION_MAJOR}::HttpServer
        Qt${QT_VERSION_MAJOR}::Sql
)

add_executable(Server main.cpp)
target_link_libraries(Server PRIVATE rssproxy_core)

include(GNUInstallDirs)
install(TARGETS Server rssproxy_core
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
)
