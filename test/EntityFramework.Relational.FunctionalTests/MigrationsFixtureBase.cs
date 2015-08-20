// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class MigrationsFixtureBase
    {
        private readonly LazyRef<IServiceProvider> _services;

        protected MigrationsFixtureBase()
        {
            _services = new LazyRef<IServiceProvider>(
                () =>
                {
                    var services = new ServiceCollection();
                    ConfigureServices(services);

                    return services.BuildServiceProvider();
                });
        }

        public DbContext CreateContext() => new MigrationsContext(_services.Value, OnConfiguring);

        protected abstract void ConfigureServices(IServiceCollection services);
        protected abstract void OnConfiguring(DbContextOptionsBuilder optionsBuilder);

        private class MigrationsContext : DbContext
        {
            private readonly Action<DbContextOptionsBuilder> _configure;

            public MigrationsContext(IServiceProvider serviceProvider, Action<DbContextOptionsBuilder> configure)
                : base(serviceProvider)
            {
                _configure = configure;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                _configure(optionsBuilder);
            }
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000001_Migration1")]
        private class Migration1 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder
                    .CreateTable(
                        name: "Table1",
                        columns: x => new
                        {
                            Id = x.Column<int>()
                        })
                    .PrimaryKey(
                        name: "PK_Table1",
                        columns: x => x.Id);
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.DropTable("Table1");
            }
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000002_Migration2")]
        private class Migration2 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.RenameTable(
                    name: "Table1",
                    newName: "Table2");
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.RenameTable(
                    name: "Table2",
                    newName: "Table1");
            }
        }
    }
}
