// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class SqlServerRetryingExecutionStrategyTests
    {
        [ConditionalFact]
        public void GetNextDelay_returns_shorter_delay_for_InMemory_transient_errors()
        {
            var strategy = new TestSqlServerRetryingExecutionStrategy(CreateContext());
            var inMemoryOltpError = SqlExceptionFactory.CreateSqlException(41302);
            var delays = new List<TimeSpan>();
            var delay = strategy.GetNextDelay(inMemoryOltpError);
            while (delay != null)
            {
                delays.Add(delay.Value);
                delay = strategy.GetNextDelay(inMemoryOltpError);
            }

            var expectedDelays = new List<TimeSpan>
            {
                TimeSpan.FromMilliseconds(0),
                TimeSpan.FromMilliseconds(1),
                TimeSpan.FromMilliseconds(3),
                TimeSpan.FromMilliseconds(7),
                TimeSpan.FromMilliseconds(15),
                TimeSpan.FromMilliseconds(31)
            };

            Assert.Equal(expectedDelays.Count, delays.Count);
            for (var i = 0; i < expectedDelays.Count; i++)
            {
                Assert.True(
                    Math.Abs((delays[i] - expectedDelays[i]).TotalMilliseconds)
                    <= expectedDelays[i].TotalMilliseconds * 0.1 + 1,
                    string.Format("Expected: {0}; Actual: {1}", expectedDelays[i], delays[i]));
            }
        }

        protected DbContext CreateContext()
            => SqlServerTestHelpers.Instance.CreateContext();
    }
}
