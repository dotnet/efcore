// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;

namespace Microsoft.Data.Sqlite
{
    public class SqliteFactory : DbProviderFactory
    {
        private SqliteFactory()
        {
        }

        public readonly static SqliteFactory Instance = new SqliteFactory();

        public override DbCommand CreateCommand()
            => new SqliteCommand();

        public override DbConnection CreateConnection()
            => new SqliteConnection();

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
            => new SqliteConnectionStringBuilder();

        public override DbParameter CreateParameter()
            => new SqliteParameter();
    }
}
