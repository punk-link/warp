FROM node:24-alpine AS frontend-deps
RUN corepack enable
WORKDIR /src/Warp.ClientApp
COPY ["Warp.ClientApp/package.json", "Warp.ClientApp/yarn.lock", "./"]
RUN --mount=type=cache,target=/root/.yarn-cache yarn install --frozen-lockfile --non-interactive

FROM node:24-alpine AS frontend-builder
RUN corepack enable
WORKDIR /src/Warp.ClientApp
COPY --from=frontend-deps /src/Warp.ClientApp/node_modules ./node_modules
COPY ["Warp.ClientApp/", "./"]
RUN yarn vitest --run
RUN --mount=type=cache,target=/tmp/vite-cache yarn build

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
RUN apk add --no-cache icu-data-full icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Warp.WebApp/Warp.WebApp.csproj", "Warp.WebApp/"]
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore "./Warp.WebApp/Warp.WebApp.csproj" --runtime linux-x64
COPY . .
WORKDIR "/src/Warp.WebApp"
COPY --from=frontend-builder /src/Warp.ClientApp/dist/. ./wwwroot/
RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish "./Warp.WebApp.csproj" -c $BUILD_CONFIGURATION \
    --runtime linux-x64 --no-restore -o /app/publish /p:UseAppHost=false

FROM build AS test
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore "./Warp.WebApp.Tests/Warp.WebApp.Tests.csproj" --runtime linux-x64
RUN dotnet test --verbosity normal -c $BUILD_CONFIGURATION \
    --blame-hang-timeout 60s \
    -- xUnit.parallelizeTestCollections=true xUnit.maxParallelThreads=0

FROM base AS final
ARG PNKL_VAULT_TOKEN
ENV PNKL_VAULT_TOKEN=$PNKL_VAULT_TOKEN
WORKDIR /app
COPY --from=build /app/publish .

HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1/health || exit 1

ENTRYPOINT ["dotnet", "Warp.WebApp.dll"]