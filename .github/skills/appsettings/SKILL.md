---
name: appsettings
description: "Rules for modifying ASP.NET Core configuration files (appsettings*.json) in the Warp project. Use when: adding, removing, or changing any configuration key in appsettings files; creating new Options classes that bind to configuration; the user mentions 'appsettings', 'configuration', 'options', 'config value', 'environment config', 'feature flag', or 'FeatureManagement'; adding a new service that reads IConfiguration or IOptions<T>. Also use when you need to decide which appsettings file a value belongs in. Prevents unnecessary duplication of config entries across environment files."
---

# AppSettings Workflow

This skill defines how configuration is structured across environments in the Warp project. The most common mistake is scattering values into every environment-specific file when they belong in the shared base or only in specific environments. Follow these rules to keep configuration minimal and correct.

## Environment Hierarchy

The project uses ASP.NET Core's layered configuration. Later files override earlier ones.

| Priority | File | Purpose | Config source |
|----------|------|---------|---------------|
| 1 (base) | `appsettings.json` | Shared defaults for all environments | Checked into repo |
| 2 | `appsettings.Local.json` | Local developer machine | **Gitignored** — not in source control |
| 3 | `appsettings.E2E.json` | End-to-end tests in Docker Compose | Checked into repo |
| 4 | `appsettings.E2ELocal.json` | End-to-end tests running locally | Checked into repo |
| 5 | Vault + Consul | Development and Production | Injected at runtime, not in repo |

### How environments are resolved

The environment is set via `ASPNETCORE_ENVIRONMENT`. The loading logic lives in `WebApplicationBuilderExtensions.AddConfiguration`:

- **Local** → loads `appsettings.json` + `appsettings.Local.json`
- **E2E / E2ELocal** → loads `appsettings.json` + `appsettings.{E2E|E2ELocal}.json` + Vault token from file
- **Development / Production** → loads `appsettings.json` + Vault secrets + Consul KV store (no environment-specific JSON file)

This means Development and Production **never read** an environment-specific JSON file from disk — their configuration comes entirely from `appsettings.json` (base) plus Vault/Consul at runtime.

## Where to Place a New Configuration Value

Use the **least specific file** that satisfies the requirement:

### 1. `appsettings.json` (preferred default)

Put a value here when it applies to **all environments** and doesn't contain secrets or environment-specific addresses. This is the right place for:

- Feature flag defaults (`FeatureManagement`)
- Validation limits (`EntryValidatorOptions`, `ImageCacheOptions`, `MalwareScanOptions`)
- Contact emails, service name, allowed hosts
- Any option where the value is the same everywhere

**Most new configuration belongs here.** If you aren't sure, start here.

### 2. `appsettings.Local.json`

This file is **gitignored** — it never enters source control. That makes it safe for developer-specific secrets and local infrastructure addresses. Put a value here when it needs a different value for local development compared to the base. Typical entries:

- Connection strings to localhost services (`Redis`, `ConnectionStrings`, `S3Options`)
- Local encryption key paths (`EncryptionOptions`)
- Debug-level logging overrides
- Local SPA dev server URL (`Spa:ServerUrl`)
- OpenTelemetry endpoint pointing to localhost
- Sentry DSNs
- Local secrets (S3 keys, Vault addresses) — safe here because the file is not committed

### 3. `appsettings.E2E.json` / `appsettings.E2ELocal.json`

Put a value here **only** when E2E tests need a value that differs from both the base and Local. These two files are nearly identical — the difference is:

- **E2E**: services run inside Docker Compose (hostnames like `warp-keydb`, `warp-s3mock`, `warp-vault`)
- **E2ELocal**: services run on the host machine (hostnames are `localhost`)

Typical entries differ only in hostnames and Vault token paths.

### 4. Vault / Consul (Development & Production)

Secrets and environment-specific infrastructure addresses for deployed environments. Never put these values in any JSON file.

## Secrets Policy

**Real secrets (production API keys, passwords, tokens) must never appear in any JSON file.** They live exclusively in Vault.

E2E and Local files may contain isolated test credentials (e.g., S3 mock access keys like `local-dev`, Sentry DSNs). These are not real secrets — they connect to local or containerized mock services and are safe to commit.

When in doubt: if the credential could grant access to a real external service or production data, it belongs in Vault.

## Decision Flowchart

When adding a new config key, follow this sequence:

1. **Is the value a real secret (production API key, password, token)?**
   - Yes → Vault only. Do not put it in any JSON file.
   - No → continue.

2. **Is the value the same across all environments?**
   - Yes → `appsettings.json`. Done.
   - No → continue.

3. **Does only the local dev environment need a different value?**
   - Yes → base value in `appsettings.json`, override in `appsettings.Local.json`.
   - No → continue.

4. **Do E2E tests need a distinct value?**
   - Yes → override in `appsettings.E2E.json` and `appsettings.E2ELocal.json` (only the keys that differ).
   - No → the value is likely environment-specific infrastructure → Vault/Consul.

## Rules

### Never duplicate values unnecessarily

If a value in an environment file is identical to what's already in `appsettings.json`, **do not add it** to the environment file. ASP.NET Core's layered config handles inheritance automatically — environment files should contain **only overrides**.

### Keep environment files minimal

Each environment file should contain the smallest possible set of keys — only those that genuinely differ from the base. Before adding a key to an environment file, check whether the base already has the right value.

### Do not create environment files for Development or Production

These environments load configuration from Vault and Consul at runtime. There are no `appsettings.Development.json` or `appsettings.Production.json` files, and none should be created.

### Do not reorganize existing configuration

The current placement of values across files is intentional. Do not move existing keys between files unless explicitly asked. This skill governs **new** entries and **modifications** — not audits of existing structure.

### Match the existing Options pattern when adding new sections

New configuration sections should follow the established pattern:

1. Create an Options class in `Warp.WebApp/Models/Options/`
2. Register it in `ServiceCollectionExtensions.AddOptions` using `.AddOptions<T>().Bind(...).ValidateOnStart()`
3. Add default values in `appsettings.json`
4. Override only in the specific environment files that need different values

### Feature flags go in the base file

`FeatureManagement` entries belong in `appsettings.json`. Override in environment files only when a feature must be explicitly toggled differently (e.g., disabling `MalwareScan` in E2E where the scanner is unavailable).

## Consul KV Mapping (Development & Production)

Development and Production get their environment-specific configuration from a single Consul KV entry. Understanding how this maps to `IConfiguration` keys is essential when adding sections that need different values per deployed environment.

### KV key path

The Consul KV key is built as:

```
{ASPNETCORE_ENVIRONMENT}/{ServiceName}
```

Both parts are lowercased. `ServiceName` comes from `appsettings.json` (currently `"warp"`).

| Environment | Consul KV key |
|-------------|---------------|
| Development | `development/warp` |
| Production | `production/warp` |

### KV value format

The value stored at the KV key is a **single JSON object**. Its structure mirrors the same section names used in `appsettings.json`. The Consul provider recursively flattens nested JSON into ASP.NET Core's `:` delimited configuration keys.

**Example Consul KV value:**

```json
{
  "AnalyticsOptions": {
    "GoogleGTag": "G-XXXXX",
    "YandexMetrikaNumber": "123456"
  },
  "OpenGraph": {
    "DefaultImageUrl": "https://cdn.example.com/og.png",
    "Title": "Warp"
  },
  "Redis": {
    "Host": "keydb.internal",
    "Port": 6379
  }
}
```

This flattens to:

```
AnalyticsOptions:GoogleGTag       = G-XXXXX
AnalyticsOptions:YandexMetrikaNumber = 123456
OpenGraph:DefaultImageUrl         = https://cdn.example.com/og.png
OpenGraph:Title                   = Warp
Redis:Host                        = keydb.internal
Redis:Port                        = 6379
```

### How Consul overrides work

Consul values are loaded **after** `appsettings.json`, so they override any base defaults. The section names in the Consul JSON must exactly match the section names in `appsettings.json` (case-sensitive).

Arrays use index-based keys: `"AllowedExtensions": [".jpg", ".png"]` becomes `AllowedExtensions:0 = .jpg`, `AllowedExtensions:1 = .png`.

### What goes in Consul vs Vault

- **Consul** stores non-secret, environment-specific configuration: service addresses, feature flags, option values that differ between Development and Production.
- **Vault** stores secrets: `ConsulAddress`, `ConsulToken`, and `S3SecretAccessKey` are retrieved from Vault first, then Consul is queried using those credentials.

### Implementation reference

The mapping logic lives in:
- `Warp.WebApp/Helpers/Configuration/ConsulHelper.cs` — builds the KV key and calls the provider
- `Warp.WebApp/Helpers/Configuration/ConfigurationProviders/ConsulConfigurationProvider.cs` — fetches, parses, and flattens the JSON
- `Warp.WebApp/Models/Options/ProgramSecrets.cs` — Vault secrets model (`ConsulAddress`, `ConsulToken`, `S3SecretAccessKey`)

## Front-End Configuration Delivery

Front-end config is **not** served from static files. The backend dynamically generates two JavaScript endpoints that inject configuration into `window`:

| Endpoint | What it serves | Source |
|----------|---------------|--------|
| `GET /config.js` | `window.appConfig` — environment name, validation limits, Sentry DSN/sample rates, contact emails | `EntryValidatorOptions`, `Sentry:*`, `ContactEmails:*`, `IWebHostEnvironment` |
| `GET /analytics.js` | Google GTag and Yandex Metrika inline scripts | `AnalyticsOptions` |

Both endpoints set `Cache-Control: no-store` so values are always fresh.

When adding a new value that the SPA needs:

1. Add the config key to the appropriate `appsettings*.json` file(s) following the placement rules above.
2. Read it in `SpaExtensions.MapSpaConfigs` (for app config) or `SpaExtensions.MapSpaAnalytics` (for analytics) and include it in the response dictionary.
3. Consume it in the SPA via `window.appConfig`.

Do **not** create separate static config files for the front-end.

## Current Configuration Sections

For reference, these sections are currently bound to Options classes:

| Section | Options class |
|---------|--------------|
| `EntryValidatorOptions` | `EntryValidatorOptions` |
| `ImageCacheOptions` | `ImageCacheOptions` |
| `MalwareScanOptions` | `MalwareScanOptions` |
| `ImageUploadOptions` | `ImageUploadOptions` |
| `EntryCleanupOptions` | `EntryCleanupOptions` |
| `OrphanImageCleanupOptions` | `OrphanImageCleanupOptions` |
| `EncryptionOptions` | `EncryptionOptions` |
| `S3Options` | `S3Options` |
| `AnalyticsOptions` | `AnalyticsOptions` |
| `OpenGraph` | `OpenGraphOptions` |
| `FeatureManagement` | Built-in (`AddFeatureManagement`) |
