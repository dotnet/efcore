# Entity Framework Core - GitHub Copilot Instructions

This document provides guidance for GitHub Copilot when generating code for the Entity Framework Core project. Follow these guidelines to ensure that generated code aligns with the project's coding standards and architectural principles.

If you are not sure, do not guess, just tell me that you don't know or ask clarifying questions. Don't copy code that follows the same pattern in a different context. Don't rely just on names, evaluate the code based on the implementation and usage. Verify that the generated code is correct and compilable.

## Code Style

### General Guidelines

- Follow the [.NET coding guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) unless explicitly overridden below
- Use the rules defined in the .editorconfig file in the root of the repository for any ambigous cases
- Write code that is clean, maintainable, and easy to understand
- Favor readability over brevity, but keep methods focused and concise
- Only add comments to explain why something is done, not how it is done. The code should be self-explanatory
- Add license header to all files:
```
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
```
- Don't add the UTF-8 BOM to files unless they have non-ASCII characters
- All types should be public. Types in .Internal namespaces should have this comment on all members:
```
/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
```
- Avoid breaking public APIs. If you need to break a public API, add a new API instead and mark the old one as obsolete. Use `ObsoleteAttribute` with the message pointing to the new API

### Formatting

- Use spaces for indentation (4 spaces)
- Use braces for all blocks including single-line blocks
- Place braces on new lines
- Limit line length to 140 characters
- Trim trailing whitespace
- Insert a final newline at the end of files

### C# Specific Guidelines

- File scoped namespace declarations
- Use `var` for local variables
- Use expression-bodied members where appropriate
- Use collection initializers when possible
- Use `is` pattern matching instead of `as` and null checks
- Prefer `switch` expressions over `switch` statements when appropriate

### Naming Conventions

- Use PascalCase for:
  - Classes, structs, enums, properties, methods, events, namespaces, delegates
  - Public fields
  - Static private fields
  - Constants
- Use camelCase for:
  - Parameters
  - Local variables
- Use `_camelCase` for instance private fields
- Prefix interfaces with `I`
- Prefix type parameters with `T`
- Use meaningful and descriptive names

### Nullability

- Use nullable reference types
- Use proper null-checking patterns
- Use the null-conditional operator (`?.`) and null-coalescing operator (`??`) when appropriate

## Implementation Guidelines

- Write code that is secure by default. Avoid exposing potentially private or sensitive data
- Make code NativeAOT compatible when possible. This means avoiding dynamic code generation, reflection, and other features that are not compatible with NativeAOT. If not possible, mark the code with an appropriate annotation or throw an exception

## Architecture and Design Patterns

### Dependency Injection

- Design services to be registered and resolved through the DI container for functionality that could be replaced or extended by users, providers or plug-ins
- Create sealed records with names ending in `Dependencies` containing the service dependencies

### Testing

- Follow the existing test patterns in the corresponding test projects
- Create both unit tests and functional tests where appropriate
- For database provider implementations, create tests that derive from the appropriate specification test base classes
- Add or modify `SQL` and `C#` baselines for tests when necessary

## Documentation

- Include XML documentation for all public APIs
- Add proper `<remarks>` tags with links to relevant documentation where helpful
- For keywords like `null`, `true` or `false` use `<see langword="*" />` tags
- Use `<see href="https://aka.ms/efcore-docs-*">` redirects for doc links instead of hardcoded `https://learn.microsoft.com/` links
- Include code examples in documentation where appropriate
- Overriding members should inherit the XML documentation from the base type via `/// <inheritdoc />`

## Error Handling

- Use appropriate exception types- 
- Include helpful error messages stored in the .resx file corresponding to the project
- Avoid catching exceptions without rethrowing them

## Asynchronous Programming

- Provide both synchronous and asynchronous versions of methods where appropriate
- Use the `Async` suffix for asynchronous methods
- Return `Task` or `ValueTask` from asynchronous methods
- Use `CancellationToken` parameters to support cancellation
- Avoid async void methods except for event handlers
- Call `ConfigureAwait(false)` on awaited calls to avoid deadlocks

## Performance Considerations

- Be mindful of performance implications, especially for database operations
- Avoid unnecessary allocations
- Consider using more efficient code that is expected to be on the hot path, even if it is less readable

## Entity Framework Core Specific

- Follow the provider pattern when extending EF Core's capabilities for specific databases
- Follow the existing model building design patterns
  - Add corresponding methods to the `*Builder`, `*Configuration`, `*Extensions`, `IConvention*Builder`, `IReadOnly*`, `IMutable*`and `IConvention*` types as needed
  - Make corresponding changes to the `Runtime*` types as needed.
  - For Relational-specific model changes also modify the `RelationalModel` and `*AnnotationProvider` types.
  - Make corresponding changes to the `CSharpRuntimeModelCodeGenerator`, `*CSharpRuntimeAnnotationCodeGenerator` and `CSharpDbContextGenerator` types as needed
- Use the logging infrastructure for diagnostics
- Prefer using `Check.DebugAssert` to `Debug.Assert` or comments for assertions in the codebase
- Use `Check.NotNull` and `Check.NotEmpty` for preconditions in public APIs
