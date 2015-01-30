// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryDataStoreServices : DataStoreServices
    {
        private readonly InMemoryDataStore _store;
        private readonly InMemoryDataStoreCreator _creator;
        private readonly InMemoryConnection _connection;
        private readonly InMemoryValueGeneratorCache _valueGeneratorCache;
        private readonly InMemoryDatabaseFacade _database;
        private readonly InMemoryModelBuilderFactory _modelBuilderFactory;
        private readonly InMemoryModelSource _modelSource;

        public InMemoryDataStoreServices(
            [NotNull] InMemoryDataStore store,
            [NotNull] InMemoryDataStoreCreator creator,
            [NotNull] InMemoryConnection connection,
            [NotNull] InMemoryValueGeneratorCache valueGeneratorCache,
            [NotNull] InMemoryDatabaseFacade database,
            [NotNull] InMemoryModelBuilderFactory modelBuilderFactory,
            [NotNull] InMemoryModelSource modelSource)
        {
            Check.NotNull(store, "store");
            Check.NotNull(creator, "creator");
            Check.NotNull(connection, "connection");
            Check.NotNull(valueGeneratorCache, "valueGeneratorCache");
            Check.NotNull(database, "database");
            Check.NotNull(modelBuilderFactory, "modelBuilderFactory");
            Check.NotNull(modelSource, "modelSource");

            _store = store;
            _creator = creator;
            _connection = connection;
            _valueGeneratorCache = valueGeneratorCache;
            _database = database;
            _modelBuilderFactory = modelBuilderFactory;
            _modelSource = modelSource;
        }

        public override DataStore Store => _store;

        public override DataStoreCreator Creator => _creator;

        public override DataStoreConnection Connection => _connection;

        public override ValueGeneratorCache ValueGeneratorCache => _valueGeneratorCache;

        public override Database Database => _database;

        public override ModelBuilderFactory ModelBuilderFactory => _modelBuilderFactory;

        public override ModelSource ModelSource => _modelSource;
    }
}
