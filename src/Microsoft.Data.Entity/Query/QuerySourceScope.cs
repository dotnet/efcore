// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query
{
    public abstract class QuerySourceScope
    {
        private class TypedScope<TResult> : QuerySourceScope
        {
            public readonly TResult _result;

            public TypedScope(
                [NotNull] IQuerySource querySource,
                [NotNull] TResult result,
                [CanBeNull] QuerySourceScope parentScope)
                : base(querySource, parentScope)
            {
                _result = result;
            }
        }

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
                _createMethodInfo.MakeGenericMethod(querySource.ItemType),
                Expression.Constant(querySource),
                result,
                parentScope);
        }

        public static Expression GetResult(
            [NotNull] Expression querySourceScope,
            [NotNull] IQuerySource querySource)
        {
            return Expression.Call(
                querySourceScope,
                _getResultMethodInfo.MakeGenericMethod(querySource.ItemType),
                Expression.Constant(querySource));
        }

        private readonly QuerySourceScope _parentScope;
        private readonly IQuerySource _querySource;

        private QuerySourceScope(IQuerySource querySource, QuerySourceScope parentScope)
        {
            _querySource = querySource;
            _parentScope = parentScope;
        }

        [UsedImplicitly]
        private static TypedScope<TResult> Create<TResult>(
            IQuerySource querySource, TResult result, QuerySourceScope parentScope)
        {
            return new TypedScope<TResult>(querySource, result, parentScope);
        }

        [UsedImplicitly]
        private TResult GetResult<TResult>(IQuerySource querySource)
        {
            return _querySource == querySource
                ? ((TypedScope<TResult>)this)._result
                : _parentScope.GetResult<TResult>(querySource);
        }
    }
}
