// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.Logging;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    ///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EntityFrameworkServiceCollectionExtensions
    {
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
        ///         The database you are using will also define extension methods that can be called on the returned
        ///         <see cref="EntityFrameworkServicesBuilder" /> to register the services for the database. For example,
        ///         when using EntityFramework.SqlServer you would call
        ///         <c>collection.AddEntityFramework().UseSqlServer(connectionString)</c>.
        ///     </para>
        ///     <para>
        ///         For derived contexts to resolve their services from the <see cref="IServiceProvider" /> you must chain a call
        ///         to the <see cref="EntityFrameworkServicesBuilder.AddDbContext{TContext}" /> method on the returned
        ///         <see cref="EntityFrameworkServicesBuilder" />.
        ///         This will ensure services are resolved from the <see cref="IServiceProvider" />  will be honored.
        ///     </para>
        /// </remarks>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public static EntityFrameworkServicesBuilder AddEntityFramework(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            // TODO: Is this the appropriate way to register listeners?
            serviceCollection
                .AddScoped<IEntityStateListener>(p => p.GetService<INavigationFixer>())
                .AddScoped<IForeignKeyListener>(p => p.GetService<INavigationFixer>())
                .AddScoped<INavigationListener>(p => p.GetService<INavigationFixer>())
                .AddScoped<IKeyListener>(p => p.GetService<INavigationFixer>())
                .AddScoped<IPropertyListener>(p => p.GetService<IChangeDetector>());

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
                .AddSingleton<IMemberMapper, MemberMapper>()
                .AddSingleton<IFieldMatcher, FieldMatcher>()
                .AddSingleton<IOriginalValuesFactory, OriginalValuesFactory>()
                .AddSingleton<IRelationshipsSnapshotFactory, RelationshipsSnapshotFactory>()
                .AddSingleton<IStoreGeneratedValuesFactory, StoreGeneratedValuesFactory>()
                .AddSingleton<IEntityEntryMetadataServices, EntityEntryMetadataServices>()
                .AddSingleton<ICompiledQueryCache, CompiledQueryCache>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton<ICoreConventionSetBuilder, CoreConventionSetBuilder>()
                .AddSingleton<LoggingModelValidator>()
                .AddScoped<IKeyPropagator, KeyPropagator>()
                .AddScoped<INavigationFixer, NavigationFixer>()
                .AddScoped<IStateManager, StateManager>()
                .AddScoped<IInternalEntityEntryFactory, InternalEntityEntryFactory>()
                .AddScoped<IInternalEntityEntryNotifier, InternalEntityEntryNotifier>()
                .AddScoped<IInternalEntityEntrySubscriber, InternalEntityEntrySubscriber>()
                .AddScoped<IValueGenerationManager, ValueGenerationManager>()
                .AddScoped<IEntityQueryProvider, EntityQueryProvider>()
                .AddScoped<IChangeTrackerFactory, ChangeTrackerFactory>()
                .AddScoped<IChangeDetector, ChangeDetector>()
                .AddScoped<IEntityEntryGraphIterator, EntityEntryGraphIterator>()
                .AddScoped<IDbContextServices, DbContextServices>()
                .AddScoped<IDatabaseProviderSelector, DatabaseProviderSelector>()
                .AddScoped<ValueGeneratorSelector>()
                .AddScoped(p => GetContextServices(p).Model)
                .AddScoped(p => GetContextServices(p).Context)
                .AddScoped(p => GetContextServices(p).ContextOptions)
                .AddScoped(p => GetContextServices(p).DatabaseProviderServices)
                .AddScoped(p => GetProviderServices(p).Database)
                .AddScoped(p => GetProviderServices(p).QueryContextFactory)
                .AddScoped(p => GetProviderServices(p).ValueGeneratorSelector)
                .AddScoped(p => GetProviderServices(p).Creator)
                .AddScoped(p => GetProviderServices(p).ConventionSetBuilder)
                .AddScoped(p => GetProviderServices(p).ValueGeneratorCache)
                .AddScoped(p => GetProviderServices(p).ModelSource)
                .AddScoped(p => GetProviderServices(p).ModelValidator)
                .AddSingleton<IMemoryCache, MemoryCache>()
                .AddOptions());

            return new EntityFrameworkServicesBuilder(serviceCollection);
        }

        private static IDbContextServices GetContextServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IDbContextServices>();

        private static IDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
            => GetContextServices(serviceProvider).DatabaseProviderServices;
    }
}
