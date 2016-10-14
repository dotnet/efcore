// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public abstract class EntityFrameworkServiceCollectionExtensionsTest : IDisposable
    {
        [Fact]
        public virtual void Repeated_calls_to_add_do_not_modify_collection()
        {
            var expectedCollection = AddServices(new ServiceCollection());

            var actualCollection = AddServices(AddServices(new ServiceCollection()));

            Assert.Equal(expectedCollection.Count, actualCollection.Count);
        }

        [Fact]
        public virtual void Services_wire_up_correctly()
        {
            // Listeners
            VerifyScoped<IEntityStateListener>(2, isExistingReplaced: true);
            VerifyScoped<INavigationListener>(isExistingReplaced: true);
            VerifyScoped<IKeyListener>(isExistingReplaced: true);
            VerifyScoped<IPropertyListener>(isExistingReplaced: true);

            VerifySingleton<IDbSetFinder>();
            VerifySingleton<IDbSetInitializer>();
            VerifySingleton<IDbSetSource>();
            VerifySingleton<IEntityMaterializerSource>();
            VerifySingleton<ILoggerFactory>();
            VerifySingleton<ICoreConventionSetBuilder>();
            VerifySingleton<ExecutionStrategyFactory>();
            VerifySingleton<NoopModelValidator>();

            VerifyScoped<CoreModelValidator>();
            VerifyScoped<IKeyPropagator>();
            VerifyScoped<INavigationFixer>();
            VerifyScoped<IStateManager>();
            VerifyScoped<IInternalEntityEntryFactory>();
            VerifyScoped<IInternalEntityEntryNotifier>();
            VerifyScoped<IInternalEntityEntrySubscriber>();
            VerifyScoped<IValueGenerationManager>();
            VerifyScoped<IChangeTrackerFactory>();
            VerifyScoped<IChangeDetector>();
            VerifyScoped<IEntityEntryGraphIterator>();
            VerifyScoped<IDbContextServices>();
            VerifyScoped<IDatabaseProviderSelector>();
            VerifyScoped<ValueGeneratorSelector>();
            VerifyScoped<IExecutionStrategyFactory>();

            VerifyScoped<IModel>();
            VerifyScoped<ICurrentDbContext>();
            VerifyScoped<IDbContextOptions>();
            VerifyScoped<IDatabaseProviderServices>();
            VerifyScoped<IDatabase>();
            VerifyScoped<IValueGeneratorSelector>();
            VerifyScoped<IDatabaseCreator>();
            VerifyOptionalScoped<IConventionSetBuilder>();
            VerifyScoped<IValueGeneratorCache>();
            VerifyScoped<IModelSource>();
            VerifyScoped<IModelValidator>();
            VerifySingleton<IDatabaseProvider>(isExistingReplaced: true);

            // Query
            VerifySingleton<IMemoryCache>();

            VerifyScoped<ICompiledQueryCache>();
            VerifyScoped<IAsyncQueryProvider>();
            VerifyScoped<IQueryContextFactory>();
            VerifyScoped<IQueryCompiler>();
            VerifyScoped<IQueryCompilationContextFactory>();
            VerifyScoped<ICompiledQueryCacheKeyGenerator>();
            VerifyScoped<CompiledQueryCacheKeyGenerator>();
        }

        protected virtual void AssertServicesSame(IServiceCollection services1, IServiceCollection services2)
        {
            var sortedServices1 = services1
                .OrderBy(s => s.ServiceType.GetHashCode())
                .ToList();

            var sortedServices2 = services2
                .OrderBy(s => s.ServiceType.GetHashCode())
                .ToList();

            Assert.Equal(sortedServices1.Count, sortedServices2.Count);

            for (var i = 0; i < sortedServices1.Count; i++)
            {
                Assert.Equal(sortedServices1[i].ServiceType, sortedServices2[i].ServiceType);
                Assert.Equal(sortedServices1[i].ImplementationType, sortedServices2[i].ImplementationType);
                Assert.Equal(sortedServices1[i].Lifetime, sortedServices2[i].Lifetime);
            }
        }

        private readonly TestHelpers _testHelpers;
        private readonly DbContext _firstContext;
        private readonly DbContext _secondContext;

        protected EntityFrameworkServiceCollectionExtensionsTest(TestHelpers testHelpers)
        {
            _testHelpers = testHelpers;

            var serviceProvider = AddServices(new ServiceCollection()).BuildServiceProvider();
            _firstContext = _testHelpers.CreateContext(serviceProvider);
            _secondContext = _testHelpers.CreateContext(serviceProvider);
        }

        private IServiceCollection AddServices(IServiceCollection serviceCollection)
            => _testHelpers.AddProviderServices(serviceCollection);

        public void Dispose()
        {
            _firstContext.Dispose();
            _secondContext.Dispose();
        }

        protected TService VerifySingleton<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(isExistingReplaced, isSingleton: true, isRequired: true);
        }

        protected TService VerifyScoped<TService>(int count = 1, bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(isExistingReplaced, isSingleton: false, isRequired: true, count: count);
        }

        protected TService VerifyOptionalSingleton<TService>()
            where TService : class
        {
            return VerifyService<TService>(isExistingReplaced: false, isSingleton: true, isRequired: false);
        }

        protected TService VerifyOptionalScoped<TService>()
            where TService : class
        {
            return VerifyService<TService>(isExistingReplaced: false, isSingleton: false, isRequired: false);
        }

        private TService VerifyService<TService>(
            bool isExistingReplaced,
            bool isSingleton,
            bool isRequired,
            int count = 1)
            where TService : class
        {
            var provider = ((IInfrastructure<IServiceProvider>)_firstContext).Instance;
            var service = provider.GetService<TService>();
            if (isRequired)
            {
                Assert.NotNull(service);
            }

            Assert.Same(service, provider.GetService<TService>());

            var otherScopeService = ((IInfrastructure<IServiceProvider>)_secondContext).Instance.GetService<TService>();

            if (isSingleton)
            {
                Assert.Same(service, otherScopeService);
            }
            Assert.Equal(count, ((IInfrastructure<IServiceProvider>)_firstContext).Instance.GetServices<TService>().Count());

            if (typeof(TService) != typeof(IDbContextServices))
            {
                var customServiceCollection = AddServices(new ServiceCollection().AddSingleton(p => service));

                using (var customContext = _testHelpers.CreateContext(customServiceCollection.BuildServiceProvider()))
                {
                    var serviceProviderWithCustomService = ((IInfrastructure<IServiceProvider>)customContext).Instance;
                    if (isExistingReplaced)
                    {
                        Assert.NotSame(service, serviceProviderWithCustomService.GetService<TService>());
                        Assert.Equal(count + 1, serviceProviderWithCustomService.GetServices<TService>().Count());
                    }
                    else
                    {
                        Assert.Same(service, serviceProviderWithCustomService.GetService<TService>());
                        Assert.Equal(1, serviceProviderWithCustomService.GetServices<TService>().Count());
                    }
                }
            }

            return service;
        }
    }
}
