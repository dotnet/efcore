// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
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
    public class SqlServerDataStoreServices : RelationalDataStoreServices
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerDataStoreServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));

            _serviceProvider = serviceProvider;
        }

        public override DataStore Store => _serviceProvider.GetRequiredService<SqlServerDataStore>();

        public override QueryContextFactory QueryContextFactory => _serviceProvider.GetRequiredService<SqlServerQueryContextFactory>();

        public override DataStoreCreator Creator => _serviceProvider.GetRequiredService<SqlServerDataStoreCreator>();

        public override DataStoreConnection Connection => _serviceProvider.GetRequiredService<SqlServerConnection>();

        public override IValueGeneratorSelector ValueGeneratorSelector => _serviceProvider.GetRequiredService<SqlServerValueGeneratorSelector>();

        public override Database Database => _serviceProvider.GetRequiredService<SqlServerDatabase>();

        public override ModelBuilderFactory ModelBuilderFactory => _serviceProvider.GetRequiredService<SqlServerModelBuilderFactory>();

        public override ModelDiffer ModelDiffer => _serviceProvider.GetRequiredService<SqlServerModelDiffer>();

        public override IHistoryRepository HistoryRepository => _serviceProvider.GetRequiredService<SqlServerHistoryRepository>();

        public override MigrationSqlGenerator MigrationSqlGenerator => _serviceProvider.GetRequiredService<SqlServerMigrationSqlGenerator>();

        public override ModelSource ModelSource => _serviceProvider.GetRequiredService<SqlServerModelSource>();
    }
}
