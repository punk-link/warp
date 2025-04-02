FROM node:18-alpine AS node-dependencies
WORKDIR /dependencies
COPY ["Warp.WebApp/package.json", "Warp.WebApp/yarn.lock", "./"]
RUN yarn install

FROM node:18-alpine AS frontend-builder
WORKDIR /src
COPY --from=node-dependencies /dependencies/node_modules ./node_modules
COPY ["Warp.WebApp/package.json", "Warp.WebApp/postcss.config.js", "Warp.WebApp/tailwind.config.js", "Warp.WebApp/vite.config.js", "./"]
# Copy all content that Tailwind needs to scan
COPY ["Warp.WebApp/Pages", "./Pages"]
COPY ["Warp.WebApp/wwwroot/js", "./wwwroot/js"]
COPY ["Warp.WebApp/Styles", "./Styles"]
RUN mkdir -p ./wwwroot/css ./wwwroot/dist
RUN yarn build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src
COPY ["Warp.WebApp/Warp.WebApp.csproj", "Warp.WebApp/"]
RUN dotnet restore "./Warp.WebApp/./Warp.WebApp.csproj"

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY --from=restore /src .
COPY . .
WORKDIR "/src/Warp.WebApp"
COPY --from=frontend-builder /src/wwwroot/css ./wwwroot/css
COPY --from=frontend-builder /src/wwwroot/dist ./wwwroot/dist
RUN dotnet build "./Warp.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS test
WORKDIR "/src"
RUN dotnet test --no-build --verbosity normal --configuration $BUILD_CONFIGURATION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Warp.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG PNKL_VAULT_TOKEN
ENV PNKL_VAULT_TOKEN=$PNKL_VAULT_TOKEN
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1/health || exit 1

ENTRYPOINT ["dotnet", "Warp.WebApp.dll"]