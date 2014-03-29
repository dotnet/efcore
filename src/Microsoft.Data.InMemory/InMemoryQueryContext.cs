// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryQueryContext : QueryContext
    {
        private readonly InMemoryDatabase _database;

        public InMemoryQueryContext(
            [NotNull] IModel model,
            [NotNull] StateManager stateManager,
            [NotNull] InMemoryDatabase database)
            : base(Check.NotNull(model, "model"),
                Check.NotNull(stateManager, "stateManager"))
        {
            Check.NotNull(database, "database");

            _database = database;
        }

        public virtual InMemoryDatabase Database
        {
            get { return _database; }
        }
    }
}
