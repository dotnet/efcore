// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory.Query
{
    public class InMemoryQueryContext : QueryContext
    {
        private readonly InMemoryDatabase _database;
        private readonly EntityKeyFactorySource _entityKeyFactorySource;

        public InMemoryQueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] StateManager stateManager,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] InMemoryDatabase database)
            : base(
                Check.NotNull(logger, "logger"),
                Check.NotNull(queryBuffer, "queryBuffer"),
                Check.NotNull(stateManager, "stateManager"))
        {
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(database, "database");

            _entityKeyFactorySource = entityKeyFactorySource;
            _database = database;
        }

        public virtual EntityKeyFactorySource EntityKeyFactorySource
        {
            get { return _entityKeyFactorySource; }
        }

        public virtual InMemoryDatabase Database
        {
            get { return _database; }
        }
    }
}
