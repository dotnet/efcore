// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

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
        ///         Adds the services required by the in-memory database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />. You use this method when using dependency injection
        ///         in your application, such as with ASP.NET. For more information on setting up dependency
        ///         injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external dependency injection container. If you are not using an external
        ///         dependency injection container, Entity Framework will take care of creating the services it requires.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services)
        ///         {
        ///             services
        ///                 .AddEntityFrameworkInMemoryDatabase()
        ///                 .AddDbContext&lt;MyContext&gt;((serviceProvider, options) =>
        ///                     options.UseInMemoryDatabase("MyDatabase")
        ///                            .UseInternalServiceProvider(serviceProvider));
        ///         }
        ///     </code>
        /// </example>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkInMemoryDatabase([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<InMemoryOptionsExtension>>()
                .TryAdd<IValueGeneratorSelector, InMemoryValueGeneratorSelector>()
                .TryAdd<IDatabase>(p => p.GetService<IInMemoryDatabase>())
                .TryAdd<IDbContextTransactionManager, InMemoryTransactionManager>()
                .TryAdd<IDatabaseCreator, InMemoryDatabaseCreator>()
                .TryAdd<IQueryContextFactory, InMemoryQueryContextFactory>()
                .TryAdd<IEntityQueryModelVisitorFactory, InMemoryQueryModelVisitorFactory>()
                .TryAdd<IEntityQueryableExpressionVisitorFactory, InMemoryEntityQueryableExpressionVisitorFactory>()
                .TryAdd<IEvaluatableExpressionFilter, EvaluatableExpressionFilter>()
                .TryAdd<IConventionSetBuilder, InMemoryConventionSetBuilder>()
                .TryAdd<ISingletonOptions, IInMemorySingletonOptions>(p => p.GetService<IInMemorySingletonOptions>())
                .TryAdd<ITypeMappingSource, InMemoryTypeMappingSource>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<IInMemorySingletonOptions, InMemorySingletonOptions>()
                        .TryAddSingleton<IInMemoryStoreCache, InMemoryStoreCache>()
                        .TryAddSingleton<IInMemoryTableFactory, InMemoryTableFactory>()
                        .TryAddScoped<IInMemoryDatabase, InMemoryDatabase>()
                        .TryAddScoped<IInMemoryMaterializerFactory, InMemoryMaterializerFactory>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
