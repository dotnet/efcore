# Entity Framework Core - GitHub Copilot Instructions

This document provides guidance for working with code in the Entity Framework Core project.

If you are not sure, do not guess, just tell that you don't know or ask clarifying questions.
Don't just copy code that follows the same pattern in a different context.
Don't rely just on names to guess its function, evaluate the code based on the implementation and usage.

## Code Style

- Follow the [.NET coding guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md) unless explicitly overridden below
- Use the rules defined in the .editorconfig file in the root of the repository for any ambiguous cases
- Write code that is clean, maintainable, and easy to understand
- Favor readability over brevity, but keep methods focused and concise
- **Prefer minimal comments** - The code should be self-explanatory. Add comments sparingly and only to explain *why* a non-intuitive solution was necessary, not *what* the code does. Comments are appropriate for complex logic, public APIs, or domain-specific implementations where context would otherwise be unclear. Use `Check.DebugAssert` instead of a comment if possible.

## Environment Setup
- **ALWAYS** run `restore.cmd` (Windows) or `. ./restore.sh` (Linux/Mac) first to restore dependencies
- **ALWAYS** run `. .\activate.ps1` (PowerShell) or `. ./activate.sh` (Bash) to set up the development environment with correct SDK versions before building or running the tests

## Dependency and Version Management

- **NEVER** hardcode package versions in `.csproj` files
- Use `eng/Versions.props` and `Directory.Packages.props` for NuGet package version management

## Implementation Guidelines

- Write code that is secure by default. Avoid exposing potentially private or sensitive data
- Make code NativeAOT compatible when possible. This means avoiding dynamic code generation, reflection, and other features that are not compatible with NativeAOT. If not possible, mark the code with an appropriate annotation or throw an exception
- After implementing a fix, review the surrounding code for similar patterns that might need the same change
- Be mindful of performance implications, especially for database operations
- Avoid breaking public APIs. If you need to break a public API, add a new API instead and mark the old one as obsolete. Use `ObsoleteAttribute` with the message pointing to the new API
- If a public API is changed, run EFCore.ApiBaseline.Tests
- All types should be public by default, but types in `.Internal` namespaces or annotated with `[EntityFrameworkInternal]` require a specific XML doc comment on ALL members.
- **ALL** user-facing error messages must use string resources from the `.resx` (and the generated `.Designer.cs`) file corresponding to the project
- Call `ConfigureAwait(false)` on awaited asynchronous calls to avoid deadlocks

## Agent Skills

Skill files in `.agents/skills/` provide domain-specific knowledge so that agents don't need repetitive instructions from the user. Keep skills updated: when you discover non-obvious patterns, key files, or recurring review feedback during a session, distill the insight into the relevant `SKILL.md`. Additions must be concise, broadly useful, and stable — avoid task-specific details, speculation, and statements that contradict existing content. Remove or correct stale information rather than appending conflicting rules.
