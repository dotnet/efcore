// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
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

            serviceCollection
                .AddSingleton<IModelSource, DefaultModelSource>()
                .AddSingleton<ModelBuilderFactory>()
                .AddSingleton<SimpleValueGeneratorFactory<TemporaryValueGenerator>>()
                .AddSingleton<SimpleValueGeneratorFactory<GuidValueGenerator>>()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetInitializer>()
                .AddSingleton<EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
                .AddSingleton<CollectionTypeFactory>()
                .AddSingleton<EntityMaterializerSource>()
                .AddSingleton<CompositeEntityKeyFactory>()
                .AddSingleton<ForeignKeyValueGenerator>()
                .AddSingleton<MemberMapper>()
                .AddSingleton<FieldMatcher>()
                .AddSingleton<OriginalValuesFactory>()
                .AddSingleton<RelationshipsSnapshotFactory>()
                .AddSingleton<StoreGeneratedValuesFactory>()
                .AddSingleton<ValueGeneratorSelector>()
                .AddSingleton<StateEntryMetadataServices>()
                .AddScoped<DataStoreSelector>()
                .AddScoped<StateEntryFactory>()
                .AddScoped<IEntityStateListener, NavigationFixer>()
                .AddScoped<StateEntryNotifier>()
                .AddScoped<ChangeDetector>()
                .AddScoped<StateEntrySubscriber>()
                .AddScoped<DbContextConfiguration>()
                .AddScoped<ContextSets>()
                .AddScoped<StateManager>()
                .AddScoped<LazyRef<IModel>>(DbContextConfiguration.ModelFactory)
                .AddScoped<LazyRef<DbContext>>(DbContextConfiguration.ContextFactory)
                .AddScoped<LazyRef<IDbContextOptions>>(DbContextConfiguration.ContextOptionsFactory)
                .AddScoped<LazyRef<DataStore>>(DataStoreServices.DataStoreFactory)
                .AddScoped<LazyRef<DataStoreConnection>>(DataStoreServices.ConnectionFactory)
                .AddScoped<LazyRef<Database>>(DataStoreServices.DatabaseFactory)
                .AddScoped<LazyRef<ValueGeneratorCache>>(DataStoreServices.ValueGeneratorCacheFactory)
                .AddScoped<LazyRef<DataStoreCreator>>(DataStoreServices.DataStoreCreatorFactory);

            EnsureLowLevelServices(serviceCollection);

            return new EntityServicesBuilder(serviceCollection, configuration);
        }

        private static void EnsureLowLevelServices(IServiceCollection serviceCollection)
        {
            var requiredServices = new List<Tuple<Type, Action<IServiceCollection>>>
                {
                    Tuple.Create<Type, Action<IServiceCollection>>(typeof(ILoggerFactory), c => c.AddSingleton<ILoggerFactory, LoggerFactory>()),
                    Tuple.Create<Type, Action<IServiceCollection>>(typeof(ITypeActivator), c => c.AddSingleton<ITypeActivator, TypeActivator>()),
                    Tuple.Create<Type, Action<IServiceCollection>>(typeof(IOptions<>), c => c.Add(OptionsServices.GetDefaultServices())),
                };

            foreach (var descriptor in serviceCollection)
            {
                foreach (var serviceTuple in requiredServices)
                {
                    if (serviceTuple.Item1 == descriptor.ServiceType)
                    {
                        requiredServices.Remove(serviceTuple);
                        break;
                    }
                }

                if (!requiredServices.Any())
                {
                    break;
                }
            }

            foreach (var serviceTuple in requiredServices)
            {
                serviceTuple.Item2(serviceCollection);
            }
        }

        public static EntityServicesBuilder AddDbContext<TContext>(
            [NotNull] this EntityServicesBuilder builder,
            [CanBeNull] Action<DbContextOptions> optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection.AddSingleton<DbContextOptions<TContext>>(
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

            builder.ServiceCollection.AddScoped(typeof(TContext), DbContextActivator.CreateInstance<TContext>);

            return builder;
        }
    }
}
