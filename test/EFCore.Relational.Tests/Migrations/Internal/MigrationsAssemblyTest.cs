// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class MigrationsAssemblyTest
    {
        [ConditionalFact]
        public void FindMigrationId_returns_first_candidate_when_id()
            => Assert.Equal(
                "20150302103100_Flutter",
                CreateMigrationsAssembly().FindMigrationId("20150302103100_FLUTTER"));

        [ConditionalFact]
        public void FindMigrationId_returns_first_candidate_when_name()
            => Assert.Equal(
                "20150302103100_Flutter",
                CreateMigrationsAssembly().FindMigrationId("FLUTTER"));

        [ConditionalFact]
        public void FindMigrationId_returns_null_when_no_match()
            => Assert.Null(CreateMigrationsAssembly().FindMigrationId("Spike"));

        [ConditionalFact]
        public void GetMigrationId_throws_when_no_match()
            => Assert.Equal(
                RelationalStrings.MigrationNotFound("Spike"),
                Assert.Throws<InvalidOperationException>(
                        () => CreateMigrationsAssembly().GetMigrationId("Spike"))
                    .Message);

        [ConditionalFact]
        public void Migrations_ignores_the_unattributed()
        {
            var logger = new TestLogger<DbLoggerCategory.Migrations, TestRelationalLoggingDefinitions>
            {
                EnabledFor = LogLevel.Warning
            };
            var assembly = CreateMigrationsAssembly(logger);

            var result = assembly.Migrations;

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, t => t.GetType() == typeof(MigrationWithoutAttribute));
            Assert.Equal(
                RelationalResources.LogMigrationAttributeMissingWarning(logger).GenerateMessage(nameof(MigrationWithoutAttribute)),
                logger.Message);
        }

        private IMigrationsAssembly CreateMigrationsAssembly(
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger = null)
            => new MigrationsAssembly(
                new CurrentDbContext(new Context()),
                new DbContextOptions<DbContext>(
                    new Dictionary<Type, IDbContextOptionsExtension>
                    {
                        { typeof(FakeRelationalOptionsExtension), new FakeRelationalOptionsExtension() }
                    }),
                new MigrationsIdGenerator(),
                logger ?? new FakeDiagnosticsLogger<DbLoggerCategory.Migrations>());

        private class Context : DbContext
        {
        }

        [DbContext(typeof(Context))]
        [Migration("20150302103100_Flutter")]
        private class Migration1 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(Context))]
        [Migration("20150302103100_FLUTTER")]
        private class Migration2 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }

        [DbContext(typeof(Context))]
        private class MigrationWithoutAttribute : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
