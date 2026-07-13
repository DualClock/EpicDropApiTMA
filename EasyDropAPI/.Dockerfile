# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY ["EasyDropAPI/EasyDropAPI.csproj", "EasyDropAPI/"]
RUN dotnet restore "EasyDropAPI/EasyDropAPI.csproj"

# Копируем весь код и публикуем
COPY . .
WORKDIR "/src/EasyDropAPI"
RUN dotnet publish "EasyDropAPI.csproj" -c Release -o /app/publish

# Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Railway устанавливает PORT автоматически
EXPOSE 8080

# Копируем опубликованные файлы
COPY --from=build /app/publish .

# Запускаем приложение
ENTRYPOINT ["dotnet", "EasyDropAPI.dll"]