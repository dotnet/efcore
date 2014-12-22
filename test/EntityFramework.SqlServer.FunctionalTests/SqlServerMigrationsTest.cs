// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerMigrationsTest
    {
        #region Empty Migration
        [Fact]
        public async Task Empty_Migration_Does_Not_Create_Database()
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                using (var context = CreateEmptyContext(testDatabase))
                {
                    context.Database.EnsureDeleted();

                    Assert.Throws<SqlException>(() => context.Database.AsMigrationsEnabled().ApplyMigrations());

                    Assert.False(context.Database.AsRelational().Exists());
                }
            }
        }

        private static BloggingContext CreateEmptyContext(SqlServerTestStore testStore)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .BuildServiceProvider();

            var options = new DbContextOptions();
            options.UseSqlServer(testStore.Connection.ConnectionString);

            return new BloggingContext(
                serviceProvider,
                options);
        }

        private class BloggingContext : DbContext
        {
            public BloggingContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }

        [ContextType(typeof(BloggingContext))]
        public class EmptyMigration : Migration, IMigrationMetadata
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
            }

            public string MigrationId
            {
                get { return "Empty"; }
            }

            public string ProductVersion
            {
                get { return "EF7"; }
            }

            public IModel TargetModel
            {
                get { return new Model(); }
            }
        }


        #endregion

        #region Create Database Migration

        [Fact]
        public async Task Create_Database_Command_Creates_Database()
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                using (var context = CreateDatabaseContext(testDatabase))
                {
                    context.Database.EnsureDeleted();

                    context.Database.AsMigrationsEnabled().ApplyMigrations();

                    Assert.True(context.Database.AsRelational().Exists());
                }
            }
        }

        [Fact]
        public async Task Create_Database_Command_is_Idempotent()
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                using (var context = CreateDatabaseContext(testDatabase))
                {
                    context.Database.EnsureCreated();

                    context.Database.AsMigrationsEnabled().ApplyMigrations();

                    Assert.True(context.Database.AsRelational().Exists());
                }
            }
        }

        private static CreateDatabaseBloggingContext CreateDatabaseContext(SqlServerTestStore testStore)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .BuildServiceProvider();

            var options = new DbContextOptions();
            options.UseSqlServer(testStore.Connection.ConnectionString);

            return new CreateDatabaseBloggingContext(
                serviceProvider,
                options);
        }

        private class CreateDatabaseBloggingContext : BloggingContext
        {
            public CreateDatabaseBloggingContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }
        }

        [ContextType(typeof(CreateDatabaseBloggingContext))]
        public class CreateDatabaseMigration : Migration, IMigrationMetadata
        {
            public override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateDatabaseIfNotExists();
            }

            public override void Down(MigrationBuilder migrationBuilder)
            {
            }

            public string MigrationId
            {
                get { return "CreateDatabase"; }
            }

            public string ProductVersion
            {
                get { return "EF7"; }
            }

            public IModel TargetModel
            {
                get { return new Model(); }
            }
        }

        #endregion
    }
}
