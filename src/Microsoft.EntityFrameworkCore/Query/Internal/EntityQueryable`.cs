// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityQueryable<TResult> : QueryableBase<TResult>, IAsyncEnumerable<TResult>, IDetachableContext
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityQueryable([NotNull] IAsyncQueryProvider provider)
            : base(Check.NotNull(provider, nameof(provider)))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityQueryable([NotNull] IAsyncQueryProvider provider, [NotNull] Expression expression)
            : base(
                Check.NotNull(provider, nameof(provider)),
                Check.NotNull(expression, nameof(expression)))
        {
        }

        IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
            => ((IAsyncQueryProvider)Provider).ExecuteAsync<TResult>(Expression).GetEnumerator();

        IDetachableContext IDetachableContext.DetachContext()
            => new EntityQueryable<TResult>(_nullAsyncQueryProvider);

        private static readonly IAsyncQueryProvider _nullAsyncQueryProvider = new NullAsyncQueryProvider();

        private class NullAsyncQueryProvider : IAsyncQueryProvider
        {
            public IQueryable CreateQuery(Expression expression)
            {
                throw new NotImplementedException();
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                throw new NotImplementedException();
            }

            public object Execute(Expression expression)
            {
                throw new NotImplementedException();
            }

            public TResult1 Execute<TResult1>(Expression expression)
            {
                throw new NotImplementedException();
            }

            public IAsyncEnumerable<TResult1> ExecuteAsync<TResult1>(Expression expression)
            {
                throw new NotImplementedException();
            }

            public Task<TResult1> ExecuteAsync<TResult1>(Expression expression, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
