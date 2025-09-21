#pragma once

#include <iostream>
#include <thread>
#include <mutex>
#include <queue>
#include <string>
#include <atomic>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <cstring>
#include <cerrno>

class TCPClient {
public:
    TCPClient() : running_(false), connected_(false), socket_fd_(-1) {};
    ~TCPClient()
    {
        disconnect();
    };

    bool connect(const std::string& host, int port) 
    {
        if (connected_) {
            return true;
        }

        // ������� �����
        socket_fd_ = socket(AF_INET, SOCK_STREAM, 0);
        if (socket_fd_ < 0) {
            std::cerr << "filed to create socket: " << strerror(errno) << std::endl;
            return false;
        }

        // ����������� ����� �������
        sockaddr_in server_addr{};
        server_addr.sin_family = AF_INET;
        server_addr.sin_port = htons(port);

        if (inet_pton(AF_INET, host.c_str(), &server_addr.sin_addr) <= 0) {
            std::cerr << "unable addr: " << host << std::endl;
            close(socket_fd_);
            return false;
        }

        // ������������ � �������
        if (::connect(socket_fd_, (sockaddr*)&server_addr, sizeof(server_addr)) < 0) {
            std::cerr << "failed to connect (socket): " << strerror(errno) << std::endl;
            close(socket_fd_);
            return false;
        }

        connected_ = true;
        running_ = true;

        // ��������� ����� �������
        client_thread_ = std::thread(&TCPClient::clientThread, this);

        return true;
    }
    void disconnect() 
    {
        if (!connected_) {
            return;
        }

        running_ = false;
        connected_ = false;

        if (client_thread_.joinable()) {
            client_thread_.join();
        }

        if (socket_fd_ >= 0) {
            close(socket_fd_);
            socket_fd_ = -1;
        }
    };
    bool send(const std::string& data) 
    {
        if (!connected_) {
            return false;
        }

        std::lock_guard<std::mutex> lock(send_mutex_);
        send_queue_.push(data);
        return true;
    };
    bool receive(std::string& data)
    {
        std::lock_guard<std::mutex> lock(receive_mutex_);

        if (receive_queue_.empty()) {
            return false;
        }

        data = receive_queue_.front();
        receive_queue_.pop();
        return true;
    }
    bool isConnected() const {
        return connected_;
    }

private:
    void clientThread()
    {
        while (running_ && connected_) {
            // ��������� ���� �� ������ ��� ��������
            std::string data_to_send;
            {
                std::lock_guard<std::mutex> lock(send_mutex_);
                if (!send_queue_.empty()) {
                    data_to_send = send_queue_.front();
                    send_queue_.pop();
                }
            }

            // ���������� ������ ���� ����
            if (!data_to_send.empty()) {
                ssize_t bytes_sent = ::send(socket_fd_, data_to_send.c_str(), data_to_send.length(), 0);
                if (bytes_sent < 0) {
                    std::cerr << "failed to send(socket): " << strerror(errno) << std::endl;
                    connected_ = false;
                    break;
                }
            }

            // �������� �������� ������
            char buffer[1024]{};
            ssize_t bytes_received = recv(socket_fd_, buffer, sizeof(buffer) - 1, MSG_DONTWAIT);

            if (bytes_received > 0) {
                buffer[bytes_received] = '\0';
                std::string received_data(buffer, bytes_received);

                std::lock_guard<std::mutex> lock(receive_mutex_);
                receive_queue_.push(received_data);
            }
            else if (bytes_received == 0) {
                // ���������� ������� ��������
                std::cout << "connection closed by C#" << std::endl;
                connected_ = false;
                break;
            }
            else if (errno != EAGAIN && errno != EWOULDBLOCK) {
                // ������ ������ (����� ������� ����� ������ ��� ������)
                std::cerr << "Recv failed (socket): " << strerror(errno) << std::endl;
                connected_ = false;
                break;
            }

            // ��������� ����� ����� �� ������� CPU
            usleep(10000); // 10ms
        }

        connected_ = false;
    }

    std::thread client_thread_;
    std::atomic<bool> running_;
    std::atomic<bool> connected_;

    int socket_fd_;

    std::queue<std::string> send_queue_;
    std::queue<std::string> receive_queue_;

    std::mutex send_mutex_;
    std::mutex receive_mutex_;
};
