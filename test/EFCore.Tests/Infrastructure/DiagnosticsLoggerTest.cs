// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class DiagnosticsLoggerTest
{
    [ConditionalFact]
    public void Can_filter_for_messages_of_one_category()
        => FilterTest(c => c == DbLoggerCategory.Database.Command.Name, "SQL1", "SQL2");

    [ConditionalFact]
    public void Can_filter_for_messages_of_one_subcategory()
        => FilterTest(c => c.StartsWith(DbLoggerCategory.Database.Name, StringComparison.Ordinal), "DB1", "SQL1", "DB2", "SQL2");

    [ConditionalFact]
    public void Can_filter_for_all_EF_messages()
        => FilterTest(
            c => c.StartsWith(DbLoggerCategory.Name, StringComparison.Ordinal), "DB1", "SQL1", "Query1", "DB2", "SQL2", "Query2");

    [ConditionalFact]
    public void Can_get_all_messages()
        => FilterTest(c => true, "DB1", "SQL1", "Query1", "Random1", "DB2", "SQL2", "Query2", "Random2");

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private void FilterTest(Func<string, bool> filter, params string[] expected)
    {
        var loggerFactory = new ListLoggerFactory(filter);

        var dbLogger = new DiagnosticsLogger<DbLoggerCategory.Database>(
            loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"), new TestLoggingDefinitions(),
            new NullDbContextLogger());
        var sqlLogger = new DiagnosticsLogger<DbLoggerCategory.Database.Command>(
            loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"), new TestLoggingDefinitions(),
            new NullDbContextLogger());
        var queryLogger = new DiagnosticsLogger<DbLoggerCategory.Query>(
            loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"), new TestLoggingDefinitions(),
            new NullDbContextLogger());
        var randomLogger = loggerFactory.CreateLogger("Random");

        dbLogger.Logger.LogInformation(1, "DB1");
        sqlLogger.Logger.LogInformation(2, "SQL1");
        queryLogger.Logger.LogInformation(3, "Query1");
        randomLogger.LogInformation(4, "Random1");

        dbLogger.Logger.LogInformation(1, "DB2");
        sqlLogger.Logger.LogInformation(2, "SQL2");
        queryLogger.Logger.LogInformation(3, "Query2");
        randomLogger.LogInformation(4, "Random2");

        Assert.Equal(expected, loggerFactory.Log.Select(l => l.Message));
    }
}
