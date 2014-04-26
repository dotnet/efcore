// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Relational;
#if NET45
using System.Data.SqlClient;

#else
using System;
#endif

namespace Microsoft.Data.SqlServer
{
    public class SqlServerConnection : RelationalConnection
    {
        public SqlServerConnection([NotNull] ContextConfiguration configuration)
            : base(configuration)
        {
        }

        protected override DbConnection CreateDbConnection()
        {
#if NET45
            // TODO: Consider using DbProviderFactory to create connection instance
            return new SqlConnection(ConnectionString);
#else
            throw new NotImplementedException();
#endif
        }

        public virtual DbConnection CreateMasterConnection()
        {
#if NET45
            var builder = new DbConnectionStringBuilder { ConnectionString = ConnectionString };
            builder.Add("Initial Catalog", "master");
            return new SqlConnection(builder.ConnectionString);
#else
            throw new NotImplementedException();
#endif
        }
    }
}
