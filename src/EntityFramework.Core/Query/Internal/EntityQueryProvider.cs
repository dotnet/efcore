// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class EntityQueryProvider : IEntityQueryProvider
    {
        private static readonly MethodInfo _genericCreateQueryMethod
            = typeof(EntityQueryProvider).GetRuntimeMethods()
                .Single(m => m.Name == "CreateQuery" && m.IsGenericMethod);

        private readonly IQueryCompiler _queryCompiler;

        public EntityQueryProvider([NotNull] IQueryCompiler queryCompiler)
        {
            _queryCompiler = queryCompiler;
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new EntityQueryable<TElement>(this, expression);

        public virtual IQueryable CreateQuery(Expression expression)
            => (IQueryable)_genericCreateQueryMethod
                .MakeGenericMethod(expression.Type.GetSequenceType())
                .Invoke(this, new object[] { expression });

        public virtual TResult Execute<TResult>(Expression expression)
            => _queryCompiler.Execute<TResult>(expression);

        public virtual object Execute(Expression expression)
            => Execute<object>(expression);

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => _queryCompiler.ExecuteAsync<TResult>(expression);

        public virtual Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            => _queryCompiler.ExecuteAsync<TResult>(expression, cancellationToken);
    }
}
