// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Services
{
    public class ConsoleLoggerFactoryTest
    {
        [Fact]
        public void LoggerIsCached()
        {
            var consoleLoggerFactory = new ConsoleLoggerFactory();

            var logger1 = consoleLoggerFactory.Create("Foo");

            Assert.NotNull(logger1);

            var logger2 = consoleLoggerFactory.Create("Foo");

            Assert.Same(logger1, logger2);
        }
    }
}
