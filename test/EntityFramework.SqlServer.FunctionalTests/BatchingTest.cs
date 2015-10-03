// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class BatchingTest : IDisposable
    {
        [Fact]
        public void Batches_are_divided_correctly_with_two_inserted_columns()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(_testStore.Connection);

            using (var context = new BloggingContext(_serviceProvider, optionsBuilder.Options))
            {
                context.Database.EnsureCreated();

                for (var i = 1; i < 1101; i++)
                {
                    var blog = new Blog { Id = i, Name = "Foo" + i };
                    context.Blogs.Add(blog);
                }

                context.SaveChanges();
            }

            using (var context = new BloggingContext(_serviceProvider, optionsBuilder.Options))
            {
                Assert.Equal(1100, context.Blogs.Count());
            }
        }

        private class BloggingContext : DbContext
        {
            public BloggingContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>().Property(e => e.Id).ValueGeneratedNever();
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        private readonly SqlServerTestStore _testStore;
        private readonly IServiceProvider _serviceProvider;

        public BatchingTest()
        {
            _testStore = SqlServerTestStore.CreateScratch();
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
