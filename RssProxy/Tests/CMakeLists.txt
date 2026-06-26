cmake_minimum_required(VERSION 3.16)

project(RssProxyTests LANGUAGES CXX)

if(NOT TARGET rssproxy_core)
    add_subdirectory(${CMAKE_CURRENT_SOURCE_DIR}/../Server
                     ${CMAKE_CURRENT_BINARY_DIR}/_rssproxy_server)
endif()

if(NOT DEFINED CMAKE_PREFIX_PATH OR CMAKE_PREFIX_PATH STREQUAL "")
    set(_local_qt "${CMAKE_CURRENT_SOURCE_DIR}/6.8.0/msvc2022_64")
    if(EXISTS "${_local_qt}/lib/cmake/Qt6/Qt6Config.cmake")
        list(PREPEND CMAKE_PREFIX_PATH "${_local_qt}")
    endif()
endif()

set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

find_package(QT NAMES Qt6 Qt5 REQUIRED COMPONENTS Core Network HttpServer Sql Test)
find_package(Qt${QT_VERSION_MAJOR} REQUIRED COMPONENTS Core Network HttpServer Sql Test)

# ── tst_Config ────────────────────────────────────────────────────────────────
add_executable(tst_Config tst_Config.cpp)
target_link_libraries(tst_Config PRIVATE rssproxy_core Qt${QT_VERSION_MAJOR}::Test)
add_test(NAME tst_Config COMMAND tst_Config)

# ── tst_Database ──────────────────────────────────────────────────────────────
add_executable(tst_Database tst_Database.cpp)
target_link_libraries(tst_Database PRIVATE rssproxy_core Qt${QT_VERSION_MAJOR}::Test)
add_test(NAME tst_Database COMMAND tst_Database)

# ── tst_UrlValidator ──────────────────────────────────────────────────────────
add_executable(tst_UrlValidator tst_UrlValidator.cpp)
target_link_libraries(tst_UrlValidator PRIVATE rssproxy_core Qt${QT_VERSION_MAJOR}::Test)
add_test(NAME tst_UrlValidator COMMAND tst_UrlValidator)

# ── tst_HttpServer ────────────────────────────────────────────────────────────
add_executable(tst_HttpServer tst_HttpServer.cpp)
target_link_libraries(tst_HttpServer PRIVATE rssproxy_core Qt${QT_VERSION_MAJOR}::Test)
add_test(NAME tst_HttpServer COMMAND tst_HttpServer)

if(WIN32 AND EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/6.8.0/msvc2022_64/bin")
    set(_qt_bin "${CMAKE_CURRENT_SOURCE_DIR}/6.8.0/msvc2022_64/bin")
    set(_qt_plugins "${CMAKE_CURRENT_SOURCE_DIR}/6.8.0/msvc2022_64/plugins")
    foreach(_test tst_Config tst_Database tst_UrlValidator tst_HttpServer)
        set_tests_properties(${_test} PROPERTIES
            ENVIRONMENT "QT_PLUGIN_PATH=${_qt_plugins}"
            ENVIRONMENT_MODIFICATION "PATH=path_list_prepend:${_qt_bin}"
        )
    endforeach()
endif()
