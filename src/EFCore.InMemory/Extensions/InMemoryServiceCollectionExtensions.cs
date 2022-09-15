// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     In-memory specific extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class InMemoryServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the services required by the in-memory database provider for Entity Framework
    ///     to an <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     Calling this method is no longer necessary when building most applications, including those that
    ///     use dependency injection in ASP.NET or elsewhere.
    ///     It is only needed when building the internal service provider for use with
    ///     the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
    ///     This is not recommend other than for some advanced scenarios.
    /// </remarks>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkInMemoryDatabase(this IServiceCollection serviceCollection)
    {
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
            .TryAdd<IQueryTranslationPreprocessorFactory, InMemoryQueryTranslationPreprocessorFactory>()
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
