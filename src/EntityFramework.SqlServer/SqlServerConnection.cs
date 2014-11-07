// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerConnection : RelationalConnection
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqlServerConnection()
        {
        }

        public SqlServerConnection([NotNull] DbContextConfiguration configuration, [NotNull] ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
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
