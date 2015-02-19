// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    ///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EntityServiceCollectionExtensions
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
        ///         to the <see cref="AddDbContext{TContext}" /> extension method on the returned
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
        ///         <see cref="AddDbContext{TContext}" /> extension method on the returned  
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
                .AddSingleton<TemporaryIntegerValueGeneratorFactory>()
                .AddSingleton<ValueGeneratorFactory<TemporaryStringValueGenerator>>()
                .AddSingleton<ValueGeneratorFactory<TemporaryBinaryValueGenerator>>()
                .AddSingleton<ValueGeneratorFactory<GuidValueGenerator>>()
                .AddSingleton<ValueGeneratorFactory<SequentialGuidValueGenerator>>()
                .AddSingleton<DbSetFinder>()
                .AddSingleton<DbSetInitializer>()
                .AddSingleton<DbSetSource>()
                .AddSingleton<EntityKeyFactorySource>()
                .AddSingleton<ClrPropertyGetterSource>()
                .AddSingleton<ClrPropertySetterSource>()
                .AddSingleton<ClrCollectionAccessorSource>()
                .AddSingleton<CollectionTypeFactory>()
                .AddSingleton<EntityMaterializerSource>()
                .AddSingleton<ModelValidator, LoggingModelValidator>()
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
                .AddScoped(DataStoreServices.ValueGeneratorSelectorFactory)
                .AddScoped(DataStoreServices.DataStoreCreatorFactory)
                .AddScoped(DataStoreServices.ModelBuilderFactoryFactory)
                .AddTransient<IMemoryCache, MemoryCache>()
                .AddOptions());

            return new EntityFrameworkServicesBuilder(serviceCollection, configuration);
        }

        /// <summary>
        ///     Registers the given context as a service in the <see cref="IServiceCollection" />. 
        ///     You use this method when using dependency injection in your application, such as with ASP.NET.
        ///     For more information on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        /// </summary>
        /// <remarks>
        ///     This method will ensure services that the context uses are resolved from the 
        ///     <see cref="IServiceProvider" /> and any Entity Framework configuration 
        ///     found in the configuration passed to <see cref="AddEntityFramework" /> will be honored.
        /// </remarks>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="builder"> The builder returned from <see cref="AddEntityFramework" />. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         An optional action to configure the <see cref="DbContextOptions" /> for the context. This provides an
        ///         alternative to performing configuration of the context by overriding the 
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context.
        ///     </para>
        ///     <para>
        ///         If an action is supplied here, the <see cref="DbContext.OnConfiguring" /> method will still be run if it has
        ///         been overridden on the derived context. <see cref="DbContext.OnConfiguring" /> configuration will be applied 
        ///         in addition to configuration performed here.
        ///     </para>
        ///     <para>
        ///         You do not need to expose a constructor parameter for the <see cref="DbContextOptions" /> to be passed to the
        ///         context. If you choose to expose a constructor parameter, you must type it as the generic
        ///         <see cref="DbContextOptions{T}" /> as that is the type that will be registered in the 
        ///         <see cref="IServiceCollection" /> (in order to support multiple context types being registered in the 
        ///         same <see cref="IServiceCollection" />).
        ///     </para>
        /// </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
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
