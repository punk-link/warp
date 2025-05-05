# Project Overview

Warp is a service that allows users to share short-lived texts and media with friends and colleagues, ensuring data is automatically removed after a specified time. The solution is built on ASP.NET Core and uses KeyDB as storage for sensitive user data.

## Technologies
- ASP.NET Core for the web application
- KeyDB (Redis fork) for data storage
- Docker for containerization
- .NET code generators for logging constants, domain errors and messages
- Tailwind CSS and Sass for styling

# Code Style

- Take line lenght as 160 chars.
- Use two blank lines between constructors, methods, delegates.
- Use one blank line between properties.
- Use no blank lines between private fields.
- Place private fields at the end of the file.
- Use named parameters for clarity, especially when creating new objects(e.g., `id: Guid.NewGuid()`).
- Ensure correct parameter names are used.
- Ensure unused usings are removed.
- Always use `DateTime.UtcNow` instead of `DateTime.Now`.


# Code Annotations And Comments

## .Net

- Use XML documentation.
- Add XML documentation to classes, using <inheritdoc/> for interface implementation methods and standard XML documentation for the class itself and any members not defined in the interface.
- Only add comments to public members, never to private ones.
- Do not comment tests and test classes.
- Use comments in code only for complex behavior.

## JavaScript

- Use JSDoc documentation.
- Never annotate private members.
- Never annotate exports.
- Use comments in code only for complex behavior.