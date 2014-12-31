// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QuerySourceScope
    {
        private static readonly MethodInfo _createMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethods("Create").FirstOrDefault(m => m.IsStatic && !m.IsPublic);

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethods("GetResult").FirstOrDefault(m => !m.IsStatic && !m.IsPublic);

        public static Expression Create(
            [NotNull] IQuerySource querySource,
            [NotNull] Expression result,
            [NotNull] Expression parentScope)
        {
            return Expression.Call(
                _createMethodInfo.MakeGenericMethod(result.Type),
                Expression.Constant(querySource),
                result,
                parentScope);
        }

        public static Expression GetResult(
            [NotNull] Expression querySourceScope,
            [NotNull] IQuerySource querySource,
            [NotNull] Type resultType)
        {
            return Expression.Call(
                querySourceScope,
                _getResultMethodInfo.MakeGenericMethod(resultType),
                Expression.Constant(querySource));
        }

        private readonly QuerySourceScope _parentScope;
        private readonly IQuerySource _querySource;

        protected QuerySourceScope(IQuerySource querySource, QuerySourceScope parentScope)
        {
            _querySource = querySource;
            _parentScope = parentScope;
        }

        [UsedImplicitly]
        private static QuerySourceScope<TResult> Create<TResult>(
            IQuerySource querySource, TResult result, QuerySourceScope parentScope)
        {
            return new QuerySourceScope<TResult>(querySource, result, parentScope);
        }

        [UsedImplicitly]
        private TResult GetResult<TResult>(IQuerySource querySource)
        {
            return _querySource == querySource
                ? ((QuerySourceScope<TResult>)this).Result
                : _parentScope.GetResult<TResult>(querySource);
        }

        public virtual object GetResult([NotNull] IQuerySource querySource)
        {
            return _querySource == querySource
                ? UntypedResult
                : _parentScope.GetResult(querySource);
        }

        public abstract object UntypedResult { get; }
    }
}
