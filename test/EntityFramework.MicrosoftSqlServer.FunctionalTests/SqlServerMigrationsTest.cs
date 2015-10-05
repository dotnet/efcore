// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
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
                    context.Database.Migrate();

                    Assert.True(context.GetService<IRelationalDatabaseCreator>().Exists());
                }
            }
        }

        private static BloggingContext CreateContext(SqlServerTestStore testStore)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection()
                    .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(testStore.Connection.ConnectionString);

            return new BloggingContext(serviceProvider, optionsBuilder.Options);
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

        [DbContext(typeof(BloggingContext))]
        [Migration("00000000000000_Empty")]
        public class EmptyMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
