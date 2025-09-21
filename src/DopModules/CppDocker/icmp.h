#pragma once

#include <iostream>
#include <string>
#include <vector>
#include <thread>
#include <mutex>
#include <condition_variable>
#include <atomic>
#include <unordered_set>
#include <unordered_map>
#include <chrono>
#include <algorithm>
#include <memory>
#include <queue>
#include <map>
#include <functional>
#include "icmplib.h"

class AsyncPinger {
private:
    struct PingTask {
        std::string address;
        std::chrono::minutes interval;
        std::chrono::steady_clock::time_point next_ping_time;
        int pings_remaining = 0; // Оставшееся количество пингов в текущей серии
        bool is_in_series = false; // Флаг, что сейчас выполняется серия пингов

        bool operator<(const PingTask& other) const {
            return next_ping_time > other.next_ping_time;
        }
    };

    std::vector<std::thread> workers;
    std::vector<std::unique_ptr<std::mutex>> mutexes;
    std::vector<std::unique_ptr<std::condition_variable>> cvs;
    std::vector<std::priority_queue<PingTask>> task_queues;
    std::atomic<bool> running{ false };
    const int ping_series_count = 7; // Количество пингов в серии

    // Карта для отслеживания, в каком потоке находится каждый адрес
    std::mutex address_map_mutex;
    std::unordered_map<std::string, size_t> address_to_thread;

    // Колбэк функция и мьютекс для него
    std::mutex callback_mutex;
    std::function<void(std::string, std::vector<icmplib::PingResult>)> callback;

    // Буфер для накопления результатов в рамках одной серии
    std::mutex results_mutex;
    std::unordered_map<std::string, std::vector<icmplib::PingResult>> series_results;

public:
    AsyncPinger() {
        unsigned int num_threads = std::thread::hardware_concurrency();
        if (num_threads == 0) {
            num_threads = 4;
        }

        for (unsigned int i = 0; i < num_threads; ++i) {
            mutexes.emplace_back(std::make_unique<std::mutex>());
            cvs.emplace_back(std::make_unique<std::condition_variable>());
            task_queues.emplace_back();
        }

        running = true;

        for (unsigned int i = 0; i < num_threads; ++i) {
            workers.emplace_back(&AsyncPinger::worker_thread, this, i);
        }

        std::cout << "Started " << num_threads << " worker threads" << std::endl;
    }

    ~AsyncPinger() {
        stop();
    }

    // Установка колбэк функции
    void set_callback(std::function<void(std::string, std::vector<icmplib::PingResult>)> func) {
        std::lock_guard<std::mutex> lock(callback_mutex);
        callback = std::move(func);
        std::cout << "Callback set successfully" << std::endl;
    }

    void add_address(const std::string& address, std::chrono::minutes interval = std::chrono::minutes(5)) {
        static size_t next_thread = 0;
        size_t thread_index = next_thread;
        next_thread = (next_thread + 1) % workers.size();

        auto now = std::chrono::steady_clock::now();
        PingTask task{ address, interval, now, ping_series_count, true };

        {
            std::lock_guard<std::mutex> lock(*mutexes[thread_index]);
            task_queues[thread_index].push(task);

            // Запоминаем, в какой поток добавлен адрес
            std::lock_guard<std::mutex> map_lock(address_map_mutex);
            address_to_thread[address] = thread_index;

            std::cout << "Added address: " << address << " to thread " << thread_index
                << " with interval: " << interval.count() << "ms" << std::endl;
        }
        cvs[thread_index]->notify_one();
    }

    void remove_address(const std::string& address) {
        size_t thread_index;
        bool found = false;

        {
            // Находим в каком потоке находится адрес
            std::lock_guard<std::mutex> map_lock(address_map_mutex);
            auto it = address_to_thread.find(address);
            if (it != address_to_thread.end()) {
                thread_index = it->second;
                found = true;
                address_to_thread.erase(it);
            }
        }

        if (found) {
            // Удаляем из очереди задач
            std::lock_guard<std::mutex> lock(*mutexes[thread_index]);

            std::priority_queue<PingTask> new_queue;
            auto temp_queue = task_queues[thread_index];

            while (!temp_queue.empty()) {
                auto task = temp_queue.top();
                temp_queue.pop();

                if (task.address != address) {
                    new_queue.push(task);
                }
            }

            task_queues[thread_index] = std::move(new_queue);

            // Удаляем накопленные результаты
            std::lock_guard<std::mutex> results_lock(results_mutex);
            series_results.erase(address);

            std::cout << "Removed address: " << address << " from thread " << thread_index << std::endl;
        }
        else {
            std::cout << "Address " << address << " not found in any thread" << std::endl;
        }
    }

    void update() {
        for (size_t i = 0; i < workers.size(); ++i) {
            cvs[i]->notify_one();
        }
    }

    void stop() {
        running = false;

        // Отправляем все накопленные результаты перед остановкой
        flush_all_results();

        for (auto& cv : cvs) {
            cv->notify_all();
        }

        for (auto& thread : workers) {
            if (thread.joinable()) {
                thread.join();
            }
        }

        workers.clear();
        mutexes.clear();
        cvs.clear();
        task_queues.clear();
        std::cout << "All worker threads stopped" << std::endl;
    }

private:
    void ping_host(std::string address) {
        icmplib::PingResult result;
        std::string resolved;
        try {
            if (!icmplib::IPAddress::IsCorrect(address, icmplib::IPAddress::Type::Unknown)) {
                resolved = address;
            }
            
            result = icmplib::Ping(resolved.empty() ? address : resolved, ICMPLIB_TIMEOUT_1S);
            //std::cout << "Pinged " << address << " - response: " << static_cast<int>(result.response) << std::endl;
            
        }
        catch (const std::exception& e) {
            std::cerr << "Ping error for " << address << ": " << e.what() << std::endl;
            result.response = icmplib::PingResponseType::Failure;
        }
        catch (...) {
            std::cerr << "Unknown ping error for " << address << std::endl;
            result.response = icmplib::PingResponseType::Failure;
        }

        // Добавляем результат в буфер серии
        add_result_to_series(address, result);
    }

    void add_result_to_series(const std::string& address, const icmplib::PingResult& result) {
        std::lock_guard<std::mutex> lock(results_mutex);
        series_results[address].push_back(result);
    }

    void send_series_results(const std::string& address) {
        std::vector<icmplib::PingResult> results;
        {
            std::lock_guard<std::mutex> lock(results_mutex);
            if (series_results.find(address) != series_results.end()) {
                results = std::move(series_results[address]);
                series_results.erase(address);
            }
        }

        if (!results.empty()) {
            call_callback_safe(address, results);
        }
    }

    void flush_all_results() {
        std::lock_guard<std::mutex> lock(results_mutex);
        for (auto& [address, results] : series_results) {
            if (!results.empty()) {
                call_callback_safe(address, results);
            }
        }
        series_results.clear();
    }

    // Потокобезопасный вызов колбэка
    void call_callback_safe(const std::string& host, const std::vector<icmplib::PingResult>& responses) {
        std::lock_guard<std::mutex> lock(callback_mutex);
        if (callback) {
            try {
                callback(host, responses);
            }
            catch (const std::exception& e) {
                std::cerr << "Callback error for host " << host << ": " << e.what() << std::endl;
            }
            catch (...) {
                std::cerr << "Unknown callback error for host " << host << std::endl;
            }
        }
    }

    void worker_thread(size_t thread_index) {
        while (running) {
            PingTask task;
            bool has_task = false;

            {
                std::unique_lock<std::mutex> lock(*mutexes[thread_index]);

                if (!task_queues[thread_index].empty()) {
                    auto now = std::chrono::steady_clock::now();
                    auto next_task = task_queues[thread_index].top();

                    if (next_task.next_ping_time <= now) {
                        task = next_task;
                        task_queues[thread_index].pop();
                        has_task = true;

                        if (task.is_in_series) {
                            // Выполняем пинг в серии
                            task.pings_remaining--;

                            if (task.pings_remaining > 0) {
                                // Следующий пинг сразу же
                                task.next_ping_time = now;
                            }
                            else {
                                // Завершили серию - отправляем результаты и ждем интервал
                                task.is_in_series = false;
                                task.next_ping_time = now + task.interval;
                                task.pings_remaining = ping_series_count;

                                // Отправляем результаты серии
                                std::thread([this, address = task.address]() {
                                    send_series_results(address);
                                    }).detach();
                            }
                        }
                        else {
                            // Начинаем новую серию пингов
                            task.is_in_series = true;
                            task.next_ping_time = now;
                        }

                        task_queues[thread_index].push(task);
                    }
                }

                if (!has_task) {
                    if (task_queues[thread_index].empty()) {
                        cvs[thread_index]->wait_for(lock, std::chrono::seconds(1));
                    }
                    else {
                        auto next_time = task_queues[thread_index].top().next_ping_time;
                        cvs[thread_index]->wait_until(lock, next_time);
                    }
                }
            }

            if (has_task) {
                ping_host(task.address);
                // Небольшая пауза между пингами в серии
                if (task.is_in_series && task.pings_remaining > 0) {
                    std::this_thread::sleep_for(std::chrono::milliseconds(100));
                }
            }
        }
    }
};