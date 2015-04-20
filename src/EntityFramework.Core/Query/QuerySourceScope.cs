// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
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

        private static readonly MethodInfo _getFirstResultMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethods("GetFirstResult").FirstOrDefault(m => !m.IsStatic && !m.IsPublic);

        private static readonly MethodInfo _joinMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethods("Join").FirstOrDefault(m => m.IsStatic && !m.IsPublic);

        public static Expression Join(
            [NotNull] IQuerySource querySource,
            [NotNull] Expression scope,
            [NotNull] Expression parentScope)
        {
            return Expression.Call(
                _joinMethodInfo.MakeGenericMethod(
                    scope.Type.GenericTypeArguments[0]),
                Expression.Constant(querySource),
                scope,
                parentScope);
        }

        [UsedImplicitly]
        private static QuerySourceScope<TResult> Join<TResult>(
            IQuerySource querySource,
            QuerySourceScope<TResult> scope,
            QuerySourceScope parentScope)
        {
            if (parentScope != null)
            {
                Reparent(scope, parentScope);
            }

            return new QuerySourceScope<TResult>(querySource, scope.Result, scope, null);
        }

        private static void Reparent(QuerySourceScope scope, QuerySourceScope parentScope)
        {
            if (scope.ParentScope == null
                || scope.ParentScope.QuerySource == parentScope.QuerySource)
            {
                scope.ParentScope = parentScope;
            }
            else
            {
                Reparent(scope.ParentScope, parentScope);
            }
        }

        public static Expression Create(
            [NotNull] IQuerySource querySource,
            [NotNull] Expression result,
            [NotNull] Expression parentScope,
            [CanBeNull] Expression valueReader = null)
        {
            return Expression.Call(
                _createMethodInfo.MakeGenericMethod(result.Type),
                Expression.Constant(querySource),
                result,
                parentScope,
                valueReader ?? Expression.Constant(null, typeof(IValueReader)));
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

        public static Expression GetResult(
            [NotNull] Expression querySourceScope,
            [NotNull] Type resultType)
        {
            return Expression.Call(
                querySourceScope,
                _getFirstResultMethodInfo.MakeGenericMethod(resultType));
        }

        private readonly IValueReader _valueReader;

        protected QuerySourceScope(
            IQuerySource querySource,
            QuerySourceScope parentScope,
            IValueReader valueReader)
        {
            QuerySource = querySource;
            ParentScope = parentScope;

            _valueReader = valueReader;
        }

        public abstract object UntypedResult { get; }

        public virtual IQuerySource QuerySource { get; }
        public virtual QuerySourceScope ParentScope { get; private set; }
        public virtual IValueReader ValueReader => _valueReader;

        [UsedImplicitly]
        private static QuerySourceScope<TResult> Create<TResult>(
            IQuerySource querySource,
            TResult result,
            QuerySourceScope parentScope,
            IValueReader valueReader)
        {
            return new QuerySourceScope<TResult>(querySource, result, parentScope, valueReader);
        }

        [UsedImplicitly]
        private TResult GetResult<TResult>(IQuerySource querySource)
        {
            return QuerySource == querySource
                ? ((QuerySourceScope<TResult>)this).Result
                : ParentScope.GetResult<TResult>(querySource);
        }

        [UsedImplicitly]
        private TResult GetFirstResult<TResult>()
        {
            return ((QuerySourceScope<TResult>)this).Result;
        }

        public virtual object GetResult([NotNull] IQuerySource querySource)
        {
            return QuerySource == querySource
                ? UntypedResult
                : ParentScope.GetResult(querySource);
        }

        public virtual IValueReader GetValueReader([NotNull] object result)
        {
            return UntypedResult == result
                   && _valueReader != null
                ? _valueReader
                : ParentScope?.GetValueReader(result);
        }
    }
}
