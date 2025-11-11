# Warp.

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

Warp includes a code generation system for managing logging events, domain errors, and messages. The generator creates consistent logging constants, enums, domain error factories, and helper methods based on a JSON configuration file.

### Logging Configuration

The logging configuration is defined in `Warp.WebApp/CodeGeneration/log-events.json`. The configuration follows a hierarchical structure where logging events are organized by categories. Each logging event includes:

- `id` - Unique numeric identifier
- `name` - Name of the logging event
- `description` - Description template with optional parameters in format `{ParameterName:Type}`
- `domainErrorDescription` - Optional alternative description for domain error messages
- `logLevel` - Severity level (Debug, Information, Warning, Error, Critical)
- `generateLogMessage` - Boolean flag to control whether a log method should be generated
- `obsolete` - Boolean flag to mark deprecated log events that will be removed in future versions
- `httpCode` - HTTP status code that generates a ProducesResponseType attribute

Example event:
```json
{
  "id": 12201,
  "name": "ImageUploadError",
  "description": "An error occurred while uploading the image. Details: '{ErrorMessage:string}'.",
  "domainErrorDescription": "Failed to upload image: {ErrorMessage:string}",
  "logLevel": "Error",
  "generateLogMessage": true,
  "obsolete": false,
  "httpCode": 400
}
```

### Generated Files

The code generator produces:

1. `LogEvents.cs` - Enum definitions for all logging events with Description attributes and HTTP status code decorations
2. `LogMessages.cs` - Extension methods for ILogger with structured logging support using the [LoggerMessage] attribute pattern
3. `DomainErrors.cs` - Static methods to create strongly-typed DomainError instances with appropriate error codes and messages

When a log event is marked as `obsolete: true`, all related artifacts (enum value, log method, and domain error factory method) are decorated with the `[Obsolete]` attribute, generating compiler warnings when used.

When a log event includes an `httpCode` value, the enum will be decorated with `[HttpStatusCode]`, which can be used for API documentation and response type specification.

### Parameter Extraction

The code generation system automatically extracts parameters from message templates using a regular expression pattern. Parameters are formatted as `{ParameterName}` or `{ParameterName:Type}` where:

- `ParameterName` is converted to camelCase for method parameters
- `Type` defines the parameter type (defaults to `string?` if not specified)

For example, a template like `"User {UserId:Guid} uploaded {FileCount:int} files"` generates parameters `Guid userId, int fileCount`.

### Running Code Generation

Code generation runs automatically during the release build process through a target in the project file. You can also run it manually:

```bash
dotnet run --project Warp.CodeGen/Warp.CodeGen.csproj -- --json Warp.WebApp/CodeGeneration/log-events.json --constants Warp.WebApp/Constants/Logging/LogEvents.cs --messages Warp.WebApp/Telemetry/Logging/LogMessages.cs --domain-errors Warp.WebApp/Extensions/DomainErrors.cs
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


## Encryption Key Management

Warp uses AES-256 encryption for all data stored in KeyDB. The encryption key can be managed using the `warp-keymanager` tool.

### Installing warp-keymanager

Build and install the tool as a .NET global tool:

```bash
cd Warp.KeyManager
dotnet pack
dotnet tool install --global --add-source ./nupkg Warp.KeyManager
```

### Generating a Local Encryption Key

To generate a new encryption key for local development:

```bash
# Generate and display a Base64 key
warp-keymanager generate --base64

# Generate and save a key to a file
warp-keymanager generate --base64 --output C:\ProgramData\Warp\encryption-key.txt
```

### Configuring the Local Application to Use the Key

Update your `appsettings.json` or `appsettings.Local.json`:

```json
"EncryptionOptions": {
  "KeyFilePath": "C:\\ProgramData\\Warp\\encryption-key.txt",
  "TransitKeyName": "warp-key",
  "Type": "AesEncryptionService"
}
```

Alternatively, you can set an environment variable:

```bash
# Windows
set WARP_ENCRYPTION_KEY=your_base64_key_here

# Linux/macOS
export WARP_ENCRYPTION_KEY=your_base64_key_here
```

### Using TransitEncryptionService

Warp offers a `TransitEncryptionService` which integrates with HashiCorp Vault's Transit secrets engine. To use it, set the following configuration:

```json
"EncryptionOptions": {
  "Type": "TransitEncryptionService",
  "TransitKeyName": "warp-keydb"
}
```

The `EncryptionOptions:Type` setting is mandatory and must be set to either `AesEncryptionService` or `TransitEncryptionService` to specify which encryption service implementation to use. The service implementation is selected at application startup based on this configuration value.


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


## Tracing

Refer to [docs/tracing.md](docs/tracing.md) for details on the end-to-end tracing pipeline, header propagation rules, and troubleshooting tips.
