// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleRelationalConnection : RelationalConnection, IOracleConnection
    {
        // ReSharper disable once InconsistentNaming
        public const string EFPDBAdminUser = "ef_pdb_admin";

        internal const int DefaultMasterConnectionCommandTimeout = 60;

        public OracleRelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override DbConnection CreateDbConnection() => new OracleConnection(ConnectionString);

        public override bool IsMultipleActiveResultSetsEnabled => true;

        public virtual IOracleConnection CreateMasterConnection()
        {
            var connectionStringBuilder
                = new OracleConnectionStringBuilder(ConnectionString)
                {
                    UserID = EFPDBAdminUser,
                    Password = EFPDBAdminUser
                };

            var contextOptions = new DbContextOptionsBuilder()
                .UseOracle(
                    connectionStringBuilder.ConnectionString,
                    b => b.CommandTimeout(CommandTimeout ?? DefaultMasterConnectionCommandTimeout))
                .Options;

            return new OracleRelationalConnection(Dependencies.With(contextOptions));
        }
    }
}
