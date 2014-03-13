// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
#if NET45
using System.Data.SqlClient;
#endif
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDataStore : RelationalDataStore
    {
        public SqlServerDataStore([NotNull] string connectionString)
            : this(Check.NotEmpty(connectionString, "connectionString"), NullLogger.Instance)
        {
        }

        public SqlServerDataStore([NotNull] string connectionString, [NotNull] ILogger logger)
            : base(Check.NotEmpty(connectionString, "connectionString"), Check.NotNull(logger, "logger"))
        {
        }

        public override DbConnection CreateConnection(string connectionString)
        {
            Check.NotEmpty(connectionString, "connectionString");

#if NET45
            return new SqlConnection(connectionString);
#else
            throw new System.NotImplementedException();
#endif
        }
    }
}
