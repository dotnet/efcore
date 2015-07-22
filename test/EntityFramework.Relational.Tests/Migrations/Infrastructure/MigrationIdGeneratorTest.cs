// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class MigrationIdGeneratorTest
    {
        [Fact]
        public void CreateId_works()
        {
            var id = new MigrationIdGenerator().CreateId("Twilight");

            Assert.Matches("[0-9]{14}_Twilight", id);
        }

        [Fact]
        public void CreateId_always_increments_timestamp()
        {
            var generator = new MigrationIdGenerator();

            var id1 = generator.CreateId("Rainbow");
            var id2 = generator.CreateId("Rainbow");

            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void GetName_works()
        {
            var name = new MigrationIdGenerator().GetName("20150302100620_Apple");

            Assert.Equal("Apple", name);
        }

        [Fact]
        public void IsValidId_returns_true_when_valid()
        {
            var valid = new MigrationIdGenerator().IsValidId("20150302100930_Rarity");

            Assert.True(valid);
        }

        [Fact]
        public void IsValidId_returns_false_when_invalid()
        {
            var valid = new MigrationIdGenerator().IsValidId("Rarity");

            Assert.False(valid);
        }

        [Fact]
        public void ResolveId_matches_id_when_exact_case()
        {
            var migrations = new[]
            {
                new MockMigration("20150302103100_FLUTTER"),
                new MockMigration("20150302103100_Flutter")
            };
            var id = new MigrationIdGenerator().ResolveId("20150302103100_Flutter", migrations);

            Assert.Equal("20150302103100_Flutter", id);
        }

        [Fact]
        public void ResolveId_returns_first_candidate_id()
        {
            var migrations = new[]
            {
                new MockMigration("20150302103100_FLUTTER"),
                new MockMigration("20150302103100_Flutter")
            };
            var id = new MigrationIdGenerator().ResolveId("20150302103100_flutter", migrations);

            Assert.Equal("20150302103100_FLUTTER", id);
        }

        [Fact]
        public void ResolveId_matches_name_when_exact_case()
        {
            var migrations = new[]
            {
                new MockMigration("20150302103630_PINKIE"),
                new MockMigration("20150302103600_Pinkie")
            };
            var id = new MigrationIdGenerator().ResolveId("Pinkie", migrations);

            Assert.Equal("20150302103600_Pinkie", id);
        }

        [Fact]
        public void ResolveId_returns_id_of_first_candidate_name()
        {
            var migrations = new[]
            {
                new MockMigration("20150302103630_PINKIE"),
                new MockMigration("20150302103600_Pinkie")
            };
            var id = new MigrationIdGenerator().ResolveId("pinkie", migrations);

            Assert.Equal("20150302103630_PINKIE", id);
        }

        [Fact]
        public void ResolveId_throws_when_no_match()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new MigrationIdGenerator().ResolveId("Spike", new Migration[0]));

            Assert.Equal(Strings.MigrationNotFound("Spike"), ex.Message);
        }

        private class MockMigration : Migration
        {
            public MockMigration(string id)
            {
                Id = id;
            }

            public override string Id { get; }

            public override void Down(MigrationBuilder migrationBuilder)
            {
            }

            public override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
