using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using MediatR;
using Hackathon.Application.Behaviors;
using Hackathon.Domain.Services;
using Hackathon.Application.Services;

namespace Hackathon.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services
    )
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            x.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            x.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IIntervalService, IntervalService>();

        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());

        return services;
    }
}