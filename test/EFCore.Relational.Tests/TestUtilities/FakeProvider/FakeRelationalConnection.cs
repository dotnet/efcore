// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider
{
    public class FakeRelationalConnection : RelationalConnection
    {
        private readonly List<FakeDbConnection> _dbConnections = new List<FakeDbConnection>();

        public FakeRelationalConnection(IDbContextOptions options)
            : base(
                new RelationalConnectionDependencies(
                    options,
                    new DiagnosticsLogger<LoggerCategory.Database.Transaction>(
                        new LoggerFactory(),
                        new LoggingOptions(),
                        new DiagnosticListener("FakeDiagnosticListener")),
                    new DiagnosticsLogger<LoggerCategory.Database.Connection>(
                        new LoggerFactory(),
                        new LoggingOptions(),
                        new DiagnosticListener("FakeDiagnosticListener"))))
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
