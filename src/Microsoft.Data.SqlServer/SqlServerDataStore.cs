// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity;
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
        private readonly SqlServerSqlGenerator _sqlGenerator;
        private readonly LazyRef<string> _masterConnectionString;
        private readonly string _connectionString;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqlServerDataStore()
        {
        }

        public SqlServerDataStore(
            [NotNull] ContextConfiguration configuration,
            [NotNull] SqlServerSqlGenerator sqlGenerator)
            : base(configuration)
        {
            Check.NotNull(sqlGenerator, "sqlGenerator");

            _sqlGenerator = sqlGenerator;

            var storeConfig = configuration.EntityConfiguration.Extensions
                .OfType<SqlServerConfigurationExtension>()
                .FirstOrDefault();

            // TODO: Consider finding connection string in config file by convention
            _connectionString = storeConfig == null ? null : storeConfig.ConnectionString;

            if (_connectionString == null)
            {
                // TODO: Proper message
                throw new InvalidOperationException("No connection string configured.");
            }

            _masterConnectionString = new LazyRef<string>(() =>
                {
                    var builder = new DbConnectionStringBuilder { ConnectionString = _connectionString };
                    builder.Add("Initial Catalog", "master");
                    return builder.ConnectionString;
                });
        }

        protected override SqlGenerator SqlGenerator
        {
            get { return _sqlGenerator; }
        }

        public override DbConnection CreateConnection()
        {
            return CreateConnection(_connectionString);
        }

        public virtual DbConnection CreateMasterConnection()
        {
            return CreateConnection(_masterConnectionString.Value);
        }

        private DbConnection CreateConnection(string connectionString)
        {
#if NET45
            return new SqlConnection(connectionString);
#else
            throw new System.NotImplementedException();
#endif
        }
    }
}
