// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class DatabaseErrorLogStateTest
    {
        [Fact]
        public async Task SaveChanges_logs_DatabaseErrorLogState_nonasync()
        {
            await SaveChanges_logs_DatabaseErrorLogState(async: false);
        }

        [Fact]
        public async Task SaveChanges_logs_DatabaseErrorLogState_async()
        {
            await SaveChanges_logs_DatabaseErrorLogState(async: true);
        }

        public async Task SaveChanges_logs_DatabaseErrorLogState(bool async)
        {
            var loggerFactory = new TestLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .GetService()
                .AddInstance<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

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

                Assert.Same(ex, loggerFactory.Logger.LastDatabaseErrorException);
                Assert.Same(typeof(BloggingContext), loggerFactory.Logger.LastDatabaseErrorState.ContextType);
                Assert.EndsWith(ex.ToString(), loggerFactory.Logger.LastDatabaseErrorFormatter(loggerFactory.Logger.LastDatabaseErrorState, ex));
            }
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_DbSet_enumeration()
        {
            Query_logs_DatabaseErrorLogState(c => c.Blogs.ToList());
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_DbSet_enumeration_async()
        {
            Query_logs_DatabaseErrorLogState(c => c.Blogs.ToListAsync().Wait());
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_LINQ_enumeration()
        {
            Query_logs_DatabaseErrorLogState(c =>
                c.Blogs
                    .OrderBy(b => b.Name)
                    .Where(b => b.Url.StartsWith("http://"))
                    .ToList());
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_LINQ_enumeration_async()
        {
            Query_logs_DatabaseErrorLogState(c =>
                c.Blogs
                    .OrderBy(b => b.Name)
                    .Where(b => b.Url.StartsWith("http://"))
                    .ToListAsync()
                    .Wait());
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_single()
        {
            Query_logs_DatabaseErrorLogState(c => c.Blogs.FirstOrDefault());
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_single_async()
        {
            Query_logs_DatabaseErrorLogState(c => c.Blogs.FirstOrDefaultAsync().Wait());
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_scalar()
        {
            Query_logs_DatabaseErrorLogState(c => c.Blogs.Count());
        }

        [Fact]
        public void Query_logs_DatabaseErrorLogState_during_scalar_async()
        {
            Query_logs_DatabaseErrorLogState(c => c.Blogs.CountAsync().Wait());
        }

        public void Query_logs_DatabaseErrorLogState(Action<BloggingContext> test)
        {
            var loggerFactory = new TestLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .ServiceCollection()
                .AddInstance<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new BloggingContext(serviceProvider))
            {
                context.Blogs.Add(new BloggingContext.Blog(false) { Url = "http://sample.com" });
                context.SaveChanges();
                var entry = context.ChangeTracker.Entries().Single().GetService();
                context.ChangeTracker.GetService().StopTracking(entry);

                var ex = Assert.ThrowsAny<Exception>(() => test(context));
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                Assert.Equal("Jim said to throw from ctor!", ex.Message);
                Assert.Same(ex, loggerFactory.Logger.LastDatabaseErrorException);
                Assert.Same(typeof(BloggingContext), loggerFactory.Logger.LastDatabaseErrorState.ContextType);
                Assert.EndsWith(ex.ToString(), loggerFactory.Logger.LastDatabaseErrorFormatter(loggerFactory.Logger.LastDatabaseErrorState, ex));
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

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
                => optionsBuilder.UseInMemoryDatabase();
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            public LogLevel MinimumLevel { get; set; }

            public readonly TestLogger Logger = new TestLogger();

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string name)
            {
                return Logger;
            }

            public class TestLogger : ILogger
            {
                public IDisposable BeginScopeImpl(object state)
                {
                    return NullScope.Instance;
                }

                public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
                {
                    var error = state as DatabaseErrorLogState;
                    if (error != null)
                    {
                        LastDatabaseErrorState = error;
                        LastDatabaseErrorException = exception;
                        LastDatabaseErrorFormatter = formatter;
                    }
                }

                public bool IsEnabled(LogLevel logLevel)
                {
                    return true;
                }

                public IDisposable BeginScope(object state)
                {
                    throw new NotImplementedException();
                }

                public DatabaseErrorLogState LastDatabaseErrorState { get; set; }
                public Exception LastDatabaseErrorException { get; set; }
                public Func<object, Exception, string> LastDatabaseErrorFormatter { get; set; }
            }
        }
    }
}
