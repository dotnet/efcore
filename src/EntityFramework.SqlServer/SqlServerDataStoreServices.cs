// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStoreServices : ISqlServerDataStoreServices
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerDataStoreServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        public virtual IDataStore Store => _serviceProvider.GetRequiredService<ISqlServerDataStore>();

        public virtual IQueryContextFactory QueryContextFactory => _serviceProvider.GetRequiredService<ISqlServerQueryContextFactory>();

        public virtual IDataStoreCreator Creator => _serviceProvider.GetRequiredService<ISqlServerDataStoreCreator>();

        public virtual IDataStoreConnection Connection => _serviceProvider.GetRequiredService<ISqlServerConnection>();

        public virtual IRelationalConnection RelationalConnection => _serviceProvider.GetRequiredService<ISqlServerConnection>();

        public virtual IValueGeneratorSelector ValueGeneratorSelector => _serviceProvider.GetRequiredService<ISqlServerValueGeneratorSelector>();

        public virtual IDatabaseFactory DatabaseFactory => _serviceProvider.GetRequiredService<ISqlServerDatabaseFactory>();

        public virtual IModelBuilderFactory ModelBuilderFactory => _serviceProvider.GetRequiredService<ISqlServerModelBuilderFactory>();

        public virtual IModelDiffer ModelDiffer => _serviceProvider.GetRequiredService<ISqlServerModelDiffer>();

        public virtual IHistoryRepository HistoryRepository => _serviceProvider.GetRequiredService<ISqlServerHistoryRepository>();

        public virtual IMigrationSqlGenerator MigrationSqlGenerator => _serviceProvider.GetRequiredService<ISqlServerMigrationSqlGenerator>();

        public virtual IModelSource ModelSource => _serviceProvider.GetRequiredService<ISqlServerModelSource>();

        public virtual ISqlGenerator SqlGenerator => _serviceProvider.GetRequiredService<ISqlServerSqlGenerator>();
    }
}
