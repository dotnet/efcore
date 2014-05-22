// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AzureTableStorageQueryContext : QueryContext
    {
        private readonly AzureTableStorageConnection _database;

        public AzureTableStorageQueryContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] StateManager stateManager,
            [NotNull] AzureTableStorageConnection database)
            : base(model, logger, stateManager)
        {
            Check.NotNull(model, "model");
            Check.NotNull(logger, "logger");
            Check.NotNull(stateManager, "stateManager");

            _database = database;
        }

        public virtual AzureTableStorageConnection Database
        {
            get { return _database; }
        }
    }
}
