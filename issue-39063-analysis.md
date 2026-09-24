# Analysis of issue #39063

## Reproducing the failure

The broad claim that `AddInterceptors` never works with `UseInternalServiceProvider` is not correct. For example, scoped interceptors such as `SaveChangesInterceptor` are stored in the context options and are combined with injected interceptors by the scoped `IInterceptors` service.

There is, however, a concrete failing case for singleton interceptors. `IMaterializationInterceptor` implements `ISingletonInterceptor`, so this configuration fails during context initialization:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

var interceptor = new StampMaterializationInterceptor();
var internalProvider = new ServiceCollection()
    .AddEntityFrameworkInMemoryDatabase()
    .BuildServiceProvider(validateScopes: true);

var options = new DbContextOptionsBuilder<DemoContext>()
    .UseInMemoryDatabase("failing")
    .AddInterceptors(interceptor)
    .UseInternalServiceProvider(internalProvider)
    .Options;

await using var context = new DemoContext(options);
_ = context.Model;

public sealed class DemoContext(DbContextOptions<DemoContext> options) : DbContext(options);

public sealed class StampMaterializationInterceptor : IMaterializationInterceptor
{
}
```

The result on the current codebase is:

```text
A call was made to 'AddInterceptors', but Entity Framework is not building its own internal service provider. Either allow Entity Framework to build the service provider by removing the call to 'UseInternalServiceProvider', or build the 'ISingletonInterceptor' services to use into the service provider before passing it to 'UseInternalServiceProvider'.
```

`AddInterceptors` identifies singleton interceptors and records them separately ([`DbContextOptionsBuilder.cs`](src/EFCore/DbContextOptionsBuilder.cs#L670-L681)). When EF creates the internal provider, `CoreOptionsExtension.ApplyServices` adds those interceptors to that provider. With an externally supplied provider, EF deliberately validates and rejects this combination ([`CoreOptionsExtension.cs`](src/EFCore/Infrastructure/CoreOptionsExtension.cs#L651-L657)).

The same boundary affects a custom `IDbContextOptionsExtension`: its `ApplyServices` method runs when EF creates the provider, but not when `UseInternalServiceProvider` supplies an already-built provider. The contract documents this explicitly ([`IDbContextOptionsExtension.cs`](src/EFCore/Infrastructure/IDbContextOptionsExtension.cs#L27-L33)).

## Workaround

The preferred workaround is to remove `UseInternalServiceProvider` and let EF build and cache its internal provider. If an external internal provider is required, register the singleton interceptor before building that provider and do not pass it to `AddInterceptors`:

```csharp
var interceptor = new StampMaterializationInterceptor();
var internalProvider = new ServiceCollection()
    .AddEntityFrameworkInMemoryDatabase()
    .AddSingleton<ISingletonInterceptor>(interceptor)
    .BuildServiceProvider(validateScopes: true);

var options = new DbContextOptionsBuilder<DemoContext>()
    .UseInMemoryDatabase("working")
    .UseInternalServiceProvider(internalProvider)
    .Options;
```

This was verified with a materialization query; the interceptor ran once.

A library that must support both ownership modes should expose two matching registration entry points:

1. An options-builder method backed by `IDbContextOptionsExtension` for the normal EF-owned provider.
2. An `IServiceCollection` method that registers the same services for callers that opt into `UseInternalServiceProvider`.

This follows the existing provider pattern: callers of `UseInternalServiceProvider` must add all required EF, database-provider, and extension services before building the provider ([`DbContextOptionsBuilder.cs`](src/EFCore/DbContextOptionsBuilder.cs#L409-L430)).

## Why a unified registration hook does not fit EF Core's design

`UseInternalServiceProvider` is an ownership boundary, not an alternative source from which EF can recover registrations. In `ServiceProviderCache.GetOrAdd`, EF validates options and immediately returns the supplied provider. Only the EF-owned path creates an `IServiceCollection` and invokes every extension's `ApplyServices` method ([`ServiceProviderCache.cs`](src/EFCore/Internal/ServiceProviderCache.cs#L34-L53), [`ServiceProviderCache.cs`](src/EFCore/Internal/ServiceProviderCache.cs#L180-L201)).

Supporting the requested hook would conflict with that design:

- An `IServiceProvider` exposes resolved services, not its original `ServiceDescriptor` collection. EF cannot safely add registrations, preserve factory registrations, or rebuild an arbitrary provider.
- Service lifetimes, scopes, singleton identity, validation, disposal ownership, and provider-specific behavior are fixed when the external provider is built. Copying resolved instances into another provider changes those semantics.
- One internal provider can be shared by many contexts, while context options can differ per instance. Applying per-context service registrations to that shared provider would make its service graph depend on whichever context initialized it.
- The application provider and EF's internal provider have intentionally different roles. Automatically moving services between them would introduce cross-container service location and lifetime coupling.

The existing split is therefore intentional: option-level interceptor instances are context configuration, while singleton interceptors and other internal services belong to the internal provider. Once an application chooses `UseInternalServiceProvider`, it also chooses responsibility for constructing that provider completely.
