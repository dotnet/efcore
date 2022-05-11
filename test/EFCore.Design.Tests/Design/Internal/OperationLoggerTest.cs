// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design.Internal;

public class OperationLoggerTests
{
    [ConditionalFact]
    public void Log_dampens_logLevel_when_CommandExecuted()
    {
        var reporter = new TestOperationReporter();
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new OperationLoggerProvider(reporter));

        var logger = loggerFactory.CreateLogger(DbLoggerCategory.Database.Command.Name);
        logger.Log<object>(
            LogLevel.Information,
            RelationalEventId.CommandExecuted,
            null,
            null,
            (_, __) => "-- Can't stop the SQL");

        Assert.Collection(
            reporter.Messages,
            x =>
            {
                Assert.Equal("-- Can't stop the SQL", x.Message);
                Assert.Equal(LogLevel.Debug, x.Level);
            });
    }
}
