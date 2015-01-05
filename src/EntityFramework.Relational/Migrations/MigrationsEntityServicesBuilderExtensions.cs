// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    public static class MigrationsEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddMigrations([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder
                .AddRelational().ServiceCollection
                .TryAdd(new ServiceCollection()
                    .AddScoped<MigrationAssembly>()
                    .AddScoped<HistoryRepository>()
                    .AddScoped(MigrationsDataStoreServices.MigratorFactory));

            return builder;
        }
    }
}
