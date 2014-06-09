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
    public class AtsQueryContext : QueryContext
    {
        private readonly AtsConnection _connection;

        public AtsQueryContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] StateManager stateManager,
            [NotNull] AtsConnection connection,
            [NotNull] AtsValueReaderFactory readerFactory)
            : base(model, logger, stateManager)
        {
            Check.NotNull(model, "model");
            Check.NotNull(logger, "logger");
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(readerFactory, "readerFactory");

            _connection = connection;
            ValueReaderFactory = readerFactory;
        }

        public virtual AtsConnection Connection
        {
            get { return _connection; }
        }

        public AtsValueReaderFactory ValueReaderFactory { get; private set; }
    }
}
