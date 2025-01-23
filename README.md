# Warp

Warp is a service where you can share short-lived texts and media with your friends and colleagues and be sure the data will be removed whenever you expect. The solution based on Asp.Net Core service and backed with KeyDB as a storage for sensitive user data.


## How to Run


### Environment Variables

To run the service you have to pass the following environment variables:

|Variable        |Type  |Notes|Description                        |
|----------------|------|-----|-----------------------------------|
|PNKL_VAULT_ADDR |String|     |An address of a Vault instance     |
|PNKL_VAULT_TOKEN|String|     |An access token of a Vault instance|
|DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS|Boolean|Local env only| Dsiables the telemetry dashboard login|


### Docker Compose

Run the compose file inside the root directory. It sets up external dependencies like a database etc.


### Other

:exclamation: You may use _appSettings.Local.json_ for local runs.

Also you might need to override DB settings to run locally. Set up the _Redis_ section of your _appSettings.Local.json_.


## Database Migrations

### Requirements
- .NET EF Core tools: `dotnet tool install --global dotnet-ef`
- PostgreSQL running (see docker-compose.yml)


### Creating Migrations

1. Go to the application folder.
2. Set environment:
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Local"
```
3. Create migration:
```powershell
dotnet ef migrations add <MigrationName> --project Warp.WebApp.csproj --configuration Local --context WarpDbContext --output-dir Data/Relational/Migrations
```
4. Apply migration:
```powershell
dotnet ef database update
```
\
Migrations are stored in [Migrations](https://github.com/punk-link/warp/tree/master/Warp.WebApp/Data/Relational/Migrations).


## Styles

The project uses Sass to build its styles. To compile the styles:
1. Install Sass `npm install --global sass`
2. Install Yarn `npm install --global yarn`
3. Go to the root project folder and update project dependencies `yarn upgrade`
4. Go to _Warp.WebApp/Styles_ folder and execute the following command `sass --style compressed main.scss ../wwwroot/css/main.min.css`


## Project Icons

The project uses these [icofont](https://icofont.com) glyphs:

- align-left
- bin
- clock
- close
- exclamation-tringle
- eye
- hill-sunny
- link
- loop
- pencil-alt-2
- plus
- simple-left
- worried