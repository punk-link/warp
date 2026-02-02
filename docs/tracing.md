# Tracing Architecture

This document summarizes how request tracing flows from the Warp SPA through the ASP.NET Core backend and into downstream dependencies. The goal is to ensure every user interaction can be correlated end to end when analysing logs, telemetry, and error reports.

## Architecture Overview

```
Frontend → Backend → OTEL Collector → Loki (logs)
  (W3C)      (OTLP)                 → Tempo (traces)
                                    → Prometheus (metrics)
                                    ↓
                                 Grafana (visualization)
```

**Local Development:** Optional Grafana LGTM stack via `docker-compose.telemetry.yml`  
**Kubernetes:** OTEL Collector in `observability` namespace routes to Loki, Tempo, and Prometheus

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
- OpenTelemetry exporters remain configurable through `OpenTelemetry:Endpoint`. The endpoint points to an OTEL Collector that fans out to Loki (logs), Tempo (traces), and Prometheus (metrics).

## Observability Stack

**Local Development:**
- Start full stack: `docker compose -f docker-compose.yml -f docker-compose.telemetry.yml up`
- Access Grafana: http://localhost:3000 (anonymous access enabled)
- OTEL Collector endpoint: http://localhost:4317

**Stack Versions:**
- OTEL Collector: 0.144.0
- Loki: 3.6.4
- Tempo: 2.7.1 (pinned to avoid v2.10.0 MetricsGenerator gRPC registration bug)
- Prometheus: v3.5.1
- Grafana: 12.3

**Kubernetes:**
- OTEL Collector: `otel-collector.observability.svc.cluster.local:4317`
- Configured via `OpenTelemetry__Endpoint` environment variable in deployment

**Using Grafana:**
1. **Explore → Tempo**: Search traces by trace ID, service name, or duration
2. **Explore → Loki**: Query logs with LogQL (e.g., `{app="warp"} |= "error"`)
3. **Trace-to-Logs**: Click any trace span → "Logs for this span" → see correlated logs
4. **Dashboards**: Pre-configured datasources with correlation enabled

## Testing & Verification
- SPA unit tests (`src/__tests__/traceContext.spec.ts`) validate id generation, header construction, and extraction semantics.
- Backend tests (`Warp.WebApp.Tests/Middlewares/TraceContextMiddlewareTests`) verify the middleware’s header emission and activity bootstrap behaviour.
- To manually exercise the flow:
  1. Start telemetry stack: `docker compose -f docker-compose.yml -f docker-compose.telemetry.yml up`
  2. Run the API: `dotnet run --project Warp.WebApp`
  3. Run the client: `yarn dev`
  4. Generate traffic and open Grafana at http://localhost:3000
  5. **Verify traces:** Explore → Tempo → search by service name `warp`
  6. **Verify logs:** Explore → Loki → query `{app="warp"}`
  7. **Verify correlation:** Click a trace span → "Logs for this span" → see related logs with matching trace ID
