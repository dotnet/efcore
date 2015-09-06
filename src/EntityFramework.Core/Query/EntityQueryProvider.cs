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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryProvider : IEntityQueryProvider
    {
        private static readonly MethodInfo _genericCreateQueryMethod
            = typeof(EntityQueryProvider).GetRuntimeMethods()
                .Single(m => m.Name == "CreateQuery" && m.IsGenericMethod);

        private readonly IQueryCompiler _queryCompiler;

        public EntityQueryProvider([NotNull] IQueryCompiler queryCompiler)
        {
            Check.NotNull(queryCompiler, nameof(queryCompiler));

            _queryCompiler = queryCompiler;
        }

        public virtual IQueryable<TElement> CreateQuery<TElement>([NotNull] Expression expression)
            => new EntityQueryable<TElement>(
                this,
                Check.NotNull(expression, nameof(expression)));

        public virtual IQueryable CreateQuery(Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var sequenceType = expression.Type.GetSequenceType();

            return (IQueryable)_genericCreateQueryMethod
                .MakeGenericMethod(sequenceType)
                .Invoke(this, new object[] { expression });
        }

        public virtual TResult Execute<TResult>([NotNull] Expression expression)
            => _queryCompiler.Execute<TResult>(
                Check.NotNull(expression, nameof(expression)));

        public virtual object Execute(Expression expression)
            => Execute<object>(expression);

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>([NotNull] Expression expression)
            => _queryCompiler.ExecuteAsync<TResult>(
                Check.NotNull(expression, nameof(expression)));

        public virtual Task<TResult> ExecuteAsync<TResult>([NotNull] Expression expression, CancellationToken cancellationToken)
            => _queryCompiler.ExecuteAsync<TResult>(
                Check.NotNull(expression, nameof(expression)),
                cancellationToken);
    }
}
