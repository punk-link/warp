# KeyDB Image Binary Caching

Since all Warp entries are temporal with a known TTL, we can cache image binaries in KeyDB at entry save time to avoid S3 round-trips on every read. Images are cached directly inside `ImageService` (no new service class), with a configurable size threshold to protect KeyDB memory from large uploads. A cache-aside fallback repopulates the cache if an image is missing.

## Steps

1. **Create `CachedImage` model** — add `Warp.WebApp/Models/Files/CachedImage.cs`. Define a `readonly record struct` with `byte[] Content` and `string ContentType`. This is the JSON-serializable counterpart of `Image` (which uses `Stream`). `KeyDbStore` will serialize/encrypt it automatically via the existing pipeline.

2. **Add Redis DB index for `CachedImage`** — in `RedisStoreBase.ToDatabaseIndex`, add `CachedImage => 7` to the switch expression. Add a `using Warp.WebApp.Models.Files;` import. This isolates cached image data in its own Redis database, consistent with the project's partitioning convention.

3. **Add cache key builder method** — in `CacheKeyBuilder`, add `BuildImageContentCacheKey(in Guid entryId, in Guid imageId)` returning `"CachedImage::{entryId}::{imageId}"`.

4. **Create `ImageCacheOptions`** — add `Warp.WebApp/Models/Options/ImageCacheOptions.cs` with a single property: `long MaxCachableFileSize` (with `[Required]` and `[Range]` attributes, matching the existing options style). Default: 5MB (5,242,880 bytes). Images larger than this skip the cache and are served from S3 + the existing output cache.

5. **Register `ImageCacheOptions` in config** — in `ServiceCollectionExtensions.AddOptions`, add binding for `ImageCacheOptions` from configuration, with `ValidateDataAnnotations().ValidateOnStart()`.

6. **Add config section to appsettings** — add an `"ImageCacheOptions"` section with `"MaxCachableFileSize": 5242880` to `appsettings.Local.json`, `appsettings.E2E.json`, and `appsettings.E2ELocal.json`.

7. **Modify `ImageService` constructor** — add two new dependencies: `IDistributedStore distributedStore` (for KeyDB-only caching, bypassing the two-tier `DataStorage` to avoid in-memory L1 pressure) and `IOptions<ImageCacheOptions> cacheOptions`. Store as private fields `_distributedStore` and `_cacheOptions`.

8. **Add private cache helper methods to `ImageService`** — add three private methods:
   - `TryGetFromCache(Guid entryId, Guid imageId, CancellationToken)` → `Task<CachedImage?>` — calls `_distributedStore.TryGet<CachedImage>` with the cache key.
   - `SetInCache(Guid entryId, Guid imageId, byte[] content, string contentType, TimeSpan expiresIn, CancellationToken)` → `Task` — checks `content.Length <= _cacheOptions.MaxCachableFileSize`, if so calls `_distributedStore.Set<CachedImage>(...)` with the given TTL. Silently skips if over threshold.
   - `RemoveFromCache(Guid entryId, Guid imageId, CancellationToken)` → `Task` — calls `_distributedStore.Remove<CachedImage>(...)`.

9. **Modify `ImageService.Get`** — before calling `_s3FileStorage.Get`, call `TryGetFromCache`. On cache hit, wrap `CachedImage.Content` in a `MemoryStream` and return an `Image`. On cache miss, fetch from S3 as now. After building the `Image`, read the stream content into a `byte[]`, then call `SetInCache` with a TTL derived from the entry's remaining lifetime. To get the TTL: look up `EntryInfo` via `_dataStorage.TryGet<EntryInfo?>` using `CacheKeyBuilder.BuildEntryInfoCacheKey`, compute `ExpiresAt - DateTimeOffset.UtcNow`. If entry lookup fails, fall back to `CachingConstants.MaxSupportedCachingTime`. Reset stream position after reading bytes so the caller still gets a usable stream.

10. **Modify `ImageService.Remove`** — add a call to `RemoveFromCache(entryId, imageId, cancellationToken)` alongside the existing hash cleanup.

11. **Add `CacheImages` method to `ImageService`** — a new public method: `Task CacheImages(Guid entryId, List<Guid> imageIds, TimeSpan expiresIn, CancellationToken)`. For each image ID, fetch from S3 via `_s3FileStorage.Get(...)`, convert the stream to `byte[]`, call `SetInCache(...)`. This is the bulk-cache path called at entry save time.

12. **Extend `IImageService` interface** — in `IImageService.cs`, add `Task CacheImages(Guid entryId, List<Guid> imageIds, TimeSpan expiresIn, CancellationToken)` with XML documentation.

13. **Modify `EntryInfoService.Add` to trigger caching** — add a `.Tap(CacheEntryImages)` step after `CacheEntryInfo` and before `TrackEntryLifecycle`:
    ```
    .Tap(CacheEntryInfo)
    .Tap(CacheEntryImages)   // ← new
    .Tap(TrackEntryLifecycle)
    ```
    The local function extracts image IDs from `entryInfo.ImageInfos` and calls `_imageService.CacheImages(entryInfoId, imageIds, entryRequest.ExpiresIn, cancellationToken)`.

14. **No changes needed for `Copy` and `Update`** — both flows ultimately call `EntryInfoService.Add`, which now includes the `CacheEntryImages` step. Copied and updated entries get their images cached automatically.

## Verification

- **Unit tests**: Test `ImageService.Get` for cache hit (no S3 call), cache miss (S3 called + cache populated), and oversized images (S3 called, cache not populated). Test `ImageService.CacheImages` for bulk caching. Test `ImageService.Remove` for cache eviction.
- **E2E tests**: Existing `entry-crud.spec.ts` tests should pass unchanged — the API contract is unmodified.
- **Manual**: Create an entry with images, verify images load. Check Redis DB 7 for cached entries. Upload a >5MB image, confirm it's not in Redis DB 7 but still loads from S3. Delete an entry, confirm cache keys are removed.

## Decisions

- **No separate service**: Caching implemented as private methods inside `ImageService`, following the same pattern as the existing hash caching in that class.
- **KeyDB-only (no L1 memory)**: `IDistributedStore` injected directly, bypassing `DataStorage` two-tier cache. The existing ASP.NET output cache (10 min) handles hot HTTP-level caching.
- **5MB size threshold**: Configurable via `ImageCacheOptions.MaxCachableFileSize`. Images above this skip caching to protect KeyDB memory. Base64 + encryption overhead on a 5MB image ≈ ~7MB in KeyDB — acceptable. 50MB images would become ~70MB — not acceptable.
- **Cache at entry save time**: `CacheImages` is called in `EntryInfoService.Add` with the exact entry TTL, ensuring consistent expiry.
- **Cache-aside fallback in `Get`**: If an image is missing from cache, it's re-fetched from S3 and re-cached with the remaining TTL.
- **Redis DB 7**: Isolates cached image binaries from other data types.
