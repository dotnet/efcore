// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Design.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Design.Internal
{
    public class OperationLoggerTests
    {
        [Fact]
        public void Log_dampens_logLevel_when_ExecutedCommand()
        {
            var reporter = new TestOperationReporter();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new LoggerProvider(categoryName => new OperationLogger(categoryName, reporter)));

            var logger = loggerFactory.CreateLogger<RelationalCommandBuilderFactory>();
            logger.Log<object>(
                LogLevel.Information,
                (int)RelationalEventId.ExecutedCommand,
                null,
                null,
                (_, __) => "-- Can't stop the SQL");

            Assert.Collection(
                reporter.Messages,
                x => Assert.Equal("verbose: -- Can't stop the SQL", x));
        }
    }
}
