using NotificationService.EmailService.Models;
using NotificationService.EmailService.Services;
using NotificationService.Worker.Models;
using NotificationService.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Конфигурация SMTP
        services.Configure<SmtpSettings>(context.Configuration.GetSection("SmtpSettings"));
        
        // Конфигурация RabbitMQ
        services.Configure<RabbitMqSettings>(context.Configuration.GetSection("RabbitMqSettings"));

        
        // Регистрируем Worker
        services.AddHostedService<NotificationWorker>();

        services.AddScoped<IEmailService, EmailService>();

        // Логирование
        services.AddLogging(configure => 
            configure.AddConsole().AddDebug());
    })
    .Build();

await host.RunAsync();