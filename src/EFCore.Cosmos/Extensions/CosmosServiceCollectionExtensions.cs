// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Cosmos-specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class CosmosServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Azure Cosmos database provider for Entity Framework
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
        public static IServiceCollection AddEntityFrameworkCosmos([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, CosmosLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<CosmosOptionsExtension>>()
                .TryAdd<IDatabase, CosmosDatabaseWrapper>()
                .TryAdd<IExecutionStrategyFactory, CosmosExecutionStrategyFactory>()
                .TryAdd<IDbContextTransactionManager, CosmosTransactionManager>()
                .TryAdd<IModelValidator, CosmosModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, CosmosConventionSetBuilder>()
                .TryAdd<IValueGeneratorSelector, CosmosValueGeneratorSelector>()
                .TryAdd<IDatabaseCreator, CosmosDatabaseCreator>()
                .TryAdd<IQueryContextFactory, CosmosQueryContextFactory>()
                .TryAdd<ITypeMappingSource, CosmosTypeMappingSource>()

                // New Query pipeline
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, CosmosQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IShapedQueryCompilingExpressionVisitorFactory, CosmosShapedQueryCompilingExpressionVisitorFactory>()
                .TryAdd<ISingletonOptions, ICosmosSingletonOptions>(p => p.GetService<ICosmosSingletonOptions>())
                .TryAdd<IQueryTranslationPreprocessorFactory, CosmosQueryTranslationPreprocessorFactory>()
                .TryAdd<IQueryCompilationContextFactory, CosmosQueryCompilationContextFactory>()
                .TryAdd<IQueryTranslationPostprocessorFactory, CosmosQueryTranslationPostprocessorFactory>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<ICosmosSingletonOptions, CosmosSingletonOptions>()
                        .TryAddSingleton<SingletonCosmosClientWrapper, SingletonCosmosClientWrapper>()
                        .TryAddSingleton<ISqlExpressionFactory, SqlExpressionFactory>()
                        .TryAddSingleton<IQuerySqlGeneratorFactory, QuerySqlGeneratorFactory>()
                        .TryAddSingleton<IMethodCallTranslatorProvider, CosmosMethodCallTranslatorProvider>()
                        .TryAddSingleton<IMemberTranslatorProvider, CosmosMemberTranslatorProvider>()
                        .TryAddScoped<CosmosClientWrapper, CosmosClientWrapper>()
                );

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
