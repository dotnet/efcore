// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class DatabaseFacadeTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Methods_delegate_to_configured_store_creator(bool async)
    {
        var creator = new FakeDatabaseCreator();

        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDatabaseCreator>(creator));

        if (async)
        {
            Assert.True(await context.Database.EnsureCreatedAsync());
            Assert.Equal(1, creator.EnsureCreatedAsyncCount);

            Assert.True(await context.Database.EnsureDeletedAsync());
            Assert.Equal(1, creator.EnsureDeletedAsyncCount);

            Assert.True(await context.Database.CanConnectAsync());
            Assert.Equal(1, creator.CanConnectAsyncCount);
        }
        else
        {
            Assert.True(context.Database.EnsureCreated());
            Assert.Equal(1, creator.EnsureCreatedCount);

            Assert.True(context.Database.EnsureDeleted());
            Assert.Equal(1, creator.EnsureDeletedCount);

            Assert.True(context.Database.CanConnect());
            Assert.Equal(1, creator.CanConnectCount);
        }
    }

    private class FakeDatabaseCreator : IDatabaseCreator
    {
        public int CanConnectCount;
        public int CanConnectAsyncCount;
        public int EnsureDeletedCount;
        public int EnsureDeletedAsyncCount;
        public int EnsureCreatedCount;
        public int EnsureCreatedAsyncCount;

        public bool EnsureDeleted()
        {
            EnsureDeletedCount++;
            return true;
        }

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
        {
            EnsureDeletedAsyncCount++;
            return Task.FromResult(true);
        }

        public bool EnsureCreated()
        {
            EnsureCreatedCount++;
            return true;
        }

        public Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            EnsureCreatedAsyncCount++;
            return Task.FromResult(true);
        }

        public bool CanConnect()
        {
            CanConnectCount++;
            return true;
        }

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            CanConnectAsyncCount++;
            return Task.FromResult(true);
        }
    }

    [ConditionalFact]
    public void Can_get_IServiceProvider()
    {
        using var context = InMemoryTestHelpers.Instance.CreateContext();
        Assert.Same(
            ((IInfrastructure<IServiceProvider>)context).Instance,
            ((IInfrastructure<IServiceProvider>)context.Database).Instance);
    }

    [ConditionalFact]
    public void Can_get_DatabaseCreator()
    {
        using var context = InMemoryTestHelpers.Instance.CreateContext();
        Assert.Same(
            context.GetService<IDatabaseCreator>(),
            context.Database.GetService<IDatabaseCreator>());
    }

    [ConditionalFact]
    public void Can_get_Model()
    {
        using var context = InMemoryTestHelpers.Instance.CreateContext();
        Assert.Same(context.GetService<IModel>(), context.Database.GetService<IModel>());
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_begin_transaction(bool async)
    {
        var transaction = new FakeDbContextTransaction();

        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDbContextTransactionManager>(
                new FakeDbContextTransactionManager(transaction)));

        Assert.Same(
            transaction,
            async
                ? await context.Database.BeginTransactionAsync()
                : context.Database.BeginTransaction());
    }

    private class FakeDbContextTransactionManager(FakeDbContextTransaction transaction) : IDbContextTransactionManager
    {
        private readonly FakeDbContextTransaction _transaction = transaction;
        public int CommitCalls;
        public int RollbackCalls;
        public int CreateSavepointCalls;
        public int RollbackSavepointCalls;
        public int ReleaseSavepointCalls;
        public int SupportsSavepointsCalls;

        public IDbContextTransaction BeginTransaction()
            => _transaction;

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IDbContextTransaction>(_transaction);

        public void CommitTransaction()
            => CommitCalls++;

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            CommitCalls++;
            return Task.CompletedTask;
        }

        public void RollbackTransaction()
            => RollbackCalls++;

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            RollbackCalls++;
            return Task.CompletedTask;
        }

        public void CreateSavepoint(string name)
            => CreateSavepointCalls++;

        public Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            CreateSavepointCalls++;
            return Task.CompletedTask;
        }

        public void RollbackToSavepoint(string name)
            => RollbackSavepointCalls++;

        public Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            RollbackSavepointCalls++;
            return Task.CompletedTask;
        }

        public void ReleaseSavepoint(string name)
            => ReleaseSavepointCalls++;

        public Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
        {
            ReleaseSavepointCalls++;
            return Task.CompletedTask;
        }

        public bool SupportsSavepoints
        {
            get
            {
                SupportsSavepointsCalls++;
                return true;
            }
        }

        public IDbContextTransaction CurrentTransaction
            => _transaction;

        public Transaction EnlistedTransaction { get; }

        public void EnlistTransaction(Transaction transaction)
            => throw new NotImplementedException();

        public void ResetState()
            => throw new NotImplementedException();

        public Task ResetStateAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private class FakeDbContextTransaction : IDbContextTransaction
    {
        public void Dispose()
            => throw new NotImplementedException();

        public ValueTask DisposeAsync()
            => throw new NotImplementedException();

        public Guid TransactionId { get; }

        public void Commit()
            => throw new NotImplementedException();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void Rollback()
            => throw new NotImplementedException();

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    [ConditionalFact]
    public void Can_commit_transaction()
    {
        var manager = new FakeDbContextTransactionManager(new FakeDbContextTransaction());

        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDbContextTransactionManager>(manager));

        context.Database.CommitTransaction();

        Assert.Equal(1, manager.CommitCalls);
    }

    [ConditionalFact]
    public async Task Can_commit_transaction_async()
    {
        var manager = new FakeDbContextTransactionManager(new FakeDbContextTransaction());

        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDbContextTransactionManager>(manager));

        await context.Database.CommitTransactionAsync();

        Assert.Equal(1, manager.CommitCalls);
    }

    [ConditionalFact]
    public void Can_roll_back_transaction()
    {
        var manager = new FakeDbContextTransactionManager(new FakeDbContextTransaction());

        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDbContextTransactionManager>(manager));

        context.Database.RollbackTransaction();

        Assert.Equal(1, manager.RollbackCalls);
    }

    [ConditionalFact]
    public async Task Can_roll_back_transaction_async()
    {
        var manager = new FakeDbContextTransactionManager(new FakeDbContextTransaction());

        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDbContextTransactionManager>(manager));

        await context.Database.RollbackTransactionAsync();

        Assert.Equal(1, manager.RollbackCalls);
    }

    [ConditionalFact]
    public void Can_get_current_transaction()
    {
        var transaction = new FakeDbContextTransaction();

        var context = InMemoryTestHelpers.Instance.CreateContext(
            new ServiceCollection().AddSingleton<IDbContextTransactionManager>(
                new FakeDbContextTransactionManager(transaction)));

        Assert.Same(transaction, context.Database.CurrentTransaction);
    }

    [ConditionalFact]
    public void Cannot_use_DatabaseFacade_after_dispose()
    {
        var context = InMemoryTestHelpers.Instance.CreateContext();
        var facade = context.Database;
        context.Dispose();

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Database.GetService<IModel>()).Message);

        foreach (var methodInfo in facade.GetType().GetMethods(BindingFlags.Public))
        {
            Assert.StartsWith(
                CoreStrings.ContextDisposed,
                Assert.Throws<ObjectDisposedException>(() => methodInfo.Invoke(facade, null)).Message);
        }
    }
}
