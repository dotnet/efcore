// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Sql;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerDataStoreServices : RelationalDataStoreServices
    {
        private readonly SqlServerDataStore _store;
        private readonly SqlServerDataStoreCreator _creator;
        private readonly SqlServerConnection _connection;
        private readonly SqlServerValueGeneratorSelector _valueGeneratorSelector;
        private readonly SqlServerDatabase _database;
        private readonly SqlServerModelBuilderFactory _modelBuilderFactory;
        private readonly SqlServerModelSource _modelSource;

        public SqlServerDataStoreServices(
            [NotNull] SqlServerDataStore store,
            [NotNull] SqlServerDataStoreCreator creator,
            [NotNull] SqlServerConnection connection,
            [NotNull] SqlServerValueGeneratorSelector valueGeneratorSelector,
            [NotNull] SqlServerDatabase database,
            [NotNull] SqlServerModelBuilderFactory modelBuilderFactory,
            [NotNull] SqlServerModelDiffer modelDiffer,
            [NotNull] SqlServerHistoryRepository historyRepository,
            [NotNull] SqlServerMigrationSqlGenerator migrationSqlGenerator,
            [NotNull] SqlServerModelSource modelSource)
        {
            Check.NotNull(store, nameof(store));
            Check.NotNull(creator, nameof(creator));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector));
            Check.NotNull(database, nameof(database));
            Check.NotNull(modelBuilderFactory, nameof(modelBuilderFactory));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(historyRepository, nameof(historyRepository));
            Check.NotNull(migrationSqlGenerator, nameof(migrationSqlGenerator));
            Check.NotNull(modelSource, nameof(modelSource));

            _store = store;
            _creator = creator;
            _connection = connection;
            _valueGeneratorSelector = valueGeneratorSelector;
            _database = database;
            _modelBuilderFactory = modelBuilderFactory;
            ModelDiffer = modelDiffer;
            HistoryRepository = historyRepository;
            MigrationSqlGenerator = migrationSqlGenerator;
            _modelSource = modelSource;
        }

        public override DataStore Store => _store;

        public override DataStoreCreator Creator => _creator;

        public override DataStoreConnection Connection => _connection;

        public override ValueGeneratorSelectorContract ValueGeneratorSelector => _valueGeneratorSelector;

        public override Database Database => _database;

        public override ModelBuilderFactory ModelBuilderFactory => _modelBuilderFactory;

        public override ModelDiffer ModelDiffer { get; }

        public override IHistoryRepository HistoryRepository { get; }

        public override MigrationSqlGenerator MigrationSqlGenerator { get; }

        public override ModelSource ModelSource => _modelSource;
    }
}
