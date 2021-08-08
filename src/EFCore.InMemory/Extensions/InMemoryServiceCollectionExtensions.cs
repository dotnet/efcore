// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     In-memory specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class InMemoryServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Registers the given Entity Framework <see cref="DbContext"/> as a service in the <see cref="IServiceCollection" />
        ///         and configures it to use the Entity Framework in-memory database.
        ///     </para>
        ///     <para>
        ///         This method is a shortcut for configuring a <see cref="DbContext"/> to use the in-memory database. It does not support
        ///         all options. Use <see cref="M:EntityFrameworkServiceCollectionExtensions.AddDbContext"/> and related methods for full
        ///         control of this process.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure the in-memory database provider.
        ///     </para>
        ///     <para>
        ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply 
        ///         an optional action to configure the <see cref="DbContextOptions" /> for the context. 
        ///     </para>
        ///     <para>
        ///         For more information on how to use this method, see the Entity Framework Core documentation at https://aka.ms/efdocs.
        ///         For more information on using dependency injection, see https://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="databaseName">
        ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
        ///     independently of the context. The in-memory database is shared anywhere the same name is used.
        /// </param>
        /// <param name="inMemoryOptionsAction"> An optional action to allow additional in-memory database specific configuration. </param>
        /// <param name="optionsAction"> An optional action to configure the <see cref="DbContextOptions" /> for the context. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddInMemoryDatabase<TContext>(
            this IServiceCollection serviceCollection,
            string databaseName,
            Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null,
            Action<DbContextOptionsBuilder>? optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            Check.NotEmpty(databaseName, nameof(databaseName));

            return serviceCollection.AddDbContext<TContext>(
                (serviceProvider, options) =>
                {
                    optionsAction?.Invoke(options);
                    options.UseInMemoryDatabase(databaseName, inMemoryOptionsAction);
                });
        }
        
        /// <summary>
        ///     <para>
        ///         Adds the services required by the in-memory database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Warning: Do not call this method accidentally. It is much more likely you need
        ///         to call <see cref="AddInMemoryDatabase{TContext}"/>.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IServiceCollection AddEntityFrameworkInMemoryDatabase(this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, InMemoryLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<InMemoryOptionsExtension>>()
                .TryAdd<IValueGeneratorSelector, InMemoryValueGeneratorSelector>()
                .TryAdd<IDatabase>(p => p.GetRequiredService<IInMemoryDatabase>())
                .TryAdd<IDbContextTransactionManager, InMemoryTransactionManager>()
                .TryAdd<IDatabaseCreator, InMemoryDatabaseCreator>()
                .TryAdd<IQueryContextFactory, InMemoryQueryContextFactory>()
                .TryAdd<IProviderConventionSetBuilder, InMemoryConventionSetBuilder>()
                .TryAdd<IModelValidator, InMemoryModelValidator>()
                .TryAdd<ITypeMappingSource, InMemoryTypeMappingSource>()
                .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, InMemoryShapedQueryCompilingExpressionVisitorFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, InMemoryQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<ISingletonOptions, IInMemorySingletonOptions>(p => p.GetRequiredService<IInMemorySingletonOptions>())
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<IInMemorySingletonOptions, InMemorySingletonOptions>()
                        .TryAddSingleton<IInMemoryStoreCache, InMemoryStoreCache>()
                        .TryAddSingleton<IInMemoryTableFactory, InMemoryTableFactory>()
                        .TryAddScoped<IInMemoryDatabase, InMemoryDatabase>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
