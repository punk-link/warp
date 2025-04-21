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


## Code Generation

Warp includes a code generation system for managing logging events and messages. The generator creates consistent logging constants, enums, and helper methods based on a JSON configuration file.

### Logging Configuration

The logging configuration is defined in `Warp.WebApp/CodeGeneration/LoggingEvents.json`. Each logging event includes:

- `id` - Unique numeric identifier
- `name` - Name of the logging event
- `description` - Description template with optional parameters in format `{ParameterName:Type}`
- `logLevel` - Severity level (Debug, Information, Warning, Error, Critical)
- `generateLogMessage` - Boolean flag to control whether a log method should be generated
- `obsolete` - Boolean flag to mark deprecated log events that will be removed in future versions

Example event:
```json
{
  "id": 12001,
  "name": "DefaultCacheValueError",
  "description": "Unable to store a default value {CacheValue}.",
  "logLevel": "Warning",
  "generateLogMessage": true,
  "obsolete": false
}
```

### Generated Files

The code generator produces:

1. `LoggingConstants.cs` - Enum definitions for all logging events with Description attributes
2. `LogMessages.cs` - Extension methods for ILogger with structured logging support

When a log event is marked as `obsolete: true`, both the enum value and log method are decorated with the `[Obsolete]` attribute, generating compiler warnings when used.

### Running Code Generation

Code generation runs automatically during the build process through a target in the project file. You can also run it manually:

```bash
dotnet run --project Warp.CodeGen/Warp.CodeGen.csproj -- --json Warp.WebApp/CodeGeneration/logging-events.json --constants Warp.WebApp/Constants/Logging/LoggingConstants.cs --messages Warp.WebApp/Telemetry/Logging/LogMessages.cs
```

## Styles

The project uses a modern build process for its styles, leveraging both Sass and Tailwind CSS. All style assets are generated via a series of npm scripts defined in the _package.json_. Follow these steps to build the styles:
1. **Install Node Modules**

   Make sure you have Node.js and Yarn installed. Then, from the root **project** folder, run:

   ```bash
   yarn install
   ```
2. **Build Styles**

   To compile all styles (both Sass and Tailwind), simply run:

   ```bash
   yarn build
   ```


## Project Icons

The project uses these [icofont](https://icofont.com) glyphs:

- align-left
- bin
- clock
- close
- copy
- exclamation-tringle
- eye
- hill-sunny
- link
- loop
- pencil-alt-2
- plus
- simple-left
- worried
