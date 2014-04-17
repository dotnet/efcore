// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class RelationalQueryContext : QueryContext
    {
        private readonly RelationalDataStore _dataStore;

        public RelationalQueryContext(
            [NotNull] IModel model,
            [NotNull] StateManager stateManager,
            [NotNull] RelationalDataStore dataStore)
            : base(Check.NotNull(model, "model"), Check.NotNull(stateManager, "stateManager"))
        {
            Check.NotNull(dataStore, "dataStore");

            _dataStore = dataStore;
        }

        public virtual RelationalDataStore DataStore
        {
            get { return _dataStore; }
        }
    }
}
