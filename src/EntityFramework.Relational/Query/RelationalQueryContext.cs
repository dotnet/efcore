// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryContext : QueryContext
    {
        private readonly RelationalConnection _connection;
        private readonly RelationalValueReaderFactory _valueReaderFactory;

        public RelationalQueryContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] StateManager stateManager,
            [NotNull] RelationalConnection connection,
            [NotNull] RelationalValueReaderFactory valueReaderFactory)
            : base(model, logger, stateManager)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(valueReaderFactory, "valueReaderFactory");

            _connection = connection;
            _valueReaderFactory = valueReaderFactory;
        }

        public virtual RelationalValueReaderFactory ValueReaderFactory
        {
            get { return _valueReaderFactory; }
        }

        public virtual RelationalConnection Connection
        {
            get { return _connection; }
        }
    }
}
