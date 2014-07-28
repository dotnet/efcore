// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.Data.Entity.Query
{
    public class AsyncResultOperatorHandler : IResultOperatorHandler
    {
        private static readonly Dictionary<Type, Func<Expression, Type, ResultOperatorBase, Expression>>
            _asyncHandlers = new Dictionary<Type, Func<Expression, Type, ResultOperatorBase, Expression>>
                {
                    { typeof(AnyResultOperator), (e, t, r) => HandleResultOperator(e, t, (AnyResultOperator)r) },
                    { typeof(CountResultOperator), (e, t, r) => HandleResultOperator(e, t, (CountResultOperator)r) },
                    { typeof(SingleResultOperator), (e, t, r) => HandleResultOperator(e, t, (SingleResultOperator)r) },
                    { typeof(FirstResultOperator), (e, t, r) => HandleResultOperator(e, t, (FirstResultOperator)r) }
                };

        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor");
            Check.NotNull(resultOperator, "resultOperator");
            Check.NotNull(queryModel, "queryModel");

            Func<Expression, Type, ResultOperatorBase, Expression> asyncHandler;
            if (!_asyncHandlers.TryGetValue(resultOperator.GetType(), out asyncHandler))
            {
                // TODO: Implement the rest...
                throw new NotImplementedException();
            }

            return asyncHandler(
                entityQueryModelVisitor.Expression,
                entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType,
                resultOperator);
        }

        private static Expression HandleResultOperator(Expression expression, Type expressionItemType, AnyResultOperator _)
        {
            return Expression.Call(_any.MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _any
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("_Any");

        [UsedImplicitly]
        private static Task<bool> _Any<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Any();
        }

        private static Expression HandleResultOperator(Expression expression, Type expressionItemType, CountResultOperator _)
        {
            return Expression.Call(_count.MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _count
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("_Count");

        [UsedImplicitly]
        private static Task<int> _Count<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Count();
        }

        private static Expression HandleResultOperator(
            Expression expression, Type expressionItemType, SingleResultOperator singleResultOperator)
        {
            return Expression.Call(
                (singleResultOperator.ReturnDefaultWhenEmpty
                    ? _singleOrDefault
                    : _single)
                    .MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _single
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("_Single");

        [UsedImplicitly]
        private static Task<TSource> _Single<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Single();
        }

        private static readonly MethodInfo _singleOrDefault
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("_SingleOrDefault");

        [UsedImplicitly]
        private static Task<TSource> _SingleOrDefault<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.SingleOrDefault();
        }

        private static Expression HandleResultOperator(
            Expression expression, Type expressionItemType, FirstResultOperator firstResultOperator)
        {
            return Expression.Call(
                (firstResultOperator.ReturnDefaultWhenEmpty
                    ? _firstOrDefault
                    : _first)
                    .MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _first
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("_First");

        [UsedImplicitly]
        private static Task<TSource> _First<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.First();
        }

        private static readonly MethodInfo _firstOrDefault
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("_FirstOrDefault");

        [UsedImplicitly]
        private static Task<TSource> _FirstOrDefault<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.FirstOrDefault();
        }
    }
}
