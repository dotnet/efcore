// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class SqlServerConditionAttribute(SqlServerCondition conditions) : Attribute, ITestCondition
{
    public SqlServerCondition Conditions { get; set; } = conditions;

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

        if (Conditions.HasFlag(SqlServerCondition.SupportsTemporalTablesCascadeDelete))
        {
            isMet &= TestEnvironment.IsTemporalTablesCascadeDeleteSupported;
        }

        if (Conditions.HasFlag(SqlServerCondition.SupportsUtf8))
        {
            isMet &= TestEnvironment.IsUtf8Supported;
        }

        if (Conditions.HasFlag(SqlServerCondition.SupportsJsonPathExpressions))
        {
            isMet &= TestEnvironment.SupportsJsonPathExpressions;
        }

        if (Conditions.HasFlag(SqlServerCondition.SupportsSqlClr))
        {
            isMet &= TestEnvironment.IsSqlClrSupported;
        }

        if (Conditions.HasFlag(SqlServerCondition.SupportsFunctions2017))
        {
            isMet &= TestEnvironment.IsFunctions2017Supported;
        }

        if (Conditions.HasFlag(SqlServerCondition.SupportsFunctions2019))
        {
            isMet &= TestEnvironment.IsFunctions2019Supported;
        }

        if (Conditions.HasFlag(SqlServerCondition.SupportsFunctions2022))
        {
            isMet &= TestEnvironment.IsFunctions2022Supported;
        }

        return ValueTask.FromResult(isMet);
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
