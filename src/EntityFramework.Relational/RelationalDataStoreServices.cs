// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDataStoreServices : DataStoreServices
    {
        public abstract ModelDiffer ModelDiffer { get; }
        public abstract IHistoryRepository HistoryRepository { get; }
        public abstract MigrationSqlGenerator MigrationSqlGenerator { get; }

        public static Func<IServiceProvider, ModelDiffer> ModelDifferFactory => p => GetStoreServices(p).ModelDiffer;

        public static Func<IServiceProvider, IHistoryRepository> HistoryRepositoryFactory => p => GetStoreServices(p).HistoryRepository;

        public static Func<IServiceProvider, MigrationSqlGenerator> MigrationSqlGeneratorFactory => p => GetStoreServices(p).MigrationSqlGenerator;

        private static RelationalDataStoreServices GetStoreServices(IServiceProvider serviceProvider)
            => (RelationalDataStoreServices)serviceProvider.GetRequiredService<DbContextServices>().DataStoreServices;
    }
}
