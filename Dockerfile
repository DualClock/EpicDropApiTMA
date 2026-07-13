# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy entire repo so ..\-relative project references resolve
COPY . .
RUN dotnet restore "EasyDropAPI/EasyDropAPI.csproj"
RUN dotnet publish "EasyDropAPI/EasyDropAPI.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Bind to Railway-provided PORT on all interfaces
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "EasyDropAPI.dll"]
