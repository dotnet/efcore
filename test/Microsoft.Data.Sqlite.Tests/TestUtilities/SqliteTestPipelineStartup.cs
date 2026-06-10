// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using SQLitePCL;
using Xunit.Sdk;
using Xunit.v3;
using static SQLitePCL.raw;

[assembly: TestPipelineStartup(typeof(Microsoft.Data.Sqlite.Tests.TestUtilities.SqliteTestPipelineStartup))]

namespace Microsoft.Data.Sqlite.Tests.TestUtilities;

#if WINSQLITE3
public static class Batteries_V2
{
    public static void Init()
    {
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_winsqlite3());
    }
}
#endif

#if SQLITE3
public static class Batteries_V2
{
    public static void Init()
    {
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
    }
}
#endif

public class SqliteTestPipelineStartup : ITestPipelineStartup
{
    public ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        try
        {
            Batteries_V2.Init();
        }
        catch (DllNotFoundException ex)
        {
            SqliteTestEnvironment.SkipReason = ex.Message;
            return default;
        }

        var version = sqlite3_libversion().utf8_to_string();
        if (new Version(version) < new Version(3, 16, 0))
        {
            SqliteTestEnvironment.SkipReason = "SQLite " + version + " isn't supported. Upgrade to 3.16.0 or higher";
            return default;
        }

        if (sqlite3_compileoption_used("ENABLE_COLUMN_METADATA") == 0)
        {
            SqliteTestEnvironment.SkipReason = "SQLite compiled without -DSQLITE_ENABLE_COLUMN_METADATA";
            return default;
        }

        return default;
    }

    public ValueTask StopAsync()
        => default;
}

public static class SqliteTestEnvironment
{
    public static string? SkipReason { get; set; }

    public static bool IsAvailable => SkipReason is null;
}
