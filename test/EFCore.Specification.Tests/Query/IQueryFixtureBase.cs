// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public interface IQueryFixtureBase
{
    Func<DbContext> GetContextCreator();

    Func<DbContext, ISetSource> GetSetSourceCreator()
        => context => new DefaultSetSource(context);

    ISetSource GetExpectedData();

    IReadOnlyDictionary<Type, object> GetEntitySorters();

    IReadOnlyDictionary<Type, object> GetEntityAsserters();

    private class DefaultSetSource : ISetSource
    {
        private readonly DbContext _context;

        public DefaultSetSource(DbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();
    }
}
