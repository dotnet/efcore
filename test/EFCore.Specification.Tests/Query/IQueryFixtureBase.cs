// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
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
}
