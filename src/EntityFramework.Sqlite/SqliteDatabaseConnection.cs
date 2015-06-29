// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteDatabaseConnection : RelationalConnection
    {
        public SqliteDatabaseConnection([NotNull] IDbContextOptions options, [NotNull] ILoggerFactory loggerFactory)
            : base(options, loggerFactory)
        {
        }

        protected override DbConnection CreateDbConnection() => new SqliteConnection(ConnectionString);

        public override bool IsMultipleActiveResultSetsEnabled => true;
    }
}
