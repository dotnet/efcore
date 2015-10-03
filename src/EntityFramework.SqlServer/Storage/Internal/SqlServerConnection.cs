// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqlServerConnection : RelationalConnection, ISqlServerConnection
    {
        public SqlServerConnection(
            [NotNull] IDbContextOptions options,
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] ILogger<SqlServerConnection> logger)
            : base(options, logger)
        {
        }

        private SqlServerConnection(
            [NotNull] IDbContextOptions options, [NotNull] ILogger logger)
            : base(options, logger)
        {
        }

        protected override DbConnection CreateDbConnection() => new SqlConnection(ConnectionString);

        public virtual ISqlServerConnection CreateMasterConnection()
        {
            var builder = new SqlConnectionStringBuilder { ConnectionString = ConnectionString, InitialCatalog = "master" };

            // TODO use clone connection method once implemented see #1406
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(builder.ConnectionString).CommandTimeout(CommandTimeout);

            return new SqlServerConnection(optionsBuilder.Options, Logger);
        }

        public override bool IsMultipleActiveResultSetsEnabled
            => new SqlConnectionStringBuilder(ConnectionString).MultipleActiveResultSets;
    }
}
