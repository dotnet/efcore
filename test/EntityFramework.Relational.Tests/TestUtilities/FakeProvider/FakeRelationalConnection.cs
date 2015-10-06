// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.TestUtilities.FakeProvider
{
    public class FakeRelationalConnection : RelationalConnection
    {
        private readonly List<FakeDbConnection> _dbConnections = new List<FakeDbConnection>();

        public FakeRelationalConnection(IDbContextOptions options)
            : base(options, new Logger<FakeRelationalConnection>(new LoggerFactory()))
        {
        }

        public IReadOnlyList<FakeDbConnection> DbConnections => _dbConnections;

        protected override DbConnection CreateDbConnection()
        {
            var connection = new FakeDbConnection(ConnectionString);

            _dbConnections.Add(connection);

            return connection;
        }
    }
}