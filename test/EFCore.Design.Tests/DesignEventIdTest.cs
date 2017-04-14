// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.FunctionalTests;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;
using Microsoft.EntityFrameworkCore.Migrations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.Design.Tests
{
    public class DesignEventIdTest
    {
        [Fact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(Type), () => typeof(object) },
                { typeof(Migration), () => new FakeMigration() },
                { typeof(ModelSnapshot), () => new FakeModelSnapshot() },
                { typeof(ScaffoldedMigration), () => new FakeScaffoldedMigration() },
                { typeof(string), () => "Fake" },
                { typeof(IEnumerable<MigrationOperation>), () => new List<MigrationOperation>() }
            };

            InMemoryTestHelpers.Instance.TestEventLogging(typeof(DesignEventId), typeof(DesignLoggerExtensions), fakeFactories);
        }

        private class FakeScaffoldedMigration : ScaffoldedMigration
        {
            public FakeScaffoldedMigration()
                : base("A", "B", "C", "D", "E", "F", "G", "H", "I")
            {
            }
        }

        private class FakeMigration : Migration
        {
            protected override void Up([NotNull] MigrationBuilder migrationBuilder) => throw new NotImplementedException();
        }

        private class FakeModelSnapshot : ModelSnapshot
        {
            protected override void BuildModel([NotNull] ModelBuilder modelBuilder) => throw new NotImplementedException();
        }
    }
}
