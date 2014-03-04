// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Services;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Services
{
    public class ConsoleLoggerFactoryTest
    {
        [Fact]
        public void Logger_is_cached()
        {
            var consoleLoggerFactory = new ConsoleLoggerFactory();

            var logger1 = consoleLoggerFactory.Create("Foo");

            Assert.NotNull(logger1);

            var logger2 = consoleLoggerFactory.Create("Foo");

            Assert.Same(logger1, logger2);
        }

        [Fact]
        public void Can_check_logging_enabled()
        {
            var consoleLoggerFactory = new ConsoleLoggerFactory();
            var logger = consoleLoggerFactory.Create("Foo");

            Assert.True(logger.WriteCore(TraceType.Information, 0, null, null, null));
        }
    }
}
