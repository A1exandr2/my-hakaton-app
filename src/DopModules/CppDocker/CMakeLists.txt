# CMakeList.txt: проект CMake для CppDocker; включите исходный код и определения,
# укажите здесь логику для конкретного проекта.
#
cmake_minimum_required (VERSION 3.10)

# Включение горячей перезагрузки для компиляторов MSVC, если поддерживается.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project ("CppDocker")

# Поиск библиотеки cURL
find_package(CURL REQUIRED)
# Поиск OpenSSL
find_package(OpenSSL REQUIRED)  

# Добавьте источник в исполняемый файл этого проекта.
add_executable(CppDocker "main.cpp" "main.h" "icmp.h" "http.h" "tcp.h" "icmplib.h" "json.hpp")

# Подключение библиотеки cURL к целевому исполняемому файлу
target_link_libraries(CppDocker PRIVATE 
    CURL::libcurl
    OpenSSL::SSL
    OpenSSL::Crypto
)

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET CppDocker PROPERTY CXX_STANDARD 20)
endif()

# Добавление путей к include-директориям
target_include_directories(CppDocker PRIVATE
    ${CURL_INCLUDE_DIRS}
    ${OPENSSL_INCLUDE_DIR}
)

# Проверка поддержки SSL в cURL
if(NOT CURL_VERSION_STRING VERSION_LESS "7.20.0")
    message(STATUS "cURL found with version: ${CURL_VERSION_STRING}")
    message(STATUS "cURL supports SSL: ${CURL_HAVE_SSL}")
else()
    message(WARNING "cURL version may be too old for full SSL support")
endif()

# TODO: Добавьте тесты и целевые объекты, если это необходимо.
