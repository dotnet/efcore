// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.InMemory;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;
using System.Threading.Tasks;
using Remotion.Linq;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class ExceptionInterceptionTest
    {
        [Fact]
        public async Task SaveChanges_throws_DataStoreOperationException_for_store_exception_nonasync()
        {
            await SaveChanges_throws_DataStoreOperationException_for_store_exception(async: false);
        }

        [Fact]
        public async Task SaveChanges_throws_DataStoreOperationException_for_store_exception_async()
        {
            await SaveChanges_throws_DataStoreOperationException_for_store_exception(async: true);
        }

        public async Task SaveChanges_throws_DataStoreOperationException_for_store_exception(bool async)
        {
            using (var context = new BloggingContext())
            {
                context.Blogs.Add(new BloggingContext.Blog(jimSaysThrow: false) { Url = "http://sample.com" });
                context.SaveChanges();
                context.ChangeTracker.Entries().Single().State = EntityState.Added;

                Exception ex;
                if (async)
                {
                    ex = await Assert.ThrowsAsync<DataStoreException>(() => context.SaveChangesAsync());
                }
                else
                {
                    ex = Assert.Throws<DataStoreException>(() => context.SaveChanges());
                }

                Assert.IsType<DataStoreException>(ex);
                Assert.Equal(context, ((DataStoreException)ex).Context);
                // Original exception from DataStore is nested within an AggregateException
                Assert.IsType<ArgumentException>(ex.InnerException);
            }
        }

        [Fact]
        public async Task Query_throws_DatabaseOperationException_when_DataStore_query_throws_nonasync()
        {
            await Query_throws_DatabaseOperationException_when_DataStore_query_throws(async: false);
        }

        [Fact]
        public async Task Query_throws_DatabaseOperationException_when_DataStore_query_throws_async()
        {
            await Query_throws_DatabaseOperationException_when_DataStore_query_throws(async: true);
        }

        public async Task Query_throws_DatabaseOperationException_when_DataStore_query_throws(bool async)
        {
            var services = new ServiceCollection();
            services.AddEntityFramework()
                .AddInMemoryStore()
                .ServiceCollection
                .AddScoped<InMemoryDataStore, FaultedDataStore>();

            using (var context = new BloggingContext(services.BuildServiceProvider()))
            {
                Exception ex;
                if (async)
                {
                    ex = await Assert.ThrowsAsync<DataStoreException>(() => context.Blogs.FirstOrDefaultAsync());
                }
                else
                {
                    ex = Assert.Throws<DataStoreException>(() => context.Blogs.FirstOrDefault());
                }
                Assert.IsType<DataStoreException>(ex);
                Assert.Equal(context, ((DataStoreException)ex).Context);
                Assert.IsType<ArgumentException>(ex.InnerException);
                Assert.Equal(
                    async
                        ? "Jim said to throw from AsyncQuery!"
                        : "Jim said to throw from Query!",
                    ex.InnerException.Message);
            }
        }

        public class FaultedDataStore : InMemoryDataStore
        {
            public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel, StateManager stateManager)
            {
                throw new ArgumentException("Jim said to throw from Query!");
            }

            public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(QueryModel queryModel, StateManager stateManager)
            {
                throw new ArgumentException("Jim said to throw from AsyncQuery!");
            }
        }

        [Fact]
        public void Query_throws_DatabaseOperationException_for_store_exception_during_DbSet_enumeration()
        {
            Query_throws_DatabaseOperationException_for_store_exception(c => c.Blogs.ToList(), asyncOperation: false);
        }

        [Fact]
        public void Query_throws_DatabaseOperationException_for_store_exception_during_DbSet_enumeration_async()
        {
            Query_throws_DatabaseOperationException_for_store_exception(c => c.Blogs.ToListAsync().Wait(), asyncOperation: true);
        }

        [Fact]
        public void Query_throws_DatabaseOperationException_for_store_exception_during_LINQ_enumeration()
        {
            Query_throws_DatabaseOperationException_for_store_exception(c =>
                c.Blogs
                 .OrderBy(b => b.Name)
                 .Where(b => b.Url.StartsWith("http://"))
                 .ToList(),
                asyncOperation: false);
        }

        [Fact]
        public void Query_throws_DatabaseOperationException_for_store_exception_during_LINQ_enumeration_async()
        {
            Query_throws_DatabaseOperationException_for_store_exception(c =>
                c.Blogs
                 .OrderBy(b => b.Name)
                 .Where(b => b.Url.StartsWith("http://"))
                 .ToListAsync()
                 .Wait(),
                asyncOperation: true);
        }

        public void Query_throws_DatabaseOperationException_for_store_exception(Action<BloggingContext> test, bool asyncOperation)
        {
            using (var context = new BloggingContext())
            {
                context.Blogs.Add(new BloggingContext.Blog(false) { Url = "http://sample.com" });
                context.SaveChanges();
                var entry = context.ChangeTracker.StateManager.StateEntries.Single();
                context.ChangeTracker.StateManager.StopTracking(entry);


                var ex = Assert.ThrowsAny<Exception>(() => test(context));
                if (asyncOperation)
                {
                    // Get rid of AggregateExceptions from async operation
                    ex = ex.InnerException.InnerException;
                }

                Assert.IsType<DataStoreException>(ex);
                Assert.Equal(context, ((DataStoreException)ex).Context);
                Assert.IsType<ArgumentException>(ex.InnerException);
                Assert.Equal("Jim said to throw from ctor!", ex.InnerException.Message);
            }
        }

        public class BloggingContext : DbContext
        {
            public BloggingContext()
            { }

            public BloggingContext(IServiceProvider provider)
                : base(provider)
            { }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public Blog()
                    : this(true)
                { }

                public Blog(bool jimSaysThrow)
                {
                    if (jimSaysThrow)
                    {
                        throw new ArgumentException("Jim said to throw from ctor!");
                    }
                }

                public string Url { get; set; }
                public string Name { get; set; }
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore(persist: false);
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<Blog>().Key(b => b.Url);
            }
        }
    }
}
