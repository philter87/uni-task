# Stage 1: Build React frontend
FROM node:20-alpine AS frontend
WORKDIR /src/UniTask/ClientApp
COPY UniTask/ClientApp/package*.json ./
RUN npm ci
COPY UniTask/ClientApp/ ./
RUN npm run build
# Output lands in /src/UniTask/wwwroot (vite outDir: '../wwwroot')

# Stage 2: Publish .NET backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY UniTask/UniTask.csproj UniTask/
RUN dotnet restore UniTask/UniTask.csproj
COPY UniTask/ UniTask/
COPY --from=frontend /src/UniTask/wwwroot/ UniTask/wwwroot/
RUN dotnet publish UniTask/UniTask.csproj -c Release -o /app/publish /p:NoBuildWebpack=true

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "UniTask.dll"]
