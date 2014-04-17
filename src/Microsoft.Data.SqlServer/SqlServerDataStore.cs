// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Services;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;
#if NET45
using System.Data.SqlClient;

#endif

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDataStore : RelationalDataStore
    {
        private readonly SqlGenerator _sqlGenerator;
        private readonly ThreadSafeLazyRef<string> _masterConnectionString;

        public SqlServerDataStore([NotNull] string connectionString)
            : this(Check.NotEmpty(connectionString, "connectionString"), NullLogger.Instance, new SqlServerSqlGenerator())
        {
        }

        public SqlServerDataStore([NotNull] string connectionString, [NotNull] ILogger logger, [NotNull] SqlGenerator sqlGenerator)
            : base(Check.NotEmpty(connectionString, "connectionString"), Check.NotNull(logger, "logger"))
        {
            Check.NotNull(sqlGenerator, "sqlGenerator");
            _sqlGenerator = sqlGenerator;

            _masterConnectionString = new ThreadSafeLazyRef<string>(() =>
                {
                    var builder = new DbConnectionStringBuilder();
                    builder.ConnectionString = connectionString;
                    builder.Add("Initial Catalog", "master");
                    return builder.ConnectionString;
                });
        }

        protected override SqlGenerator SqlGenerator
        {
            get { return _sqlGenerator; }
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

        public virtual DbConnection CreateMasterConnection()
        {
            return CreateConnection(_masterConnectionString.Value);
        }
    }
}
