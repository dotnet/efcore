# Entity Framework Core - GitHub Copilot Instructions

This document provides guidance for GitHub Copilot when generating code for the Entity Framework Core project. Follow these guidelines to ensure that generated code aligns with the project's coding standards and architectural principles.

If you are not sure, do not guess, just tell that you don't know or ask clarifying questions. Don't copy code that follows the same pattern in a different context. Don't rely just on names, evaluate the code based on the implementation and usage. Verify that the generated code is correct and compilable.

## Code Style

### General Guidelines

- Follow the [.NET coding guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) unless explicitly overridden below
- Use the rules defined in the .editorconfig file in the root of the repository for any ambiguous cases
- Write code that is clean, maintainable, and easy to understand
- Favor readability over brevity, but keep methods focused and concise
- Only add comments rarely to explain why a non-intuitive solution was used. The code should be self-explanatory otherwise
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
- All declarations must begin on a new line
- Use a single blank line to separate logical sections of code when appropriate
- Insert a final newline at the end of files

### C# Specific Guidelines

- File scoped namespace declarations
- Use `var` for local variables
- Use expression-bodied members where appropriate
- Prefer using collection expressions when possible
- Use `is` pattern matching instead of `as` and null checks
- Prefer `switch` expressions over `switch` statements when appropriate
- Prefer field-backed property declarations using field contextual keyword instead of an explicit field.
- Prefer range and index from end operators for indexer access
- The projects use implicit namespaces, so do not add `using` directives for namespaces that are already imported by the project
- When verifying that a file doesn't produce compiler errors rebuild the whole project

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

## Architecture and Design Patterns

### Dependency Injection

- Design services to be registered and resolved through the DI container for functionality that could be replaced or extended by users, providers or plug-ins
- Create sealed records with names ending in `Dependencies` containing the service dependencies

### Testing

- Follow the existing test patterns in the corresponding test projects
- Create both unit tests and functional tests where appropriate
- Fix `SQL` and `C#` baselines for tests when necessary by setting the `EF_TEST_REWRITE_BASELINES` env var to `1`
- Before building or running the tests execute `restore.cmd` or `restore.sh` and `activate.ps1` or `activate.sh` to set up the environment
- When running the tests specify the test project and let it be rebuilt by not adding `--no-build`

## Documentation

- Include XML documentation for all public APIs
- Add proper `<remarks>` tags with links to relevant documentation where helpful
- For keywords like `null`, `true` or `false` use `<see langword="*" />` tags
- Use `<see href="https://aka.ms/efcore-docs-*">` redirects for doc links instead of hardcoded `https://learn.microsoft.com/` links
- Include code examples in documentation where appropriate
- Overriding members should inherit the XML documentation from the base type via `/// <inheritdoc />`

## Error Handling

- Use appropriate exception types. 
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

## Implementation Guidelines

- Write code that is secure by default. Avoid exposing potentially private or sensitive data
- Make code NativeAOT compatible when possible. This means avoiding dynamic code generation, reflection, and other features that are not compatible with NativeAOT. If not possible, mark the code with an appropriate annotation or throw an exception

### Entity Framework Core Specific guidelines

- Follow the provider pattern when extending EF Core's capabilities for specific databases
- Follow the existing model building design patterns
  - Add corresponding methods to the `*Builder`, `*Configuration`, `*Extensions`, `IConvention*Builder`, `IReadOnly*`, `IMutable*`and `IConvention*` types as needed
  - Make corresponding changes to the `Runtime*` types as needed.
  - For Relational-specific model changes also modify the `RelationalModel` and `*AnnotationProvider` types.
  - Make corresponding changes to the `CSharpRuntimeModelCodeGenerator`, `*CSharpRuntimeAnnotationCodeGenerator` and `CSharpDbContextGenerator` types as needed
- Use the logging infrastructure for diagnostics
- Prefer using `Check.DebugAssert` instead of `Debug.Assert` or comments
- Use `Check.NotNull` and `Check.NotEmpty` for preconditions in public APIs
- Unit tests should build the model using the corresponding `*TestHelpers.Instance.CreateConventionBuilder()` model and finalizing it
- The services should be resolved from the `IServiceProvider` returned by `*TestHelpers.Instance.CreateContextServices`, note that it has overloads allowing to specify the model and mock services
- For functional tests, create tests in projects corresponding to the database providers that derive from the appropriate test base classes in the `EFCore.*Specification.Tests` projects

## Repository Structure

- src/: Main product source code, including providers, tools, and analyzers
- test/: All test projects, including unit, functional, and specification tests for different providers
- benchmark/: Performance and benchmarking projects for EFCore
- tools/: Utility scripts and resources for development
- eng/: Build and test infrastructure files related to [Arcade SDK](https://github.com/dotnet/arcade/blob/main/Documentation/ArcadeSdk.md) used for building the project, and running the tests
- docs/: Documentation files for contributors and users. Full documentation is available at [EF Core | Learn](https://learn.microsoft.com/ef/core/)
- .github/: GitHub-specific files, workflows, and Copilot instructions
- .config/: AzDo pipelines configuration files

## Overview of Entity Framework Core

Entity Framework Core (EF Core) is an object-database mapper for .NET. Below is a concise summary of its core architecture and concepts:

### DbContext & Pooling
- `DbContext` is the main API for interacting with EF Core. It manages entity objects, queries, and changes.
- Configuration is done via `OnConfiguring` or dependency injection (DI) in modern apps.
- Pooling (`AddDbContextPool`) reuses context instances for performance in high-throughput scenarios.
- Contexts are short-lived; pooling resets state but does not dispose underlying services.

### Query Pipeline
- LINQ queries build expression trees, which EF Core translates into database queries (e.g., SQL).
- The pipeline includes translation, optimization, provider-specific SQL generation, and execution.
- Compiled queries are cached for performance.
- Deferred execution: queries run when enumerated (e.g., `ToList()`).

### Materialization (Shaper)
- Converts raw database results into .NET entity objects.
- Handles property assignment, relationship fix-up, and tracking (if enabled).
- Uses compiled functions for efficient repeated materialization.

### Change Tracking
- Tracks entity states: Added, Modified, Deleted, Unchanged, Detached.
- Detects changes via snapshots or proxies.
- Drives the update pipeline for `SaveChanges()`.
- Non-tracking queries (`AsNoTracking`) improve read performance.

### SaveChanges & Update Pipeline
- Gathers tracked changes and generates database commands (INSERT, UPDATE, DELETE).
- Orders operations to maintain referential integrity.
- Executes in a transaction by default; supports batching.
- Handles concurrency via tokens and exceptions.

### Bulk Operations
- `ExecuteUpdate`/`ExecuteDelete` perform set-based updates/deletes directly in the database, bypassing change tracking.
- Bulk inserts are batched, but not true multi-row SQL by default.

### Model Building & Conventions
- The model defines entities, properties, keys, relationships, and mappings.
- Built via conventions, data annotations, and the fluent API (`OnModelCreating`).
- Pre-convention configuration allows global rules.
- Model is cached for performance.

### Model Components
- Entity Types: .NET classes mapped to tables/collections.
- Properties: Scalar, navigation, shadow, or service properties.
- Keys: Primary and alternate keys for identity and relationships.
- Foreign Keys/Relationships: Define associations between entities.
- Navigation Properties: Reference or collection navigations.
- Owned Types: Entity types that are part of an aggregate, no independent identity.
- Complex Types: Value objects embedded in entities, no independent identity.
- Primitive Collections: Collections of scalars, often mapped to JSON columns.

### Storage & Type Mapping
- Maps .NET types to database types, with support for value converters.
- Provider-specific logic handles differences (e.g., SQL Server vs. SQLite).
- Ensures data round-trips correctly between CLR and database.

### Scaffolding (Reverse Engineering)
- Generates model and context code from an existing database schema.
- Useful for starting code-first development from a legacy database.

### Database Providers
- EF Core is database-agnostic; providers implement translation, type mapping, and database-specific behaviors.
- Choose the correct provider (e.g., SQL Server, SQLite, PostgreSQL) via NuGet and configuration.

### Migrations
- Incrementally evolve the database schema to match model changes.
- Add, update, or remove migrations; apply them via CLI or code.
- Maintains a migration history table in the database.

### Command-Line Tools
- `dotnet ef` and Package Manager Console (PMC) provide commands for migrations, scaffolding, and model optimization.
- Tools integrate with MSBuild and require the design package.

### Compiled Models
- A generated static representation of the model for faster startup in large projects.
- Must be regenerated if the model changes.
- Required for NativeAOT scenarios.
