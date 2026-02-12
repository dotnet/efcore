// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class QueryFixtureBase<TContext> : SharedStoreFixtureBase<TContext>, IQueryFixtureBase
    where TContext : DbContext
{
    public virtual Func<DbContext> GetContextCreator()
        => CreateContext;

    public virtual Func<DbContext, ISetSource> GetSetSourceCreator()
        => context => new DefaultSetSource(context);

    public abstract ISetSource GetExpectedData();

    public abstract IReadOnlyDictionary<Type, object> EntitySorters { get; }

    public abstract IReadOnlyDictionary<Type, object> EntityAsserters { get; }

    public virtual ISetSource? GetFilteredExpectedData(DbContext context)
        => null;

    public virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    public virtual Action<DatabaseFacade, IDbContextTransaction> GetUseTransaction()
        => UseTransaction;

    string IQueryFixtureBase.StoreName
        => StoreName;

    #region Non-shared test store support

    private TestStore? _nonSharedTestStore;

    public TestStore GetOrCreateNonSharedTestStore(Func<TestStore> createTestStore)
        => _nonSharedTestStore ??= createTestStore();

    public ITestStoreFactory GetTestStoreFactory()
        => TestStoreFactory;

    public TestStore NonSharedTestStore
        => _nonSharedTestStore ?? throw new InvalidOperationException("No non-shared test store has been created, call GetOrCreateNonSharedTestStore() first.");

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();

        if (_nonSharedTestStore != null)
        {
            await _nonSharedTestStore.DisposeAsync();
            _nonSharedTestStore = null;
        }
    }

    #endregion

    private class DefaultSetSource(DbContext context) : ISetSource
    {
        private readonly DbContext _context = context;

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();
    }
}

