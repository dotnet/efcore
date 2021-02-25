// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
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
        ///         Adds the services required by the in-memory database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
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
        public static IServiceCollection AddEntityFrameworkInMemoryDatabase([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, InMemoryLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<InMemoryOptionsExtension>>()
                .TryAdd<IValueGeneratorSelector, InMemoryValueGeneratorSelector>()
                .TryAdd<IDatabase>(p => p.GetService<IInMemoryDatabase>())
                .TryAdd<IDbContextTransactionManager, InMemoryTransactionManager>()
                .TryAdd<IDatabaseCreator, InMemoryDatabaseCreator>()
                .TryAdd<IQueryContextFactory, InMemoryQueryContextFactory>()
                .TryAdd<IProviderConventionSetBuilder, InMemoryConventionSetBuilder>()
                .TryAdd<IModelValidator, InMemoryModelValidator>()
                .TryAdd<ITypeMappingSource, InMemoryTypeMappingSource>()
                .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, InMemoryShapedQueryCompilingExpressionVisitorFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, InMemoryQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<ISingletonOptions, IInMemorySingletonOptions>(p => p.GetService<IInMemorySingletonOptions>())
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
