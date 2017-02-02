// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Infrastructure
{
    public class ServiceCollectionMapTest
    {
        [Fact]
        public void Can_add_delegate_services()
        {
            Func<IServiceProvider, FakeService> factory = p => new FakeService();

            AddServiceDelegateTest(m => m.TryAddTransient<IFakeService, FakeService>(factory), factory, ServiceLifetime.Transient);
            AddServiceDelegateTest(m => m.TryAddScoped<IFakeService, FakeService>(factory), factory, ServiceLifetime.Scoped);
            AddServiceDelegateTest(m => m.TryAddSingleton<IFakeService, FakeService>(factory), factory, ServiceLifetime.Singleton);
            AddServiceDelegateTest(m => m.TryAddTransient<IFakeService>(factory), factory, ServiceLifetime.Transient);
            AddServiceDelegateTest(m => m.TryAddScoped<IFakeService>(factory), factory, ServiceLifetime.Scoped);
            AddServiceDelegateTest(m => m.TryAddSingleton<IFakeService>(factory), factory, ServiceLifetime.Singleton);
            AddServiceDelegateTest(m => m.TryAddTransient(typeof(IFakeService), factory), factory, ServiceLifetime.Transient);
            AddServiceDelegateTest(m => m.TryAddScoped(typeof(IFakeService), factory), factory, ServiceLifetime.Scoped);
            AddServiceDelegateTest(m => m.TryAddSingleton(typeof(IFakeService), factory), factory, ServiceLifetime.Singleton);
        }

        private void AddServiceDelegateTest(
            Func<ServiceCollectionMap, ServiceCollectionMap> adder,
            Func<IServiceProvider, object> factory,
            ServiceLifetime lifetime)
        {
            var serviceCollectionMap = adder(new ServiceCollectionMap(new ServiceCollection()));

            var descriptor = serviceCollectionMap.ServiceCollection.Single();

            Assert.Same(typeof(IFakeService), descriptor.ServiceType);
            Assert.Same(factory, descriptor.ImplementationFactory);
            Assert.Equal(lifetime, descriptor.Lifetime);
        }

        [Fact]
        public void Can_add_concrete_services()
        {
            AddServiceConcreteTest(m => m.TryAddTransient<IFakeService, DerivedFakeService>(), ServiceLifetime.Transient);
            AddServiceConcreteTest(m => m.TryAddScoped<IFakeService, DerivedFakeService>(), ServiceLifetime.Scoped);
            AddServiceConcreteTest(m => m.TryAddSingleton<IFakeService, DerivedFakeService>(), ServiceLifetime.Singleton);
            AddServiceConcreteTest(m => m.TryAddTransient(typeof(IFakeService), typeof(DerivedFakeService)), ServiceLifetime.Transient);
            AddServiceConcreteTest(m => m.TryAddScoped(typeof(IFakeService), typeof(DerivedFakeService)), ServiceLifetime.Scoped);
            AddServiceConcreteTest(m => m.TryAddSingleton(typeof(IFakeService), typeof(DerivedFakeService)), ServiceLifetime.Singleton);
        }

        private void AddServiceConcreteTest(
            Func<ServiceCollectionMap, ServiceCollectionMap> adder,
            ServiceLifetime lifetime)
        {
            var serviceCollectionMap = adder(new ServiceCollectionMap(new ServiceCollection()));

            var descriptor = serviceCollectionMap.ServiceCollection.Single();

            Assert.Same(typeof(IFakeService), descriptor.ServiceType);
            Assert.Same(typeof(DerivedFakeService), descriptor.ImplementationType);
            Assert.Equal(lifetime, descriptor.Lifetime);
        }

        [Fact]
        public void Can_add_instance_services()
        {
            var instance = new FakeService();

            AddServiceInstanceTest(m => m.TryAddSingleton<IFakeService>(instance), instance);
            AddServiceInstanceTest(m => m.TryAddSingleton(typeof(IFakeService), instance), instance);
        }

        private void AddServiceInstanceTest(
            Func<ServiceCollectionMap, ServiceCollectionMap> adder,
            object instance)
        {
            var serviceCollectionMap = adder(new ServiceCollectionMap(new ServiceCollection()));

            var descriptor = serviceCollectionMap.ServiceCollection.Single();

            Assert.Same(typeof(IFakeService), descriptor.ServiceType);
            Assert.Same(instance, descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void Existing_services_are_not_replaced()
        {
            ExistingServiceTest(m => m.TryAddTransient<IFakeService, FakeService>());
            ExistingServiceTest(m => m.TryAddScoped<IFakeService, FakeService>());
            ExistingServiceTest(m => m.TryAddSingleton<IFakeService, FakeService>());
            ExistingServiceTest(m => m.TryAddTransient(typeof(IFakeService), typeof(FakeService)));
            ExistingServiceTest(m => m.TryAddScoped(typeof(IFakeService), typeof(FakeService)));
            ExistingServiceTest(m => m.TryAddSingleton(typeof(IFakeService), typeof(FakeService)));
            ExistingServiceTest(m => m.TryAddTransient<IFakeService, FakeService>(p => new FakeService()));
            ExistingServiceTest(m => m.TryAddScoped<IFakeService, FakeService>(p => new FakeService()));
            ExistingServiceTest(m => m.TryAddSingleton<IFakeService, FakeService>(p => new FakeService()));
            ExistingServiceTest(m => m.TryAddTransient<IFakeService>(p => new FakeService()));
            ExistingServiceTest(m => m.TryAddScoped<IFakeService>(p => new FakeService()));
            ExistingServiceTest(m => m.TryAddSingleton<IFakeService>(p => new FakeService()));
            ExistingServiceTest(m => m.TryAddTransient(typeof(IFakeService), p => new FakeService()));
            ExistingServiceTest(m => m.TryAddScoped(typeof(IFakeService), p => new FakeService()));
            ExistingServiceTest(m => m.TryAddSingleton(typeof(IFakeService), p => new FakeService()));
            ExistingServiceTest(m => m.TryAddSingleton<IFakeService>(new FakeService()));
            ExistingServiceTest(m => m.TryAddSingleton(typeof(IFakeService), new FakeService()));
        }

        private void ExistingServiceTest(Func<ServiceCollectionMap, ServiceCollectionMap> adder)
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IFakeService, FakeService>();

            var descriptor = serviceCollection.Single();

            var serviceCollectionMap = adder(new ServiceCollectionMap(serviceCollection));

            Assert.Same(serviceCollection, serviceCollectionMap.ServiceCollection);
            Assert.Same(descriptor, serviceCollection.Single());
        }

        [Fact]
        public void Can_add_multiple_concrete_services()
        {
            AddServiceConcreteEnumerableTest(
                m => m.TryAddTransientEnumerable<IFakeService, FakeService>(),
                m => m.TryAddTransientEnumerable<IFakeService, DerivedFakeService>(),
                ServiceLifetime.Transient);

            AddServiceConcreteEnumerableTest(
                m => m.TryAddScopedEnumerable<IFakeService, FakeService>(),
                m => m.TryAddScopedEnumerable<IFakeService, DerivedFakeService>(),
                ServiceLifetime.Scoped);

            AddServiceConcreteEnumerableTest(
                m => m.TryAddSingletonEnumerable<IFakeService, FakeService>(),
                m => m.TryAddSingletonEnumerable<IFakeService, DerivedFakeService>(),
                ServiceLifetime.Singleton);

            AddServiceConcreteEnumerableTest(
                m => m.TryAddTransientEnumerable(typeof(IFakeService), typeof(FakeService)),
                m => m.TryAddTransientEnumerable(typeof(IFakeService), typeof(DerivedFakeService)),
                ServiceLifetime.Transient);

            AddServiceConcreteEnumerableTest(
                m => m.TryAddScopedEnumerable(typeof(IFakeService), typeof(FakeService)),
                m => m.TryAddScopedEnumerable(typeof(IFakeService), typeof(DerivedFakeService)),
                ServiceLifetime.Scoped);

            AddServiceConcreteEnumerableTest(
                m => m.TryAddSingletonEnumerable(typeof(IFakeService), typeof(FakeService)),
                m => m.TryAddSingletonEnumerable(typeof(IFakeService), typeof(DerivedFakeService)),
                ServiceLifetime.Singleton);
        }

        private void AddServiceConcreteEnumerableTest(
            Func<ServiceCollectionMap, ServiceCollectionMap> adder1,
            Func<ServiceCollectionMap, ServiceCollectionMap> adder2,
            ServiceLifetime lifetime)
        {
            var serviceCollection = new ServiceCollection();
            adder2(adder1(adder2(adder1(new ServiceCollectionMap(serviceCollection)))));

            Assert.Equal(2, serviceCollection.Count);

            Assert.Same(typeof(IFakeService), serviceCollection[0].ServiceType);
            Assert.Same(typeof(FakeService), serviceCollection[0].ImplementationType);
            Assert.Equal(lifetime, serviceCollection[0].Lifetime);

            Assert.Same(typeof(IFakeService), serviceCollection[1].ServiceType);
            Assert.Same(typeof(DerivedFakeService), serviceCollection[1].ImplementationType);
            Assert.Equal(lifetime, serviceCollection[1].Lifetime);
        }

        [Fact]
        public void Can_add_multiple_delegate_services()
        {
            Func<IServiceProvider, FakeService> factory1 = p => new FakeService();
            Func<IServiceProvider, DerivedFakeService> factory2 = p => new DerivedFakeService();

            AddServiceDelegateEnumerableTest(
                m => m.TryAddTransientEnumerable<IFakeService, FakeService>(factory1),
                m => m.TryAddTransientEnumerable<IFakeService, DerivedFakeService>(factory2),
                factory1, factory2, ServiceLifetime.Transient);

            AddServiceDelegateEnumerableTest(
                m => m.TryAddScopedEnumerable<IFakeService, FakeService>(factory1),
                m => m.TryAddScopedEnumerable<IFakeService, DerivedFakeService>(factory2),
                factory1, factory2, ServiceLifetime.Scoped);

            AddServiceDelegateEnumerableTest(
                m => m.TryAddSingletonEnumerable<IFakeService, FakeService>(factory1),
                m => m.TryAddSingletonEnumerable<IFakeService, DerivedFakeService>(factory2),
                factory1, factory2, ServiceLifetime.Singleton);
        }

        private void AddServiceDelegateEnumerableTest(
            Func<ServiceCollectionMap, ServiceCollectionMap> adder1,
            Func<ServiceCollectionMap, ServiceCollectionMap> adder2,
            Func<IServiceProvider, object> factory1,
            Func<IServiceProvider, object> factory2,
            ServiceLifetime lifetime)
        {
            var serviceCollection = new ServiceCollection();
            adder2(adder1(adder2(adder1(new ServiceCollectionMap(serviceCollection)))));

            Assert.Equal(2, serviceCollection.Count);

            Assert.Same(typeof(IFakeService), serviceCollection[0].ServiceType);
            Assert.Same(factory1, serviceCollection[0].ImplementationFactory);
            Assert.Equal(lifetime, serviceCollection[0].Lifetime);

            Assert.Same(typeof(IFakeService), serviceCollection[1].ServiceType);
            Assert.Same(factory2, serviceCollection[1].ImplementationFactory);
            Assert.Equal(lifetime, serviceCollection[1].Lifetime);
        }

        [Fact]
        public void Can_add_multiple_instance_services()
        {
            var instance1 = new FakeService();
            var instance2 = new DerivedFakeService();

            AddServiceInstanceEnumerableTest(
                m => m.TryAddSingletonEnumerable<IFakeService>(instance1),
                m => m.TryAddSingletonEnumerable<IFakeService>(instance2),
                instance1, instance2);

            AddServiceInstanceEnumerableTest(
                m => m.TryAddSingletonEnumerable(typeof(IFakeService), instance1),
                m => m.TryAddSingletonEnumerable(typeof(IFakeService), instance2),
                instance1, instance2);
        }

        private void AddServiceInstanceEnumerableTest(
            Func<ServiceCollectionMap, ServiceCollectionMap> adder1,
            Func<ServiceCollectionMap, ServiceCollectionMap> adder2,
            object instance1,
            object instance2)
        {
            var serviceCollection = new ServiceCollection();
            adder2(adder1(adder2(adder1(new ServiceCollectionMap(serviceCollection)))));

            Assert.Equal(2, serviceCollection.Count);

            Assert.Same(typeof(IFakeService), serviceCollection[0].ServiceType);
            Assert.Same(instance1, serviceCollection[0].ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, serviceCollection[0].Lifetime);

            Assert.Same(typeof(IFakeService), serviceCollection[1].ServiceType);
            Assert.Same(instance2, serviceCollection[1].ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, serviceCollection[1].Lifetime);
        }

        [Fact]
        public void Can_patch_transient_service_with_concrete_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddTransient<IFakeService, FakeService>();
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_transient_service(serviceMap);
        }

        [Fact]
        public void Can_patch_transient_service_with_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddTransient<IFakeService, FakeService>(p => new FakeService());
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_transient_service(serviceMap);
        }

        [Fact]
        public void Can_patch_transient_service_with_service_typed_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddTransient<IFakeService>(p => new FakeService());
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_transient_service(serviceMap);
        }

        [Fact]
        public void Can_patch_transient_service_with_untyped_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddTransient(typeof(IFakeService), p => new FakeService());
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_transient_service(serviceMap);
        }

        [Fact]
        public void Can_patch_transient_service_with_concrete_implementation_already_registered()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddTransient<FakeService, DerivedFakeService>();
            serviceMap.TryAddTransient<IFakeService, FakeService>();
            serviceMap.DoPatchInjection<IFakeService>();

            Assert.IsType<DerivedFakeService>(Can_patch_transient_service(serviceMap));
        }

        private static FakeService Can_patch_transient_service(ServiceCollectionMap serviceMap)
        {
            var serviceProvider = serviceMap.ServiceCollection.BuildServiceProvider();

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

        [Fact]
        public void Can_patch_scoped_service_with_concrete_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddScoped<IFakeService, FakeService>();
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_scoped_service(serviceMap);
        }

        [Fact]
        public void Can_patch_scoped_service_with_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddScoped<IFakeService, FakeService>(p => new FakeService());
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_scoped_service(serviceMap);
        }

        [Fact]
        public void Can_patch_scoped_service_with_service_typed_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddScoped<IFakeService>(p => new FakeService());
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_scoped_service(serviceMap);
        }

        [Fact]
        public void Can_patch_scoped_service_with_untyped_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddScoped(typeof(IFakeService), p => new FakeService());
            serviceMap.DoPatchInjection<IFakeService>();

            Can_patch_scoped_service(serviceMap);
        }

        [Fact]
        public void Can_patch_scoped_service_with_concrete_implementation_already_registered()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddScoped<FakeService, DerivedFakeService>();
            serviceMap.TryAddScoped<IFakeService, FakeService>();
            serviceMap.DoPatchInjection<IFakeService>();

            Assert.IsType<DerivedFakeService>(Can_patch_scoped_service(serviceMap));
        }

        private static FakeService Can_patch_scoped_service(ServiceCollectionMap serviceMap)
        {
            var serviceProvider = serviceMap.ServiceCollection.BuildServiceProvider();

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

        [Fact]
        public void Can_patch_singleton_service_with_concrete_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddSingleton<IFakeSingletonService, FakeSingletonService>();
            serviceMap.DoPatchInjection<IFakeSingletonService>();

            Can_patch_singleton_service(serviceMap);
        }

        [Fact]
        public void Can_patch_singleton_service_with_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddSingleton<IFakeSingletonService, FakeSingletonService>(p => new FakeSingletonService());
            serviceMap.DoPatchInjection<IFakeSingletonService>();

            Can_patch_singleton_service(serviceMap);
        }

        [Fact]
        public void Can_patch_singleton_service_with_service_typed_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddSingleton<IFakeSingletonService>(p => new FakeSingletonService());
            serviceMap.DoPatchInjection<IFakeSingletonService>();

            Can_patch_singleton_service(serviceMap);
        }

        [Fact]
        public void Can_patch_singleton_service_with_untyped_delegate_implementation()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddSingleton(typeof(IFakeSingletonService), p => new FakeSingletonService());
            serviceMap.DoPatchInjection<IFakeSingletonService>();

            Can_patch_singleton_service(serviceMap);
        }

        [Fact]
        public void Can_patch_singleton_service_with_concrete_implementation_already_registered()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddSingleton<FakeSingletonService, DerivedFakeSingletonService>();
            serviceMap.TryAddSingleton<IFakeSingletonService, FakeSingletonService>();
            serviceMap.DoPatchInjection<IFakeSingletonService>();

            Assert.IsType<DerivedFakeSingletonService>(Can_patch_singleton_service(serviceMap));
        }

        [Fact]
        public void Can_patch_singleton_service_with_instance_registered()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddSingleton<IFakeSingletonService>(new DerivedFakeSingletonService());
            serviceMap.DoPatchInjection<IFakeSingletonService>();

            Assert.IsType<DerivedFakeSingletonService>(Can_patch_singleton_service(serviceMap));
        }

        [Fact]
        public void Can_patch_singleton_service_with_instance_registered_non_generic()
        {
            var serviceMap = CreateServiceMap();

            serviceMap.TryAddSingleton(typeof(IFakeSingletonService), new DerivedFakeSingletonService());
            serviceMap.DoPatchInjection<IFakeSingletonService>();

            Assert.IsType<DerivedFakeSingletonService>(Can_patch_singleton_service(serviceMap));
        }

        [Fact]
        public virtual void Same_INavigationFixer_is_returned_for_all_registrations()
        {
            using (var context = new DbContext(new DbContextOptionsBuilder().UseInMemoryDatabase().Options))
            {
                var navFixer = context.GetService<INavigationFixer>();

                Assert.Contains(navFixer, context.GetService<IEnumerable<IEntityStateListener>>());
                Assert.Contains(navFixer, context.GetService<IEnumerable<INavigationListener>>());
                Assert.Contains(navFixer, context.GetService<IEnumerable<IKeyListener>>());
                Assert.Contains(navFixer, context.GetService<IEnumerable<IQueryTrackingListener>>());
            }
        }

        private static FakeSingletonService Can_patch_singleton_service(ServiceCollectionMap serviceMap)
        {
            var serviceProvider = serviceMap.ServiceCollection.BuildServiceProvider();

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

        private static ServiceCollectionMap CreateServiceMap()
            => new ServiceCollectionMap(new ServiceCollection().AddEntityFrameworkInMemoryDatabase());

        private static DbContext CreateContext(IServiceProvider serviceProvider)
            => new DbContext(new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseInMemoryDatabase()
                .Options);

        private interface IFakeService
        {
        }

        private class FakeService : IFakeService, IPatchServiceInjectionSite
        {
            public DbContext Context { get; private set; }

            void IPatchServiceInjectionSite.InjectServices(IServiceProvider serviceProvider)
                => Context = serviceProvider.GetService<ICurrentDbContext>().Context;
        }

        private class DerivedFakeService : FakeService
        {
        }

        private interface IFakeSingletonService
        {
        }

        private class FakeSingletonService : IFakeSingletonService, IPatchServiceInjectionSite
        {
            public IModelSource ModelSource { get; private set; }

            void IPatchServiceInjectionSite.InjectServices(IServiceProvider serviceProvider)
                => ModelSource = serviceProvider.GetService<IModelSource>();
        }

        private class DerivedFakeSingletonService : FakeSingletonService
        {
        }
    }
}
