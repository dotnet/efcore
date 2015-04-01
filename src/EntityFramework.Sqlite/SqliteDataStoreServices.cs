// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Sqlite.Query;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDataStoreServices : ISqliteDataStoreServices
    {
        private readonly IServiceProvider _services;

        public SqliteDataStoreServices([NotNull] IServiceProvider services)
        {
            Check.NotNull(services, nameof(services));

            _services = services;
        }

        public virtual IDataStoreConnection Connection => _services.GetRequiredService<ISqliteConnection>();
        public virtual IDataStoreCreator Creator => _services.GetRequiredService<ISqliteDataStoreCreator>();
        public virtual IDatabaseFactory DatabaseFactory => _services.GetRequiredService<ISqliteDatabaseFactory>();
        public virtual IHistoryRepository HistoryRepository => _services.GetRequiredService<ISqliteHistoryRepository>();
        public virtual IMigrationSqlGenerator MigrationSqlGenerator => _services.GetRequiredService<ISqliteMigrationSqlGenerator>();
        public virtual IModelBuilderFactory ModelBuilderFactory => _services.GetRequiredService<ISqliteModelBuilderFactory>();
        public virtual IModelDiffer ModelDiffer => _services.GetRequiredService<ISqliteModelDiffer>();
        public virtual IModelSource ModelSource => _services.GetRequiredService<ISqliteModelSource>();
        public virtual IQueryContextFactory QueryContextFactory => _services.GetRequiredService<ISqliteQueryContextFactory>();
        public virtual IRelationalConnection RelationalConnection => _services.GetRequiredService<ISqliteConnection>();
        public virtual ISqlGenerator SqlGenerator => _services.GetRequiredService<ISqliteSqlGenerator>();
        public virtual IDataStore Store => _services.GetRequiredService<ISqliteDataStore>();
        public virtual IValueGeneratorSelector ValueGeneratorSelector => _services.GetRequiredService<ISqliteValueGeneratorSelector>();
    }
}
