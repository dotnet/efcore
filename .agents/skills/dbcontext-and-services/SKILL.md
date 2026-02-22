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

`EntityFrameworkServicesBuilder` maintains a `CoreServices` dictionary mapping service types to `ServiceCharacteristics` (lifetime + multi-registration flag).

Pattern: Providers call `TryAddProviderSpecificServices()` first, then `TryAddCoreServices()` fills remaining services without overriding existing registrations.

## Dependencies Pattern

Services receive dependencies via sealed records (not constructor injection of individual services):
```csharp
public sealed record MyServiceDependencies(IDep1 Dep1, IDep2 Dep2);
```

## Other Key Files

- `src/EFCore/DbContext.cs` — main context, implements `IDbContextPoolable`
- `src/EFCore/Infrastructure/DbContextOptions.cs` — immutable sorted dictionary of `IDbContextOptionsExtension`
- `src/EFCore/Internal/DbContextServices.cs` — scoped service resolver, model creation, provider validation
- `src/EFCore/Internal/DbContextPool.cs` — manages a pool of `IDbContextPoolable` instances. Options are frozen at pool creation.

## Testing

Unit tests: `DbContextTest.cs`, `EntityFrameworkServiceCollectionExtensionsTest.cs`.

## Validation

- Service resolution uses appropriate lifetimes and doesn't throw
