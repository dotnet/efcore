// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DbContextFactoryTest
    {
        [ConditionalTheory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void Factory_creates_new_context_instance(ServiceLifetime lifetime)
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)),
                    lifetime)
                .BuildServiceProvider(validateScopes: true);

            if (lifetime == ServiceLifetime.Scoped)
            {
                serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            }

            var contextFactory = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>();

            using var context1 = contextFactory.CreateDbContext();
            using var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));
        }

        [ConditionalFact]
        public void Factory_can_use_pool()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddPooledDbContextFactory<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)))
                .BuildServiceProvider(validateScopes: true);

            var contextFactory = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>();

            var context1 = contextFactory.CreateDbContext();
            var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));

            context1.Dispose();
            context2.Dispose();

            using var context1b = contextFactory.CreateDbContext();
            using var context2b = contextFactory.CreateDbContext();

            Assert.NotSame(context1b, context2b);
            Assert.Same(context1, context1b);
            Assert.Same(context2, context2b);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1b));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2b));
        }

        [ConditionalFact]
        public void Factory_can_use_shared_pool()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContext<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)), ServiceLifetime.Scoped, ServiceLifetime.Singleton)
                .AddPooledDbContextFactory<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)))
                .BuildServiceProvider(validateScopes: true);

            var scope = serviceProvider.CreateScope();
            var contextFactory = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>();
            Assert.Same(contextFactory, scope.ServiceProvider.GetService<IDbContextFactory<WoolacombeContext>>());

            var context1 = contextFactory.CreateDbContext();
            var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));

            var context3 = scope.ServiceProvider.GetService<WoolacombeContext>();

            Assert.Same(context3, scope.ServiceProvider.GetService<WoolacombeContext>());
            Assert.NotSame(context1, context3);
            Assert.NotSame(context2, context3);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context3));

            context1.Dispose();
            context2.Dispose();
            scope.Dispose();

            using var scope1 = serviceProvider.CreateScope();

            using var context1b = contextFactory.CreateDbContext();
            using var context2b = contextFactory.CreateDbContext();
            var context3b = scope1.ServiceProvider.GetService<WoolacombeContext>();

            Assert.Same(context3b, scope1.ServiceProvider.GetService<WoolacombeContext>());
            Assert.NotSame(context1b, context3b);
            Assert.NotSame(context2b, context3b);
        }

        [ConditionalTheory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void Factory_and_options_have_the_same_lifetime(ServiceLifetime lifetime)
        {
            var serviceCollection = new ServiceCollection()
                .AddDbContextFactory<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)),
                    lifetime);

            Assert.Equal(lifetime, serviceCollection.Single(e => e.ServiceType == typeof(IDbContextFactory<WoolacombeContext>)).Lifetime);
            Assert.Equal(lifetime, serviceCollection.Single(e => e.ServiceType == typeof(DbContextOptions<WoolacombeContext>)).Lifetime);
        }

        [ConditionalFact]
        public void Default_lifetime_is_singleton()
        {
            var serviceCollection = new ServiceCollection()
                .AddDbContextFactory<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)));

            Assert.Equal(
                ServiceLifetime.Singleton,
                serviceCollection.Single(e => e.ServiceType == typeof(IDbContextFactory<WoolacombeContext>)).Lifetime);

            Assert.Equal(
                ServiceLifetime.Singleton,
                serviceCollection.Single(e => e.ServiceType == typeof(DbContextOptions<WoolacombeContext>)).Lifetime);
        }

        [ConditionalFact]
        public void Lifetime_is_singleton_when_pooling()
        {
            var serviceCollection = new ServiceCollection()
                .AddPooledDbContextFactory<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)));

            Assert.Equal(
                ServiceLifetime.Singleton,
                serviceCollection.Single(e => e.ServiceType == typeof(IDbContextPool<WoolacombeContext>)).Lifetime);

            Assert.Equal(
                ServiceLifetime.Singleton,
                serviceCollection.Single(e => e.ServiceType == typeof(IDbContextFactory<WoolacombeContext>)).Lifetime);

            Assert.Equal(
                ServiceLifetime.Singleton,
                serviceCollection.Single(e => e.ServiceType == typeof(DbContextOptions<WoolacombeContext>)).Lifetime);
        }

        private class WoolacombeContext : DbContext
        {
            public WoolacombeContext(DbContextOptions<WoolacombeContext> options)
                : base(options)
            {
            }
        }

        [ConditionalFact]
        public void Factory_can_use_constructor_with_non_generic_builder()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<CroydeContext>(
                    b => b.UseInMemoryDatabase(nameof(CroydeContext)))
                .BuildServiceProvider(validateScopes: true);

            using var context = serviceProvider.GetService<IDbContextFactory<CroydeContext>>().CreateDbContext();

            Assert.Equal(nameof(CroydeContext), GetStoreName(context));
        }

        private class CroydeContext : DbContext
        {
            public CroydeContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        [ConditionalFact]
        public void Factory_can_use_parameterless_constructor()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<MortehoeContext>()
                .BuildServiceProvider(validateScopes: true);

            using var context = serviceProvider.GetService<IDbContextFactory<MortehoeContext>>().CreateDbContext();

            Assert.Equal(nameof(MortehoeContext), GetStoreName(context));
        }

        private class MortehoeContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(nameof(MortehoeContext));
        }

        [ConditionalFact]
        public void Factory_can_use_DbContext_directly()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<DbContext>(b => b.UseInMemoryDatabase(nameof(DbContext)))
                .BuildServiceProvider(validateScopes: true);

            using var context = serviceProvider.GetService<IDbContextFactory<DbContext>>().CreateDbContext();

            Assert.Equal(nameof(DbContext), GetStoreName(context));
        }

        [ConditionalTheory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void Can_always_inject_singleton_and_transient_services(ServiceLifetime lifetime)
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddSingleton<SingletonService>()
                .AddTransient<TransientService>()
                .AddDbContextFactory<IlfracombeContext>(
                    b => b.UseInMemoryDatabase(nameof(IlfracombeContext)),
                    lifetime)
                .BuildServiceProvider(validateScopes: true);

            if (lifetime == ServiceLifetime.Scoped)
            {
                serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            }

            var contextFactory = serviceProvider.GetService<IDbContextFactory<IlfracombeContext>>();

            using var context1 = contextFactory.CreateDbContext();
            using var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);

            Assert.NotNull(context1.SingletonService);
            Assert.Same(context1.SingletonService, context2.SingletonService);

            Assert.NotNull(context1.TransientService);
            Assert.NotNull(context2.TransientService);
            Assert.NotSame(context1.TransientService, context2.TransientService);
        }

        private class TransientService : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
                => IsDisposed = true;
        }

        private class SingletonService
        {
        }

        private class IlfracombeContext : DbContext
        {
            public IlfracombeContext(
                DbContextOptions<IlfracombeContext> options,
                SingletonService singletonService,
                TransientService transientService)
                : base(options)
            {
                SingletonService = singletonService;
                TransientService = transientService;
            }

            public SingletonService SingletonService { get; }
            public TransientService TransientService { get; }
        }

        [ConditionalFact]
        public void Can_inject_singleton_transient_and_scoped_services_into_scoped_factory()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddSingleton<SingletonService>()
                .AddScoped<ScopedService>()
                .AddTransient<TransientService>()
                .AddDbContextFactory<CombeMartinContext>(
                    b => b.UseInMemoryDatabase(nameof(CombeMartinContext)),
                    ServiceLifetime.Scoped)
                .BuildServiceProvider(validateScopes: true);

            var scope = serviceProvider.CreateScope();
            var scopedServiceProvider = scope.ServiceProvider;

            var contextFactory = scopedServiceProvider.GetService<IDbContextFactory<CombeMartinContext>>();

            using var context1 = contextFactory.CreateDbContext();
            using var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);

            var singletonService = context1.SingletonService;
            Assert.NotNull(singletonService);
            Assert.Same(singletonService, context2.SingletonService);

            var scopedService = context1.ScopedService;
            Assert.NotNull(scopedService);
            Assert.Same(scopedService, context2.ScopedService);

            var transientService = context1.TransientService;
            Assert.NotNull(transientService);
            Assert.NotNull(context2.TransientService);
            Assert.NotSame(transientService, context2.TransientService);

            scope.Dispose();

            Assert.True(transientService.IsDisposed);
            Assert.True(scopedService.IsDisposed);

            scope = serviceProvider.CreateScope();
            scopedServiceProvider = scope.ServiceProvider;
            contextFactory = scopedServiceProvider.GetService<IDbContextFactory<CombeMartinContext>>();

            using var context = contextFactory.CreateDbContext();

            Assert.Same(singletonService, context.SingletonService);
            Assert.NotNull(context.ScopedService);
            Assert.NotSame(scopedService, context.ScopedService);
            Assert.NotNull(context.TransientService);
            Assert.NotSame(transientService, context.TransientService);

            scope.Dispose();
        }

        private class ScopedService : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
                => IsDisposed = true;
        }

        private class CombeMartinContext : DbContext
        {
            public CombeMartinContext(
                DbContextOptions<CombeMartinContext> options,
                SingletonService singletonService,
                ScopedService scopedService,
                TransientService transientService)
                : base(options)
            {
                SingletonService = singletonService;
                ScopedService = scopedService;
                TransientService = transientService;
            }

            public SingletonService SingletonService { get; }
            public ScopedService ScopedService { get; }
            public TransientService TransientService { get; }
        }

        [ConditionalTheory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void Can_resolve_from_the_service_provider_in_options_action(ServiceLifetime lifetime)
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddSingleton<SingletonService>()
                .AddScoped<ScopedService>()
                .AddTransient<TransientService>()
                .AddDbContextFactory<WoolacombeContext>(
                    (p, b) =>
                    {
                        Assert.NotNull(p.GetService<SingletonService>());
                        Assert.NotNull(p.GetService<TransientService>());

                        if (lifetime == ServiceLifetime.Scoped)
                        {
                            Assert.NotNull(p.GetService<ScopedService>());
                        }

                        b.UseInMemoryDatabase(nameof(WoolacombeContext));
                    },
                    lifetime)
                .BuildServiceProvider(validateScopes: true);

            if (lifetime == ServiceLifetime.Scoped)
            {
                serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            }

            var contextFactory = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>();

            using var context1 = contextFactory.CreateDbContext();
            using var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));
        }

        [ConditionalFact]
        public void Can_resolve_from_the_service_provider_when_pooling()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddSingleton<SingletonService>()
                .AddTransient<TransientService>()
                .AddDbContextFactory<WoolacombeContext>(
                    (p, b) =>
                    {
                        Assert.NotNull(p.GetService<SingletonService>());
                        Assert.NotNull(p.GetService<TransientService>());

                        b.UseInMemoryDatabase(nameof(WoolacombeContext));
                    })
                .BuildServiceProvider(validateScopes: true);

            var contextFactory = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>();

            using var context1 = contextFactory.CreateDbContext();
            using var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));
        }

        [ConditionalFact]
        public void Throws_if_dependencies_are_not_in_DI()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<WestwardHoContext>(
                    b => b.UseInMemoryDatabase(nameof(WestwardHoContext)))
                .BuildServiceProvider(validateScopes: true);

            var factory = serviceProvider.GetService<IDbContextFactory<WestwardHoContext>>();

            Assert.Contains(
                typeof(Random).FullName,
                Assert.Throws<InvalidOperationException>(() => factory.CreateDbContext()).Message);
        }

        private class WestwardHoContext : DbContext
        {
            public WestwardHoContext(DbContextOptions options, Random random)
                : base(options)
            {
            }
        }

        [ConditionalFact]
        public void Can_register_factories_for_multiple_contexts_even_with_non_generic_options()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<CroydeContext>(
                    b => b.UseInMemoryDatabase(nameof(CroydeContext)))
                .AddDbContextFactory<ClovellyContext>(
                    b => b.UseInMemoryDatabase(nameof(ClovellyContext)))
                .BuildServiceProvider(validateScopes: true);

            using var context1 = serviceProvider.GetService<IDbContextFactory<CroydeContext>>().CreateDbContext();
            using var context2 = serviceProvider.GetService<IDbContextFactory<ClovellyContext>>().CreateDbContext();

            Assert.Equal(nameof(CroydeContext), GetStoreName(context1));
            Assert.Equal(nameof(ClovellyContext), GetStoreName(context2));
        }

        private class ClovellyContext : DbContext
        {
            public ClovellyContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        [ConditionalFact]
        public void Can_register_factories_for_multiple_contexts()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<WidemouthBayContext>(
                    b => b.UseInMemoryDatabase(nameof(WidemouthBayContext)))
                .AddDbContextFactory<WoolacombeContext>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)))
                .BuildServiceProvider(validateScopes: true);

            using var context1 = serviceProvider.GetService<IDbContextFactory<WidemouthBayContext>>().CreateDbContext();
            using var context2 = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>().CreateDbContext();

            Assert.Equal(nameof(WidemouthBayContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));
        }

        private class WidemouthBayContext : DbContext
        {
            public WidemouthBayContext(DbContextOptions<WidemouthBayContext> options)
                : base(options)
            {
            }
        }

        [ConditionalFact]
        public void Application_can_register_explicit_factory_implementation()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddSingleton<IDbContextFactory<WoolacombeContext>, WoolacombeContextFactory>()
                .AddDbContextFactory<WoolacombeContext>(b => b.UseInMemoryDatabase(nameof(WoolacombeContext)))
                .BuildServiceProvider(validateScopes: true);

            var contextFactory = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>();

            Assert.IsType<WoolacombeContextFactory>(contextFactory);

            using var context1 = contextFactory.CreateDbContext();
            using var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));
        }

        [ConditionalFact]
        public void Application_can_register_factory_implementation_in_AddDbContextFactory()
        {
            var serviceProvider = (IServiceProvider)new ServiceCollection()
                .AddDbContextFactory<WoolacombeContext, WoolacombeContextFactory>(
                    b => b.UseInMemoryDatabase(nameof(WoolacombeContext)))
                .BuildServiceProvider(validateScopes: true);

            var contextFactory = serviceProvider.GetService<IDbContextFactory<WoolacombeContext>>();

            Assert.IsType<WoolacombeContextFactory>(contextFactory);

            using var context1 = contextFactory.CreateDbContext();
            using var context2 = contextFactory.CreateDbContext();

            Assert.NotSame(context1, context2);
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context1));
            Assert.Equal(nameof(WoolacombeContext), GetStoreName(context2));
        }

        private class WoolacombeContextFactory : IDbContextFactory<WoolacombeContext>
        {
            private readonly DbContextOptions<WoolacombeContext> _options;

            public WoolacombeContextFactory(DbContextOptions<WoolacombeContext> options)
                => _options = options;

            public WoolacombeContext CreateDbContext()
                => new WoolacombeContext(_options);
        }

        private static string GetStoreName(DbContext context1)
            => context1.GetService<IDbContextOptions>().FindExtension<InMemoryOptionsExtension>().StoreName;
    }
}
