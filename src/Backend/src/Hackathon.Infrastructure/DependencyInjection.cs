using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hackathon.Infrastructure.Services;
using Hackathon.Infrastructure.Data;
using Hackathon.Domain.Repositories;
using Hackathon.Infrastructure.Repositories;

namespace Hackathon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("Postgresql");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
        );

        services.AddScoped<ClickHouseInitializer>();

        services.AddScoped<IUserRepository, UsersRepository>();
        services.AddScoped<IServersRepository, ServersRepository>();
        services.AddScoped<IPingLogsRepository, PingLogsClickHouseRepository>();
        return services;
    }
}