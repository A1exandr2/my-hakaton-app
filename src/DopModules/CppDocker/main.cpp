#include "main.h"

AsyncPinger pinger;
WebResourceMonitor monitor;
TCPClient client;

bool exit_flag{ false }; 
static void handler(int s) {
    if (s == 2) // crtl+c
    {
        pinger.stop();
        monitor.stop();
        client.disconnect();
        exit_flag = true;
    }
}

int main()
{
    struct sigaction sigIntHandler{};

    sigIntHandler.sa_handler = handler;
    sigemptyset(&sigIntHandler.sa_mask);
    sigIntHandler.sa_flags = 0;
    sigaction(SIGINT, &sigIntHandler, NULL);

    // Устанавливаем колбэк функцию
    std::map<std::string, int> icmp_host_id; 
    pinger.set_callback([&icmp_host_id](std::string host, std::vector<icmplib::PingResult> results) { 
        std::cout << "[CALLBACK] Host: " << host << ", Response: ";
        
        bool success{false};
        int c{0};
        const auto& it = icmp_host_id.find(host); // not null

        for (const auto& result : results) 
        {
            c++;
            switch (result.response) {
            case icmplib::PingResponseType::Success: 
                {
                //std::cout << "Success, time: " << result.delay << " ttl: " << static_cast<std::uint16_t>(result.ttl);
                success = true;
                nlohmann::json obj{};
                obj["Id"] = it->second; // int
                obj["Host"] = host; // str
                obj["Protocol"] = 3; // int
                obj["Result"] = "Success"; //str | Success / Failed
                obj["Delay"] = result.delay; // int задержка
                if (client.isConnected())
                {
                    client.send(obj.dump());
                }
                return;
                }
            }
            //std::cout << std::endl;
        }

        if (!success)
        {
            nlohmann::json obj{};
            obj["Id"] = it->second; // int
            obj["Host"] = host; // str
            obj["Protocol"] = 3; // int
            obj["Result"] = "Failed"; //str | Success / Failed
            obj["Delay"] = results[0].delay; // задержка
            if (client.isConnected())
            {
                client.send(obj.dump());
            }
        }

        });

    
    //// Добавляем адреса для пинга (получить из C#)
    //pinger.add_address("8.8.8.8");
    //pinger.add_address("ya.ru");
    //pinger.add_address("github.com");
    //pinger.add_address("google.com");
    //pinger.add_address("127.0.0.1");

    // Работаем 10 секунд
    //std::this_thread::sleep_for(std::chrono::seconds(10));

    // Убираем один адрес
    //pinger.remove_address("8.8.8.8");
    //pinger.remove_address("127.0.0.1");
    
   
    // Добавляем хосты ДО запуска
    //monitor.add_address("https://example.com", 10);
    //monitor.add_address("https://google.com", 10);
    //monitor.add_address("https://httpbin.org/get", 10);

    monitor.register_cb([](const std::string& host, const ResponseData& response, bool success, int id, int proto) {
       /* std::cout << std::endl << "==================================================\n";
        std::cout << "[" << std::chrono::system_clock::now().time_since_epoch().count()
            << "] Host: " << host << std::endl
            << " | Status: " << (success ? "OK" : "FAIL") << std::endl
            << " | Time: " << response.total_time << "s" << std::endl
            << " | Headers: \n" << response.headers << std::endl
            << " | Http code: " << response.http_code << std::endl
            << " | Redirect count" << response.redirect_count << std::endl
            << " | SSL/TLS info: " << response.ssl_cert_info << std::endl
            << " | SSL Verified: " << (response.ssl_verify_result ? "Yes" : "No") << std::endl
            << std::endl << "==================================================" << std::endl;*/
            nlohmann::json obj{};
            obj["Id"] = id; // int
            obj["Host"] = host; // str
            obj["Protocol"] = proto; // int
            obj["Result"] = success ? "Success" : "Failed"; // str
            obj["Delay"] = response.total_time; // double
            obj["HttpCode"] = response.http_code; // int 
            obj["SslVerifyResult"] = response.ssl_verify_result ? "Yes" : "No"; // str
            obj["ErrorMessage"] = response.error_message; // str
            obj["SslCertInfo"] = response.ssl_cert_info; // str
            obj["RedirectCount"] = response.redirect_count; // int
            obj["Headers"] = response.headers; // str
            if (client.isConnected())
            {
                client.send(obj.dump());
            }
        });

    
    

    // Связка с C#
    if (client.connect("localhost", 7777))
    {
        std::cout << "Connected to C# server" << std::endl;
        std::cout << "Starting monitoring with immediate checks..." << std::endl;
        monitor.start(); // Все хосты проверятся немедленно!

        // Читаем ответы
        std::string response{};
        while (client.isConnected())
        {
            if (client.receive(response)) {
                //std::cout << "Получено: " << response << std::endl;
                //client.send(response);
                try
                {
                    const auto& js_obj = nlohmann::json::parse(response);
                    for (const auto& object : js_obj)
                    {
                        // object
                        /*
                        {
                          "Id": 3, //int
                          "Host": "server03.local",
                          "IntervalMinutes": 15, int
                          "Protocol": 3 //int
                        }
                        */
                        int id = object["Id"].get<int>();
                        std::string host = object["Host"].get<std::string>();
                        int IntervalMinutes = object["IntervalMinutes"].get<int>();
                        int Protocol = object["Protocol"].get<int>();

                        if (Protocol == 1 || Protocol == 2)
                        {
                            monitor.add_address(host, IntervalMinutes * 60000, Protocol, id);
                        }
                        else if (Protocol == 3) // icmp
                        {
                            icmp_host_id.emplace(host, id); // 5:22 утра
                            pinger.add_address(host, std::chrono::minutes(IntervalMinutes));
                        }
                        else
                        {
                            std::cerr << "Unknown protocol: " << Protocol << " with " << host << " | skipped" << std::endl;
                        }
                    }
                }
                catch (const nlohmann::json::exception& ex)
                {
                    std::cerr << "Json error: " << ex.what() << std::endl;
                }
                catch (std::exception& ex)
                {
                    std::cerr << "Uknown json error" << ex.what() << std::endl;
                }
            }
            std::this_thread::sleep_for(std::chrono::milliseconds(30));

            if (!client.isConnected()) {
                std::cout << "Соединение разорвано" << std::endl;
                break;
            }
            
            pinger.update();
            std::this_thread::sleep_for(std::chrono::milliseconds(5));
            
            if(exit_flag)
                break;
        }
    }
    else
    {
        std::cerr << "Failed to connect to C# server" << std::endl;
        return -1;
    }    

    return 0;
}
