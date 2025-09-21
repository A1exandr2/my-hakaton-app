#pragma once

#include <iostream>
#include <string>
#include <unordered_map>
#include <vector>
#include <thread>
#include <mutex>
#include <atomic>
#include <functional>
#include <condition_variable>
#include <chrono>
#include <memory>
#include <queue>
#include <future>
#include <set>
#include <curl/curl.h>

// ��������� ��� �������� ������ ������
struct ResponseData {
    std::string headers;
    std::string content;
    std::string ssl_cert_info;
    long http_code = 0;
    long ssl_verify_result = 0;
    CURLcode curl_error = CURLE_OK;
    std::string error_message;
    double total_time = 0;
    long redirect_count = 0;
};

// ��������� ��� �������� ���������� � �������������� �������
struct MonitoredResource {
    std::string host;
    int request_interval;
    std::chrono::steady_clock::time_point last_request_time;
    bool active;
    bool in_progress = false;
    int id;
    int proto;
};

// Callback �������
static size_t WriteCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    size_t total_size = size * nmemb;
    std::string* data = static_cast<std::string*>(userp);
    data->append(static_cast<char*>(contents), total_size);
    return total_size;
}

static size_t HeaderCallback(void* contents, size_t size, size_t nmemb, void* userp) {
    size_t total_size = size * nmemb;
    std::string* headers = static_cast<std::string*>(userp);
    headers->append(static_cast<char*>(contents), total_size);
    return total_size;
}

class WebResourceMonitor {
public:
    using CallbackType = std::function<void(const std::string& host, const ResponseData& response, bool success, int id, int proto)>;

    WebResourceMonitor() : running_(false) {
        curl_global_init(CURL_GLOBAL_DEFAULT);
    }

    ~WebResourceMonitor() {
        stop();
        curl_global_cleanup();
    }

    // ���������� ������ ��� �����������
    bool add_address(const std::string& host, int request_interval, int proto, int id) {
        bool should_check_immediately = false;

        {
            std::lock_guard<std::mutex> lock(resources_mutex_);

            if (resources_.find(host) != resources_.end()) {
                return false;
            }

            // ������������� ����� ���������� ������� � ������� �������
            MonitoredResource resource{
                host,
                request_interval,
                std::chrono::steady_clock::now() - std::chrono::hours(24),
                true,
                false,
                id,
                proto
            };
            resources_[host] = resource;

            // ����������, ����� �� ���������� ���������
            should_check_immediately = running_;
        }

        // ���� ���������� ��� �������, ���������� ��������� ����� ���� (��� ����������)
        if (should_check_immediately) {
            start_immediate_check(host);
        }

        return true;
    }

    // �������� ������ �� �����������
    bool remove_address(const std::string& host) {
        std::lock_guard<std::mutex> lock(resources_mutex_);
        return resources_.erase(host) > 0;
    }

    // ����������� callback �������
    void register_cb(CallbackType callback) {
        std::lock_guard<std::mutex> lock(callback_mutex_);
        callback_ = std::move(callback);
    }

    // ������ �����������
    void start() {
        if (running_) return;

        running_ = true;

        // ���������� ��������� ��� ������������ ����� ��� ������
        check_all_hosts_immediately();

        monitor_thread_ = std::thread(&WebResourceMonitor::monitor_loop, this);
    }

    // ��������� �����������
    void stop() {
        if (!running_) return;

        running_ = false;
        cv_.notify_all();
        if (monitor_thread_.joinable()) {
            monitor_thread_.join();
        }

        wait_for_completion();
    }

private:
    // ����������� �������� ���� ������ ��� ������
    void check_all_hosts_immediately() {
        std::vector<std::string> hosts_to_check;

        {
            std::lock_guard<std::mutex> lock(resources_mutex_);
            for (auto& [host, resource] : resources_) {
                if (resource.active && !resource.in_progress) {
                    hosts_to_check.push_back(host);
                    resource.in_progress = true;
                }
            }
        }

        for (const auto& host : hosts_to_check) {
            std::thread(&WebResourceMonitor::check_resource_async, this, host).detach();
        }
    }


    // ����������� �������� ������ �����
    void start_immediate_check(const std::string& host) {
        bool should_check = false;

        {
            std::lock_guard<std::mutex> lock(resources_mutex_);
            if (auto it = resources_.find(host); it != resources_.end() && !it->second.in_progress) {
                it->second.in_progress = true;
                should_check = true;
            }
        }

        if (should_check) {
            // ��������� �������� � ��������� ������
            std::thread([this, host]() {
                check_resource_async(host);
                }).detach();
        }
    }

    // �������� ���� �����������
    void monitor_loop() {
        while (running_) {
            try {
                {
                    std::unique_lock<std::mutex> lock(resources_mutex_);
                    if (resources_.empty()) {
                        cv_.wait_for(lock, std::chrono::milliseconds(100),
                            [this] { return !running_ || !resources_.empty(); });
                        continue;
                    }
                }

                update(); // ��������� �������� �� ����������

                std::this_thread::sleep_for(std::chrono::milliseconds(100));
            }
            catch (const std::exception& e) {
                std::cerr << "Exception in monitor_loop: " << e.what() << std::endl;
                std::this_thread::sleep_for(std::chrono::seconds(1));
            }
            catch (...) {
                std::cerr << "Unknown exception in monitor_loop" << std::endl;
                std::this_thread::sleep_for(std::chrono::seconds(1));
            }
        }
    }

    // ���������� ��������� ����������� (�� ����������)
    void update() {
        std::vector<std::string> hosts_to_check;

        {
            std::lock_guard<std::mutex> lock(resources_mutex_);
            auto now = std::chrono::steady_clock::now();

            for (auto& [host, resource] : resources_) {
                if (resource.active && !resource.in_progress) {
                    auto elapsed = std::chrono::duration_cast<std::chrono::seconds>(
                        now - resource.last_request_time);

                    if (elapsed.count() >= resource.request_interval) {
                        hosts_to_check.push_back(host);
                        resource.in_progress = true;
                    }
                }
            }
        }

        for (const auto& host : hosts_to_check) {
            std::thread(&WebResourceMonitor::check_resource_async, this, host).detach();
        }
    }

    // ����������� �������� �������
    void check_resource_async(const std::string& host) {
        try {
            ResponseData response;
            bool success = fetch_website_data(host, response);
            int id{}, proto{};
            // ��������� ��������� �������
            {
                std::lock_guard<std::mutex> lock(resources_mutex_);
                if (auto it = resources_.find(host); it != resources_.end()) {
                    it->second.last_request_time = std::chrono::steady_clock::now();
                    it->second.in_progress = false;
                    id = it->second.id;
                    proto = it->second.proto;
                }
            }

            // �������� callback
            {
                std::lock_guard<std::mutex> lock(callback_mutex_);
                if (callback_) {
                    callback_(host, response, success, id, proto);
                }
            }
        }
        catch (const std::exception& e) {
            // ��������� ���������� � ��������� ������
            std::lock_guard<std::mutex> lock(resources_mutex_);
            if (auto it = resources_.find(host); it != resources_.end()) {
                it->second.in_progress = false;
            }
            std::cerr << "Exception in check_resource_async for " << host << ": " << e.what() << std::endl;
        }
        catch (...) {
            std::lock_guard<std::mutex> lock(resources_mutex_);
            if (auto it = resources_.find(host); it != resources_.end()) {
                it->second.in_progress = false;
            }
            std::cerr << "Unknown exception in check_resource_async for " << host << std::endl;
        }
    }

    // �������� ���������� ���� �������� ��������
    void wait_for_completion() {
        const int max_wait_time = 30000;
        const int check_interval = 100;

        int waited = 0;
        while (waited < max_wait_time) {
            {
                std::lock_guard<std::mutex> lock(resources_mutex_);
                bool all_done = true;
                for (const auto& [host, resource] : resources_) {
                    if (resource.in_progress) {
                        all_done = false;
                        break;
                    }
                }
                if (all_done) break;
            }

            std::this_thread::sleep_for(std::chrono::milliseconds(check_interval));
            waited += check_interval;
        }
    }

    // ����� ��� ��������� ������ � �����
    bool fetch_website_data(const std::string& url, ResponseData& response) {
        CURL* curl = curl_easy_init();
        if (!curl) {
            response.error_message = "CURL initialization failed";
            return false;
        }

        // ������� ���������
        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
        curl_easy_setopt(curl, CURLOPT_USERAGENT, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, 10L);
        curl_easy_setopt(curl, CURLOPT_CONNECTTIMEOUT, 5L);
        curl_easy_setopt(curl, CURLOPT_NOSIGNAL, 1L);

        // Callback �������
        curl_easy_setopt(curl, CURLOPT_HEADERFUNCTION, HeaderCallback);
        curl_easy_setopt(curl, CURLOPT_HEADERDATA, &response.headers);
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response.content);

        // SSL ���������
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 1L);
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 2L);
        curl_easy_setopt(curl, CURLOPT_SSL_OPTIONS, CURLSSLOPT_NO_REVOKE);

        // �������� ������
        curl_easy_setopt(curl, CURLOPT_ACCEPT_ENCODING, "gzip, deflate");
        curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 3L);

        // ���������� �������
        CURLcode res = curl_easy_perform(curl);
        response.curl_error = res;

        if (res != CURLE_OK) {
            response.error_message = curl_easy_strerror(res);
            curl_easy_cleanup(curl);
            return false;
        }

        // ��������� ���������� � ������
        curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &response.http_code);
        curl_easy_getinfo(curl, CURLINFO_TOTAL_TIME, &response.total_time);
        curl_easy_getinfo(curl, CURLINFO_REDIRECT_COUNT, &response.redirect_count);

        // SSL ����������
        long verify_result = -1;
        if (curl_easy_getinfo(curl, CURLINFO_SSL_VERIFYRESULT, &verify_result) == CURLE_OK) {
            response.ssl_verify_result = verify_result;
            response.ssl_cert_info = "SSL: " + std::to_string(verify_result);
        }

        curl_easy_cleanup(curl);
        return true;
    }

private:
    std::unordered_map<std::string, MonitoredResource> resources_;
    std::mutex resources_mutex_;

    CallbackType callback_;
    std::mutex callback_mutex_;

    std::atomic<bool> running_;
    std::thread monitor_thread_;
    std::condition_variable cv_;
};
