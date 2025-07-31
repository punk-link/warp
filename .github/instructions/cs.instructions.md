---
applyTo: '**/*.cs'
---

# Code Style

- Limit line length to a maximum of 160 characters.
- Insert exactly two blank lines between constructors, methods, and delegates to improve readability.
- Insert one blank line between properties for clear separation.
- Do not include blank lines between private fields when grouping related fields.
- Place all private fields at the end of the file, after public members and methods.
- Use named parameters for clarity, especially when creating new objects (e.g., `id: Guid.NewGuid()`).
- Ensure parameter names in method calls match the method's parameter declarations.
- Remove any unused using directives to keep files clean and efficient.
- Always use `DateTime.UtcNow` instead of `DateTime.Now` for time-zone independence.
- Omit braces for single-line if statements or loops, but always place the condition and body on separate lines (e.g., `if (condition) \n    DoSomething();`).
- Use braces for multi-line if statements when at least one branch contains multiple lines of code.
- Add a blank line after if statements or loops when they are not immediately followed by the end of a function.


# Class Member Ordering

Follow this specific order for members within a class:
1. Public constructors
2. Public properties and events
3. Public methods (including interface implementations with `<inheritdoc/>`)
4. Protected properties and methods
5. Private properties
6. Private methods 
7. Private fields (at the end of the file)

Use XML documentation comments before each public member and class definition. Keep related members together when appropriate.


# Code Annotations And Comments

- Use XML documentation for all public types and members.
- Add XML documentation to classes, using <inheritdoc/> for interface implementation methods and standard XML documentation for the class itself and any members not defined in the interface.
- Only add comments to public members, never to private ones.
- Do not add documentation comments to test classes and test methods.
- Include comments in code only to explain complex behavior or non-obvious implementations.