// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Extensions.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Diagnostics;

public class SqlServerEventIdTest : EventIdTestBase
{
    [ConditionalFact]
    public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
    {
        var entityType = new EntityType(typeof(object), new Model(new ConventionSet()), owned: false, ConfigurationSource.Convention);
        var property = new Property(
            "A", typeof(int), null, null, entityType, ConfigurationSource.Convention, ConfigurationSource.Convention);
        entityType.Model.FinalizeModel();

        var fakeFactories = new Dictionary<Type, Func<object>>
        {
            { typeof(IList<string>), () => new List<string> { "Fake1", "Fake2" } },
            { typeof(IProperty), () => property },
            { typeof(IReadOnlyProperty), () => property },
            { typeof(string), () => "Fake" }
        };

        TestEventLogging(
            typeof(SqlServerEventId),
            typeof(SqlServerLoggerExtensions),
            new SqlServerLoggingDefinitions(),
            fakeFactories);
    }
}
