---
name: dbcontext-and-services
description: 'Implementation details for EF Core DbContext and the DI service infrastructure. Use when changing context configuration, service registration, service lifetimes, DbContext pooling, or the Dependencies pattern.'
user-invokable: false
---

# DbContext & Services

EF Core's internal DI container, service registration, and context lifecycle management.

## Service Registration

`EntityFrameworkServicesBuilder` maintains a `CoreServices` dictionary mapping service types to `ServiceCharacteristics` (lifetime + multi-registration flag).

## Dependencies Pattern

Services receive dependencies via sealed records (not constructor injection of individual services):
```csharp
public sealed record MyServiceDependencies(IDep1 Dep1, IDep2 Dep2);
```
