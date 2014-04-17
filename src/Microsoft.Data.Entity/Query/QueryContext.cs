// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly IModel _model;
        private readonly StateManager _stateManager;

        public QueryContext(
            [NotNull] IModel model,
            [NotNull] StateManager stateManager)
        {
            Check.NotNull(model, "model");
            Check.NotNull(stateManager, "stateManager");

            _model = model;
            _stateManager = stateManager;
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual StateManager StateManager
        {
            get { return _stateManager; }
        }
    }
}
