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