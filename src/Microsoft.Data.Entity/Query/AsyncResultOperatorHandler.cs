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
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query
{
    public class AsyncResultOperatorHandler : IResultOperatorHandler
    {
        private static readonly Dictionary<Type, Func<Expression, Type, ResultOperatorBase, Expression>>
            _asyncHandlers = new Dictionary<Type, Func<Expression, Type, ResultOperatorBase, Expression>>
                {
                    { typeof(AnyResultOperator), (e, t, r) => ProcessResultOperator(e, t, (AnyResultOperator)r) },
                    { typeof(CountResultOperator), (e, t, r) => ProcessResultOperator(e, t, (CountResultOperator)r) }
                };

        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            IStreamedDataInfo streamedDataInfo,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor");
            Check.NotNull(streamedDataInfo, "streamedDataInfo");
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

        private static Expression ProcessResultOperator(Expression expression, Type expressionItemType, AnyResultOperator _)
        {
            return Expression.Call(_anyAsyncShim.MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _anyAsyncShim
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("AnyAsyncShim");

        [UsedImplicitly]
        private static Task<bool> AnyAsyncShim<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Any();
        }

        private static Expression ProcessResultOperator(Expression expression, Type expressionItemType, CountResultOperator _)
        {
            return Expression.Call(_countAsyncShim.MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _countAsyncShim
            = typeof(AsyncResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("CountAsyncShim");

        [UsedImplicitly]
        private static Task<int> CountAsyncShim<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Count();
        }
    }
}
