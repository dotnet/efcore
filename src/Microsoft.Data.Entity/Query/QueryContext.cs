// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class QueryContext
    {
        private readonly IModel _model;
        private readonly ILogger _logger;
        private readonly StateManager _stateManager;

        public QueryContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] StateManager stateManager)
        {
            Check.NotNull(model, "model");
            Check.NotNull(logger, "logger");
            Check.NotNull(stateManager, "stateManager");

            _model = model;
            _logger = logger;
            _stateManager = stateManager;
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual StateManager StateManager
        {
            get { return _stateManager; }
        }
    }
}
