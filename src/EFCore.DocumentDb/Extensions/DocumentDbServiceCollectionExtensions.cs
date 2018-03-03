// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public static class DocumentDbServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkDocumentDb(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<DocumentDbOptionsExtension>>()
                .TryAdd<IQueryContextFactory, DocumentDbQueryContextFactory>()
                .TryAdd<IDatabase, DocumentDbDatabase>()
                .TryAdd<IEntityQueryModelVisitorFactory, DocumentDbEntityQueryModelVisitorFactory>()
                .TryAdd<IDatabaseCreator, DocumentDbDatabaseCreator>()
                .TryAdd<IEntityQueryableExpressionVisitorFactory, DocumentDbEntityQueryableExpressionVisitorFactory>()
                .TryAdd<IExpressionPrinter, DocumentDbPrinter>()
                .TryAdd<IDbContextTransactionManager, DocumentDbTransactionManager>()
                .TryAdd<IConventionSetBuilder, DocumentDbConventionSetBuilder>()
                .TryAdd<IModelCustomizer, DocumentDbModelCustomizer>()
                .TryAdd<IProjectionExpressionVisitorFactory, DocumentDbProjectionExpressionVisitorFactory>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddScoped<IDocumentDbClientService, DocumentDbClientService>()
                        .TryAddScoped<ISqlTranslatingExpressionVisitorFactory, SqlTranslatingExpressionVisitorFactory>()
                        .TryAddScoped<IDocumentCollectionServiceFactory, DocumentCollectionServiceFactory>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
