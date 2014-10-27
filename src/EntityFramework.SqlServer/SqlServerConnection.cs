// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerConnection : RelationalConnection
    {
        public SqlServerConnection([NotNull] DbContextConfiguration configuration)
            : base(configuration)
        {
        }

        protected override DbConnection CreateDbConnection()
        {
            // TODO: Consider using DbProviderFactory to create connection instance
            // Issue #774
            return new SqlConnection(ConnectionString);
        }

        public virtual SqlConnection CreateMasterConnection()
        {
            var builder = new SqlConnectionStringBuilder { ConnectionString = ConnectionString };
            builder.InitialCatalog = "master";
            return new SqlConnection(builder.ConnectionString);
        }
    }
}
