# Provisional packaging; image tags and deployment target must match instructor environment sheet.
FROM node:22.16.0-bookworm-slim AS web-build
WORKDIR /app/client
COPY client/package*.json ./
RUN npm install --no-audit --no-fund
COPY client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build
WORKDIR /src
COPY server/Celd.Api/ ./server/Celd.Api/
RUN dotnet restore server/Celd.Api/Celd.Api.csproj
COPY --from=web-build /app/client/dist/ ./server/Celd.Api/wwwroot/
RUN dotnet publish server/Celd.Api/Celd.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=api-build /app/publish/ ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
VOLUME ["/app/data"]
# Supply ConnectionStrings__Celd=Data Source=/app/data/celd.db via secured deployment configuration.
ENTRYPOINT ["dotnet","Celd.Api.dll"]
