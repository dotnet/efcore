// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Xunit;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class DataStoreErrorLogStateTest
    {
        [Fact]
        public async Task SaveChanges_logs_DataStoreErrorLogState_nonasync()
        {
            await SaveChanges_logs_DataStoreErrorLogState(async: false);
        }

        [Fact]
        public async Task SaveChanges_logs_DataStoreErrorLogState_async()
        {
            await SaveChanges_logs_DataStoreErrorLogState(async: true);
        }
        
        public async Task SaveChanges_logs_DataStoreErrorLogState(bool async)
        {
            var loggerFactory = new TestLoggerFactory();
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore().UseLoggerFactory(loggerFactory);
            var serviceProvider = services.BuildServiceProvider();

            using (var context = new BloggingContext(serviceProvider))
            {
                context.Blogs.Add(new BloggingContext.Blog(jimSaysThrow: false) { Url = "http://sample.com" });
                context.SaveChanges();
                context.ChangeTracker.Entries().Single().State = EntityState.Added;

                Exception ex;
                if (async)
                {
                    ex = await Assert.ThrowsAsync<ArgumentException>(() => context.SaveChangesAsync());
                }
                else
                {
                    ex = Assert.Throws<ArgumentException>(() => context.SaveChanges());
                }

                Assert.Same(ex, loggerFactory.Logger.LastDataStoreErrorException);
                Assert.Same(typeof(BloggingContext), loggerFactory.Logger.LastDataStoreErrorState.ContextType);
                Assert.EndsWith(ex.ToString(), loggerFactory.Logger.LastDataStoreErrorFormatter(loggerFactory.Logger.LastDataStoreErrorState, ex));
            }
        }

        [Fact]
        public void Query_logs_DataStoreErrorLogState_during_DbSet_enumeration()
        {
            Query_logs_DataStoreErrorLogState(c => c.Blogs.ToList());
        }

        [Fact]
        public void Query_logs_DataStoreErrorLogState_during_DbSet_enumeration_async()
        {
            Query_logs_DataStoreErrorLogState(c => c.Blogs.ToListAsync().Wait());
        }

        [Fact]
        public void Query_logs_DataStoreErrorLogState_during_LINQ_enumeration()
        {
            Query_logs_DataStoreErrorLogState(c =>
                c.Blogs
                    .OrderBy(b => b.Name)
                    .Where(b => b.Url.StartsWith("http://"))
                    .ToList());
        }

        [Fact]
        public void Query_logs_DataStoreErrorLogState_during_LINQ_enumeration_async()
        {
            Query_logs_DataStoreErrorLogState(c =>
                c.Blogs
                    .OrderBy(b => b.Name)
                    .Where(b => b.Url.StartsWith("http://"))
                    .ToListAsync()
                    .Wait());
        }

        public void Query_logs_DataStoreErrorLogState(Action<BloggingContext> test)
        {
            var loggerFactory = new TestLoggerFactory();
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore().UseLoggerFactory(loggerFactory);
            var serviceProvider = services.BuildServiceProvider();

            using (var context = new BloggingContext(serviceProvider))
            {
                context.Blogs.Add(new BloggingContext.Blog(false) { Url = "http://sample.com" });
                context.SaveChanges();
                var entry = context.ChangeTracker.StateManager.StateEntries.Single();
                context.ChangeTracker.StateManager.StopTracking(entry);

                var ex = Assert.ThrowsAny<Exception>(() => test(context));
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                Assert.Equal("Jim said to throw from ctor!", ex.Message);
                Assert.Same(ex, loggerFactory.Logger.LastDataStoreErrorException);
                Assert.Same(typeof(BloggingContext), loggerFactory.Logger.LastDataStoreErrorState.ContextType);
                Assert.EndsWith(ex.ToString(), loggerFactory.Logger.LastDataStoreErrorFormatter(loggerFactory.Logger.LastDataStoreErrorState, ex));
            }
        }

        public class BloggingContext : DbContext
        {
            public BloggingContext(IServiceProvider provider)
                : base(provider)
            { 
            }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public Blog()
                    : this(true)
                {
                }

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

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Blog>().Key(b => b.Url);
            }
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            public readonly TestLogger Logger = new TestLogger();

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger Create(string name)
            {
                return Logger;
            }

            public class TestLogger : ILogger
            {
                public IDisposable BeginScope(object state)
                {
                    return NullScope.Instance;
                }

                public bool WriteCore(TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
                {
                    var error = state as DataStoreErrorLogState;
                    if (error != null)
                    {
                        LastDataStoreErrorState = error;
                        LastDataStoreErrorException = exception;
                        LastDataStoreErrorFormatter = formatter;
                    }

                    return true;
                }

                public DataStoreErrorLogState LastDataStoreErrorState { get; set; }
                public Exception LastDataStoreErrorException { get; set; }
                public Func<object, Exception, string> LastDataStoreErrorFormatter { get; set; }
            }
        }
    }
}
