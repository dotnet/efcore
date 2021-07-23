// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqliteEventIdTest : EventIdTestBase
    {
        [ConditionalFact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var entityType = new EntityType(typeof(object), new Model(new ConventionSet()), owned: false, ConfigurationSource.Convention);
            entityType.Model.FinalizeModel();

            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(string), () => "Fake" },
                { typeof(IEntityType), () => entityType },
                { typeof(IReadOnlySequence), () => new FakeSequence() },
                { typeof(Type), () => typeof(object) }
            };

            TestEventLogging(
                typeof(SqliteEventId),
                typeof(SqliteLoggerExtensions),
                new SqliteLoggingDefinitions(),
                fakeFactories);
        }

        private class FakeSequence : Annotatable, IReadOnlySequence
        {
            public string Name
                => "SequenceName";

            public string Schema
                => throw new NotImplementedException();

            public long StartValue
                => throw new NotImplementedException();

            public int IncrementBy
                => throw new NotImplementedException();

            public long? MinValue
                => throw new NotImplementedException();

            public long? MaxValue
                => throw new NotImplementedException();

            public Type ClrType
                => throw new NotImplementedException();

            public Type Type
                => throw new NotImplementedException();

            public IReadOnlyModel Model
                => throw new NotImplementedException();

            public bool IsCyclic
                => throw new NotImplementedException();
        }
    }
}
