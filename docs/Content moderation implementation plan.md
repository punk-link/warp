# Content Moderation Implementation Plan

Add an informational-only, fully asynchronous moderation pipeline that rates entry text and attached images with the OpenAI Moderation API after save, persists those results with entry and image metadata, and exposes pending/completed moderation state through the existing entry APIs.

## Architecture

The system uses a KeyDB-backed worker pattern to ensure save latency stays flat even for entries with many images.

### Key Components

- **Moderation Domain Model**: A shared model representing provider, status (Pending, Completed, Failed), flagged state, and category details.
- **Moderation Service**: Service that interacts with OpenAI Moderation API for text and images.
- **Moderation Job Store**: KeyDB-backed index for scheduling and tracking moderation work.
- **Moderation Background Worker**: Hosted service that processes the moderation queue with retry and backoff logic.

## Implementation Steps

1. **Domain Model**: Attach text moderation results to `EntryInfo` and per-image results to `ImageInfo`.
2. **Configuration**: Add `ContentModerationOptions` and feature flags to `appsettings.json`.
3. **Save Flow Integration**: Extend `EntryInfoService.Add` to initialize moderation state as pending and enqueue background work.
4. **Moderation Worker**: Implement a background service that:
   - Acquires a processing lock.
   - Derives plain text from entry content (sanitizing HTML for Advanced mode).
   - Fetches image bytes via `ImageService`.
   - Calls OpenAI Moderation API.
   - Persists results to `EntryInfo` and `ImageInfo`.
5. **API Exposure**: Update `EntryApiResponse` and `ImageInfoResponse` to include moderation status and results.
6. **Observability**: Add log events, domain errors, and metrics for moderation activity.

## Testing Strategy

- **Unit Tests**: Coverage for `EntryInfoService` add/update/copy flows, moderation client mapping, worker retry logic, and DTO serialization.
- **Integration Tests**: Verify end-to-end async processing path in local development and E2E environments.

## Scope Exclusions

- Blocking or excluding flagged content (informational only).
- Image blurring or sensitive image defaults (Issue #190).
- Admin dashboard or audit UI.
- Changes to existing GuardDuty malware scanning behavior.
