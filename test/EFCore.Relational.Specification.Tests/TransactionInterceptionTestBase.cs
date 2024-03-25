// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class TransactionInterceptionTestBase : InterceptionTestBase
{
    protected TransactionInterceptionTestBase(InterceptionFixtureBase fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task BeginTransaction_without_interceptor(bool async)
    {
        using var context = await CreateContextAsync(Enumerable.Empty<IInterceptor>());
        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
        using (var transaction = async
                   ? await context.Database.BeginTransactionAsync()
                   : context.Database.BeginTransaction())
        {
            Assert.NotNull(transaction.GetDbTransaction());
        }

        AssertBeginTransactionEvents(listener);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task UseTransaction_without_interceptor(bool async)
    {
        using var context = await CreateContextAsync(Enumerable.Empty<IInterceptor>());
        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
        using var transaction = context.Database.GetDbConnection().BeginTransaction();
        var contextTransaction = async
            ? await context.Database.UseTransactionAsync(transaction)
            : context.Database.UseTransaction(transaction);

        {
            Assert.NotNull(contextTransaction.GetDbTransaction());
            Assert.Same(transaction, contextTransaction.GetDbTransaction());
        }

        AssertUseTransactionEvents(listener);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_BeginTransaction(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            using (var _ = async
                       ? await context.Database.BeginTransactionAsync()
                       : context.Database.BeginTransaction())
            {
                AssertBeginTransaction(context, interceptor, async);
            }

            AssertBeginTransactionEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_BeginTransaction_with_isolation_level(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            using (var _ = async
                       ? await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted)
                       : context.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                AssertBeginTransaction(context, interceptor, async);
            }

            AssertBeginTransactionEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_BeginTransaction_to_suppress(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<SuppressingTransactionInterceptor>();
        using (context)
        {
            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            using var _ = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            AssertBeginTransaction(context, interceptor, async);

            // Throws if a real transaction has been created
            using (context.Database.GetDbConnection().BeginTransaction())
            {
            }

            AssertBeginTransactionEvents(listener);
        }
    }

    protected class SuppressingTransactionInterceptor : TransactionInterceptor
    {
        public override InterceptionResult<DbTransaction> TransactionStarting(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result)
        {
            base.TransactionStarting(connection, eventData, result);

            return InterceptionResult<DbTransaction>.SuppressWithResult(new FakeDbTransaction(connection, eventData.IsolationLevel));
        }

        public override async ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result,
            CancellationToken cancellationToken = default)
        {
            await base.TransactionStartingAsync(connection, eventData, result, cancellationToken);

            return InterceptionResult<DbTransaction>.SuppressWithResult(new FakeDbTransaction(connection, eventData.IsolationLevel));
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_BeginTransaction_to_wrap(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<WrappingTransactionInterceptor>();
        using (context)
        {
            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            using var transaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            AssertBeginTransaction(context, interceptor, async);

            Assert.IsType<WrappedDbTransaction>(transaction.GetDbTransaction());

            AssertBeginTransactionEvents(listener);
        }
    }

    protected class WrappingTransactionInterceptor : TransactionInterceptor
    {
        public override DbTransaction TransactionStarted(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result)
        {
            result = base.TransactionStarted(connection, eventData, result);

            return new WrappedDbTransaction(result);
        }

        public override async ValueTask<DbTransaction> TransactionStartedAsync(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            result = await base.TransactionStartedAsync(connection, eventData, result, cancellationToken);

            return new WrappedDbTransaction(result);
        }

        public override DbTransaction TransactionUsed(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result)
        {
            result = base.TransactionUsed(connection, eventData, result);

            return new WrappedDbTransaction(result);
        }

        public override async ValueTask<DbTransaction> TransactionUsedAsync(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            result = await base.TransactionUsedAsync(connection, eventData, result, cancellationToken);

            return new WrappedDbTransaction(result);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_UseTransaction(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            using (var transaction = context.Database.GetDbConnection().BeginTransaction())
            {
                var contextTransaction = async
                    ? await context.Database.UseTransactionAsync(transaction)
                    : context.Database.UseTransaction(transaction);

                AssertUseTransaction(context, contextTransaction, interceptor, async);
            }

            AssertUseTransactionEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_UseTransaction_to_wrap(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<WrappingTransactionInterceptor>();
        using (context)
        {
            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            using var transaction = context.Database.GetDbConnection().BeginTransaction();
            var contextTransaction = async
                ? await context.Database.UseTransactionAsync(transaction)
                : context.Database.UseTransaction(transaction);

            Assert.IsType<WrappedDbTransaction>(contextTransaction.GetDbTransaction());

            AssertUseTransaction(context, contextTransaction, interceptor, async);

            AssertUseTransactionEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_Commit(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            interceptor.Reset();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            if (async)
            {
                await contextTransaction.CommitAsync();
            }
            else
            {
                contextTransaction.Commit();
            }

            AssertCommit(context, contextTransaction, interceptor, async);

            AssertCommitEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_Commit_to_suppress(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<CommitSuppressingTransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            interceptor.Reset();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            if (async)
            {
                await contextTransaction.CommitAsync();
            }
            else
            {
                contextTransaction.Commit();
            }

            // Will throw if Commit was already called
            contextTransaction.GetDbTransaction().Commit();

            AssertCommit(context, contextTransaction, interceptor, async);

            AssertCommitEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_Rollback(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            interceptor.Reset();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            if (async)
            {
                await contextTransaction.RollbackAsync();
            }
            else
            {
                contextTransaction.Rollback();
            }

            AssertRollBack(context, contextTransaction, interceptor, async);

            AssertRollBackEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_Rollback_to_suppress(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<CommitSuppressingTransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            interceptor.Reset();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            if (async)
            {
                await contextTransaction.RollbackAsync();
            }
            else
            {
                contextTransaction.Rollback();
            }

            // Will throw if Commit was already called
            contextTransaction.GetDbTransaction().Commit();

            AssertRollBack(context, contextTransaction, interceptor, async);

            AssertRollBackEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_CreateSavepoint(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            interceptor.Reset();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            if (async)
            {
                await contextTransaction.CreateSavepointAsync("dummy");
            }
            else
            {
                contextTransaction.CreateSavepoint("dummy");
            }

            AssertCreateSavepoint(context, contextTransaction, interceptor, async);

            AssertCreateSavepointEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_RollbackToSavepoint(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            if (async)
            {
                await contextTransaction.CreateSavepointAsync("dummy");
            }
            else
            {
                contextTransaction.CreateSavepoint("dummy");
            }

            interceptor.Reset();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            if (async)
            {
                await contextTransaction.RollbackToSavepointAsync("dummy");
            }
            else
            {
                contextTransaction.RollbackToSavepoint("dummy");
            }

            AssertRollbackToSavepoint(context, contextTransaction, interceptor, async);

            AssertRollbackToSavepointEvents(listener);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_ReleaseSavepoint(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            if (async)
            {
                await contextTransaction.CreateSavepointAsync("dummy");
            }
            else
            {
                contextTransaction.CreateSavepoint("dummy");
            }

            interceptor.Reset();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
            if (async)
            {
                await contextTransaction.ReleaseSavepointAsync("dummy");
            }
            else
            {
                contextTransaction.ReleaseSavepoint("dummy");
            }

            AssertReleaseSavepoint(context, contextTransaction, interceptor, async);

            AssertReleaseSavepointEvents(listener);
        }
    }

    protected class CommitSuppressingTransactionInterceptor : TransactionInterceptor
    {
        public override InterceptionResult TransactionCommitting(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            base.TransactionCommitting(transaction, eventData, result);

            return InterceptionResult.Suppress();
        }

        public override async ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            await base.TransactionCommittingAsync(transaction, eventData, result, cancellationToken);

            return InterceptionResult.Suppress();
        }

        public override InterceptionResult TransactionRollingBack(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            base.TransactionRollingBack(transaction, eventData, result);

            return InterceptionResult.Suppress();
        }

        public override async ValueTask<InterceptionResult> TransactionRollingBackAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            await base.TransactionRollingBackAsync(transaction, eventData, result, cancellationToken);

            return InterceptionResult.Suppress();
        }
    }

    [ConditionalTheory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public virtual async Task Intercept_error_on_commit_or_rollback(bool async, bool commit)
    {
        var (context, interceptor) = await CreateContextAsync<TransactionInterceptor>();
        using (context)
        {
            using var contextTransaction = async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction();
            interceptor.Reset();

            contextTransaction.GetDbTransaction().Commit();

            try
            {
                if (async)
                {
                    if (commit)
                    {
                        await contextTransaction.CommitAsync();
                    }
                    else
                    {
                        await contextTransaction.RollbackAsync();
                    }
                }
                else
                {
                    if (commit)
                    {
                        contextTransaction.Commit();
                    }
                    else
                    {
                        contextTransaction.Rollback();
                    }
                }

                Assert.False(true);
            }
            catch (Exception exception)
            {
                Assert.Same(exception, interceptor.Exception);
            }

            AssertError(context, interceptor, async);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_with_multiple_interceptors(bool async)
    {
        var interceptor1 = new TransactionInterceptor();
        var interceptor2 = new WrappingTransactionInterceptor();
        var interceptor3 = new TransactionInterceptor();
        var interceptor4 = new WrappingTransactionInterceptor();
        using var context = await CreateContextAsync(
            new IInterceptor[] { new NoOpTransactionInterceptor(), interceptor1, interceptor2 },
            new IInterceptor[] { interceptor3, interceptor4, new NoOpTransactionInterceptor() });
        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);
        using var contextTransaction = async
            ? await context.Database.BeginTransactionAsync()
            : context.Database.BeginTransaction();
        Assert.IsType<WrappedDbTransaction>(contextTransaction.GetDbTransaction());

        AssertBeginTransaction(context, interceptor1, async);
        AssertBeginTransaction(context, interceptor2, async);
        AssertBeginTransaction(context, interceptor3, async);
        AssertBeginTransaction(context, interceptor4, async);

        AssertBeginTransactionEvents(listener);
    }

    protected class NoOpTransactionInterceptor : DbConnectionInterceptor;

    private class WrappedDbTransaction(DbTransaction transaction) : DbTransaction
    {
        private readonly DbTransaction _transaction = transaction;

        public override void Commit()
            => _transaction.Commit();

        public override void Rollback()
            => _transaction.Rollback();

        protected override DbConnection DbConnection
            => _transaction.Connection;

        public override IsolationLevel IsolationLevel
            => _transaction.IsolationLevel;

        protected override void Dispose(bool disposing)
            => _transaction.Dispose();
    }

    private class FakeDbTransaction(DbConnection dbConnection, IsolationLevel isolationLevel) : DbTransaction
    {
        public override void Commit()
        {
        }

        public override void Rollback()
        {
        }

        protected override DbConnection DbConnection { get; } = dbConnection;

        public override IsolationLevel IsolationLevel { get; } = isolationLevel == IsolationLevel.Unspecified
            ? IsolationLevel.Snapshot
            : isolationLevel;
    }

    private static void AssertBeginTransaction(DbContext context, TransactionInterceptor interceptor, bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.True(interceptor.StartingCalled);
        Assert.True(interceptor.StartedCalled);
        Assert.False(interceptor.UsedCalled);
        Assert.False(interceptor.CommittingCalled);
        Assert.False(interceptor.CommittedCalled);
        Assert.False(interceptor.RollingBackCalled);
        Assert.False(interceptor.RolledBackCalled);
        Assert.False(interceptor.CreatingSavepointCalled);
        Assert.False(interceptor.CreatedSavepointCalled);
        Assert.False(interceptor.RollingBackToSavepointCalled);
        Assert.False(interceptor.RolledBackToSavepointCalled);
        Assert.False(interceptor.ReleasingSavepointCalled);
        Assert.False(interceptor.ReleasedSavepointCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
    }

    private static void AssertUseTransaction(
        DbContext context,
        IDbContextTransaction contextTransaction,
        TransactionInterceptor interceptor,
        bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.True(interceptor.UsedCalled);
        Assert.False(interceptor.StartingCalled);
        Assert.False(interceptor.CommittingCalled);
        Assert.False(interceptor.CommittedCalled);
        Assert.False(interceptor.RollingBackCalled);
        Assert.False(interceptor.RolledBackCalled);
        Assert.False(interceptor.CreatingSavepointCalled);
        Assert.False(interceptor.CreatedSavepointCalled);
        Assert.False(interceptor.RollingBackToSavepointCalled);
        Assert.False(interceptor.RolledBackToSavepointCalled);
        Assert.False(interceptor.ReleasingSavepointCalled);
        Assert.False(interceptor.ReleasedSavepointCalled);
        Assert.False(interceptor.StartedCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Equal(contextTransaction.TransactionId, interceptor.TransactionId);
    }

    private static void AssertCommit(
        DbContext context,
        IDbContextTransaction contextTransaction,
        TransactionInterceptor interceptor,
        bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.True(interceptor.CommittingCalled);
        Assert.True(interceptor.CommittedCalled);
        Assert.False(interceptor.RollingBackCalled);
        Assert.False(interceptor.RolledBackCalled);
        Assert.False(interceptor.CreatingSavepointCalled);
        Assert.False(interceptor.CreatedSavepointCalled);
        Assert.False(interceptor.RollingBackToSavepointCalled);
        Assert.False(interceptor.RolledBackToSavepointCalled);
        Assert.False(interceptor.ReleasingSavepointCalled);
        Assert.False(interceptor.ReleasedSavepointCalled);
        Assert.False(interceptor.UsedCalled);
        Assert.False(interceptor.StartingCalled);
        Assert.False(interceptor.StartedCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Equal(contextTransaction.TransactionId, interceptor.TransactionId);
    }

    private static void AssertRollBack(
        DbContext context,
        IDbContextTransaction contextTransaction,
        TransactionInterceptor interceptor,
        bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.False(interceptor.CommittingCalled);
        Assert.False(interceptor.CommittedCalled);
        Assert.True(interceptor.RollingBackCalled);
        Assert.True(interceptor.RolledBackCalled);
        Assert.False(interceptor.CreatingSavepointCalled);
        Assert.False(interceptor.CreatedSavepointCalled);
        Assert.False(interceptor.RollingBackToSavepointCalled);
        Assert.False(interceptor.RolledBackToSavepointCalled);
        Assert.False(interceptor.ReleasingSavepointCalled);
        Assert.False(interceptor.ReleasedSavepointCalled);
        Assert.False(interceptor.UsedCalled);
        Assert.False(interceptor.StartingCalled);
        Assert.False(interceptor.StartedCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Equal(contextTransaction.TransactionId, interceptor.TransactionId);
    }

    private static void AssertCreateSavepoint(
        DbContext context,
        IDbContextTransaction contextTransaction,
        TransactionInterceptor interceptor,
        bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.False(interceptor.CommittingCalled);
        Assert.False(interceptor.CommittedCalled);
        Assert.False(interceptor.RollingBackCalled);
        Assert.False(interceptor.RolledBackCalled);
        Assert.True(interceptor.CreatingSavepointCalled);
        Assert.True(interceptor.CreatedSavepointCalled);
        Assert.False(interceptor.RollingBackToSavepointCalled);
        Assert.False(interceptor.RolledBackToSavepointCalled);
        Assert.False(interceptor.ReleasingSavepointCalled);
        Assert.False(interceptor.ReleasedSavepointCalled);
        Assert.False(interceptor.UsedCalled);
        Assert.False(interceptor.StartingCalled);
        Assert.False(interceptor.StartedCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Equal(contextTransaction.TransactionId, interceptor.TransactionId);
    }

    private static void AssertRollbackToSavepoint(
        DbContext context,
        IDbContextTransaction contextTransaction,
        TransactionInterceptor interceptor,
        bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.False(interceptor.CommittingCalled);
        Assert.False(interceptor.CommittedCalled);
        Assert.False(interceptor.RollingBackCalled);
        Assert.False(interceptor.RolledBackCalled);
        Assert.False(interceptor.CreatingSavepointCalled);
        Assert.False(interceptor.CreatedSavepointCalled);
        Assert.True(interceptor.RollingBackToSavepointCalled);
        Assert.True(interceptor.RolledBackToSavepointCalled);
        Assert.False(interceptor.ReleasingSavepointCalled);
        Assert.False(interceptor.ReleasedSavepointCalled);
        Assert.False(interceptor.UsedCalled);
        Assert.False(interceptor.StartingCalled);
        Assert.False(interceptor.StartedCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Equal(contextTransaction.TransactionId, interceptor.TransactionId);
    }

    private static void AssertReleaseSavepoint(
        DbContext context,
        IDbContextTransaction contextTransaction,
        TransactionInterceptor interceptor,
        bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.False(interceptor.CommittingCalled);
        Assert.False(interceptor.CommittedCalled);
        Assert.False(interceptor.RollingBackCalled);
        Assert.False(interceptor.RolledBackCalled);
        Assert.False(interceptor.CreatingSavepointCalled);
        Assert.False(interceptor.CreatedSavepointCalled);
        Assert.False(interceptor.RollingBackToSavepointCalled);
        Assert.False(interceptor.RolledBackToSavepointCalled);
        Assert.True(interceptor.ReleasingSavepointCalled);
        Assert.True(interceptor.ReleasedSavepointCalled);
        Assert.False(interceptor.UsedCalled);
        Assert.False(interceptor.StartingCalled);
        Assert.False(interceptor.StartedCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Equal(contextTransaction.TransactionId, interceptor.TransactionId);
    }

    private static void AssertError(
        DbContext context,
        TransactionInterceptor interceptor,
        bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.True(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
    }

    private static void AssertBeginTransactionEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(
            RelationalEventId.TransactionStarting.Name,
            RelationalEventId.TransactionStarted.Name);

    private static void AssertUseTransactionEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(RelationalEventId.TransactionUsed.Name);

    private static void AssertCommitEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(
            RelationalEventId.TransactionCommitting.Name,
            RelationalEventId.TransactionCommitted.Name);

    private static void AssertRollBackEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(
            RelationalEventId.TransactionRollingBack.Name,
            RelationalEventId.TransactionRolledBack.Name);

    private static void AssertCreateSavepointEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(
            RelationalEventId.CreatingTransactionSavepoint.Name,
            RelationalEventId.CreatedTransactionSavepoint.Name);

    private static void AssertRollbackToSavepointEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(
            RelationalEventId.RollingBackToTransactionSavepoint.Name,
            RelationalEventId.RolledBackToTransactionSavepoint.Name);

    private static void AssertReleaseSavepointEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(
            RelationalEventId.ReleasingTransactionSavepoint.Name,
            RelationalEventId.ReleasedTransactionSavepoint.Name);

    protected class TransactionInterceptor : IDbTransactionInterceptor
    {
        public DbContext Context { get; set; }
        public Exception Exception { get; set; }
        public Guid TransactionId { get; set; }
        public Guid ConnectionId { get; set; }
        public IsolationLevel IsolationLevel { get; set; }
        public bool AsyncCalled { get; set; }
        public bool SyncCalled { get; set; }
        public bool StartingCalled { get; set; }
        public bool StartedCalled { get; set; }
        public bool UsedCalled { get; set; }
        public bool CommittingCalled { get; set; }
        public bool CommittedCalled { get; set; }
        public bool RollingBackCalled { get; set; }
        public bool RolledBackCalled { get; set; }
        public bool CreatingSavepointCalled { get; set; }
        public bool CreatedSavepointCalled { get; set; }
        public bool RollingBackToSavepointCalled { get; set; }
        public bool RolledBackToSavepointCalled { get; set; }
        public bool ReleasingSavepointCalled { get; set; }
        public bool ReleasedSavepointCalled { get; set; }
        public bool FailedCalled { get; set; }

        public void Reset()
        {
            Context = null;
            Exception = null;
            ConnectionId = default;
            AsyncCalled = false;
            SyncCalled = false;
            StartingCalled = false;
            StartedCalled = false;
            UsedCalled = false;
            CommittingCalled = false;
            CommittedCalled = false;
            RollingBackCalled = false;
            RolledBackCalled = false;
            CreatingSavepointCalled = false;
            CreatedSavepointCalled = false;
            RollingBackToSavepointCalled = false;
            RolledBackToSavepointCalled = false;
            ReleasingSavepointCalled = false;
            ReleasedSavepointCalled = false;
            FailedCalled = false;
        }

        protected virtual void AssertStarting(DbConnection connection, TransactionStartingEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.TransactionId);

            Context = eventData.Context;
            TransactionId = eventData.TransactionId;
            ConnectionId = eventData.ConnectionId;
            IsolationLevel = eventData.IsolationLevel;

            StartingCalled = true;
        }

        protected virtual void AssertStarted(DbConnection connection, TransactionEndEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(TransactionId, eventData.TransactionId);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            if (IsolationLevel == IsolationLevel.Unspecified)
            {
                Assert.NotEqual(IsolationLevel.Unspecified, eventData.Transaction.IsolationLevel);
            }
            else
            {
                Assert.Equal(IsolationLevel, eventData.Transaction.IsolationLevel);
            }

            StartedCalled = true;
        }

        protected virtual void AssertCommitting(TransactionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.TransactionId);

            Context = eventData.Context;
            TransactionId = eventData.TransactionId;
            ConnectionId = eventData.ConnectionId;

            CommittingCalled = true;
        }

        protected virtual void AssertCommitted(TransactionEndEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(TransactionId, eventData.TransactionId);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            CommittedCalled = true;
        }

        protected virtual void AssertRollingBack(TransactionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.TransactionId);

            Context = eventData.Context;
            TransactionId = eventData.TransactionId;
            ConnectionId = eventData.ConnectionId;

            RollingBackCalled = true;
        }

        protected virtual void AssertRolledBack(TransactionEndEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(TransactionId, eventData.TransactionId);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            RolledBackCalled = true;
        }

        protected virtual void AssertCreatingSavepoint(TransactionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.TransactionId);

            Context = eventData.Context;
            TransactionId = eventData.TransactionId;
            ConnectionId = eventData.ConnectionId;

            CreatingSavepointCalled = true;
        }

        protected virtual void AssertCreatedSavepoint(TransactionEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(TransactionId, eventData.TransactionId);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            CreatedSavepointCalled = true;
        }

        protected virtual void AssertRollingBackToSavepoint(TransactionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.TransactionId);

            Context = eventData.Context;
            TransactionId = eventData.TransactionId;
            ConnectionId = eventData.ConnectionId;

            RollingBackToSavepointCalled = true;
        }

        protected virtual void AssertRolledBackToSavepoint(TransactionEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(TransactionId, eventData.TransactionId);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            RolledBackToSavepointCalled = true;
        }

        protected virtual void AssertReleasingSavepoint(TransactionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.TransactionId);

            Context = eventData.Context;
            TransactionId = eventData.TransactionId;
            ConnectionId = eventData.ConnectionId;

            ReleasingSavepointCalled = true;
        }

        protected virtual void AssertReleasedSavepoint(TransactionEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(TransactionId, eventData.TransactionId);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            ReleasedSavepointCalled = true;
        }

        protected virtual void AssertFailed(TransactionErrorEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(TransactionId, eventData.TransactionId);
            Assert.Equal(ConnectionId, eventData.ConnectionId);
            Assert.NotNull(eventData.Exception);

            Exception = eventData.Exception;
            FailedCalled = true;
        }

        protected virtual void AssertUsed(DbConnection connection, TransactionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.TransactionId);

            Context = eventData.Context;
            TransactionId = eventData.TransactionId;
            ConnectionId = eventData.ConnectionId;
            UsedCalled = true;
        }

        public virtual InterceptionResult<DbTransaction> TransactionStarting(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertStarting(connection, eventData);

            return result;
        }

        public virtual DbTransaction TransactionStarted(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertStarted(connection, eventData);

            return result;
        }

        public virtual ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertStarting(connection, eventData);

            return ValueTask.FromResult(result);
        }

        public virtual ValueTask<DbTransaction> TransactionStartedAsync(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertStarted(connection, eventData);

            return new ValueTask<DbTransaction>(Task.FromResult(result));
        }

        public virtual DbTransaction TransactionUsed(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertUsed(connection, eventData);

            return result;
        }

        public virtual ValueTask<DbTransaction> TransactionUsedAsync(
            DbConnection connection,
            TransactionEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertUsed(connection, eventData);

            return ValueTask.FromResult(result);
        }

        public virtual InterceptionResult TransactionCommitting(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertCommitting(eventData);

            return result;
        }

        public virtual void TransactionCommitted(
            DbTransaction transaction,
            TransactionEndEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertCommitted(eventData);
        }

        public virtual ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertCommitting(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual Task TransactionCommittedAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertCommitted(eventData);

            return Task.CompletedTask;
        }

        public virtual InterceptionResult TransactionRollingBack(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertRollingBack(eventData);

            return result;
        }

        public virtual void TransactionRolledBack(
            DbTransaction transaction,
            TransactionEndEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertRolledBack(eventData);
        }

        public virtual ValueTask<InterceptionResult> TransactionRollingBackAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertRollingBack(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual Task TransactionRolledBackAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertRolledBack(eventData);

            return Task.CompletedTask;
        }

        public virtual InterceptionResult CreatingSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertCreatingSavepoint(eventData);

            return result;
        }

        public virtual void CreatedSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertCreatedSavepoint(eventData);
        }

        public virtual ValueTask<InterceptionResult> CreatingSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertCreatingSavepoint(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual Task CreatedSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertCreatedSavepoint(eventData);

            return Task.CompletedTask;
        }

        public virtual InterceptionResult RollingBackToSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertRollingBackToSavepoint(eventData);

            return result;
        }

        public virtual void RolledBackToSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertRolledBackToSavepoint(eventData);
        }

        public virtual ValueTask<InterceptionResult> RollingBackToSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertRollingBackToSavepoint(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual Task RolledBackToSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertRolledBackToSavepoint(eventData);

            return Task.CompletedTask;
        }

        public virtual InterceptionResult ReleasingSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertReleasingSavepoint(eventData);

            return result;
        }

        public virtual void ReleasedSavepoint(
            DbTransaction transaction,
            TransactionEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertReleasedSavepoint(eventData);
        }

        public virtual ValueTask<InterceptionResult> ReleasingSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertReleasingSavepoint(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual Task ReleasedSavepointAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertReleasedSavepoint(eventData);

            return Task.CompletedTask;
        }

        public virtual void TransactionFailed(
            DbTransaction transaction,
            TransactionErrorEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertFailed(eventData);
        }

        public virtual Task TransactionFailedAsync(
            DbTransaction transaction,
            TransactionErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertFailed(eventData);

            return Task.CompletedTask;
        }
    }
}
