# Этап 1: Сборка
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем всё содержимое проекта
COPY . ./

# Восстанавливаем зависимости — ИСПРАВЛЕНО
RUN dotnet restore src/Backend/src/Hackathon.Api/Hackathon.Api.csproj

# Публикуем приложение — ИСПРАВЛЕНО
RUN dotnet publish src/Backend/src/Hackathon.Api/Hackathon.Api.csproj -c Release -o /app/publish

# Этап 2: Запуск
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=build /app/publish .

# Слушаем порт 80 (по умолчанию)
EXPOSE 80

# Создаём пользователя (опционально)
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Запускаем API
ENTRYPOINT ["dotnet", "Hackathon.Api.dll"]
