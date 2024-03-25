// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class TestStore(string name, bool shared) : IDisposable
{
    private static readonly TestStoreIndex GlobalTestStoreIndex = new();
    public IServiceProvider? ServiceProvider { get; protected set; }

    public string Name { get; protected set; } = name;
    public bool Shared { get; } = shared;

    public virtual async Task<TestStore> InitializeAsync(
        IServiceProvider? serviceProvider,
        Func<DbContext>? createContext,
        Func<DbContext, Task>? seed = null,
        Func<DbContext, Task>? clean = null)
    {
        ServiceProvider = serviceProvider;
        if (createContext == null)
        {
            createContext = CreateDefaultContext;
        }

        if (Shared)
        {
            await GetTestStoreIndex(serviceProvider).CreateSharedAsync(
                GetType().Name + Name, async () => await InitializeAsync(createContext, seed, clean));
        }
        else
        {
            await GetTestStoreIndex(serviceProvider).CreateNonSharedAsync(
                GetType().Name + Name, async () => await InitializeAsync(createContext, seed, clean));
        }

        return this;
    }

    public virtual Task<TestStore> InitializeAsync(
        IServiceProvider serviceProvider,
        Func<TestStore, DbContext> createContext,
        Func<DbContext, Task>? seed = null,
        Func<DbContext, Task>? clean = null)
        => InitializeAsync(serviceProvider, () => createContext(this), seed, clean);

    public virtual Task<TestStore> InitializeAsync<TContext>(
        IServiceProvider serviceProvider,
        Func<TestStore, TContext> createContext,
        Func<TContext, Task>? seed = null,
        Func<TContext, Task>? clean = null)
        where TContext : DbContext
        => InitializeAsync(
            serviceProvider,
            () => createContext(this),
            // ReSharper disable twice RedundantCast
            seed == null ? (Func<DbContext, Task>?)null : c => seed((TContext)c),
            clean == null ? (Func<DbContext, Task>?)null : c => clean((TContext)c));

    protected virtual async Task InitializeAsync(Func<DbContext> createContext, Func<DbContext, Task>? seed, Func<DbContext, Task>? clean)
    {
        using var context = createContext();
        if (clean != null)
        {
            await clean(context);
        }

        await CleanAsync(context);

        if (seed != null)
        {
            await seed(context);
        }
    }

    public abstract DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder);

    public virtual Task CleanAsync(DbContext context)
        => Task.CompletedTask;

    protected virtual DbContext CreateDefaultContext()
        => new(AddProviderOptions(new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options);

    protected virtual TestStoreIndex GetTestStoreIndex(IServiceProvider? serviceProvider)
        => GlobalTestStoreIndex;

    public virtual void Dispose()
    {
    }

    public virtual Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    private static readonly SemaphoreSlim _transactionSyncRoot = new(1);

    public static IDisposable CreateTransactionScope(bool useTransaction = true)
    {
        if (useTransaction)
        {
            _transactionSyncRoot.Wait(TimeSpan.FromMinutes(1));
            var listener = new DistributedTransactionListener();
            var transaction = new CommittableTransaction(TimeSpan.FromMinutes(10));
            transaction.TransactionCompleted += (_, __) => _transactionSyncRoot.Release();

            return new CompositeDisposable(
                listener,
                transaction,
                new TransactionScope(transaction, TimeSpan.FromMinutes(10), TransactionScopeAsyncFlowOption.Enabled));
        }

        return new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
    }

    private class DistributedTransactionListener : IDisposable
    {
        public DistributedTransactionListener()
        {
            TransactionManager.DistributedTransactionStarted += DistributedTransactionStarted;
        }

        private void DistributedTransactionStarted(object? sender, TransactionEventArgs e)
            => Assert.Fail("Distributed transaction started");

        public void Dispose()
            => TransactionManager.DistributedTransactionStarted -= DistributedTransactionStarted;
    }

    private class CompositeDisposable(params IDisposable[] disposables) : IDisposable
    {
        private readonly IDisposable[] _disposables = disposables;

        public void Dispose()
        {
            var exceptions = new List<Exception>();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = _disposables.Length - 1; i >= 0; i--)
            {
                try
                {
                    _disposables[i].Dispose();
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
