// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class SaveChangesInterceptionTestBase : InterceptionTestBase
{
    protected SaveChangesInterceptionTestBase(InterceptionFixtureBase fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Intercept_SaveChanges_passively(bool async, bool inject, bool noAcceptChanges)
    {
        var (context, interceptor) = await CreateContextAsync<PassiveSaveChangesInterceptor>(inject);

        using var _ = context;

        var savingEventCalled = false;
        var resultFromEvent = 0;
        Exception exceptionFromEvent = null;

        context.SavingChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            savingEventCalled = true;
        };

        context.SavedChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            resultFromEvent = args.EntitiesSavedCount;
        };

        context.SaveChangesFailed += (sender, args) =>
        {
            Assert.Same(context, sender);
            exceptionFromEvent = args.Exception;
        };

        await context.AddAsync(new Singularity { Id = 35, Type = "Red Dwarf" });

        using var transaction = context.Database.BeginTransaction();

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        var savedCount = noAcceptChanges
            ? async
                ? await context.SaveChangesAsync()
                : context.SaveChanges()
            : async
                ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                : context.SaveChanges(acceptAllChangesOnSuccess: false);

        Assert.Equal(1, savedCount);

        Assert.True(savingEventCalled);
        Assert.Equal(savedCount, resultFromEvent);
        Assert.Null(exceptionFromEvent);

        AssertNormalOutcome(context, interceptor, async);

        listener.AssertEventsInOrder(
            CoreEventId.SaveChangesStarting.Name,
            CoreEventId.SaveChangesCompleted.Name);

        Assert.Equal(1, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == 35));
    }

    protected class PassiveSaveChangesInterceptor : SaveChangesInterceptorBase;

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Intercept_SaveChanges_to_suppress_save(bool async, bool inject, bool noAcceptChanges)
    {
        var (context, interceptor) = await CreateContextAsync<SuppressingSaveChangesInterceptor>(inject);

        using var _ = context;

        var savingEventCalled = false;
        var resultFromEvent = 0;
        Exception exceptionFromEvent = null;

        context.SavingChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            savingEventCalled = true;
        };

        context.SavedChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            resultFromEvent = args.EntitiesSavedCount;
        };

        context.SaveChangesFailed += (sender, args) =>
        {
            Assert.Same(context, sender);
            exceptionFromEvent = args.Exception;
        };

        await context.AddAsync(new Singularity { Id = 35, Type = "Red Dwarf" });

        using var transaction = context.Database.BeginTransaction();

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        var savedCount = noAcceptChanges
            ? async
                ? await context.SaveChangesAsync()
                : context.SaveChanges()
            : async
                ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                : context.SaveChanges(acceptAllChangesOnSuccess: false);

        Assert.Equal(-1, savedCount);

        Assert.True(savingEventCalled);
        Assert.Equal(savedCount, resultFromEvent);
        Assert.Null(exceptionFromEvent);

        AssertNormalOutcome(context, interceptor, async);

        listener.AssertEventsInOrder(
            CoreEventId.SaveChangesStarting.Name,
            CoreEventId.SaveChangesCompleted.Name);

        Assert.Equal(0, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == 35));
    }

    protected class SuppressingSaveChangesInterceptor : SaveChangesInterceptorBase
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            base.SavingChanges(eventData, result);

            return InterceptionResult<int>.SuppressWithResult(-1);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await base.SavingChangesAsync(eventData, result, cancellationToken);

            return InterceptionResult<int>.SuppressWithResult(-1);
        }
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Intercept_SaveChanges_to_change_result(bool async, bool inject, bool noAcceptChanges)
    {
        var (context, interceptor) = await CreateContextAsync<ResultMutatingSaveChangesInterceptor>(inject);

        using var _ = context;

        var savingEventCalled = false;
        var resultFromEvent = 0;
        Exception exceptionFromEvent = null;

        context.SavingChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            savingEventCalled = true;
        };

        context.SavedChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            resultFromEvent = args.EntitiesSavedCount;
        };

        context.SaveChangesFailed += (sender, args) =>
        {
            Assert.Same(context, sender);
            exceptionFromEvent = args.Exception;
        };

        await context.AddAsync(new Singularity { Id = 35, Type = "Red Dwarf" });

        using var transaction = context.Database.BeginTransaction();

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        var savedCount = noAcceptChanges
            ? async
                ? await context.SaveChangesAsync()
                : context.SaveChanges()
            : async
                ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                : context.SaveChanges(acceptAllChangesOnSuccess: false);

        Assert.Equal(777, savedCount);

        Assert.True(savingEventCalled);
        Assert.Equal(savedCount, resultFromEvent);
        Assert.Null(exceptionFromEvent);

        AssertNormalOutcome(context, interceptor, async);

        listener.AssertEventsInOrder(
            CoreEventId.SaveChangesStarting.Name,
            CoreEventId.SaveChangesCompleted.Name);

        Assert.Equal(1, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == 35));
    }

    protected class ResultMutatingSaveChangesInterceptor : SaveChangesInterceptorBase
    {
        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            base.SavedChanges(eventData, result);

            return 777;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            await base.SavedChangesAsync(eventData, result, cancellationToken);

            return 777;
        }
    }

    [ConditionalTheory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(true, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, true, true, true)]
    public virtual async Task Intercept_SaveChanges_failed(bool async, bool inject, bool noAcceptChanges, bool concurrencyError)
    {
        if (concurrencyError
            && !SupportsOptimisticConcurrency)
        {
            return;
        }

        var (context, interceptor) = await CreateContextAsync<PassiveSaveChangesInterceptor>(inject);

        using var _ = context;

        using var transaction = context.Database.BeginTransaction();

        if (!concurrencyError)
        {
            await context.AddAsync(new Singularity { Id = 35, Type = "Red Dwarf" });
            var ___ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            context.ChangeTracker.Clear();
        }

        var savingEventCalled = false;
        var resultFromEvent = -1;
        Exception exceptionFromEvent = null;

        context.SavingChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            savingEventCalled = true;
        };

        context.SavedChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            resultFromEvent = args.EntitiesSavedCount;
        };

        context.SaveChangesFailed += (sender, args) =>
        {
            Assert.Same(context, sender);
            exceptionFromEvent = args.Exception;
        };

        var entry = context.Entry(new Singularity { Id = 35, Type = "Red Dwarf" });
        entry.State = concurrencyError ? EntityState.Modified : EntityState.Added;

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        Exception thrown = null;

        try
        {
            var __ = noAcceptChanges
                ? async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges()
                : async
                    ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                    : context.SaveChanges(acceptAllChangesOnSuccess: false);
        }
        catch (Exception e)
        {
            thrown = e;
        }

        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.Equal(concurrencyError, !interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(thrown, interceptor.Exception);

        Assert.True(savingEventCalled);
        Assert.Equal(-1, resultFromEvent);
        Assert.Same(thrown, exceptionFromEvent);

        if (concurrencyError)
        {
            Assert.True(interceptor.ConcurrencyExceptionCalled);
            Assert.Equal(1, interceptor.Entries.Count);
            Assert.Same(entry.Entity, interceptor.Entries[0].Entity);

            listener.AssertEventsInOrder(
                CoreEventId.SaveChangesStarting.Name,
                CoreEventId.OptimisticConcurrencyException.Name);
        }
        else
        {
            listener.AssertEventsInOrder(
                CoreEventId.SaveChangesStarting.Name,
                CoreEventId.SaveChangesFailed.Name);
        }
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Intercept_to_suppress_concurrency_exception(bool async, bool inject, bool noAcceptChanges)
    {
        if (!SupportsOptimisticConcurrency)
        {
            return;
        }

        var (context, interceptor) = await CreateContextAsync<ConcurrencySuppressingSaveChangesInterceptor>(inject);

        using var _ = context;

        using var transaction = context.Database.BeginTransaction();

        var savingEventCalled = false;
        var resultFromEvent = -1;
        Exception exceptionFromEvent = null;

        context.SavingChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            savingEventCalled = true;
        };

        context.SavedChanges += (sender, args) =>
        {
            Assert.Same(context, sender);
            resultFromEvent = args.EntitiesSavedCount;
        };

        context.SaveChangesFailed += (sender, args) =>
        {
            Assert.Same(context, sender);
            exceptionFromEvent = args.Exception;
        };

        var entry = context.Entry(new Singularity { Id = 35, Type = "Red Dwarf" });
        entry.State = EntityState.Modified;

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        Exception thrown = null;

        try
        {
            var __ = noAcceptChanges
                ? async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges()
                : async
                    ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                    : context.SaveChanges(acceptAllChangesOnSuccess: false);
        }
        catch (Exception e)
        {
            thrown = e;
        }

        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Null(thrown);

        Assert.True(savingEventCalled);
        Assert.Equal(1, resultFromEvent);
        Assert.Null(exceptionFromEvent);

        Assert.True(interceptor.ConcurrencyExceptionCalled);
        Assert.Equal(1, interceptor.Entries.Count);
        Assert.Same(entry.Entity, interceptor.Entries[0].Entity);

        listener.AssertEventsInOrder(
            CoreEventId.SaveChangesStarting.Name,
            CoreEventId.OptimisticConcurrencyException.Name,
            CoreEventId.SaveChangesCompleted.Name);
    }

    protected class ConcurrencySuppressingSaveChangesInterceptor : SaveChangesInterceptorBase
    {
        public override InterceptionResult ThrowingConcurrencyException(ConcurrencyExceptionEventData eventData, InterceptionResult result)
        {
            base.ThrowingConcurrencyException(eventData, result);

            return InterceptionResult.Suppress();
        }

        public override async ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
            ConcurrencyExceptionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            await base.ThrowingConcurrencyExceptionAsync(eventData, result, cancellationToken);

            return InterceptionResult.Suppress();
        }
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Intercept_SaveChanges_with_multiple_interceptors(bool async, bool inject, bool noAcceptChanges)
    {
        var interceptor1 = new PassiveSaveChangesInterceptor();
        var interceptor2 = new ResultMutatingSaveChangesInterceptor();
        var interceptor3 = new ResultMutatingSaveChangesInterceptor();
        var interceptor4 = new PassiveSaveChangesInterceptor();

        using var context = await CreateContextAsync(
            new IInterceptor[] { new PassiveSaveChangesInterceptor(), interceptor1, interceptor2 },
            new IInterceptor[] { interceptor3, interceptor4, new PassiveSaveChangesInterceptor() });

        await context.AddAsync(new Singularity { Id = 35, Type = "Red Dwarf" });

        using var transaction = context.Database.BeginTransaction();

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        var savedCount = noAcceptChanges
            ? async
                ? await context.SaveChangesAsync()
                : context.SaveChanges()
            : async
                ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                : context.SaveChanges(acceptAllChangesOnSuccess: false);

        Assert.Equal(777, savedCount);

        AssertNormalOutcome(context, interceptor1, async);
        AssertNormalOutcome(context, interceptor2, async);
        AssertNormalOutcome(context, interceptor3, async);
        AssertNormalOutcome(context, interceptor4, async);

        listener.AssertEventsInOrder(
            CoreEventId.SaveChangesStarting.Name,
            CoreEventId.SaveChangesCompleted.Name);

        Assert.Equal(1, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == 35));
    }

    protected abstract class SaveChangesInterceptorBase : ISaveChangesInterceptor
    {
        public DbContext Context { get; set; }
        public Exception Exception { get; set; }
        public IReadOnlyList<EntityEntry> Entries { get; set; }
        public bool AsyncCalled { get; set; }
        public bool SyncCalled { get; set; }
        public bool FailedCalled { get; set; }
        public bool ConcurrencyExceptionCalled { get; set; }
        public bool CanceledCalled { get; set; }
        public bool SavedChangesCalled { get; set; }
        public bool SavingChangesCalled { get; set; }

        public virtual InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Assert.NotNull(eventData.Context);

            Context = eventData.Context;
            SavingChangesCalled = true;
            SyncCalled = true;

            return result;
        }

        public virtual int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            Assert.NotNull(eventData.Context);

            Context = eventData.Context;
            SavedChangesCalled = true;
            SyncCalled = true;

            return result;
        }

        public virtual void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotNull(eventData.Exception);

            Context = eventData.Context;
            Exception = eventData.Exception;
            FailedCalled = true;
            SyncCalled = true;
        }

        public virtual void SaveChangesCanceled(DbContextEventData eventData)
        {
            Assert.NotNull(eventData.Context);

            Context = eventData.Context;
            CanceledCalled = true;
            SyncCalled = true;
        }

        public virtual ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Assert.NotNull(eventData.Context);

            Context = eventData.Context;
            SavingChangesCalled = true;
            AsyncCalled = true;

            return ValueTask.FromResult(result);
        }

        public virtual ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            Assert.NotNull(eventData.Context);

            Context = eventData.Context;
            SavedChangesCalled = true;
            AsyncCalled = true;

            return ValueTask.FromResult(result);
        }

        public virtual Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotNull(eventData.Exception);

            Context = eventData.Context;
            Exception = eventData.Exception;
            FailedCalled = true;
            AsyncCalled = true;

            return Task.CompletedTask;
        }

        public virtual Task SaveChangesCanceledAsync(
            DbContextEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.NotNull(eventData.Context);

            Context = eventData.Context;
            CanceledCalled = true;
            AsyncCalled = true;

            return Task.CompletedTask;
        }

        public virtual InterceptionResult ThrowingConcurrencyException(ConcurrencyExceptionEventData eventData, InterceptionResult result)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotNull(eventData.Exception);

            Context = eventData.Context;
            Exception = eventData.Exception;
            Entries = eventData.Entries;
            ConcurrencyExceptionCalled = true;
            SyncCalled = true;

            return result;
        }

        public virtual ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
            ConcurrencyExceptionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotNull(eventData.Exception);

            Context = eventData.Context;
            Exception = eventData.Exception;
            Entries = eventData.Entries;
            ConcurrencyExceptionCalled = true;
            AsyncCalled = true;

            return ValueTask.FromResult(result);
        }
    }

    private static void AssertNormalOutcome(DbContext context, SaveChangesInterceptorBase interceptor, bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
    }

    protected virtual bool SupportsOptimisticConcurrency
        => true;
}
