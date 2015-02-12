// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDataStoreServices : DataStoreServices
    {
        public abstract ModelDiffer ModelDiffer { get; }
        public abstract IHistoryRepository HistoryRepository { get; }
        public abstract MigrationSqlGenerator MigrationSqlGenerator { get; }

        public static Func<IServiceProvider, DbContextService<ModelDiffer>> ModelDifferFactory =>
            p => new DbContextService<ModelDiffer>(() => GetStoreServices(p).ModelDiffer);

        public static Func<IServiceProvider, DbContextService<IHistoryRepository>> HistoryRepositoryFactory =>
            p => new DbContextService<IHistoryRepository>(() => GetStoreServices(p).HistoryRepository);

        public static Func<IServiceProvider, DbContextService<MigrationSqlGenerator>> MigrationSqlGeneratorFactory =>
            p => new DbContextService<MigrationSqlGenerator>(() => GetStoreServices(p).MigrationSqlGenerator);

        protected new static RelationalDataStoreServices GetStoreServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            return (RelationalDataStoreServices)serviceProvider.GetRequiredService<DbContextServices>().DataStoreServices;
        }
    }
}
