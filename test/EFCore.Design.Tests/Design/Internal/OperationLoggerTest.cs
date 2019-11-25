// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
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
                x => Assert.Equal("verbose: -- Can't stop the SQL", x));
        }
    }
}
