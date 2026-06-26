cmake_minimum_required(VERSION 3.16)

project(RssProxy VERSION 2.0 LANGUAGES CXX)

option(RSSPROXY_BUILD_TESTS "Build unit tests" ON)
option(RSSPROXY_BUILD_CLIENT "Build CLI client" ON)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(MSVC)
    add_compile_options(/W4 /permissive-)
endif()

add_subdirectory(Server)

if(RSSPROXY_BUILD_CLIENT)
    add_subdirectory(Client)
endif()

if(RSSPROXY_BUILD_TESTS)
    enable_testing()
    add_subdirectory(Tests)
endif()
