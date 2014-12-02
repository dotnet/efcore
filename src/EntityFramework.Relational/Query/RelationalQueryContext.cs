// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryContext : QueryContext
    {
        private readonly RelationalConnection _connection;
        private readonly RelationalValueReaderFactory _valueReaderFactory;

        public RelationalQueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] RelationalConnection connection,
            [NotNull] RelationalValueReaderFactory valueReaderFactory)
            : base(
                Check.NotNull(logger, "logger"),
                Check.NotNull(queryBuffer, "queryBuffer"))
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
