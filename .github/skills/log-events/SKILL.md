---
name: log-events
description: "Workflow for adding, modifying, or removing log events, domain errors, and log messages in the Warp project. Use when: adding new log events to log-events.json; creating or updating domain errors; adding structured logging to a service; the user mentions 'log event', 'log message', 'domain error', 'LogEvents', 'DomainErrors', 'LogMessages', 'code generator', 'CodeGen', or 'log-events.json'. Also use when diagnosing FormatException errors in logging or build failures related to generated files."
---

# Log Events Workflow

This skill covers the full lifecycle of adding, modifying, or removing structured log events in the Warp project. The project uses a code generator (`Warp.CodeGen`) that reads a single JSON source of truth and produces three auto-generated C# files. **Never edit the generated files directly.**

## Architecture

### Source of Truth

`Warp.WebApp/CodeGeneration/log-events.json` — single JSON file defining all log events, grouped by category.

### Generated Files (never edit these)

| File | What it contains |
|---|---|
| `Warp.WebApp/Constants/Logging/LogEvents.cs` | `EventId` constants |
| `Warp.WebApp/Telemetry/Logging/LogMessages.cs` | `[LoggerMessage]` partial method declarations |
| `Warp.WebApp/Extensions/DomainErrors.cs` | Static factory methods for `DomainError` instances |

### Generator

Run from the workspace root:

```bash
dotnet run --project Warp.CodeGen
```

Launch settings in `Warp.CodeGen/Properties/launchSettings.json` supply the file paths.

## Step-by-Step: Adding a New Log Event

### Step 1 — Choose the category and ID

Open `log-events.json`. Events are grouped under `loggingCategories[].events[]`. Each category has a name prefix that determines the ID range. Pick the next available ID within the correct category.

Existing categories and their ID ranges (check the file for current values):
- `Generic` — 10xxx
- `Startup` — 11xxx
- `Infrastructure` — 12xxx
- `Domain.Entry` — 201xx
- `Domain.Creator` — 202xx
- `Domain.OpenGraph` — 203xx
- `Domain.Image` — 204xx
- `Domain.File` — 205xx

### Step 2 — Write the event JSON

Add an entry to the correct category's `events` array:

```json
{
  "id": 20407,
  "name": "MyNewEvent",
  "description": "Something happened to image '{ImageId:Guid}' in entry '{EntryId:Guid}'.",
  "logLevel": "Warning",
  "generateLogMessage": true,
  "obsolete": false,
  "httpCode": 400
}
```

**Field reference:**

| Field | Required | Notes |
|---|---|---|
| `id` | Yes | Unique integer. Must be sequential within its category. |
| `name` | Yes | PascalCase. Becomes the `LogEvents.MyNewEvent` constant and `LogMessages.LogMyNewEvent()` method. |
| `description` | Yes | Message template. Parameters use `{ParamName:Type}` syntax (see below). |
| `domainErrorDescription` | No | If present, generates a `DomainErrors.MyNewEvent()` factory method. May use `{ParamName:Type}` for method parameters and `{N}` positional placeholders for `string.Format`. **This text is returned to the client in API responses — never include sensitive details such as internal IDs, stack traces, or infrastructure names.** |
| `logLevel` | Yes | One of: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`. |
| `generateLogMessage` | Yes | `true` to emit a `LogMessages.LogMyNewEvent()` method; `false` for events that only need a constant. |
| `includeException` | No | `true` to add an `Exception exception` parameter to the log method signature. |
| `obsolete` | Yes | `true` marks the event with `[Obsolete]`. |
| `httpCode` | Yes | HTTP status code associated with the event (used in domain error responses). |

### Step 3 — Parameter syntax in `description`

Use `{ParamName:Type}` where `Type` is a C# type name:

| Annotation | Generated parameter | Example |
|---|---|---|
| `{ImageId:Guid}` | `Guid imageId` | IDs |
| `{ErrorMessage:string}` | `string errorMessage` | Freeform text |
| `{Count:int}` | `int count` | Numeric values |
| `{Name:string?}` | `string? name` | Nullable strings |
| `{Value}` (no type) | `string? value` | Default fallback |

**Important:** The `:Type` annotation is stripped from the `[LoggerMessage]` template at generation time. It is only used to determine the C# parameter type. The generated template will contain `{ImageId}`, not `{ImageId:Guid}`. This prevents `FormatException` errors at runtime (e.g., `Guid.ToString()` does not accept `Guid` as a format string).

### Step 4 — Run the code generator

```bash
dotnet run --project Warp.CodeGen
```

This regenerates all three output files. Verify the console output shows all three files generated successfully.

### Step 5 — Build

```bash
dotnet build Warp.sln
```

The build must succeed. Common failure causes:
- **Missing parameter in test constructors** — if you added a new service dependency to support the log event, update all test files that construct that service.
- **JSON syntax error** — the generator will print `Error: JSON file not found or invalid.`

### Step 6 — Use the generated methods

In your service code:

```csharp
// Log message (from LogMessages.cs)
_logger.LogMyNewEvent(imageId, entryId);

// Domain error (from DomainErrors.cs) — only if domainErrorDescription was set
return DomainErrors.MyNewEvent(imageId, entryId);

// Event constant (from LogEvents.cs)
var eventId = LogEvents.MyNewEvent;
```

## Step-by-Step: Adding a Domain Error (without log message)

If you only need a `DomainErrors.X()` factory method (no runtime logging):

1. Add the event with `"generateLogMessage": false`
2. Include `"domainErrorDescription"` with the user-facing message
3. Run the code generator
4. Build

## Common Mistakes

| Mistake | Symptom | Fix |
|---|---|---|
| Editing `LogMessages.cs`, `LogEvents.cs`, or `DomainErrors.cs` directly | Changes are overwritten on next generator run | Edit `log-events.json` instead, then run the generator |
| Forgetting to run the generator after editing `log-events.json` | Build errors: method not found | Run `dotnet run --project Warp.CodeGen` |
| Forgetting to build after running the generator | Stale binaries, runtime errors | Run `dotnet build Warp.sln` |
| Duplicate event ID | Generator may succeed but runtime `EventId` collision | Check IDs are unique and sequential |
| Using a type annotation as format specifier (e.g. `{Id:Guid}` in a hand-written `[LoggerMessage]`) | `FormatException: Format string can be only "D", "d", "N", "n", "P", "p", "B", "b", "X" or "x".` | Always go through the code generator; it strips type annotations from templates |

## Checklist

Use this after every log event change:

- [ ] Edited only `log-events.json` (not generated files)
- [ ] Event ID is unique and sequential within its category
- [ ] `description` uses `{ParamName:Type}` syntax for parameters
- [ ] `domainErrorDescription` added if a `DomainErrors.X()` method is needed
- [ ] Ran `dotnet run --project Warp.CodeGen` — all three files regenerated
- [ ] Ran `dotnet build Warp.sln` — build succeeded
- [ ] Updated test files if a new service dependency was introduced
