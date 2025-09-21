
using Hackathon.Api.Common.Handlers;
using Hackathon.Application;
using Hackathon.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Hackathon.Api.Services;
using Hackathon.Infrastructure.Config;
using Hackathon.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Hackathon.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

builder.Services.AddExceptionHandler<CustomExeptionHandler>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = false;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));
builder.Services.AddSingleton<RabbitMqSettings>(sp => sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value);
builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddSingleton<TcpServerManager>();
builder.Services.AddHostedService<TcpLogServer>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(options => { });

using var scope = app.Services.CreateScope();
var initializer = scope.ServiceProvider.GetRequiredService<ClickHouseInitializer>();
try
{
    await initializer.InitializeAsync();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}
catch
{
    throw;
}


app.MapControllers();
app.UseHttpsRedirection();

app.Run();
