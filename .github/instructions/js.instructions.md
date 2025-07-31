---
applyTo: '**/*.js'
---

# Code Style

- Limit line length to a maximum of 160 characters.
- Insert exactly two blank lines between constructors and functions to improve readability.
- Do not include blank lines between private fields when grouping related fields.
- Place all private fields at the start of the file, before functions and constructor definitions.
- Remove any unused imports to keep files clean and efficient.
- Omit braces for single-line if statements or loops, but always place the condition and body on separate lines (e.g., `if (condition) \n    doSomething();`).
- Use braces for multi-line if statements when at least one branch contains multiple lines of code.
- Add a blank line after if statements or loops when they are not immediately followed by the end of a function.


# Code Annotations And Comments

- Use JSDoc documentation for all public functions and methods.
- Do not add JSDoc annotations to private members or variables.
- Do not add JSDoc annotations to exported functions or objects.
- Include comments in code only to explain complex behavior or non-obvious implementations.


# JavaScript Modular Architecture Rulebook for Warp Project

## Overview
This rulebook defines mandatory patterns and conventions for implementing modular, service-oriented JavaScript architecture in the Warp project. These rules ensure maintainability, testability, and consistent code quality across all frontend components.

## Mandatory Architecture Patterns

### 1. Page Controller Pattern
**RULE**: Every page MUST implement a single Page Controller class that orchestrates all functionality.

```js
// REQUIRED: Page-specific controller class
export class [PageName]PageController {
    constructor(elements) {
        this.elements = elements;
        // Initialize all services in constructor
        this.apiService = new [PageName]ApiService();
        this.uiService = new [PageName]UIService(elements);
        this.eventService = new [PageName]EventService();
        this.formService = new [PageName]FormService(elements, this.apiService.getApis());
    }

    async initialize() {
        // REQUIRED: Single entry point for all initialization
    }

    cleanup() {
        // REQUIRED: Resource cleanup method
    }
}
```


### 2. Service Naming Convention

**RULE**: All services MUST follow the `[ServiceType]Service` naming pattern. 
Page specific services MUST prefix with the page name to avoid conflicts: `[PageName][ServiceType]Service`.

**REQUIRED Naming Examples**:
- `IndexFormService` (not `FormService`)
- `IndexUIService` (not `UIService`) 
- `IndexApiService` (not `ApiService`)
- `IndexEventService` (not `EventService`)


### 3. Service Responsibilities (Single Responsibility Principle)

**RULE**: Each service MUST handle only one specific concern.

#### FormService Responsibilities:
- Form data collection and validation
- Form submission with retry logic
- Form-related error handling

#### UIService Responsibilities:
- DOM element initialization
- UI state management
- User feedback (errors, loading states)

#### EventService Responsibilities:
- Event listener management
- Event cleanup with AbortController
- Event listener tracking

#### ApiService Responsibilities:
- API call coordination
- Response formatting
- API error handling


### 4. Error Handling Rules

#### Fail-Fast DOM Elements
**RULE**: UI services MUST NOT check for element existence. Let JavaScript fail naturally.
```js
// FORBIDDEN:
if (!element) {
    throw new Error('Element not found');
}
```

```js
// FORBIDDEN:
if (!element) return;
```

```js
// REQUIRED: Direct usage
element.addEventListener('click', handler);
```

#### Consistent Error Response Format
**RULE**: All service methods MUST return consistent error response objects.
```js
// REQUIRED format for service responses:
return {
    success: boolean,
    data?: any,          // Only on success
    error?: Error,       // Only on failure
    userMessage?: string // User-friendly message on failure
};
```

#### Error Boundary Implementation
**RULE**: Controllers MUST implement try-catch blocks around service calls.
```js
try {
    const result = await this.service.someMethod();
    if (!result.success) {
        this.uiService.showError(result.userMessage);
        return;
    }
    // Handle success
} catch (error) {
    console.error('Unexpected error:', error);
    this.uiService.showError('An unexpected error occurred');
}
```

### 5. Resource Management Rules

#### Event Cleanup
**RULE**: All event services MUST use AbortController for automatic cleanup.
```js
export class [PageName]EventService {
    constructor() {
        this.abortController = new AbortController();
    }

    addEventListener(element, type, handler) {
        element.addEventListener(type, handler, {
            signal: this.abortController.signal
        });
    }

    cleanup() {
        this.abortController.abort();
    }
}
```

#### Page Controller Cleanup
**RULE**: Page controllers MUST implement cleanup on page unload.
```js
// REQUIRED in main page file:
window.addEventListener('beforeunload', () => {
    pageController?.cleanup();
});
```

### 6. Dependency Injection Rules

**RULE**: Services MUST accept dependencies through constructor parameters.
```js
// REQUIRED: Dependency injection in constructor
export class [PageName]FormService {
    constructor(elements, apis) {
        this.elements = elements;
        this.apis = apis;
    }
}
```

```js
// FORBIDDEN: Direct imports within service methods
import { someApi } from './some-api.js'; // Don't do this inside methods
```


### 7. Configuration Management

**RULE**: All magic numbers and configuration MUST be centralized in constants.


### 8. Async/Await Patterns

#### Retry Logic Implementation
**RULE**: API services MUST implement retry logic with exponential backoff.
```js
async submitWithRetry(data, maxAttempts = CONFIG.RETRY.MAX_ATTEMPTS) {
    let lastError;
    
    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        try {
            const response = await this.apis.someApi.submit(data);
            return { success: true, data: response };
        } catch (error) {
            lastError = error;
            if (attempt < maxAttempts) {
                await this._delay(CONFIG.RETRY.DELAY_MS * attempt);
            }
        }
    }
    
    return { 
        success: false, 
        error: lastError,
        userMessage: 'Operation failed. Please try again.'
    };
}
```

#### Loading State Management
**RULE**: Controllers MUST manage loading states during async operations.
```js
async _handleSubmit() {
    this.uiService.setLoadingState(true);
    try {
        // Async operations
    } finally {
        this.uiService.setLoadingState(false);
    }
}
```


### 9. Public API Preservation

**RULE**: Page refactoring MUST NOT break existing public APIs.
```js
// REQUIRED: Preserve existing global functions for Razor Pages
export const addIndexEvents = async () => {
    pageController = new IndexPageController(elements);
    await pageController.initialize();
};

window.addIndexEvents = addIndexEvents; // REQUIRED for Razor Pages
```


### 10. File Organization Rules

**RULE**: Each page MUST have the following file structure:
/features/[page-name]/
├── index.js                        // Main entry point
├── [page-name]-page-controller.js  // Main controller
├── [page-name]-form-service.js     // [PageName]FormService
├── [page-name]-ui-service.js       // [PageName]UIService  
├── [page-name]-event-service.js    // [PageName]EventService
├── [page-name]-api-service.js      // [PageName]ApiService
└── elements.js                     // DOM element accessors


### 11. Testing Requirements

**RULE**: Services MUST be designed for independent testing.
```js
// REQUIRED: Testable service design
const elements = mockElements();
const apis = mockApis();
const formService = new IndexFormService(elements, apis);

// Should be able to test in isolation
const result = formService.validateFormData(testData);
```


### 12. Documentation Standards

**RULE**: All service classes MUST include JSDoc documentation.
```js
/**
 * Service for handling form data collection and validation for the [page] page
 */
export class [PageName]FormService {
    /**
     * Collects form data from UI elements
     * @param {string} entryId - The entry
```