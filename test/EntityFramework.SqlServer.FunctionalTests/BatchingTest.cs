// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class BatchingTest : IDisposable
    {
        [Fact]
        public void Batches_are_divided_correctly_with_two_inserted_columns()
        {
            var options = new DbContextOptions();
            options.UseSqlServer(_testStore.Connection);

            using (var context = new BloggingContext(_serviceProvider, options))
            {
                context.Database.EnsureCreated();

                for (var i = 1; i < 1101; i++)
                {
                    var blog = new Blog { Id = i, Name = "Foo" + i };
                    context.Set<Blog>().Add(blog);
                }

                context.SaveChanges();
            }

            using (var context = new BloggingContext(_serviceProvider, options))
            {
                Assert.Equal(1100, context.Set<Blog>().Count());
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
                modelBuilder.Entity<Blog>().Property(b => b.Id).GenerateValueOnAdd(generateValue: false);
            }
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
                .ServiceCollection
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
