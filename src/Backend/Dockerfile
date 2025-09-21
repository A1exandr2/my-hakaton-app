# Этап 1: Сборка
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем всё содержимое проекта (включая .sln, .csproj)
COPY . ./

# Восстанавливаем зависимости
RUN dotnet restore src/Hackathon.Api/Hackathon.Api.csproj

# Публикуем приложение
RUN dotnet publish src/Hackathon.Api/Hackathon.Api.csproj -c Release -o /app/publish

# Этап 2: Запуск
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=build /app/publish .

# Открываем порты
EXPOSE 80
EXPOSE 5050

# Создаём пользователя (опционально)
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Запускаем API
ENTRYPOINT ["dotnet", "Hackathon.Api.dll"]