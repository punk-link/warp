FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Warp.WebApp/Warp.WebApp.csproj", "Warp.WebApp/"]
RUN dotnet restore "./Warp.WebApp/./Warp.WebApp.csproj"
COPY . .
WORKDIR "/src/Warp.WebApp"
RUN dotnet build "./Warp.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Warp.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=6s --timeout=10s --retries=3 CMD curl -sS 127.0.0.1/health || exit 1

ENTRYPOINT ["dotnet", "Warp.WebApp.dll"]