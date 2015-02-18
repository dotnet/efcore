// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ValueGeneration;
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
        private readonly InMemoryValueGeneratorSelector _valueGeneratorSelector;
        private readonly InMemoryDatabaseFacade _database;
        private readonly InMemoryModelBuilderFactory _modelBuilderFactory;
        private readonly InMemoryModelSource _modelSource;

        public InMemoryDataStoreServices(
            [NotNull] InMemoryDataStore store,
            [NotNull] InMemoryDataStoreCreator creator,
            [NotNull] InMemoryConnection connection,
            [NotNull] InMemoryValueGeneratorSelector valueGeneratorSelector,
            [NotNull] InMemoryDatabaseFacade database,
            [NotNull] InMemoryModelBuilderFactory modelBuilderFactory,
            [NotNull] InMemoryModelSource modelSource)
        {
            Check.NotNull(store, nameof(store));
            Check.NotNull(creator, nameof(creator));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector));
            Check.NotNull(database, nameof(database));
            Check.NotNull(modelBuilderFactory, nameof(modelBuilderFactory));
            Check.NotNull(modelSource, nameof(modelSource));

            _store = store;
            _creator = creator;
            _connection = connection;
            _valueGeneratorSelector = valueGeneratorSelector;
            _database = database;
            _modelBuilderFactory = modelBuilderFactory;
            _modelSource = modelSource;
        }

        public override DataStore Store => _store;

        public override DataStoreCreator Creator => _creator;

        public override DataStoreConnection Connection => _connection;

        public override ValueGeneratorSelectorContract ValueGeneratorSelector => _valueGeneratorSelector;

        public override Database Database => _database;

        public override ModelBuilderFactory ModelBuilderFactory => _modelBuilderFactory;

        public override ModelSource ModelSource => _modelSource;
    }
}
