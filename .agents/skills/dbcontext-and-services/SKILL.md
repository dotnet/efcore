---
name: dbcontext-and-services
description: 'EF Core DbContext, DbContextOptions, dependency injection, service registration, DbContext pooling, IServiceProvider management. Use when working on context configuration, service lifetimes, or the Dependencies pattern.'
user-invokable: false
---

# DbContext & Services

EF Core's internal DI container, service registration, and context lifecycle management.

## When to Use

- Adding a new EF Core service or changing service lifetimes
- Working on DbContext pooling behavior
- Modifying how options extensions are processed
- Understanding the `Dependencies` record pattern for service injection

## Service Registration

`EntityFrameworkServicesBuilder` (`src/EFCore/Infrastructure/EntityFrameworkServicesBuilder.cs`) maintains a `CoreServices` dictionary mapping service types to `ServiceCharacteristics` (lifetime + multi-registration flag). `TryAddCoreServices()` registers ~60+ services.

Pattern: Providers call `TryAddProviderSpecificServices()` first, then `TryAddCoreServices()` fills remaining defaults.

## Dependencies Pattern

Services receive dependencies via sealed records (not constructor injection of individual services):
```csharp
public sealed record MyServiceDependencies(IDep1 Dep1, IDep2 Dep2);
```

## DbContext Pooling

`DbContextPool<TContext>` uses `ConcurrentQueue<IDbContextPoolable>`. Default pool size: 1024. Set via `CoreOptionsExtension.MaxPoolSize`. Options are frozen at pool creation.

## Key Files

- `src/EFCore/DbContext.cs` — main context, implements `IDbContextPoolable`
- `src/EFCore/Infrastructure/DbContextOptions.cs` — immutable sorted dictionary of `IDbContextOptionsExtension`
- `src/EFCore/Infrastructure/EntityFrameworkServicesBuilder.cs` — service registration
- `src/EFCore/Internal/DbContextServices.cs` — scoped service resolver, model creation, provider validation
- `src/EFCore/Internal/DbContextPool.cs` — pooling implementation

## Testing

Unit tests: `test/EFCore.Tests/` (e.g., `DbContextTest.cs`, `EntityFrameworkServiceCollectionExtensionsTest.cs`).

## Validation

- Service resolution uses appropriate lifetimes and doesn't throw
