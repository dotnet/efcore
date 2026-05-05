// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable StringStartsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore;

public class DatabaseErrorLogStateTest
{
    [ConditionalFact]
    public Task SaveChanges_logs_DatabaseErrorLogState_nonasync()
        => SaveChanges_logs_DatabaseErrorLogState_test(async: false);

    [ConditionalFact]
    public Task SaveChanges_logs_DatabaseErrorLogState_async()
        => SaveChanges_logs_DatabaseErrorLogState_test(async: true);

    private async Task SaveChanges_logs_DatabaseErrorLogState_test(bool async)
    {
        var loggerFactory = new TestLoggerFactory();
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<ILoggerFactory>(loggerFactory)
            .BuildServiceProvider(validateScopes: true);

        using var context = new BloggingContext(serviceProvider);
        context.Blogs.Add(
            new BloggingContext.Blog(jimSaysThrow: false) { Url = "http://sample.com" });
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
        Assert.Same(typeof(BloggingContext), loggerFactory.Logger.LastDatabaseErrorState.Single(p => p.Key == "contextType").Value);
        Assert.EndsWith(
            ex.ToString(), loggerFactory.Logger.LastDatabaseErrorFormatter(loggerFactory.Logger.LastDatabaseErrorState, ex));
    }

    [ConditionalFact]
    public Task Query_logs_DatabaseErrorLogState_during_DbSet_enumeration()
        => Query_logs_DatabaseErrorLogState_test(c => c.Blogs.ToList());

    [ConditionalFact]
    public Task Query_logs_DatabaseErrorLogState_during_DbSet_enumeration_async()
        => Query_logs_DatabaseErrorLogState_test(c => c.Blogs.ToListAsync());

    [ConditionalFact]
    public Task Query_logs_DatabaseErrorLogState_during_LINQ_enumeration()
        => Query_logs_DatabaseErrorLogState_test(
            c => c.Blogs
                .OrderBy(b => b.Name)
                .Where(b => b.Url.StartsWith("http://"))
                .ToList());

    [ConditionalFact]
    public Task Query_logs_DatabaseErrorLogState_during_LINQ_enumeration_async()
        => Query_logs_DatabaseErrorLogState_test(
            c => c.Blogs
                .OrderBy(b => b.Name)
                .Where(b => b.Url.StartsWith("http://"))
                .ToListAsync());

    [ConditionalFact]
    public Task Query_logs_DatabaseErrorLogState_during_single()
        => Query_logs_DatabaseErrorLogState_test(c => c.Blogs.FirstOrDefault());

    [ConditionalFact]
    public Task Query_logs_DatabaseErrorLogState_during_single_async()
        => Query_logs_DatabaseErrorLogState_test(c => c.Blogs.FirstOrDefaultAsync());

    private Task Query_logs_DatabaseErrorLogState_test(Action<BloggingContext> test)
        => Query_logs_DatabaseErrorLogState_test(
            c =>
            {
                test(c);
                return Task.CompletedTask;
            });

    private async Task Query_logs_DatabaseErrorLogState_test(Func<BloggingContext, Task> test)
    {
        var loggerFactory = new TestLoggerFactory();
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<ILoggerFactory>(loggerFactory)
            .BuildServiceProvider(validateScopes: true);

        using var context = new BloggingContext(serviceProvider);
        context.Blogs.Add(
            new BloggingContext.Blog(false) { Url = "http://sample.com" });
        context.SaveChanges();
        var entry = context.ChangeTracker.Entries().Single().GetInfrastructure();
        context.GetService<IStateManager>().StopTracking(entry, entry.EntityState);

        var ex = await Assert.ThrowsAnyAsync<Exception>(() => test(context));
        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
        }

        Assert.Equal("Jim said to throw from ctor!", ex.Message);
        Assert.Same(ex, loggerFactory.Logger.LastDatabaseErrorException);
        Assert.Same(typeof(BloggingContext), loggerFactory.Logger.LastDatabaseErrorState.Single(p => p.Key == "contextType").Value);
        Assert.EndsWith(
            ex.ToString(), loggerFactory.Logger.LastDatabaseErrorFormatter(loggerFactory.Logger.LastDatabaseErrorState, ex));
    }

    public class BloggingContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

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
            => modelBuilder.Entity<Blog>().HasKey(b => b.Url);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(_serviceProvider);
    }

    private class TestLoggerFactory : ILoggerFactory
    {
        public readonly TestLogger Logger = new();

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string name)
            => Logger;

        public void Dispose()
        {
        }

        public class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state)
                => NullScope.Instance;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (eventId.Id == CoreEventId.SaveChangesFailed.Id
                    || eventId.Id == CoreEventId.QueryIterationFailed.Id)
                {
                    LastDatabaseErrorState = (IReadOnlyList<KeyValuePair<string, object>>)state;
                    LastDatabaseErrorException = exception;
                    LastDatabaseErrorFormatter = (s, e) => formatter((TState)s, e);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
                => true;

            public IReadOnlyList<KeyValuePair<string, object>> LastDatabaseErrorState { get; private set; }
            public Exception LastDatabaseErrorException { get; private set; }
            public Func<object, Exception, string> LastDatabaseErrorFormatter { get; private set; }

            private class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                    // intentionally does nothing
                }
            }
        }
    }
}
