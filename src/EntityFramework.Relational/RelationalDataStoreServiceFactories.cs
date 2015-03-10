// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational
{
    public static class RelationalDataStoreServiceFactories
    {
        public static Func<IServiceProvider, IModelDiffer> ModelDifferFactory => p => GetStoreServices(p).ModelDiffer;

        public static Func<IServiceProvider, IHistoryRepository> HistoryRepositoryFactory => p => GetStoreServices(p).HistoryRepository;

        public static Func<IServiceProvider, IMigrationSqlGenerator> MigrationSqlGeneratorFactory => p => GetStoreServices(p).MigrationSqlGenerator;

        public static Func<IServiceProvider, IRelationalConnection> RelationalConnectionFactory => p => GetStoreServices(p).RelationalConnection;

        public static Func<IServiceProvider, ISqlGenerator> SqlGeneratorFactory => p => GetStoreServices(p).SqlGenerator;

        private static IRelationalDataStoreServices GetStoreServices(IServiceProvider serviceProvider)
            => (IRelationalDataStoreServices)serviceProvider.GetRequiredService<DbContextServices>().DataStoreServices;
    }
}
