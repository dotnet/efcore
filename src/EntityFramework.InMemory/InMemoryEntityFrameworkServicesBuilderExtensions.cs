// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class InMemoryEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddInMemoryDatabase([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var service = builder.GetService();

            service.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<InMemoryDatabaseProviderServices, InMemoryOptionsExtension>>());

            service.TryAdd(new ServiceCollection()
                .AddSingleton<InMemoryValueGeneratorCache>()
                .AddSingleton<IInMemoryStore, InMemoryStore>()
                .AddSingleton<InMemoryModelSource>()
                .AddScoped<InMemoryValueGeneratorSelector>()
                .AddScoped<InMemoryDatabaseProviderServices>()
                .AddScoped<IInMemoryDatabase, InMemoryDatabase>()
                .AddScoped<InMemoryDatabaseCreator>()
                .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<IMaterializerFactory, MaterializerFactory>()
                .AddScoped<InMemoryQueryContextFactory>()
                .AddScoped<InMemoryQueryModelVisitorFactory>()
                .AddScoped<InMemoryEntityQueryableExpressionVisitorFactory>();
        }
    }
}
