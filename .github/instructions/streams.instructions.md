---
applyTo: '**/*.cs'
---

# Stream Patterns

- Always dispose intermediate streams (`MemoryStream`, S3 response streams) with `using var` after extracting the needed data.
- When materializing a stream to `byte[]`, use `using var ms = new MemoryStream()` + `CopyToAsync` + `ToArray()`, then dispose both the source and buffer streams.
- Return a fresh `new MemoryStream(bytes)` to callers — never return the intermediate buffer stream used for materialization.
- For cache storage, use `byte[]`-based record structs (JSON-serializable). For API responses, use `Stream`-based models.
