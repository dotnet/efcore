// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityQueryable<TResult>
        : IOrderedQueryable<TResult>,
          IAsyncEnumerable<TResult>,
          IDetachableContext,
          IListSource
    {
        private static readonly EntityQueryable<TResult> _detached
            = new EntityQueryable<TResult>(NullAsyncQueryProvider.Instance);

        private readonly IAsyncQueryProvider _queryProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityQueryable([NotNull] IAsyncQueryProvider queryProvider)
        {
            Check.NotNull(queryProvider, nameof(queryProvider));

            _queryProvider = queryProvider;
            Expression = Expression.Constant(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityQueryable([NotNull] IAsyncQueryProvider queryProvider, [NotNull] Expression expression)
        {
            Check.NotNull(queryProvider, nameof(queryProvider));
            Check.NotNull(expression, nameof(expression));

            _queryProvider = queryProvider;
            Expression = expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ElementType => typeof(TResult);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Expression { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IQueryProvider Provider => _queryProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerator<TResult> GetEnumerator()
            => _queryProvider.Execute<IEnumerable<TResult>>(Expression).GetEnumerator();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => _queryProvider.Execute<IEnumerable>(Expression).GetEnumerator();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
            => _queryProvider.ExecuteAsync<IAsyncEnumerable<TResult>>(Expression).GetEnumerator();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IDetachableContext IDetachableContext.DetachContext() => _detached;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        IList IListSource.GetList()
        {
            throw new NotSupportedException(CoreStrings.DataBindingWithIListSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        bool IListSource.ContainsListCollection => false;
    }
}
