﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Cosmos.Diagnostics;

public class CosmosEventIdTest : EventIdTestBase
{
    [ConditionalFact]
    public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
    {
        var model = new Model();
        var entityType = model.AddEntityType(typeof(object), owned: false, ConfigurationSource.Convention)!;
        var property = entityType.AddProperty("A", typeof(int), ConfigurationSource.Convention, ConfigurationSource.Convention);

        var fakeFactories = new Dictionary<Type, Func<object>>
        {
            { typeof(IEntityType), () => entityType },
            { typeof(IProperty), () => property },
            {
                typeof(CosmosSqlQuery), () => new CosmosSqlQuery(
                    "Some SQL...",
                    new[] { new SqlParameter("P1", "V1"), new SqlParameter("P2", "V2") })
            },
            { typeof(string), () => "Fake" }
        };

        TestEventLogging(
            typeof(CosmosEventId),
            typeof(CosmosLoggerExtensions),
            new CosmosLoggingDefinitions(),
            fakeFactories);
    }
}
