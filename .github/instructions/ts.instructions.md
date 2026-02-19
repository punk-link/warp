---
applyTo: '**/*.ts,**/*.vue'
---

# Warp ClientApp TypeScript + Vue Guidelines

This document refines general repository standards for `.ts` and `.vue` files under `Warp.ClientApp`. It complements the root `.github/copilot-instructions.md`.

## Architecture Overview

- SPA mounted at `/app` (router base configured in [Warp.ClientApp/src/router/index.ts](Warp.ClientApp/src/router/index.ts)).
- API communication via lightweight wrappers (see [`fetchJson`](Warp.ClientApp/src/api/fetchHelper.ts) and entry CRUD helpers in [`createEntry`](Warp.ClientApp/src/api/entryApi.ts), [`getEntry`](Warp.ClientApp/src/api/entryApi.ts), [`updateEntry`](Warp.ClientApp/src/api/entryApi.ts), [`deleteEntry`](Warp.ClientApp/src/api/entryApi.ts)).
- Stateful logic encapsulated in composables (`use*` patterns), e.g. [`useDraftEntry`](Warp.ClientApp/src/composables/use-draft-entry.ts), [`useMetadata`](Warp.ClientApp/src/composables/use-metadata.ts), [`useImageUpload`](Warp.ClientApp/src/composables/use-image-upload.ts), [`useGallery`](Warp.ClientApp/src/composables/use-gallery.ts), [`useEditMode`](Warp.ClientApp/src/composables/use-edit-mode.ts).
- Enum‐backed UX (e.g. Expiration + Edit modes) resolved lazily via metadata APIs (see usage in Home + Preview views).

## File & Component Conventions

1. Prefer `<script setup lang="ts">` in Vue SFCs (as in existing components like [Warp.ClientApp/src/components/ModeSwitcher.vue](Warp.ClientApp/src/components/ModeSwitcher.vue)).
2. Keep template minimal; move logic to composables when:
   - Reused in ≥2 views OR
   - Exceeds ~60 lines of logic OR
   - Encapsulates async workflows (e.g. form submission, persistence).
3. Component props:
   - Strongly type via `interface Props`.
   - Provide defaults through `withDefaults`.
   - Use `modelValue` + `update:modelValue` for two‑way binding (see [Warp.ClientApp/src/components/ExpirationSelect.vue](Warp.ClientApp/src/components/ExpirationSelect.vue)).
4. Emitted events must be documented inline via `defineEmits` typing and named in kebab case if used outside (internal only events can stay camel if not exposed).
5. Avoid deep prop drilling—promote cross‑cutting state (draft, metadata) to composables.
6. Template indentation: Use 4-space indentation inside `<template>` (consistent with TypeScript), starting from the root element.

## TypeScript Standards

- `strict` mode is enabled (see [Warp.ClientApp/tsconfig.json](Warp.ClientApp/tsconfig.json)); fix implicit `any`.
- Always narrow external data:
  - Use explicit runtime guards or mapping (pattern in [`convertToArray`](Warp.ClientApp/src/composables/use-metadata.ts)).
- Enumerations:
  - Use string enums for server parity (e.g. `ExpirationPeriod`, `EditMode`, `ApiResultKind`).
  - Keep enums in dedicated files under `types/<domain>/enums/` directories (e.g. `types/entries/enums/edit-modes.ts`).
  - When persisting to storage, coerce to `string` (see localStorage usage in [`useEditMode`](Warp.ClientApp/src/composables/use-edit-mode.ts)).
- Prefer `const` + `ref<T>` pattern; avoid mutable arrays without cloning when emitting or persisting (clone via spread before reassigning as in Home view file handling).
- Error handling:
  - Wrap network calls in `try/finally` to clear pending state.
  - Log with `console.error` only; user‑facing messaging belongs in the UI, not the composable.

### Type Organization

Types should be organized in a hierarchical folder structure under `src/types/`:

- **Domain-based grouping**: Group related types into domain folders (e.g. `apis/`, `entries/`, `galleries/`, `notifications/`, `telemetry/`).
- **Enums in subfolder**: Place enums in an `enums/` subfolder within each domain (e.g. `types/apis/enums/api-result-kind.ts`).
- **One type per file**: Each interface, type alias, or enum should be in its own file, named with kebab-case matching the type name (e.g. `api-success-result.ts` for `ApiSuccessResult`).
- **Union types**: Create separate files for each constituent type and a union file that imports and re-exports them (e.g. `gallery-item.ts` unions `LocalGalleryItem` and `RemoteGalleryItem`).
- **JSDoc on all exported types**: Add JSDoc comments to all exported interfaces, types, and enums describing their purpose.

### File & Function Naming

The following naming conventions are enforced:

1. **File names**: Use kebab-case for all TypeScript files (e.g. `use-draft-entry.ts`, `api-result-kind.ts`, `problem-details-helper.ts`).
2. **Composable files**: Name composables with `use-` prefix in kebab-case (e.g. `use-gallery.ts`, `use-notifications.ts`).
3. **Helper files**: Name helper/utility files with `-helper` suffix (e.g. `edit-mode-helper.ts`, `expiration-period-helper.ts`).
4. **Enum files**: Name enum files matching the enum name in kebab-case (e.g. `edit-modes.ts` for `EditMode`, `api-result-kind.ts` for `ApiResultKind`).
5. **Function names**: Use camelCase; exported public functions should have JSDoc documentation.
6. **View names**: Use the `ViewNames` enum from `src/router/enums/view-names.ts` for router navigation instead of string literals.

### Formatting Addendum

The following formatting rules are enforced in addition to repository defaults:

1. Multi‑line object literal returns: Any returned object with more than one property ("complex object") must be written multi‑line with one property per line, trailing comma optional per existing style. Never inline these.

```ts
// ✅ Correct
return {
    ok: true,
    kind: ApiResultKind.Success,
    data
}

// ❌ Avoid
return { ok: true, kind: ApiResultKind.Success, data }
```

2. Single statement control structures: For `if` / `for` (and `while`) containing a single statement without braces, place the statement on the next line (never on the same line as the condition). Braces remain required when the body spans multiple statements.

```ts
// ✅ Correct (single statement without braces)
if (kind === ApiResultKind.NotFound)
    return true

// ❌ Avoid (inline single statement)
if (kind === ApiResultKind.NotFound) return true
```

3. Vertical spacing: Use two blank lines between top‑level constructs (imports block vs next declaration, standalone functions, classes, interfaces, type aliases). Within a class or module, one blank line between logically distinct groups (properties, constructors, methods) is acceptable, but preserve two blank lines between separate top‑level exported items.

4. File layout order (for TypeScript files):
   - Imports (grouped: external packages first, then internal with blank line)
   - Two blank lines after imports
   - Exported items (interfaces, types, functions, classes)
   - Private module-level variables and constants at the end
   - Private/internal helper functions at the end

```ts
// ✅ Correct file layout
import { ref, computed } from 'vue'
import type { SomeType } from '../types/some-type'


/** Public composable or function. */
export function useExample() {
    // ...
}


function privateHelper() {
    // ...
}


const PRIVATE_CONSTANT = 'value'
```

5. Import statements: Prefer `import type` for type-only imports. Group imports logically:
   - External package imports first
   - Internal imports after (blank line between groups)
   - Relative imports should use consistent path style

```ts
// ✅ Correct import organization
import { ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'

import { fetchJson } from '../api/fetchHelper'
import type { ApiResult } from '../types/apis/api-result'
import { ApiResultKind } from '../types/apis/enums/api-result-kind'
```

These rules supplement (do not override) any automatic formatter; adjust formatter configuration if it conflicts.

## Async & Network

- All API calls must use `credentials: 'include'` (handled centrally in [`fetchJson`](Warp.ClientApp/src/api/fetchHelper.ts); replicate for raw `fetch` when sending `FormData` as in [`uploadImages`](Warp.ClientApp/src/composables/use-image-upload.ts)).
- Encode dynamic URL segments via `encodeURIComponent`.
- For multipart:
  - Append only non‑empty values (pattern in form building).
- Handle non‑OK responses:
  - Attempt JSON parse if `content-type` is JSON; else fallback to text (already implemented in `fetchJson`).
- Use the `ApiResult<T>` discriminated union type for API response handling, with explicit checks for `result.ok` and `result.kind`.


## Rich Text Editor (Tiptap)

The RichTextEditor component ([Warp.ClientApp/src/components/RichTextEditor.vue](Warp.ClientApp/src/components/RichTextEditor.vue)) wraps Tiptap for Advanced mode editing:

### Component Usage

```vue
<RichTextEditor
    v-model="contentDelta"
    :editable="!isReadOnly"
    :placeholder="t('create.contentPlaceholder')"
    @update:html="html = $event"
    @update:textLength="textLength = $event"
/>
```

### Props and Events

- **Props:**
  - `modelValue` (string): ProseMirror JSON string (required)
  - `editable` (boolean): Enable/disable editing (default: true)
  - `placeholder` (string): Placeholder text when empty

- **Events:**
  - `update:modelValue`: Emits ProseMirror JSON string (for persistence)
  - `update:html`: Emits sanitized HTML (for preview/rendering)
  - `update:textLength`: Emits plain text character count (for validation)

### HTML Sanitization

All HTML content must be sanitized before rendering with `v-html`:

```ts
import { sanitize } from '../helpers/sanitize-html'

const safeHtml = sanitize(rawHtml) // Uses DOMPurify with strict allow-list
```

**Never render unsanitized HTML** — always use the `sanitize()` helper which enforces:
- Allow-list of safe tags (p, strong, em, u, s, h1-h3, ul, ol, li, a, blockquote, pre, code, br)
- Removal of dangerous attributes (onclick, onerror, style, etc.)
- Removal of javascript: hrefs
- Removal of all script tags

### Content Storage Pattern

When persisting Advanced mode entries:

```ts
const contentDelta = ref('')  // ProseMirror JSON from editor modelValue
const contentHtml = ref('')   // Sanitized HTML from update:html event

// Send both to backend:
formData.append('contentDelta', contentDelta.value)  // For re-editing
formData.append('textContent', contentHtml.value)    // For display
```

### Testing Rich Text Components

- Use `@testing-library/vue`'s `render()` and `waitFor()` for component tests
- Test toolbar button presence and disabled state
- Avoid testing Tiptap internals (focus on props/events/rendered output)
- Use Playwright for E2E testing of actual editing workflows

## State & Persistence

- Draft persistence:
  - Use sessionStorage for transient editable content (see [`useDraftEntry`](Warp.ClientApp/src/composables/use-draft-entry.ts)).
  - Clear on successful save (`clearDraft()` in Preview workflow).
- Local preferences (mode) belong in localStorage (see [`useEditMode`](Warp.ClientApp/src/composables/use-edit-mode.ts)).
- Reactive collections (files, gallery items) must revoke object URLs on unmount (pattern in Preview view `cleanupObjectUrls` and composable [`useGallery`](Warp.ClientApp/src/composables/use-gallery.ts)).

## Forms & Validation

- Do not submit empty entries: validity = text trimmed OR files length > 0 (see `isValid` computed in Home).
- Keep `pending` / `saving` / `deleting` flags distinct to prevent accidental UI enablement.
- For updates vs create:
  - Detect existing ID via query param or draft (see `initiateStateFromServer` + `hydrateStateFromDraft` in Home).
  - Use same FormData shape; server distinguishes by HTTP verb (POST vs PUT).

## Images

- Client filters images by MIME `image/*` (see `accept="image/*"` and file filtering in [`isValidImageFile`](Warp.ClientApp/src/composables/use-image-upload.ts)).
- After upload, dispatch global `uploadFinished` event (`UPLOAD_FINISHED_EVENT`) for any future listeners (e.g., gallery refresh hooks).

## Expiration & Edit Mode Handling

- Load once per session (cached arrays in [`useMetadata`](Warp.ClientApp/src/composables/use-metadata.ts)).
- Always coerce legacy or stringified numbers (see `coerceExpiration` in Home).
- When persisting draft or saving entry, store string enum values.
- Use dedicated helper functions for parsing enum values from external sources:
  - `parseEditMode()` in `helpers/edit-mode-helper.ts`
  - `parseExpirationPeriod()` in `helpers/expiration-period-helper.ts`

## Accessibility & Semantics

- Provide `aria-label` when visual label differs or is conditional (`DynamicTextArea`, `ExpirationSelect`).
- Buttons that only display an icon must include a `title` attribute.

## Styling Integration

- Tailwind utility classes; follow existing patterns (e.g., button variants from global CSS).
- Avoid inline styles unless dynamic runtime sizing (acceptable for textarea auto-resize logic in [`DynamicTextArea`](Warp.ClientApp/src/components/DynamicTextArea.vue)).

## Testing

- Unit tests:
  - Place beside source or under `src/__tests__`.
  - Use Testing Library queries (`findByRole`, etc.)—see existing smoke test [Warp.ClientApp/src/__tests__/App.spec.ts](Warp.ClientApp/src/__tests__/App.spec.ts).
- E2E:
  - Keep selectors semantic; prefer roles / text. Avoid brittle `[data-...]` unless necessary.
  - Do not use mocks for e2e tests; test against real API.

## Performance & Cleanup

- Revoke object URLs and detach global listeners in `onBeforeUnmount`.
- Debounce expensive operations if added later (typing analytics, large paste handling) via lightweight utility functions (add a `utils` module if needed).
- Avoid unnecessary reactive duplication—derive computed values instead of storing both raw & derived.

## Error Views & Navigation

- On irrecoverable initialization errors in Home (e.g., creator bootstrap failure), route to Error view (`router.replace({ name: ViewNames.Error })` pattern already present).
- Keep query `id` synchronized after initial entry creation (see `router.replace({ query: { ...route.query, id } })` patterns in Home and Preview).
- Use the `ViewNames` enum for all router navigation instead of string literals (import from `src/router/enums/view-names.ts`).

## New Code Checklist (TS/Vue)

Before submitting:
- Types strict & no `any` unless justified with a comment explaining external/untyped API boundary.
- All dynamic user text output escaped by Vue (avoid `v-html` unless sanitized; current privacy page intentionally uses static HTML load isolated).
- No unused imports (ESLint will warn).
- Line length ≤ 160 chars.
- Network calls use helper or consistent error handling.
- Composables return only the minimal surface needed by consumers.

## Anti-Patterns (Avoid)

- Storing raw DOM nodes outside refs (except ephemeral in event handlers).
- Mixing concern layers: do not call router directly inside utility/data composables; keep navigation in views.
- Silent failures without at least a `console.error` in catch blocks for async critical paths (loading metadata, saving entries).
- Duplicating logic already encapsulated in existing composables.

## When Adding a New Composable

Template:

```ts
// Example composable skeleton (file: use-something.ts)
import { ref, onBeforeUnmount } from 'vue'


/** Composable for managing something. */
export function useSomething() {
    const value = ref<string | null>(null)

    async function load() {
        // network via fetchJson(...)
    }

    onBeforeUnmount(() => {
        // cleanup if needed
    })

    return { value, load }
}
```

Follow patterns from [`useMetadata`](Warp.ClientApp/src/composables/use-metadata.ts) and [`useGallery`](Warp.ClientApp/src/composables/use-gallery.ts) for async state.

## When Adding New Types

Template for a new domain type:

```ts
// File: types/<domain>/<type-name>.ts
import { SomeEnum } from './enums/some-enum'


/** Describes the purpose of this type. */
export interface SomeType {
    id: string
    status: SomeEnum
    // ...
}
```

Template for a new enum:

```ts
// File: types/<domain>/enums/<enum-name>.ts

/** Describes the purpose of this enum. */
export enum SomeEnum {
    Value1 = 'Value1',
    Value2 = 'Value2'
}
```

---

Adhere to these to ensure consistency, maintainability, and seamless integration with existing server APIs and UI paradigms.