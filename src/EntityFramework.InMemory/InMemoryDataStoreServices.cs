// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreServices : DataStoreServices
    {
        private readonly InMemoryDataStore _store;
        private readonly InMemoryDataStoreCreator _creator;
        private readonly InMemoryConnection _connection;
        private readonly InMemoryValueGeneratorCache _valueGeneratorCache;
        private readonly InMemoryDatabaseFacade _database;
        private readonly ModelBuilderFactory _modelBuilderFactory;

        public InMemoryDataStoreServices(
            [NotNull] InMemoryDataStore store,
            [NotNull] InMemoryDataStoreCreator creator,
            [NotNull] InMemoryConnection connection,
            [NotNull] InMemoryValueGeneratorCache valueGeneratorCache,
            [NotNull] InMemoryDatabaseFacade database,
            [NotNull] ModelBuilderFactory modelBuilderFactory)
        {
            Check.NotNull(store, "store");
            Check.NotNull(creator, "creator");
            Check.NotNull(connection, "connection");
            Check.NotNull(valueGeneratorCache, "valueGeneratorCache");
            Check.NotNull(database, "database");
            Check.NotNull(modelBuilderFactory, "modelBuilderFactory");

            _store = store;
            _creator = creator;
            _connection = connection;
            _valueGeneratorCache = valueGeneratorCache;
            _database = database;
            _modelBuilderFactory = modelBuilderFactory;
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
    }
}
