// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerMigrationsTest
    {
        [Fact]
        public async Task Empty_Migration_Creates_Database()
        {
            using (var testDatabase = SqlServerTestStore.CreateScratch(createDatabase: false))
            {
                using (var context = CreateContext(testDatabase))
                {
                    var creator = (SqlServerDatabaseCreator)context.GetService<IRelationalDatabaseCreator>();
                    creator.RetryTimeout = TimeSpan.FromMinutes(10);

                    await context.Database.MigrateAsync();

                    Assert.True(creator.Exists());
                }
            }
        }

        private static BloggingContext CreateContext(SqlServerTestStore testStore)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseSqlServer(testStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(600))
                .UseInternalServiceProvider(serviceProvider);

            return new BloggingContext(optionsBuilder.Options);
        }

        private class BloggingContext : DbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
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
