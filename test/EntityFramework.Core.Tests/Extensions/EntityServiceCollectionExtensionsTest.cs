// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
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
            VerifySingleton<SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator>>();
            VerifySingleton<SimpleValueGeneratorFactory<TemporaryStringValueGenerator>>();
            VerifySingleton<SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator>>();
            VerifySingleton<SimpleValueGeneratorFactory<GuidValueGenerator>>();
            VerifySingleton<EntityAttacherFactory>();
            VerifySingleton<DbSetFinder>();
            VerifySingleton<DbSetInitializer>();
            VerifySingleton<DbSetSource>();
            VerifySingleton<EntityKeyFactorySource>();
            VerifySingleton<ClrPropertyGetterSource>();
            VerifySingleton<ClrPropertySetterSource>();
            VerifySingleton<ClrCollectionAccessorSource>();
            VerifySingleton<CollectionTypeFactory>();
            VerifySingleton<EntityMaterializerSource>();
            VerifySingleton<MemberMapper>();
            VerifySingleton<FieldMatcher>();
            VerifySingleton<OriginalValuesFactory>();
            VerifySingleton<RelationshipsSnapshotFactory>();
            VerifySingleton<StoreGeneratedValuesFactory>();
            VerifySingleton<StateEntryMetadataServices>();
            VerifySingleton<ICompiledQueryCache>();
            VerifySingleton<ILoggerFactory>();
            VerifySingleton<ITypeActivator>();

            VerifyScoped<ForeignKeyValuePropagator>();
            VerifyScoped<NavigationFixer>();
            VerifyScoped<StateManager>();
            VerifyScoped<StateEntryFactory>();
            VerifyScoped<StateEntryNotifier>();
            VerifyScoped<StateEntrySubscriber>();
            VerifyScoped<ValueGenerationManager>();
            VerifyScoped<EntityQueryProvider>();
            VerifyScoped<ChangeTracker>();
            VerifyScoped<ChangeDetector>();
            VerifyScoped<EntityEntryGraphIterator>();
            VerifyScoped<DbContextServices>();
            VerifyScoped<DbContextService<IModel>>();
            VerifyScoped<DbContextService<DbContext>>();
            VerifyScoped<DbContextService<IDbContextOptions>>();
            VerifyScoped<DataStoreSelector>();
            VerifyScoped<DbContextService<DataStoreServices>>();
            VerifyScoped<DbContextService<DataStore>>();
            VerifyScoped<DbContextService<DataStoreConnection>>();
            VerifyScoped<DbContextService<Database>>();
            VerifyScoped<DbContextService<ValueGeneratorCache>>();
            VerifyScoped<DbContextService<DataStoreCreator>>();
            VerifyScoped<DbContextService<ModelBuilderFactory>>();

            var service = _serviceProvider.GetRequiredService<DbContextService<IDbContextOptions>>().Service;

            VerifyScoped<IEntityStateListener>(isExistingReplaced: true);
            VerifyScoped<IRelationshipListener>(isExistingReplaced: true);
            VerifyScoped<IPropertyListener>(isExistingReplaced: true);
        }

        protected void VerifyCommonDataStoreServices()
        {
            VerifyScoped<DataStoreSource>(isExistingReplaced: true);
            Assert.NotNull(VerifyScoped<DbContextService<IModel>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<DbContext>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<IDbContextOptions>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<DataStoreServices>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<DataStore>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<DataStoreConnection>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<Database>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<ValueGeneratorCache>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<DataStoreCreator>>().Service);
            Assert.NotNull(VerifyScoped<DbContextService<ModelBuilderFactory>>().Service);
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
                .AddEntityFramework().ServiceCollection;
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
            return VerifySingleton<TService>(_serviceProvider, isExistingReplaced, isScoped: false);
        }

        protected TService VerifyScoped<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifySingleton<TService>(((IAccessor<IServiceProvider>)_firstContext).Service, isExistingReplaced, isScoped: true);
        }

        private TService VerifySingleton<TService>(IServiceProvider serviceProvider, bool isExistingReplaced, bool isScoped)
            where TService : class
        {
            var service = serviceProvider.GetRequiredService<TService>();

            Assert.NotNull(service);
            Assert.Same(service, serviceProvider.GetRequiredService<TService>());

            var scopedService = ((IAccessor<IServiceProvider>)_secondContext).Service.GetRequiredService<TService>();

            if (isScoped)
            {
                Assert.NotSame(service, scopedService);
            }
            else
            {
                Assert.Same(service, scopedService);
            }
            Assert.Equal(1, serviceProvider.GetRequiredService<IEnumerable<TService>>().Count());

            var customServices = isScoped
                ? new ServiceCollection().AddScoped(p => service)
                : new ServiceCollection().AddSingleton(p => service);
            var serviceProviderWithCustomService = GetServices(customServices).BuildServiceProvider();
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

            return service;
        }
    }
}
