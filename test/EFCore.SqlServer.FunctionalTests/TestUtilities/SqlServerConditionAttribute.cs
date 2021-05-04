// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class SqlServerConditionAttribute : Attribute, ITestCondition
    {
        public SqlServerCondition Conditions { get; set; }

        public SqlServerConditionAttribute(SqlServerCondition conditions)
        {
            Conditions = conditions;
        }

        public ValueTask<bool> IsMetAsync()
        {
            var isMet = true;

            if (Conditions.HasFlag(SqlServerCondition.SupportsHiddenColumns))
            {
                isMet &= TestEnvironment.IsHiddenColumnsSupported;
            }

            if (Conditions.HasFlag(SqlServerCondition.SupportsMemoryOptimized))
            {
                isMet &= TestEnvironment.IsMemoryOptimizedTablesSupported;
            }

            if (Conditions.HasFlag(SqlServerCondition.IsSqlAzure))
            {
                isMet &= TestEnvironment.IsSqlAzure;
            }

            if (Conditions.HasFlag(SqlServerCondition.IsNotSqlAzure))
            {
                isMet &= !TestEnvironment.IsSqlAzure;
            }

            if (Conditions.HasFlag(SqlServerCondition.SupportsAttach))
            {
                var defaultConnection = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection);
                isMet &= defaultConnection.DataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
                    || defaultConnection.UserInstance;
            }

            if (Conditions.HasFlag(SqlServerCondition.IsNotCI))
            {
                isMet &= !TestEnvironment.IsCI;
            }

            if (Conditions.HasFlag(SqlServerCondition.SupportsFullTextSearch))
            {
                isMet &= TestEnvironment.IsFullTextSearchSupported;
            }

            if (Conditions.HasFlag(SqlServerCondition.SupportsOnlineIndexes))
            {
                isMet &= TestEnvironment.IsOnlineIndexingSupported;
            }

            return new ValueTask<bool>(isMet);
        }

        public string SkipReason
            =>
                // ReSharper disable once UseStringInterpolation
                string.Format(
                    "The test SQL Server does not meet these conditions: '{0}'",
                    string.Join(
                        ", ", Enum.GetValues(typeof(SqlServerCondition))
                            .Cast<Enum>()
                            .Where(f => Conditions.HasFlag(f))
                            .Select(f => Enum.GetName(typeof(SqlServerCondition), f))));
    }
}
