// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public class WarningsTest
{
    [ConditionalFact]
    public void Should_throw_by_default_when_transaction()
    {
        var optionsBuilder
            = new DbContextOptionsBuilder()
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        using var context = new DbContext(optionsBuilder.Options);
        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                InMemoryEventId.TransactionIgnoredWarning,
                InMemoryResources.LogTransactionsNotSupported(new TestLogger<InMemoryLoggingDefinitions>()).GenerateMessage(),
                "InMemoryEventId.TransactionIgnoredWarning"),
            Assert.Throws<InvalidOperationException>(
                () => context.Database.BeginTransaction()).Message);
    }

    [ConditionalFact]
    public void Should_throw_by_default_when_transaction_enlisted()
    {
        var optionsBuilder
            = new DbContextOptionsBuilder()
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        using var context = new DbContext(optionsBuilder.Options);
        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                InMemoryEventId.TransactionIgnoredWarning,
                InMemoryResources.LogTransactionsNotSupported(new TestLogger<InMemoryLoggingDefinitions>()).GenerateMessage(),
                "InMemoryEventId.TransactionIgnoredWarning"),
            Assert.Throws<InvalidOperationException>(
                () => context.Database.EnlistTransaction(new CommittableTransaction())).Message);
    }

    [ConditionalFact]
    public void Should_not_throw_by_default_when_transaction_and_ignored()
    {
        var optionsBuilder
            = new DbContextOptionsBuilder()
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        using var context = new DbContext(optionsBuilder.Options);
        context.Database.BeginTransaction();
    }

    [ConditionalFact]
    public void Throws_by_default_for_lazy_load_with_disposed_context()
    {
        var loggerFactory = new ListLoggerFactory();

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<ILoggerFactory>(loggerFactory)
            .BuildServiceProvider(validateScopes: true);

        using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
        {
            context.Add(
                new WarningAsErrorEntity { Nav = new IncludedEntity() });
            context.SaveChanges();
        }

        WarningAsErrorEntity entity;

        using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
        {
            entity = context.WarningAsErrorEntities.OrderBy(e => e.Id).First();
        }

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<InMemoryLoggingDefinitions>())
                    .GenerateMessage("WarningAsErrorEntity", "Nav"),
                "CoreEventId.LazyLoadOnDisposedContextWarning"),
            Assert.Throws<InvalidOperationException>(
                () => entity.Nav).Message);
    }

    [ConditionalFact]
    public void Lazy_load_with_disposed_context_can_be_configured_to_log()
    {
        var loggerFactory = new ListLoggerFactory();

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<ILoggerFactory>(loggerFactory)
            .BuildServiceProvider(validateScopes: true);

        using (var context = new WarningAsErrorContext(
                   serviceProvider,
                   defaultThrow: false,
                   CoreEventId.LazyLoadOnDisposedContextWarning))
        {
            context.Add(
                new WarningAsErrorEntity { Nav = new IncludedEntity() });
            context.SaveChanges();
        }

        WarningAsErrorEntity entity;

        using (var context = new WarningAsErrorContext(
                   serviceProvider,
                   defaultThrow: false,
                   CoreEventId.LazyLoadOnDisposedContextWarning))
        {
            entity = context.WarningAsErrorEntities.OrderBy(e => e.Id).First();
        }

        Assert.Null(entity.Nav);

        var log = loggerFactory.Log.Single(
            l => l.Message
                == CoreResources
                    .LogLazyLoadOnDisposedContext(new TestLogger<InMemoryLoggingDefinitions>())
                    .GenerateMessage("WarningAsErrorEntity", "Nav"));

        Assert.Equal(LogLevel.Warning, log.Level);
    }

    [ConditionalFact]
    public void Lazy_load_with_disposed_context_can_be_configured_to_log_at_debug_level()
    {
        var loggerFactory = new ListLoggerFactory();

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<ILoggerFactory>(loggerFactory)
            .BuildServiceProvider(validateScopes: true);

        using (var context = new WarningAsErrorContext(
                   serviceProvider,
                   defaultThrow: false,
                   toChangeLevel: (CoreEventId.LazyLoadOnDisposedContextWarning, LogLevel.Debug)))
        {
            context.Add(
                new WarningAsErrorEntity { Nav = new IncludedEntity() });
            context.SaveChanges();
        }

        WarningAsErrorEntity entity;

        using (var context = new WarningAsErrorContext(
                   serviceProvider,
                   defaultThrow: false,
                   toChangeLevel: (CoreEventId.LazyLoadOnDisposedContextWarning, LogLevel.Debug)))
        {
            entity = context.WarningAsErrorEntities.OrderBy(e => e.Id).First();
        }

        Assert.Null(entity.Nav);

        var log = loggerFactory.Log.Single(
            l => l.Message
                == CoreResources
                    .LogLazyLoadOnDisposedContext(new TestLogger<InMemoryLoggingDefinitions>())
                    .GenerateMessage("WarningAsErrorEntity", "Nav"));

        Assert.Equal(LogLevel.Debug, log.Level);
    }

    [ConditionalFact]
    public void Lazy_loading_is_logged_only_when_actually_loading()
    {
        var loggerFactory = new ListLoggerFactory();

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .AddSingleton<ILoggerFactory>(loggerFactory)
            .BuildServiceProvider(validateScopes: true);

        using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
        {
            context.Add(
                new WarningAsErrorEntity { Nav = new IncludedEntity() });
            context.SaveChanges();
        }

        using (var context = new WarningAsErrorContext(serviceProvider, defaultThrow: false))
        {
            var entity = context.WarningAsErrorEntities.OrderBy(e => e.Id).First();

            loggerFactory.Clear();
            Assert.NotNull(entity.Nav);

            Assert.Contains(
                CoreResources.LogNavigationLazyLoading(new TestLogger<InMemoryLoggingDefinitions>())
                    .GenerateMessage("WarningAsErrorEntity", "Nav"),
                loggerFactory.Log.Select(l => l.Message));

            var entityEntry = context.Entry(entity);
            Assert.True(entityEntry.Navigation("Nav").IsLoaded);
            loggerFactory.Clear();
            Assert.NotNull(entity.Nav);
            Assert.DoesNotContain(
                CoreResources.LogNavigationLazyLoading(new TestLogger<InMemoryLoggingDefinitions>())
                    .GenerateMessage("WarningAsErrorEntity", "Nav"),
                loggerFactory.Log.Select(l => l.Message));
        }
    }

    [ConditionalFact]
    public void No_throw_when_event_id_not_registered()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        using var context = new WarningAsErrorContext(serviceProvider, toThrow: CoreEventId.SensitiveDataLoggingEnabledWarning);
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        context.WarningAsErrorEntities.FirstOrDefault();
    }

    private class WarningAsErrorContext(
        IServiceProvider serviceProvider,
        bool defaultThrow = true,
        EventId? toLog = null,
        EventId? toThrow = null,
        (EventId Id, LogLevel Level)? toChangeLevel = null) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly bool _defaultThrow = defaultThrow;
        private readonly EventId? _toLog = toLog;
        private readonly EventId? _toThrow = toThrow;
        private readonly (EventId Id, LogLevel Level)? _toChangeLevel = toChangeLevel;

        public DbSet<WarningAsErrorEntity> WarningAsErrorEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder
                .Entity<WarningAsErrorEntity>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(WarningAsErrorContext)).ConfigureWarnings(
                    c =>
                    {
                        if (_toThrow != null)
                        {
                            c.Throw(_toThrow.Value);
                        }
                        else if (_toLog != null)
                        {
                            c.Log(_toLog.Value);
                        }
                        else if (_toChangeLevel != null)
                        {
                            c.Log(_toChangeLevel.Value);
                        }
                        else if (_defaultThrow)
                        {
                            c.Default(WarningBehavior.Throw);
                        }
                    });
    }

    private class WarningAsErrorEntity
    {
        private readonly Action<object, string> _loader;
        private IncludedEntity _nav;

        public WarningAsErrorEntity()
        {
        }

        private WarningAsErrorEntity(Action<object, string> lazyLoader)
        {
            _loader = lazyLoader;
        }

        public IncludedEntity Nav
        {
            get => _loader.Load(this, ref _nav);
            set => _nav = value;
        }

        public string Id { get; set; }
    }

    private class IncludedEntity
    {
        public int Id { get; set; }
    }
}
