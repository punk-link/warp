# Code Style

- Take line lenght as 160 chars.
- Use two blank lines between constructors, methods, delegates.
- Use one blank line between properties.
- Use no blank lines between private fields.
- Place private fields at the end of the file.
- Use named parameters for clarity, especially when creating new objects(e.g., `id: Guid.NewGuid()`).
- Ensure correct parameter names are used.
- Ensure unused usings are removed.


# Code Annotatins And Comments

- Use XML documentation.
- Add XML documentation to this class, using <inheritdoc/> for interface implementation methods and standard XML documentation for the class itself and any members not defined in the interface.
- Only add comments to public members, not private ones.
- Do not comment tests and test classes.