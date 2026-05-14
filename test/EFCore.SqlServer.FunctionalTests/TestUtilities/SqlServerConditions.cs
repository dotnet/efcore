// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Static helpers consumed by Arcade's <see cref="Xunit.ConditionalFactAttribute" /> /
///     <see cref="Xunit.ConditionalTheoryAttribute" /> via <c>typeof(SqlServerConditions)</c> +
///     <c>nameof(...)</c> to skip SQL-Server-specific tests when the target server lacks a
///     capability. Universal helpers (<see cref="NotOnHelix" /> etc.) are re-exported so that a
///     single <c>typeof(SqlServerConditions)</c> can cover combined SQL Server + platform skips.
/// </summary>
public static class SqlServerConditions
{
    public static bool IsAzureSql
        => TestEnvironment.IsAzureSql;

    public static bool IsNotAzureSql
        => !TestEnvironment.IsAzureSql;

    public static bool IsNotCI
        => !TestEnvironment.IsCI;

    public static bool SupportsMemoryOptimized
        => TestEnvironment.IsMemoryOptimizedTablesSupported;

    public static bool SupportsAttach
    {
        get
        {
            var defaultConnection = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection);
            return defaultConnection.DataSource.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
                || defaultConnection.UserInstance;
        }
    }

    public static bool SupportsHiddenColumns
        => TestEnvironment.IsHiddenColumnsSupported;

    public static bool SupportsFullTextSearch
        => TestEnvironment.IsFullTextSearchSupported;

    public static bool SupportsOnlineIndexes
        => TestEnvironment.IsOnlineIndexingSupported;

    public static bool SupportsTemporalTablesCascadeDelete
        => TestEnvironment.IsTemporalTablesCascadeDeleteSupported;

    public static bool SupportsUtf8
        => TestEnvironment.IsUtf8Supported;

    public static bool SupportsJsonPathExpressions
        => TestEnvironment.SupportsJsonPathExpressions;

    public static bool SupportsSqlClr
        => TestEnvironment.IsSqlClrSupported;

    public static bool SupportsFunctions2017
        => TestEnvironment.IsFunctions2017Supported;

    public static bool SupportsFunctions2019
        => TestEnvironment.IsFunctions2019Supported;

    public static bool SupportsFunctions2022
        => TestEnvironment.IsFunctions2022Supported;

    public static bool SupportsJsonType
        => TestEnvironment.IsJsonTypeSupported;

    public static bool SupportsVectorType
        => TestEnvironment.IsVectorTypeSupported;

    /// <summary>
    ///     Replacement for the former assembly-level <c>SqlServerConfiguredCondition</c>.
    ///     Note: most test projects rely on <see cref="TestEnvironment.IsConfigured" /> being
    ///     enforced at the test-class fixture level rather than referencing this helper.
    /// </summary>
    public static bool IsConfigured
        => TestEnvironment.IsConfigured
            && (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !TestEnvironment.IsLocalDb);

    // Re-exported universal helpers so that combined skips can use a single calleeType.
    public static bool NotOnHelix
        => TestConditions.NotOnHelix;

    public static bool NotOnCI
        => TestConditions.NotOnCI;

    public static bool NotOnMac
        => TestConditions.NotOnMac;

    public static bool NotOnLinux
        => TestConditions.NotOnLinux;

    public static bool NotOnWindows
        => TestConditions.NotOnWindows;

    public static bool NotOnLinuxOrMac
        => TestConditions.NotOnLinuxOrMac;
}
