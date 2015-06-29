// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTest : IDisposable
    {
        [Fact]
        public virtual void Services_wire_up_correctly()
        {
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
            VerifySingleton<ICompiledQueryCache>();
            VerifySingleton<ILoggerFactory>();
            VerifySingleton<ICoreConventionSetBuilder>();
            VerifySingleton<LoggingModelValidator>();
            VerifySingleton<IMemoryCache>();

            VerifyScoped<IKeyPropagator>();
            VerifyScoped<INavigationFixer>();
            VerifyScoped<IStateManager>();
            VerifyScoped<IInternalEntityEntryFactory>();
            VerifyScoped<IInternalEntityEntryNotifier>();
            VerifyScoped<IInternalEntityEntrySubscriber>();
            VerifyScoped<IValueGenerationManager>();
            VerifyScoped<IEntityQueryProvider>();
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
            VerifyScoped<IQueryContextFactory>();
            VerifyScoped<IValueGeneratorSelector>();
            VerifyScoped<IDatabaseCreator>();
            VerifyOptionalScoped<IConventionSetBuilder>();
            VerifyScoped<IValueGeneratorCache>();
            VerifyScoped<IModelSource>();
            VerifyScoped<IModelValidator>();
            VerifySingleton<IDatabaseProvider>(isExistingReplaced: true);
        }

        private readonly DbContext _firstContext;
        private readonly DbContext _secondContext;

        public EntityFrameworkServiceCollectionExtensionsTest()
        {
            var serviceProvider = GetServices().BuildServiceProvider();
            _firstContext = CreateContext(serviceProvider);
            _secondContext = CreateContext(serviceProvider);
        }

        protected virtual IServiceCollection GetServices(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection())
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .GetService();
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
            Assert.Equal(1, ((IAccessor<IServiceProvider>)_firstContext).Service.GetRequiredServices<TService>().Count());

            if (typeof(TService) != typeof(IDbContextServices))
            {
                var customServices = new ServiceCollection().AddSingleton(p => service);

                var serviceProviderWithCustomService = ((IAccessor<IServiceProvider>)CreateContext(GetServices(customServices).BuildServiceProvider())).Service;
                if (isExistingReplaced)
                {
                    Assert.NotSame(service, serviceProviderWithCustomService.GetService<TService>());
                    Assert.Equal(2, serviceProviderWithCustomService.GetRequiredServices<TService>().Count());
                }
                else
                {
                    Assert.Same(service, serviceProviderWithCustomService.GetService<TService>());
                    Assert.Equal(1, serviceProviderWithCustomService.GetRequiredServices<TService>().Count());
                }
            }

            return service;
        }
    }
}
