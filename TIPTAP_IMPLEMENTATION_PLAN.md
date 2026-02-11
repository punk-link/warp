# Plan: Tiptap Rich Text Editor for Advanced Mode

Integrate `@tiptap/vue-3` (ProseMirror-based, actively maintained, Vue 3-native) as the text editor in Advanced mode. Store both ProseMirror JSON (`contentDelta` — lossless for re-editing) and sanitized HTML (`textContent` — for display). Simple mode stays completely unchanged. Backend adds `Ganss.Xss.HtmlSanitizer` for server-side HTML sanitization and a histogram metric for content-size monitoring.

**Key difference vs Quill:** Tiptap is headless/unstyled and schema-constrained — ProseMirror's document model only emits HTML for nodes/marks you explicitly register, giving implicit output sanitization. Combined with server-side `HtmlSanitizer` and client-side DOMPurify, this is defense-in-depth at three layers.

## Frontend

### 1. Install dependencies
Add to [Warp.ClientApp/package.json](Warp.ClientApp/package.json):
- `@tiptap/vue-3` — Vue 3 integration + core re-exports
- `@tiptap/pm` — ProseMirror peer dependency
- `@tiptap/starter-kit` — bundles bold, italic, underline, strike, headings, blockquote, code block, bullet/ordered lists, hard break, undo/redo
- `@tiptap/extension-link` — link mark (not included in StarterKit)
- `@tiptap/html` — `generateHTML()` / `generateJSON()` utilities for offline JSON↔HTML conversion
- `dompurify` + `@types/dompurify` — client-side HTML sanitization for `v-html` defense-in-depth

### 2. Create RichTextEditor.vue component
New file in [Warp.ClientApp/src/components/RichTextEditor.vue](Warp.ClientApp/src/components/RichTextEditor.vue):

- Use the `useEditor` composable from `@tiptap/vue-3` with `EditorContent` render component.
- Configure extensions: `StarterKit.configure({ heading: { levels: [1, 2, 3] } })` and `Link.configure({ openOnClick: false, HTMLAttributes: { rel: 'noopener noreferrer nofollow' } })`.
- Props: `modelValue` (ProseMirror JSON string), `editable` (boolean, default `true`), `placeholder` (string).
- Emits: `update:modelValue` (JSON string), `update:html` (HTML string), `update:textLength` (number — via `editor.getText().length` for validation).
- In `onUpdate` callback: emit JSON via `editor.getJSON()`, HTML via `editor.getHTML()`, and text length via `editor.getText().length`.
- Watch `modelValue` for external changes (draft restore), calling `editor.commands.setContent(parsed, false)` only when content diverges.
- Watch `editable` prop, calling `editor.setEditable(value)` for toggling read-only.
- Build a toolbar component (or inline toolbar div) with buttons wired to Tiptap commands:
  - `editor.chain().focus().toggleBold().run()`
  - `.toggleItalic()`
  - `.toggleUnderline()`
  - `.toggleStrike()`
  - `.toggleHeading({ level })`
  - `.toggleBlockquote()`
  - `.toggleCodeBlock()`
  - `.toggleBulletList()`
  - `.toggleOrderedList()`
  - `.toggleLink({ href })`
  - Use `editor.isActive('bold')` etc. for active-state styling.
- Apply Tailwind utility classes to the editor container via `editorProps.attributes.class`. Use the existing design system's font, color, and spacing tokens. Style the `.tiptap` wrapper with min-height, border, focus ring. Style content elements (headings, lists, blockquotes, code) with scoped CSS or `@tailwindcss/typography` `prose` classes.

### 3. Create sanitizeHtml.ts utility
New file in [Warp.ClientApp/src/helpers/sanitizeHtml.ts](Warp.ClientApp/src/helpers/sanitizeHtml.ts).

Wrap DOMPurify with an allow-list matching the Tiptap extensions:
- Tags: `b, i, u, s, em, strong, h1, h2, h3, p, br, ol, ul, li, a, blockquote, pre, code, span`
- Attributes: `href` on `a` only, restricted to `http:`, `https:`, `mailto:` schemes
- Export a single `sanitize(html: string): string` function.

### 4. Update HomeView.vue
In [Warp.ClientApp/src/views/HomeView.vue](Warp.ClientApp/src/views/HomeView.vue):

- Add a `contentDelta` ref (string, ProseMirror JSON) and a `richTextLength` ref (number) alongside the existing `text` ref.
- When `mode === EditMode.Advanced`: render `RichTextEditor` with `v-model="contentDelta"`, listen to `@update:html` to sync into `text` (now HTML), and `@update:textLength` to update `richTextLength`.
- When `mode === EditMode.Simple`: render the existing `DynamicTextArea` with `v-model="text"` — no changes.
- Update `isValid`: for Advanced mode use `richTextLength > 0 || galleryCount > 0` instead of `text.trim().length > 0`.

### 5. Update types
In [Warp.ClientApp/src/types/](Warp.ClientApp/src/types/):

- Add `contentDelta?: string` to `DraftEntry`, `Entry`, and `EntryAddOrUpdateRequest` interfaces.
- `textContent` remains: it carries plain text in Simple mode and sanitized HTML in Advanced mode.

### 6. Update use-draft-entry.ts
In [Warp.ClientApp/src/composables/use-draft-entry.ts](Warp.ClientApp/src/composables/use-draft-entry.ts):

Persist `contentDelta` in session storage alongside `textContent`. On restore, populate both `text` (HTML) and `contentDelta` (JSON) refs.

### 7. Update entry-api.ts
In [Warp.ClientApp/src/api/entry-api.ts](Warp.ClientApp/src/api/entry-api.ts):

Append `contentDelta` as an additional `FormData` field in `addOrUpdateEntry()`.

### 8. Update PreviewView.vue
In [Warp.ClientApp/src/views/PreviewView.vue](Warp.ClientApp/src/views/PreviewView.vue):

- For Advanced mode: render a `RichTextEditor` with `:editable="false"` populated from `contentDelta`, giving a WYSIWYG preview without a second rendering path. Alternatively, use `generateHTML()` from `@tiptap/html` and render via `v-html` with DOMPurify — lighter weight but requires importing the extensions array.
- For Simple mode: keep existing `{{ text }}` display unchanged.

### 9. Update EntryView.vue
In [Warp.ClientApp/src/views/EntryView.vue](Warp.ClientApp/src/views/EntryView.vue):

- For Advanced mode entries (`entry.editMode === EditMode.Advanced`): render `entry.textContent` (backend-sanitized HTML) via `v-html` after running through the `sanitizeHtml` helper (DOMPurify) as defense-in-depth.
- For Simple mode entries: keep existing `{{ entry?.textContent }}` interpolation unchanged.
- Apply the same Tiptap-compatible content styling (scoped CSS or `prose` classes) so headings, lists, blockquotes, and code blocks render consistently between editor and viewer.

### 10. Tiptap content styling
Create a shared stylesheet or Tailwind `@apply` block under [Warp.ClientApp/src/styles/](Warp.ClientApp/src/styles/) that styles rendered rich-text content elements (`h1`–`h3`, `ul`, `ol`, `blockquote`, `pre > code`, `a`, etc.). Import this in both `RichTextEditor.vue` and `EntryView.vue` to ensure visual consistency between editing and viewing.

## Backend

### 11. Add HtmlSanitizer NuGet package
Add `Ganss.Xss.HtmlSanitizer` to [Warp.WebApp/Warp.WebApp.csproj](Warp.WebApp/Warp.WebApp.csproj).

### 12. Create IHtmlSanitizationService / HtmlSanitizationService
New service in [Warp.WebApp/Services/](Warp.WebApp/Services/):

- Configure `HtmlSanitizer` allow-list to match the Tiptap extensions' output: tags `b, i, u, s, em, strong, h1, h2, h3, p, br, ol, ul, li, a, blockquote, pre, code, span`; attribute `href` on `a` only (schemes: `http`, `https`, `mailto`). Strip all other tags, attributes, event handlers, and `style` attributes.
- Single public method: `string Sanitize(string html)`.
- Register as singleton in DI via a service-collection extension method following the existing pattern in [Warp.WebApp/Extensions/](Warp.WebApp/Extensions/).

### 13. Update Entry model
In [Warp.WebApp/Models/Entries/Entry.cs](Warp.WebApp/Models/Entries/Entry.cs):

Add a nullable `ContentDelta` string property to store ProseMirror JSON alongside the existing `Content` (sanitized HTML for Advanced, normalized plain text for Simple).

### 14. Update request/response models and converters
- Add `ContentDelta` to [EntryApiRequest.cs](Warp.WebApp/Models/Entries/EntryApiRequest.cs), [EntryApiResponse.cs](Warp.WebApp/Models/Entries/EntryApiResponse.cs), and [EntryRequest.cs](Warp.WebApp/Models/Entries/EntryRequest.cs).
- Update [EntryApiRequestConverters.cs](Warp.WebApp/Models/Entries/Converters/EntryApiRequestConverters.cs) to map `ContentDelta` through.
- Update [EntryInfoConverters.cs](Warp.WebApp/Models/Entries/Converters/EntryInfoConverters.cs) to include `ContentDelta` in the API response.

### 15. Update EntryController
In [Warp.WebApp/Controllers/EntryController.cs](Warp.WebApp/Controllers/EntryController.cs):

Parse `contentDelta` from multipart form sections alongside the existing fields, populate it in `EntryApiRequest`.

### 16. Update EntryService
In [Warp.WebApp/Services/Entries/EntryService.cs](Warp.WebApp/Services/Entries/EntryService.cs):

- Inject `IHtmlSanitizationService`.
- For Advanced mode (`EditMode.Advanced`): sanitize `TextContent` (HTML) via `IHtmlSanitizationService.Sanitize()`, pass `ContentDelta` (opaque JSON, not rendered) through as-is.
- For Simple mode: keep the existing `TextFormatter.NormalizeForMarkdownSource()` path, set `ContentDelta = null`.
- Create `Entry` with both `Content` and `ContentDelta`.

### 17. Rename / guard TextFormatter
Rename `NormalizeForMarkdownSource` to `NormalizePlainText` (or add a guard) to make it clear it should only process plain text. Ensure Advanced mode HTML is **not** run through this method (it would strip formatting-significant whitespace and add newlines that mangle HTML).

### 18. Update validators
In [EntryValidator.cs](Warp.WebApp/Models/Validators/EntryValidator.cs):

For Advanced mode, add a rule that strips HTML tags from `Content` and checks for non-whitespace content, so entries with only empty tags (e.g., `<p><br></p>`) are rejected. Use a simple regex or `HtmlSanitizer`'s text extraction.

### 19. Verify OpenGraph
In [OpenGraphService.cs](Warp.WebApp/Services/OpenGraph/OpenGraphService.cs):

The existing `StripHtmlAndNormalizeWhitespace()` regex should handle Tiptap's HTML output for generating plain-text OG descriptions. Verify it produces clean output with the new tag set (headings, lists, code blocks).

## Telemetry

### 20. Add content-size histogram
In [Warp.WebApp/Telemetry/Metrics/ApplicationMetrics.cs](Warp.WebApp/Telemetry/Metrics/ApplicationMetrics.cs):

- Add a `Histogram<int>` named `entry_content_size_chars` (unit: `"characters"`).
- Tag with `entry_info.edit_mode` (`simple` / `advanced`).

### 21. Record metric
In `EntryInfoService` (on `Add` and `Update`):

Measure the plain-text character count of the content (strip HTML for Advanced mode) and record it to the histogram. This gives percentiles, max, average, and count for dashboarding.

### 22. Add constants
Add new metric name and tag values to [Warp.WebApp/Telemetry/Metrics/](Warp.WebApp/Telemetry/Metrics/) following the existing `EntryInfoMetricNames` / `EntryInfoMetricActions` pattern.

## Security

### 23. Add CSP headers
Add `Content-Security-Policy` middleware in [Warp.WebApp/Extensions/WebApplicationExtensions.cs](Warp.WebApp/Extensions/WebApplicationExtensions.cs):

```
default-src 'self'; 
script-src 'self'; 
style-src 'self' 'unsafe-inline'; 
img-src 'self' data:
```

The `unsafe-inline` for styles is needed because Tiptap / ProseMirror may inject minimal structural inline styles. This is strongly recommended alongside introducing `v-html`.

## Tests

### 24. Backend unit tests
In [Warp.WebApp.Tests/UnitTests/](Warp.WebApp.Tests/UnitTests/):

- `HtmlSanitizationServiceTests`: allowed tags pass through; blocked tags/attributes stripped; `javascript:` hrefs removed; `<script>` tags removed; empty content detection.
- Update `EntryServiceTests`: Advanced mode with HTML content is sanitized before storage.
- Update `EntryValidatorTests`: Advanced mode rejects empty-HTML-only content (e.g., `<p><br></p>`).

### 25. Frontend unit tests
In [Warp.ClientApp/src/__tests__/](Warp.ClientApp/src/__tests__/):

- `sanitizeHtml.spec.ts`: verify DOMPurify wrapper allow-list behavior.
- `RichTextEditor.spec.ts`: verify `useEditor` lifecycle, emitted events (JSON, HTML, textLength), read-only mode.

### 26. E2E tests
Update [entry-crud.spec.ts](Warp.ClientApp/e2e/entry-crud.spec.ts) and [entry-misc.spec.ts](Warp.ClientApp/e2e/entry-misc.spec.ts):

Create an Advanced mode entry with formatted text (bold, heading, list), save, verify formatted HTML renders correctly in EntryView.

## Verification

- `dotnet test Warp.WebApp.Tests/` — all backend tests pass including new sanitization and validation tests.
- `cd Warp.ClientApp && npm run test:unit` — frontend unit tests pass.
- `cd Warp.ClientApp && npm run test:e2e` — E2E tests cover rich text round-trip.
- Manual: create Advanced entry → type formatted text with toolbar → Preview → Save → open link → verify formatting renders, no raw HTML visible, no script execution possible.
- Telemetry: create entries of varying sizes, confirm `entry_content_size_chars` histogram in OTLP output with correct `edit_mode` tags.

## Key Decisions

- **Tiptap over Quill**: Quill appears abandoned and has known CWEs; Tiptap is actively maintained (v3.x), has first-class Vue 3 support, full TypeScript types, and ProseMirror's schema-constrained output model.
- **StarterKit + Link**: reduces dependency management, covers all needed features except Link (added separately).
- **Headless Tiptap + Tailwind styling**: matches the existing design system, avoids theme CSS conflicts.
- **Ganss.Xss.HtmlSanitizer**: purpose-built, widely used in .NET, allow-list API maps to Tiptap's output tags.
- **Three-layer sanitization**: ProseMirror schema constraint (implicit) → server-side `HtmlSanitizer` → client-side DOMPurify on `v-html` render.
- **Dual storage (ProseMirror JSON + sanitized HTML)**: JSON enables lossless re-editing, HTML avoids needing Tiptap/extensions loaded for every view page.
- **No hard content-size limit**: histogram telemetry enables data-driven decisions.
- **Advanced mode only**: Simple mode remains a plain textarea, unchanged.
