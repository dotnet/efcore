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
                .GetDeclaredMethods(nameof(Create)).FirstOrDefault(m => m.IsStatic && !m.IsPublic);

        private static readonly MethodInfo _getResultMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethods(nameof(GetResult)).FirstOrDefault(m => !m.IsStatic && !m.IsPublic);

        private static readonly MethodInfo _getFirstResultMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethods(nameof(GetFirstResult)).FirstOrDefault(m => !m.IsStatic && !m.IsPublic);

        private static readonly MethodInfo _joinMethodInfo
            = typeof(QuerySourceScope).GetTypeInfo()
                .GetDeclaredMethods(nameof(Join)).FirstOrDefault(m => m.IsStatic && !m.IsPublic);

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

            return new QuerySourceScope<TResult>(querySource, scope.Result, scope, new ValueBuffer());
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
            [CanBeNull] Expression valueBuffer = null)
        {
            return Expression.Call(
                _createMethodInfo.MakeGenericMethod(result.Type),
                Expression.Constant(querySource),
                result,
                parentScope,
                valueBuffer ?? Expression.Constant(new ValueBuffer()));
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

        private readonly ValueBuffer _valueBuffer;

        protected QuerySourceScope(
            IQuerySource querySource,
            QuerySourceScope parentScope,
            ValueBuffer valueBuffer)
        {
            QuerySource = querySource;
            ParentScope = parentScope;

            _valueBuffer = valueBuffer;
        }

        public abstract object UntypedResult { get; }

        public virtual IQuerySource QuerySource { get; }
        public virtual QuerySourceScope ParentScope { get; private set; }
        public virtual ValueBuffer ValueBuffer => _valueBuffer;

        [UsedImplicitly]
        private static QuerySourceScope<TResult> Create<TResult>(
            IQuerySource querySource,
            TResult result,
            QuerySourceScope parentScope,
            ValueBuffer valueBuffer)
        {
            return new QuerySourceScope<TResult>(querySource, result, parentScope, valueBuffer);
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

        public virtual ValueBuffer GetValueBuffer([NotNull] object result)
        {
            return UntypedResult == result
                   && _valueBuffer.Count > 0
                ? _valueBuffer
                : ParentScope?.GetValueBuffer(result)
                  ?? new ValueBuffer();
        }
    }
}
