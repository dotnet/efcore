// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class EntityServiceCollectionExtensions
    {
        private const int ConfigurationOrder = -1000; // OptionsConstants is internal.

        public static EntityServicesBuilder AddEntityFramework(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] IConfiguration configuration = null)
        {
            Check.NotNull(serviceCollection, "serviceCollection");

            // TODO: Is this the appropriate way to register listeners?
            serviceCollection
                .AddScoped<IEntityStateListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<IRelationshipListener>(p => p.GetService<NavigationFixer>())
                .AddScoped<IPropertyListener>(p => p.GetService<ChangeDetector>());

            serviceCollection.TryAdd(new ServiceCollection()
                .AddSingleton<IModelSource, DefaultModelSource>()
                .AddSingleton<ModelBuilderFactory>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryIntegerValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryStringValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryBinaryValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<GuidValueGenerator>>()
                .AddSingleton<EntityAttacherFactory>()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetInitializer>()
                .AddSingleton<DbSetSource>()
                .AddSingleton<EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
                .AddSingleton<CollectionTypeFactory>()
                .AddSingleton<EntityMaterializerSource>()
                .AddSingleton<CompositeEntityKeyFactory>()
                .AddSingleton<MemberMapper>()
                .AddSingleton<FieldMatcher>()
                .AddSingleton<OriginalValuesFactory>()
                .AddSingleton<RelationshipsSnapshotFactory>()
                .AddSingleton<StoreGeneratedValuesFactory>()
                .AddSingleton<ValueGeneratorSelector>()
                .AddSingleton<StateEntryMetadataServices>()
                .AddScoped<ForeignKeyValuePropagator>()
                .AddScoped<DataStoreSelector>()
                .AddScoped<StateEntryFactory>()
                .AddScoped<NavigationFixer>()
                .AddScoped<ChangeDetector>()
                .AddScoped<StateEntryNotifier>()
                .AddScoped<StateEntrySubscriber>()
                .AddScoped<DbContextServices>()
                .AddScoped<StateManager>()
                .AddScoped<ValueGenerationManager>()
                .AddScoped<EntityQueryExecutor>()
                .AddScoped<ChangeTracker>()
                .AddScoped<EntityEntryGraphIterator>()
                .AddScoped(DbContextServices.ModelFactory)
                .AddScoped(DbContextServices.ContextFactory)
                .AddScoped(DbContextServices.ContextOptionsFactory)
                .AddScoped(DataStoreServices.DataStoreServicesFactory)
                .AddScoped(DataStoreServices.DataStoreFactory)
                .AddScoped(DataStoreServices.ConnectionFactory)
                .AddScoped(DataStoreServices.DatabaseFactory)
                .AddScoped(DataStoreServices.ValueGeneratorCacheFactory)
                .AddScoped(DataStoreServices.DataStoreCreatorFactory)
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddTypeActivator()
                .AddOptions());

            return new EntityServicesBuilder(serviceCollection, configuration);
        }

        public static EntityServicesBuilder AddDbContext<TContext>(
            [NotNull] this EntityServicesBuilder builder,
            [CanBeNull] Action<DbContextOptions> optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection.AddSingleton(
                sp => sp.GetRequiredServiceChecked<IOptions<DbContextOptions<TContext>>>().Options);

            if (builder.Configuration != null)
            {
                // TODO: Allows parser to be obtained from service provider. Issue #947
                builder.ServiceCollection.ConfigureOptions(
                    new DbContextConfigureOptions<TContext>(builder.Configuration, new DbContextOptionsParser())
                        {
                            Order = ConfigurationOrder
                        });
            }

            if (optionsAction != null)
            {
                builder.ServiceCollection.Configure<DbContextOptions<TContext>>(optionsAction);
            }

            ServiceCollectionExtensions.AddScoped(builder.ServiceCollection, typeof(TContext), (Func<IServiceProvider, object>)(DbContextActivator.CreateInstance<TContext>));

            return builder;
        }
    }
}
