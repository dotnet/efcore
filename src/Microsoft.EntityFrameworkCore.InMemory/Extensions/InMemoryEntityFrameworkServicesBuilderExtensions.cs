// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InMemoryEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddInMemoryDatabase([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var service = builder.GetInfrastructure();

            service.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<InMemoryDatabaseProviderServices, InMemoryOptionsExtension>>());

            service.TryAdd(new ServiceCollection()
                .AddSingleton<InMemoryValueGeneratorCache>()
                .AddSingleton<IInMemoryStore, InMemoryStore>()
                .AddSingleton<IInMemoryTableFactory, InMemoryTableFactory>()
                .AddSingleton<InMemoryModelSource>()
                .AddScoped<InMemoryValueGeneratorSelector>()
                .AddScoped<InMemoryDatabaseProviderServices>()
                .AddScoped<IInMemoryDatabase, InMemoryDatabase>()
                .AddScoped<InMemoryTransactionManager>()
                .AddScoped<InMemoryDatabaseCreator>()
                .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<IMaterializerFactory, MaterializerFactory>()
                .AddScoped<InMemoryQueryContextFactory>()
                .AddScoped<InMemoryQueryModelVisitorFactory>()
                .AddScoped<InMemoryEntityQueryableExpressionVisitorFactory>();
    }
}
