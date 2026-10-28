FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY TransactionsIngest.Api/TransactionsIngest.Api.csproj TransactionsIngest.Api/
RUN dotnet restore TransactionsIngest.Api/TransactionsIngest.Api.csproj
COPY TransactionsIngest.Api/ TransactionsIngest.Api/
RUN dotnet publish TransactionsIngest.Api/TransactionsIngest.Api.csproj \
    -c Release --no-restore -o /out /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /out .
RUN mkdir -p /data && chown "$APP_UID:$APP_UID" /data
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__DefaultConnection="Data Source=/data/transactions.db" \
    Database__ApplyMigrationsOnStartup=true
USER $APP_UID
EXPOSE 8080
HEALTHCHECK --interval=5s --timeout=3s --start-period=30s --retries=12 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/ || exit 1
ENTRYPOINT ["dotnet", "TransactionsIngest.Api.dll"]
