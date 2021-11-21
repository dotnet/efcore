// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class TestStore : IDisposable
{
    private static readonly TestStoreIndex _globalTestStoreIndex = new();
    public IServiceProvider ServiceProvider { get; protected set; }

    protected TestStore(string name, bool shared)
    {
        Name = name;
        Shared = shared;
    }

    public string Name { get; protected set; }
    public bool Shared { get; }

    public virtual TestStore Initialize(
        IServiceProvider serviceProvider,
        Func<DbContext> createContext,
        Action<DbContext> seed = null,
        Action<DbContext> clean = null)
    {
        ServiceProvider = serviceProvider;
        if (createContext == null)
        {
            createContext = CreateDefaultContext;
        }

        if (Shared)
        {
            GetTestStoreIndex(serviceProvider).CreateShared(GetType().Name + Name, () => Initialize(createContext, seed, clean));
        }
        else
        {
            GetTestStoreIndex(serviceProvider).CreateNonShared(GetType().Name + Name, () => Initialize(createContext, seed, clean));
        }

        return this;
    }

    public virtual TestStore Initialize(
        IServiceProvider serviceProvider,
        Func<TestStore, DbContext> createContext,
        Action<DbContext> seed = null,
        Action<DbContext> clean = null)
        => Initialize(serviceProvider, () => createContext(this), seed, clean);

    public virtual TestStore Initialize<TContext>(
        IServiceProvider serviceProvider,
        Func<TestStore, TContext> createContext,
        Action<TContext> seed = null,
        Action<TContext> clean = null)
        where TContext : DbContext
        => Initialize(
            serviceProvider,
            createContext,
            // ReSharper disable twice RedundantCast
            seed == null ? (Action<DbContext>)null : c => seed((TContext)c),
            clean == null ? (Action<DbContext>)null : c => clean((TContext)c));

    protected virtual void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
    {
        using var context = createContext();
        clean?.Invoke(context);

        Clean(context);

        seed?.Invoke(context);
    }

    public abstract DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder);
    public abstract void Clean(DbContext context);

    public virtual Task CleanAsync(DbContext context)
    {
        Clean(context);
        return Task.CompletedTask;
    }

    protected virtual DbContext CreateDefaultContext()
        => new(AddProviderOptions(new DbContextOptionsBuilder().EnableServiceProviderCaching(false)).Options);

    protected virtual TestStoreIndex GetTestStoreIndex(IServiceProvider serviceProvider)
        => _globalTestStoreIndex;

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

        private void DistributedTransactionStarted(object sender, TransactionEventArgs e)
            => Assert.False(true, "Distributed transaction started");

        public void Dispose()
            => TransactionManager.DistributedTransactionStarted -= DistributedTransactionStarted;
    }

    private class CompositeDisposable : IDisposable
    {
        private readonly IDisposable[] _disposables;

        public CompositeDisposable(params IDisposable[] disposables)
        {
            _disposables = disposables;
        }

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
