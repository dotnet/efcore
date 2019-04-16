// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CosmosServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkCosmos([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<CosmosDbOptionsExtension>>()
                .TryAdd<IDatabase, CosmosDatabase>()
                .TryAdd<IExecutionStrategyFactory, CosmosExecutionStrategyFactory>()
                .TryAdd<IDbContextTransactionManager, CosmosTransactionManager>()
                .TryAdd<IModelCustomizer, CosmosModelCustomizer>()
                .TryAdd<IConventionSetBuilder, CosmosConventionSetBuilder>()
                .TryAdd<IDatabaseCreator, CosmosDatabaseCreator>()
                .TryAdd<IQueryContextFactory, CosmosQueryContextFactory>()
                .TryAdd<IEntityQueryModelVisitorFactory, CosmosEntityQueryModelVisitorFactory>()
                .TryAdd<IEntityQueryableExpressionVisitorFactory, CosmosEntityQueryableExpressionVisitorFactory>()
                .TryAdd<IMemberAccessBindingExpressionVisitorFactory, CosmosMemberAccessBindingExpressionVisitorFactory>()
                .TryAdd<INavigationRewritingExpressionVisitorFactory, CosmosNavigationRewritingExpressionVisitorFactory>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddScoped<CosmosClient, CosmosClient>()

                );

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
