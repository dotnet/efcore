// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryable<TResult> : IOrderedQueryable<TResult>, IAsyncEnumerable<TResult>
    {
        private readonly EntityContext _entityContext;
        private readonly IQueryProvider _queryProvider;

        public EntityQueryable([NotNull] EntityContext entityContext)
        {
            Check.NotNull(entityContext, "entityContext");

            _entityContext = entityContext;
            _queryProvider = new EnumerableQuery<TResult>(this);
        }

        public IAsyncEnumerator<TResult> GetAsyncEnumerator()
        {
            return Execute().GetAsyncEnumerator();
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return Execute().GetEnumerator();
        }

        IAsyncEnumerator IAsyncEnumerable.GetAsyncEnumerator()
        {
            return GetAsyncEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return Execute().AsQueryable().Expression; }
        }

        public Type ElementType
        {
            get { return typeof(TResult); }
        }

        public IQueryProvider Provider
        {
            get { return _queryProvider; }
        }

        private IAsyncEnumerable<TResult> Execute()
        {
            return _entityContext.Configuration.DataStore
                .Query<TResult>(
                    typeof(TResult),
                    _entityContext.Configuration.Model,
                    _entityContext.Configuration.StateManager);
        }
    }
}
