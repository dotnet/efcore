// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Tests
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
            VerifyScoped<IEntityStateListener>(isExistingReplaced: true);
            VerifyScoped<IForeignKeyListener>(isExistingReplaced: true);
            VerifyScoped<INavigationListener>(isExistingReplaced: true);
            VerifyScoped<IKeyListener>(isExistingReplaced: true);
            VerifyScoped<IPropertyListener>(isExistingReplaced: true);

            VerifySingleton<IDbSetFinder>();
            VerifySingleton<IDbSetInitializer>();
            VerifySingleton<IDbSetSource>();
            VerifySingleton<IEntityKeyFactorySource>();
            VerifySingleton<IClrAccessorSource<IClrPropertyGetter>>();
            VerifySingleton<IClrAccessorSource<IClrPropertySetter>>();
            VerifySingleton<IClrCollectionAccessorSource>();
            VerifySingleton<ICollectionTypeFactory>();
            VerifySingleton<IEntityMaterializerSource>();
            VerifySingleton<IMemberMapper>();
            VerifySingleton<IFieldMatcher>();
            VerifySingleton<IOriginalValuesFactory>();
            VerifySingleton<IRelationshipsSnapshotFactory>();
            VerifySingleton<IStoreGeneratedValuesFactory>();
            VerifySingleton<IEntityEntryMetadataServices>();
            VerifySingleton<ILoggerFactory>();
            VerifySingleton<ICoreConventionSetBuilder>();
            VerifySingleton<LoggingModelValidator>();

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

            VerifyScoped<IModel>();
            VerifyScoped<DbContext>();
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
            VerifySingleton<ICompiledQueryCache>();

            VerifyScoped<IEntityQueryProvider>();
            VerifyScoped<IQueryContextFactory>();
            VerifyScoped<IQueryCompiler>();
            VerifyScoped<IQueryCompilationContextFactory>();
            VerifyScoped<ICompiledQueryCacheKeyGenerator>();
            VerifyScoped<CompiledQueryCacheKeyGenerator>();
        }

        private readonly TestHelpers _testHelpers;
        private readonly DbContext _firstContext;
        private readonly DbContext _secondContext;

        public EntityFrameworkServiceCollectionExtensionsTest(TestHelpers testHelpers)
        {
            _testHelpers = testHelpers;

            var serviceProvider = AddServices(new ServiceCollection()).BuildServiceProvider();
            _firstContext = _testHelpers.CreateContext(serviceProvider);
            _secondContext = _testHelpers.CreateContext(serviceProvider);
        }

        private IServiceCollection AddServices(IServiceCollection serviceCollection)
        {
            return _testHelpers.AddProviderServices(serviceCollection.AddEntityFramework()).GetService();
        }

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

        protected TService VerifyScoped<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(isExistingReplaced, isSingleton: false, isRequired: true);
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
            bool isRequired)
            where TService : class
        {
            var provider = ((IAccessor<IServiceProvider>)_firstContext).Service;
            var service = provider.GetService<TService>();
            if (isRequired)
            {
                Assert.NotNull(service);
            }

            Assert.Same(service, provider.GetService<TService>());

            var otherScopeService = ((IAccessor<IServiceProvider>)_secondContext).Service.GetService<TService>();

            if (isSingleton)
            {
                Assert.Same(service, otherScopeService);
            }
            Assert.Equal(1, ((IAccessor<IServiceProvider>)_firstContext).Service.GetServices<TService>().Count());

            if (typeof(TService) != typeof(IDbContextServices))
            {
                var customServiceCollection = AddServices(new ServiceCollection().AddSingleton(p => service));

                using (var customContext = _testHelpers.CreateContext(customServiceCollection.BuildServiceProvider()))
                {
                    var serviceProviderWithCustomService = ((IAccessor<IServiceProvider>)customContext).Service;
                    if (isExistingReplaced)
                    {
                        Assert.NotSame(service, serviceProviderWithCustomService.GetService<TService>());
                        Assert.Equal(2, serviceProviderWithCustomService.GetServices<TService>().Count());
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
