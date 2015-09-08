// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QueryResultScope
    {
        private static readonly MethodInfo _createMethodInfo
            = typeof(QueryResultScope).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Create));

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(QueryResultScope).GetTypeInfo()
                .GetDeclaredMethod(nameof(_GetResult));

        public static Expression Create(
            [NotNull] IQuerySource querySource,
            [NotNull] Expression result,
            [NotNull] Expression parentScope)
            => Expression.Call(
                _createMethodInfo.MakeGenericMethod(result.Type),
                Expression.Constant(querySource),
                result,
                parentScope);

        public static Expression GetResult(
            [NotNull] Expression queryResultScope,
            [NotNull] IQuerySource querySource,
            [NotNull] Type resultType)
            => Expression.Call(
                queryResultScope,
                _getResultMethodInfo.MakeGenericMethod(resultType),
                Expression.Constant(querySource));

        private readonly QueryResultScope _parentScope;
        private readonly IQuerySource _querySource;

        protected QueryResultScope(IQuerySource querySource, QueryResultScope parentScope)
        {
            _querySource = querySource;
            _parentScope = parentScope;
        }

        [UsedImplicitly]
        private static QueryResultScope<TResult> _Create<TResult>(
            IQuerySource querySource, TResult result, QueryResultScope parentScope)
            => new QueryResultScope<TResult>(querySource, result, parentScope);

        [UsedImplicitly]
        private TResult _GetResult<TResult>(IQuerySource querySource)
            => _querySource == querySource
                ? ((QueryResultScope<TResult>)this).Result
                : _parentScope._GetResult<TResult>(querySource);

        public virtual object GetResult([NotNull] IQuerySource querySource)
            => _querySource == querySource
                ? UntypedResult
                : _parentScope.GetResult(querySource);

        public abstract object UntypedResult { get; }
    }
}
