// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class EntityServiceCollectionExtensions
    {
        private const int ConfigurationOrder = -1000; // OptionsConstants is internal.

        public static EntityFrameworkServicesBuilder AddEntityFramework(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] IConfiguration configuration = null)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            // TODO: Is this the appropriate way to register listeners?
            serviceCollection
                .AddScoped<IEntityStateListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<IForeignKeyListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<INavigationListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<IKeyListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<IPropertyListener>(p => p.GetService<ChangeDetector>());

            serviceCollection.TryAdd(new ServiceCollection()
                .AddSingleton<TemporaryIntegerValueGeneratorFactory>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryStringValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<GuidValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<SequentialGuidValueGenerator>>()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetInitializer>()
                .AddSingleton<DbSetSource>()
                .AddSingleton<EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
                .AddSingleton<CollectionTypeFactory>()
                .AddSingleton<EntityMaterializerSource>()
                .AddSingleton<MemberMapper>()
                .AddSingleton<FieldMatcher>()
                .AddSingleton<OriginalValuesFactory>()
                .AddSingleton<RelationshipsSnapshotFactory>()
                .AddSingleton<StoreGeneratedValuesFactory>()
                .AddSingleton<EntityEntryMetadataServices>()
                .AddSingleton<ICompiledQueryCache, CompiledQueryCache>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddTypeActivator()
                .AddScoped<KeyPropagator>()
                .AddScoped<NavigationFixer>()
                .AddScoped<StateManager>()
                .AddScoped<InternalEntityEntryFactory>()
                .AddScoped<InternalEntityEntryNotifier>()
                .AddScoped<InternalEntityEntrySubscriber>()
                .AddScoped<ValueGenerationManager>()
                .AddScoped<EntityQueryProvider>()
                .AddScoped<ChangeTracker>()
                .AddScoped<ChangeDetector>()
                .AddScoped<EntityEntryGraphIterator>()
                .AddScoped<DbContextServices>()
                .AddScoped(DbContextServices.ModelFactory)
                .AddScoped(DbContextServices.ContextFactory)
                .AddScoped(DbContextServices.ContextOptionsFactory)
                .AddScoped<DataStoreSelector>()
                .AddScoped(DataStoreServices.DataStoreServicesFactory)
                .AddScoped(DataStoreServices.DataStoreFactory)
                .AddScoped(DataStoreServices.ConnectionFactory)
                .AddScoped(DataStoreServices.DatabaseFactory)
                .AddScoped(DataStoreServices.ValueGeneratorCacheFactory)
                .AddScoped(DataStoreServices.DataStoreCreatorFactory)
                .AddScoped(DataStoreServices.ModelBuilderFactoryFactory)
                .AddTransient<IMemoryCache, MemoryCache>()
                .AddOptions());

            return new EntityFrameworkServicesBuilder(serviceCollection, configuration);
        }

        public static EntityFrameworkServicesBuilder AddDbContext<TContext>(
            [NotNull] this EntityFrameworkServicesBuilder builder,
            [CanBeNull] Action<DbContextOptions> optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(builder, nameof(builder));

            var serviceCollection = ((IAccessor<IServiceCollection>)builder).Service;

            serviceCollection.AddSingleton(
                sp => sp.GetRequiredServiceChecked<IOptions<DbContextOptions<TContext>>>().Options);

            var configuration = ((IAccessor<IConfiguration>)builder).Service;
            if (configuration != null)
            {
                // TODO: Allows parser to be obtained from service provider. Issue #947
                serviceCollection.ConfigureOptions(
                    new DbContextConfigureOptions<TContext>(configuration, new DbContextOptionsParser())
                        {
                            Order = ConfigurationOrder
                        });
            }

            if (optionsAction != null)
            {
                serviceCollection.Configure<DbContextOptions<TContext>>(optionsAction);
            }

            serviceCollection.AddScoped(typeof(TContext), DbContextActivator.CreateInstance<TContext>);

            return builder;
        }
    }
}
