// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class MigrationsAssemblyTest
    {
        [Fact]
        public void FindMigration_matches_id_when_exact_case()
            => Assert.IsType<Migration2>(CreateMigrationsAssembly().FindMigration("20150302103100_FLUTTER"));

        [Fact]
        public void FindMigration_returns_first_candidate_migration()
            => Assert.IsType<Migration1>(CreateMigrationsAssembly().FindMigration("20150302103100_flutter"));

        [Fact]
        public void FindMigration_matches_name_when_exact_case()
            => Assert.IsType<Migration2>(CreateMigrationsAssembly().FindMigration("FLUTTER"));

        [Fact]
        public void FindMigration_returns_migration_of_first_candidate_name()
            => Assert.IsType<Migration1>(CreateMigrationsAssembly().FindMigration("flutter"));

        [Fact]
        public void FindMigration_returns_null_when_no_match()
            => Assert.Null(CreateMigrationsAssembly().FindMigration("Spike"));

        [Fact]
        public void GetMigration_throws_when_no_match()
            => Assert.Equal(
                Strings.MigrationNotFound("Spike"),
                Assert.Throws<InvalidOperationException>(
                        () => CreateMigrationsAssembly().GetMigration("Spike"))
                    .Message);

        private IMigrationsAssembly CreateMigrationsAssembly()
            => new MigrationsAssembly(
                new Context(),
                new DbContextOptions<DbContext>(
                    new Dictionary<Type, IDbContextOptionsExtension>
                    {
                        { typeof(ConcreteOptionsExtension), new ConcreteOptionsExtension() }
                    }),
                new MigrationsIdGenerator());

        private class Context : DbContext
        {
        }

        [DbContext(typeof(Context))]
        private class Migration1 : Migration
        {
            public override string Id { get; } = "20150302103100_Flutter";

            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(Context))]
        private class Migration2 : Migration
        {
            public override string Id { get; } = "20150302103100_FLUTTER";

            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }

        private class ConcreteOptionsExtension : RelationalOptionsExtension
        {
            public override void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
            }
        }
    }
}
