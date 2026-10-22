// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class InternalServiceCollectionMapTest
{
    [ConditionalFact]
    public void Can_patch_transient_service_with_concrete_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddTransient<IFakeService, FakeService>();
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_transient_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_transient_service_with_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddTransient<IFakeService, FakeService>(p => new FakeService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_transient_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_transient_service_with_service_typed_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddTransient<IFakeService>(p => new FakeService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_transient_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_transient_service_with_untyped_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddTransient(typeof(IFakeService), p => new FakeService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_transient_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_transient_service_with_concrete_implementation_already_registered()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddTransient<FakeService, DerivedFakeService>();
        serviceMap.TryAddTransient<IFakeService, FakeService>();
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Assert.IsType<DerivedFakeService>(Can_patch_transient_service(serviceMap));
    }

    private static FakeService Can_patch_transient_service(ServiceCollectionMap serviceMap)
    {
        var serviceProvider = serviceMap.ServiceCollection.BuildServiceProvider(validateScopes: true);

        FakeService service;

        using (var context = CreateContext(serviceProvider))
        {
            service = (FakeService)context.GetService<IFakeService>();
            Assert.Same(context, service.Context);
            Assert.NotSame(service, context.GetService<IFakeService>());
        }

        using (var context = CreateContext(serviceProvider))
        {
            Assert.Same(context, ((FakeService)context.GetService<IFakeService>()).Context);
            Assert.NotSame(service, context.GetService<IFakeService>());
        }

        return service;
    }

    [ConditionalFact]
    public void Can_patch_scoped_service_with_concrete_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddScoped<IFakeService, FakeService>();
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_scoped_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_scoped_service_with_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddScoped<IFakeService, FakeService>(p => new FakeService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_scoped_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_scoped_service_with_service_typed_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddScoped<IFakeService>(p => new FakeService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_scoped_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_scoped_service_with_untyped_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddScoped(typeof(IFakeService), p => new FakeService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Can_patch_scoped_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_scoped_service_with_concrete_implementation_already_registered()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddScoped<FakeService, DerivedFakeService>();
        serviceMap.TryAddScoped<IFakeService, FakeService>();
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeService>();

        Assert.IsType<DerivedFakeService>(Can_patch_scoped_service(serviceMap));
    }

    private static FakeService Can_patch_scoped_service(ServiceCollectionMap serviceMap)
    {
        var serviceProvider = serviceMap.ServiceCollection.BuildServiceProvider(validateScopes: true);

        FakeService service;

        using (var context = CreateContext(serviceProvider))
        {
            service = (FakeService)context.GetService<IFakeService>();
            Assert.Same(context, service.Context);
            Assert.Same(service, context.GetService<IFakeService>());
        }

        using (var context = CreateContext(serviceProvider))
        {
            Assert.Same(context, ((FakeService)context.GetService<IFakeService>()).Context);
            Assert.NotSame(service, context.GetService<IFakeService>());
        }

        return service;
    }

    [ConditionalFact]
    public void Can_patch_singleton_service_with_concrete_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddSingleton<IFakeSingletonService, FakeSingletonService>();
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeSingletonService>();

        Can_patch_singleton_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_singleton_service_with_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddSingleton<IFakeSingletonService, FakeSingletonService>(p => new FakeSingletonService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeSingletonService>();

        Can_patch_singleton_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_singleton_service_with_service_typed_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddSingleton<IFakeSingletonService>(p => new FakeSingletonService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeSingletonService>();

        Can_patch_singleton_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_singleton_service_with_untyped_delegate_implementation()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddSingleton(typeof(IFakeSingletonService), p => new FakeSingletonService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeSingletonService>();

        Can_patch_singleton_service(serviceMap);
    }

    [ConditionalFact]
    public void Can_patch_singleton_service_with_concrete_implementation_already_registered()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddSingleton<FakeSingletonService, DerivedFakeSingletonService>();
        serviceMap.TryAddSingleton<IFakeSingletonService, FakeSingletonService>();
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeSingletonService>();

        Assert.IsType<DerivedFakeSingletonService>(Can_patch_singleton_service(serviceMap));
    }

    [ConditionalFact]
    public void Can_patch_singleton_service_with_instance_registered()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddSingleton<IFakeSingletonService>(new DerivedFakeSingletonService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeSingletonService>();

        Assert.IsType<DerivedFakeSingletonService>(Can_patch_singleton_service(serviceMap));
    }

    [ConditionalFact]
    public void Can_patch_singleton_service_with_instance_registered_non_generic()
    {
        var serviceMap = CreateServiceMap();

        serviceMap.TryAddSingleton(typeof(IFakeSingletonService), new DerivedFakeSingletonService());
        ((InternalServiceCollectionMap)serviceMap.GetInfrastructure()).DoPatchInjection<IFakeSingletonService>();

        Assert.IsType<DerivedFakeSingletonService>(Can_patch_singleton_service(serviceMap));
    }

    private static FakeSingletonService Can_patch_singleton_service(ServiceCollectionMap serviceMap)
    {
        var serviceProvider = serviceMap.ServiceCollection.BuildServiceProvider(validateScopes: true);

        FakeSingletonService singletonService;

        using (var context = CreateContext(serviceProvider))
        {
            singletonService = (FakeSingletonService)context.GetService<IFakeSingletonService>();
            Assert.Same(context.GetService<IModelSource>(), singletonService.ModelSource);
            Assert.Same(singletonService, context.GetService<IFakeSingletonService>());
        }

        using (var context = CreateContext(serviceProvider))
        {
            Assert.Same(singletonService, context.GetService<IFakeSingletonService>());
        }

        return singletonService;
    }

    [ConditionalFact]
    public void Throws_if_attempt_is_made_to_register_dependency_as_delegate()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<DatabaseProviderDependencies>(p => null);

        var builder = new EntityFrameworkServicesBuilder(serviceCollection);

        Assert.Equal(
            CoreStrings.BadDependencyRegistration(nameof(DatabaseProviderDependencies)),
            Assert.Throws<InvalidOperationException>(
                    () => builder.TryAddCoreServices())
                .Message);
    }

    [ConditionalFact]
    public void Throws_if_attempt_is_made_to_register_dependency_as_instance()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(new DatabaseProviderDependencies());

        var builder = new EntityFrameworkServicesBuilder(serviceCollection);

        Assert.Equal(
            CoreStrings.BadDependencyRegistration(nameof(DatabaseProviderDependencies)),
            Assert.Throws<InvalidOperationException>(
                    () => builder.TryAddCoreServices())
                .Message);
    }

    private static ServiceCollectionMap CreateServiceMap()
        => new(new ServiceCollection().AddEntityFrameworkInMemoryDatabase());

    private static DbContext CreateContext(IServiceProvider serviceProvider)
        => new(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

    private interface IFakeService;

    private class FakeService : IFakeService, IPatchServiceInjectionSite
    {
        public DbContext Context { get; private set; }

        void IPatchServiceInjectionSite.InjectServices(IServiceProvider serviceProvider)
            => Context = serviceProvider.GetService<ICurrentDbContext>().Context;
    }

    private class DerivedFakeService : FakeService;

    private interface IFakeSingletonService;

    private class FakeSingletonService : IFakeSingletonService, IPatchServiceInjectionSite
    {
        public IModelSource ModelSource { get; private set; }

        void IPatchServiceInjectionSite.InjectServices(IServiceProvider serviceProvider)
            => ModelSource = serviceProvider.GetService<IModelSource>();
    }

    private class DerivedFakeSingletonService : FakeSingletonService;
}
