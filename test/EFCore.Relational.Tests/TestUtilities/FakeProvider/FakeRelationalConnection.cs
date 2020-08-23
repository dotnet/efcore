// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeRelationalConnection : RelationalConnection
    {
        private DbConnection _connection;

        private readonly List<FakeDbConnection> _dbConnections = new List<FakeDbConnection>();

        public FakeRelationalConnection(IDbContextOptions options = null)
            : base(
                new RelationalConnectionDependencies(
                    options ?? CreateOptions(),
                    new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                        new LoggerFactory(),
                        new LoggingOptions(),
                        new DiagnosticListener("FakeDiagnosticListener"),
                        new TestRelationalLoggingDefinitions(),
                        new NullDbContextLogger()),
                    new DiagnosticsLogger<DbLoggerCategory.Database.Connection>(
                        new LoggerFactory(),
                        new LoggingOptions(),
                        new DiagnosticListener("FakeDiagnosticListener"),
                        new TestRelationalLoggingDefinitions(),
                        new NullDbContextLogger()),
                    new NamedConnectionStringResolver(options ?? CreateOptions()),
                    new RelationalTransactionFactory(new RelationalTransactionFactoryDependencies()),
                    new CurrentDbContext(new FakeDbContext())))
        {
        }

        private class FakeDbContext : DbContext
        {
        }

        private static IDbContextOptions CreateOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(new FakeRelationalOptionsExtension().WithConnectionString("Database=Dummy"));

            return optionsBuilder.Options;
        }

        public void UseConnection(DbConnection connection)
            => _connection = connection;

        public override DbConnection DbConnection
            => _connection ?? base.DbConnection;

        public IReadOnlyList<FakeDbConnection> DbConnections
            => _dbConnections;

        protected override DbConnection CreateDbConnection()
        {
            var connection = new FakeDbConnection(ConnectionString);

            _dbConnections.Add(connection);

            return connection;
        }
    }
}
