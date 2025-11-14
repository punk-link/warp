FROM node:22-alpine AS frontend-deps
ENV NODE_ENV=production
WORKDIR /src/Warp.ClientApp
COPY ["Warp.ClientApp/package.json", "Warp.ClientApp/yarn.lock", "./"]
RUN --mount=type=cache,target=/root/.yarn-cache yarn install --frozen-lockfile --non-interactive

FROM node:22-alpine AS frontend-builder
WORKDIR /src/Warp.ClientApp
COPY ["Warp.ClientApp/", "./"]
COPY --from=frontend-deps /src/Warp.ClientApp/node_modules ./node_modules
RUN yarn vitest --run
RUN --mount=type=cache,target=/tmp/vite-cache yarn build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Warp.WebApp/Warp.WebApp.csproj", "Warp.WebApp/"]
COPY ["Warp.WebApp.Tests/Warp.WebApp.Tests.csproj", "Warp.WebApp.Tests/"]
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore "./Warp.WebApp/Warp.WebApp.csproj" --runtime linux-x64
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore "./Warp.WebApp.Tests/Warp.WebApp.Tests.csproj" --runtime linux-x64
COPY . .
WORKDIR "/src/Warp.WebApp"
COPY --from=frontend-builder /src/Warp.ClientApp/dist/. ./wwwroot/
RUN --mount=type=cache,target=/root/.nuget/packages dotnet build "./Warp.WebApp.csproj" -c $BUILD_CONFIGURATION \
    -o /app/build --runtime linux-x64

FROM build AS test
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN dotnet test --verbosity normal -c $BUILD_CONFIGURATION \
    --blame-hang-timeout 60s \
    -- xUnit.parallelizeTestCollections=true xUnit.maxParallelThreads=0

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish "./Warp.WebApp.csproj" -c $BUILD_CONFIGURATION \
    --runtime linux-x64 --no-restore --no-build -o /app/publish /p:OutputPath=/app/build /p:UseAppHost=false

FROM base AS final
ARG PNKL_VAULT_TOKEN
ENV PNKL_VAULT_TOKEN=$PNKL_VAULT_TOKEN
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1/health || exit 1

ENTRYPOINT ["dotnet", "Warp.WebApp.dll"]