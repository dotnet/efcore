// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityServiceCollectionExtensionsTest : IDisposable
    {
        [Fact]
        public virtual void Services_wire_up_correctly()
        {
            VerifySingleton<TemporaryIntegerValueGeneratorFactory>();
            VerifySingleton<ValueGeneratorFactory<TemporaryStringValueGenerator>>();
            VerifySingleton<ValueGeneratorFactory<TemporaryBinaryValueGenerator>>();
            VerifySingleton<ValueGeneratorFactory<GuidValueGenerator>>();
            VerifySingleton<ValueGeneratorFactory<SequentialGuidValueGenerator>>();
            VerifySingleton<DbSetFinder>();
            VerifySingleton<DbSetInitializer>();
            VerifySingleton<DbSetSource>();
            VerifySingleton<EntityKeyFactorySource>();
            VerifySingleton<ClrPropertyGetterSource>();
            VerifySingleton<ClrPropertySetterSource>();
            VerifySingleton<ClrCollectionAccessorSource>();
            VerifySingleton<CollectionTypeFactory>();
            VerifySingleton<EntityMaterializerSource>();
            VerifySingleton<ModelValidator>();
            VerifySingleton<MemberMapper>();
            VerifySingleton<FieldMatcher>();
            VerifySingleton<OriginalValuesFactory>();
            VerifySingleton<RelationshipsSnapshotFactory>();
            VerifySingleton<StoreGeneratedValuesFactory>();
            VerifySingleton<EntityEntryMetadataServices>();
            VerifySingleton<ICompiledQueryCache>();
            VerifySingleton<ILoggerFactory>();
            VerifySingleton<ITypeActivator>();

            VerifyCached<IModelBuilderFactory>();
            VerifyCached<IModel>();

            VerifyScoped<KeyPropagator>();
            VerifyScoped<NavigationFixer>();
            VerifyScoped<StateManager>();
            VerifyScoped<InternalEntityEntryFactory>();
            VerifyScoped<InternalEntityEntryNotifier>();
            VerifyScoped<InternalEntityEntrySubscriber>();
            VerifyScoped<ValueGenerationManager>();
            VerifyScoped<EntityQueryProvider>();
            VerifyScoped<ChangeTracker>();
            VerifyScoped<ChangeDetector>();
            VerifyScoped<EntityEntryGraphIterator>();
            VerifyScoped<DbContextServices>();
            VerifyScoped<DbContext>();
            VerifyScoped<IDbContextOptions>();
            VerifyScoped<IDataStoreSelector>();
            VerifyScoped<IDataStore>();
            VerifyScoped<IDataStoreConnection>();
            VerifyScoped<Database>();
            VerifyScoped<IValueGeneratorSelector>();
            VerifyScoped<IDataStoreCreator>();

            VerifyScoped<IEntityStateListener>(isExistingReplaced: true);
            VerifyScoped<IForeignKeyListener>(isExistingReplaced: true);
            VerifyScoped<INavigationListener>(isExistingReplaced: true);
            VerifyScoped<IKeyListener>(isExistingReplaced: true);
            VerifyScoped<IPropertyListener>(isExistingReplaced: true);
        }

        protected void VerifyCommonDataStoreServices()
        {
            VerifyScoped<IDataStoreSource>(isExistingReplaced: true);
            Assert.NotNull(VerifyCached<IModel>());
            Assert.NotNull(VerifyScoped<DbContext>());
            Assert.NotNull(VerifyScoped<IDbContextOptions>());
            Assert.NotNull(VerifyScoped<IDataStore>());
            Assert.NotNull(VerifyScoped<IDataStoreConnection>());
            Assert.NotNull(VerifyScoped<Database>());
            Assert.NotNull(VerifyScoped<IValueGeneratorSelector>());
            Assert.NotNull(VerifyScoped<IDataStoreCreator>());
            Assert.NotNull(VerifySingleton<IModelBuilderFactory>());
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContext _firstContext;
        private readonly DbContext _secondContext;

        public EntityServiceCollectionExtensionsTest()
        {
            _serviceProvider = GetServices().BuildServiceProvider();
            _firstContext = CreateContext(_serviceProvider);
            _secondContext = CreateContext(_serviceProvider);
        }

        protected virtual IServiceCollection GetServices(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection())
                .AddEntityFramework()
                .AddInMemoryStore()
                .ServiceCollection();
        }

        protected virtual DbContext CreateContext(IServiceProvider serviceProvider)
        {
            return TestHelpers.Instance.CreateContext(serviceProvider);
        }

        public void Dispose()
        {
            _firstContext.Dispose();
            _secondContext.Dispose();
        }

        protected TService VerifySingleton<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(((IAccessor<IServiceProvider>)_firstContext).Service, isExistingReplaced, isScoped: false, isCached: false);
        }

        protected TService VerifyScoped<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(((IAccessor<IServiceProvider>)_firstContext).Service, isExistingReplaced, isScoped: true, isCached: false);
        }

        protected TService VerifyCached<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(((IAccessor<IServiceProvider>)_firstContext).Service, isExistingReplaced, isScoped: true, isCached: true);
        }

        private TService VerifyService<TService>(IServiceProvider serviceProvider, bool isExistingReplaced, bool isScoped, bool isCached)
            where TService : class
        {
            var service = serviceProvider.GetRequiredService<TService>();

            Assert.NotNull(service);
            Assert.Same(service, serviceProvider.GetRequiredService<TService>());

            var scopedService = ((IAccessor<IServiceProvider>)_secondContext).Service.GetRequiredService<TService>();

            if (isCached)
            {
                Assert.Same(service, scopedService);
            }
            else if (isScoped)
            {
                Assert.NotSame(service, scopedService);
            }
            else
            {
                Assert.Same(service, scopedService);
            }
            Assert.Equal(1, serviceProvider.GetRequiredService<IEnumerable<TService>>().Count());

            if (typeof(TService) != typeof(DbContextServices))
            {
                var customServices = isScoped
                    ? new ServiceCollection().AddScoped(p => service)
                    : new ServiceCollection().AddSingleton(p => service);

                var serviceProviderWithCustomService = ((IAccessor<IServiceProvider>)new DbContext(GetServices(customServices).BuildServiceProvider())).Service;
                if (isExistingReplaced)
                {
                    Assert.NotSame(service, serviceProviderWithCustomService.GetRequiredService<TService>());
                    Assert.Equal(2, serviceProviderWithCustomService.GetRequiredService<IEnumerable<TService>>().Count());
                }
                else
                {
                    Assert.Same(service, serviceProviderWithCustomService.GetRequiredService<TService>());
                    Assert.Equal(1, serviceProviderWithCustomService.GetRequiredService<IEnumerable<TService>>().Count());
                }
            }

            return service;
        }
    }
}
