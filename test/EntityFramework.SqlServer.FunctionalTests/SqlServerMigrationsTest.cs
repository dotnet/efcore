// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerMigrationsTest
    {
        [Fact]
        public async Task Empty_Migration_Creates_Database()
        {
            using (var testDatabase = await SqlServerTestStore.CreateScratchAsync(createDatabase: false))
            {
                using (var context = CreateContext(testDatabase))
                {
                    context.Database.AsMigrationsEnabled().ApplyMigrations();

                    Assert.True(context.Database.AsRelational().Exists());
                }
            }
        }

        private static BloggingContext CreateContext(SqlServerTestStore testStore)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .BuildServiceProvider();

            var options =new DbContextOptions();
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
    }
}
