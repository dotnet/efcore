// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class CosmosEventIdTest : EventIdTestBase
    {
        [ConditionalFact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(CosmosSqlQuery), () => new CosmosSqlQuery(
                    "Some SQL...",
                    new[] { new SqlParameter("P1", "V1"), new SqlParameter("P2", "V2") }) },
                { typeof(string), () => "Fake" }
            };

            TestEventLogging(
                typeof(CosmosEventId),
                typeof(CosmosLoggerExtensions),
                new CosmosLoggingDefinitions(),
                fakeFactories);
        }
    }
}
