// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    ///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EntityFrameworkServiceCollectionExtensions
    {
        private const int ConfigurationOrder = -1000; // OptionsConstants is internal.

        /// <summary>
        ///     Adds the services required by the core of Entity Framework to an <see cref="IServiceCollection" />.
        ///     You use this method when using dependency injection in your application, such as with ASP.NET.
        ///     For more information on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external <see cref="IServiceProvider" />. If you are not using an external
        ///         <see cref="IServiceProvider" /> Entity Framework will take care of creating the services it requires.
        ///     </para>
        ///     <para>
        ///         The data store you are using will also define extension methods that can be called on the returned
        ///         <see cref="EntityFrameworkServicesBuilder" /> to register the services for the data store. For example,
        ///         when using EntityFramework.SqlServer you would call
        ///         <c>collection.AddEntityFramework(config).UseSqlServer()</c>.
        ///     </para>
        ///     <para>
        ///         For derived contexts to resolve their services from the <see cref="IServiceProvider" /> you must chain a call
        ///         to the <see cref="EntityFrameworkServicesBuilder.AddDbContext{TContext}" /> method on the returned
        ///         <see cref="EntityFrameworkServicesBuilder" />.
        ///         This will ensure services are resolved from the <see cref="IServiceProvider" /> and any Entity Framework
        ///         configuration from the supplied <paramref name="configuration" /> will be honored.
        ///     </para>
        /// </remarks>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="configuration">
        ///     <para>
        ///         The configuration being used for the current application. Providing this allows configuration under the
        ///         'entityFramework' node to be applied to contexts that are resolved from the <see cref="IServiceProvider" />.
        ///         For this configuration to be applied you must register any derived contexts using the
        ///         <see cref="EntityFrameworkServicesBuilder.AddDbContext{TContext}" /> method on the returned
        ///         <see cref="EntityFrameworkServicesBuilder" />.
        ///     </para>
        /// </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
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
                .AddSingleton<IDbSetFinder, DbSetFinder>()
                .AddSingleton<IDbSetInitializer, DbSetInitializer>()
                .AddSingleton<IDbSetSource, DbSetSource>()
                .AddSingleton<IEntityKeyFactorySource, EntityKeyFactorySource>()
                .AddSingleton<IClrAccessorSource<IClrPropertyGetter>, ClrPropertyGetterSource>()
                .AddSingleton<IClrAccessorSource<IClrPropertySetter>, ClrPropertySetterSource>()
                .AddSingleton<IClrCollectionAccessorSource, ClrCollectionAccessorSource>()
                .AddSingleton<ICollectionTypeFactory, CollectionTypeFactory>()
                .AddSingleton<IEntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<IModelValidator, LoggingModelValidator>()
                .AddSingleton<IMemberMapper, MemberMapper>()
                .AddSingleton<IFieldMatcher, FieldMatcher>()
                .AddSingleton<IOriginalValuesFactory, OriginalValuesFactory>()
                .AddSingleton<IRelationshipsSnapshotFactory, RelationshipsSnapshotFactory>()
                .AddSingleton<IStoreGeneratedValuesFactory,StoreGeneratedValuesFactory>()
                .AddSingleton<IEntityEntryMetadataServices,EntityEntryMetadataServices>()
                .AddSingleton<ICompiledQueryCache, CompiledQueryCache>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton<IDbContextOptionsParser, DbContextOptionsParser>()
                .AddSingleton<IBoxedValueReaderSource, BoxedValueReaderSource>()
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
                .AddScoped<IDataStoreSelector, DataStoreSelector>()
                .AddScoped(DataStoreServiceFactories.DataStoreFactory)
                .AddScoped(DataStoreServiceFactories.QueryContextFactoryFactory)
                .AddScoped(DataStoreServiceFactories.ConnectionFactory)
                .AddScoped(DataStoreServiceFactories.DatabaseFactoryFactory)
                .AddScoped(DataStoreServiceFactories.ValueGeneratorSelectorFactory)
                .AddScoped(DataStoreServiceFactories.DataStoreCreatorFactory)
                .AddScoped(DataStoreServiceFactories.ModelBuilderFactoryFactory)
                .AddTransient<IMemoryCache, MemoryCache>()
                .AddOptions());

            return new EntityFrameworkServicesBuilder(serviceCollection, configuration);
        }
    }
}
