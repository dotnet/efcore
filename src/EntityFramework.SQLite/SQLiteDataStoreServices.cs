// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDataStoreServices : MigrationsDataStoreServices
    {
        private readonly SQLiteDataStore _store;
        private readonly SQLiteDataStoreCreator _creator;
        private readonly SQLiteConnection _connection;
        private readonly SQLiteValueGeneratorCache _valueGeneratorCache;
        private readonly SQLiteDatabase _database;
        private readonly ModelBuilderFactory _modelBuilderFactory;
        private readonly SQLiteMigrator _migrator;

        public SQLiteDataStoreServices(
            [NotNull] SQLiteDataStore store,
            [NotNull] SQLiteDataStoreCreator creator,
            [NotNull] SQLiteConnection connection,
            [NotNull] SQLiteValueGeneratorCache valueGeneratorCache,
            [NotNull] SQLiteDatabase database,
            [NotNull] ModelBuilderFactory modelBuilderFactory,
            [NotNull] SQLiteMigrator migrator)
        {
            Check.NotNull(store, "store");
            Check.NotNull(creator, "creator");
            Check.NotNull(connection, "connection");
            Check.NotNull(valueGeneratorCache, "valueGeneratorCache");
            Check.NotNull(database, "database");
            Check.NotNull(modelBuilderFactory, "modelBuilderFactory");
            Check.NotNull(migrator, "migrator");

            _store = store;
            _creator = creator;
            _connection = connection;
            _valueGeneratorCache = valueGeneratorCache;
            _database = database;
            _modelBuilderFactory = modelBuilderFactory;
            _migrator = migrator;
        }

        public override DataStore Store
        {
            get { return _store; }
        }

        public override DataStoreCreator Creator
        {
            get { return _creator; }
        }

        public override DataStoreConnection Connection
        {
            get { return _connection; }
        }

        public override ValueGeneratorCache ValueGeneratorCache
        {
            get { return _valueGeneratorCache; }
        }

        public override Database Database
        {
            get { return _database; }
        }

        public override IModelBuilderFactory ModelBuilderFactory
        {
            get { return _modelBuilderFactory; }
        }

        public override Migrator Migrator
        {
            get { return _migrator; }
        }
    }
}
