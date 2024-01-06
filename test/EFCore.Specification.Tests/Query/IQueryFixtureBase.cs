// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public interface IQueryFixtureBase
{
    Func<DbContext> GetContextCreator();

    Func<DbContext, ISetSource> GetSetSourceCreator()
        => context => new DefaultSetSource(context);

    ISetSource GetExpectedData();

    IReadOnlyDictionary<Type, object> EntitySorters { get; }

    IReadOnlyDictionary<Type, object> EntityAsserters { get; }

    private class DefaultSetSource(DbContext context) : ISetSource
    {
        private readonly DbContext _context = context;

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();
    }
}
