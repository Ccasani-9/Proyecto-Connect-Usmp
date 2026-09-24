# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY UsmpConnect/UsmpConnect.csproj UsmpConnect/
RUN dotnet restore UsmpConnect/UsmpConnect.csproj
COPY UsmpConnect/ UsmpConnect/
RUN dotnet publish UsmpConnect/UsmpConnect.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Carpetas con escritura para la BD SQLite y los archivos subidos
RUN mkdir -p /app/data /app/wwwroot/uploads && chown -R $APP_UID /app/data /app/wwwroot/uploads
USER $APP_UID
ENV ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/usmpconnect.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "UsmpConnect.dll"]
