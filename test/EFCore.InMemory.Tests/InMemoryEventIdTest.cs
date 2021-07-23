// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryEventIdTest : EventIdTestBase
    {
        [ConditionalFact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(IEnumerable<IUpdateEntry>), () => new List<IUpdateEntry>() }
            };

            TestEventLogging(
                typeof(InMemoryEventId),
                typeof(InMemoryLoggerExtensions),
                new InMemoryLoggingDefinitions(),
                fakeFactories);
        }
    }
}
