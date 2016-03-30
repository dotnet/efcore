// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Migrations.Internal
{
    public class MigrationsAssemblyTest
    {
        [Fact]
        public void FindMigrationId_returns_first_candidate_when_id()
            => Assert.Equal(
                "20150302103100_Flutter",
                CreateMigrationsAssembly().FindMigrationId("20150302103100_FLUTTER"));

        [Fact]
        public void FindMigrationId_returns_first_candidate_when_name()
            => Assert.Equal(
                "20150302103100_Flutter",
                CreateMigrationsAssembly().FindMigrationId("FLUTTER"));

        [Fact]
        public void FindMigrationId_returns_null_when_no_match()
            => Assert.Null(CreateMigrationsAssembly().FindMigrationId("Spike"));

        [Fact]
        public void GetMigrationId_throws_when_no_match()
            => Assert.Equal(
                RelationalStrings.MigrationNotFound("Spike"),
                Assert.Throws<InvalidOperationException>(
                        () => CreateMigrationsAssembly().GetMigrationId("Spike"))
                    .Message);

        private IMigrationsAssembly CreateMigrationsAssembly()
            => new MigrationsAssembly(
                new CurrentDbContext(new Context()),
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

        private class ConcreteOptionsExtension : RelationalOptionsExtension
        {
            public override void ApplyServices(IServiceCollection services)
            {
            }
        }
    }
}
