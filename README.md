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
  "KeyFilePath": "C:\\ProgramData\\Warp\\encryption-key.txt"
}
```

Alternatively, you can set an environment variable:

```bash
# Windows
set WARP_ENCRYPTION_KEY=your_base64_key_here

# Linux/macOS
export WARP_ENCRYPTION_KEY=your_base64_key_here
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
