// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerConnection : RelationalConnection, ISqlServerConnection
    {
        private readonly ILoggerFactory _loggerFactory;

        public SqlServerConnection([NotNull] IDbContextOptions options, [NotNull] ILoggerFactory loggerFactory)
            : base(options, loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _loggerFactory = loggerFactory;
        }

        // TODO: Consider using DbProviderFactory to create connection instance
        // Issue #774
        protected override DbConnection CreateDbConnection() => new SqlConnection(ConnectionString);

        public virtual ISqlServerConnection CreateMasterConnection()
        {
            var builder = new SqlConnectionStringBuilder { ConnectionString = ConnectionString, InitialCatalog = "master" };

            // TODO use clone connection method once implimented see #1406
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(builder.ConnectionString).CommandTimeout(CommandTimeout);

            return new SqlServerConnection(optionsBuilder.Options, _loggerFactory);
        }

        public override bool IsMultipleActiveResultSetsEnabled
            => new SqlConnectionStringBuilder(ConnectionString).MultipleActiveResultSets;
    }
}
