// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqlServerTimeSpanTypeMapping : TimeSpanTypeMapping
    {
        public SqlServerTimeSpanTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null)
            : base(storeType, dbType)
        {
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            // Workaround for a SQLClient bug
            if (DbType == System.Data.DbType.Time)
            {
                ((SqlParameter)parameter).SqlDbType = SqlDbType.Time;
            }
        }
    }
}
