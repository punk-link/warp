# Tracing Architecture

This document summarizes how request tracing flows from the Warp SPA through the ASP.NET Core backend and into downstream dependencies. The goal is to ensure every user interaction can be correlated end to end when analysing logs, telemetry, and error reports.

## Frontend Instrumentation
- `fetchJson` now issues [W3C Trace Context](https://www.w3.org/TR/trace-context/) headers (`traceparent`, `tracestate`) and a convenience `x-trace-id` header for every request, ensuring each outbound call has a unique trace identifier.
- A new helper (`telemetry/traceContext.ts`) centralises trace id generation, normalisation, and response parsing. Direct `fetch` usages (CSRF bootstrap, multipart uploads) were updated to reuse this helper so that binary and JSON flows share the same propagation path.
- The error bridge and telemetry emitters automatically include the resolved trace id, making it trivial to copy diagnostics or locate the correlated backend log entries.

## Backend Propagation
- `TraceContextMiddleware` is introduced at the start of the pipeline. It
  - adopts incoming W3C trace headers (or creates a new activity on fallback),
  - ensures `HttpContext.TraceIdentifier` uses the activity trace id,
  - exposes the id in `HttpContext.Items["TraceId"]`, and
  - appends `x-trace-id`, `traceparent`, and `tracestate` headers to every response.
- `ActivityTraceFlags.Recorded` is enforced so downstream `HttpClient` instrumentation emits spans that stay attached to the originating request.
- Domain and cancellation handlers now extract `Activity.Current.TraceId`, guaranteeing consistent correlation ids inside problem details, custom logs, and retry surfaces.

## Logging & Diagnostics
- The middleware opens a logging scope containing `TraceId` and `SpanId`. Because logging is configured with `IncludeScopes = true`, every structured log automatically inherits these properties.
- Server-side exceptions enrich their responses and Sentry payloads with the propagated trace id even when the ASP.NET pipeline generates a new identifier.
- Problem details continue to expose the `trace-id` extension that the SPA surfaces in notification UI and clipboard diagnostics.

## Developer Workflow
- The tracing helpers are framework-agnostic and can be reused by future composables or API clients by calling `buildTraceHeaders()`.
- When adding new backend endpoints or background services, prefer accessing the active trace id via `Activity.Current?.TraceId.ToString()` or `HttpContext.TraceIdentifier` to keep consistency with response headers.
- OpenTelemetry exporters remain configurable through `OpenTelemetry:Endpoint`. No additional runtime dependencies were introduced; the middleware relies on the BCL `Activity` APIs already referenced by the project.

## Testing & Verification
- SPA unit tests (`src/__tests__/traceContext.spec.ts`) validate id generation, header construction, and extraction semantics.
- Backend tests (`Warp.WebApp.Tests/Middlewares/TraceContextMiddlewareTests`) verify the middleware’s header emission and activity bootstrap behaviour.
- To manually exercise the flow, run `yarn dev` for the client and `dotnet run --project Warp.WebApp` for the API. Inspect network requests in the browser dev tools to confirm trace headers, and check server logs for matching `TraceId` values.
