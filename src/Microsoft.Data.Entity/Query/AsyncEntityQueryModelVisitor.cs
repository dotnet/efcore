// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    public abstract class AsyncEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        protected AsyncEntityQueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
            : base(new AsyncLinqOperatorProvider(), parentQueryModelVisitor)
        {
        }

        public new Func<QueryContext, IAsyncEnumerable<TResult>> CreateQueryExecutor<TResult>([NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, "queryModel");

            VisitQueryModel(queryModel);

            if (_streamedSequenceInfo == null)
            {
                _expression
                    = Expression.Call(
                        _taskToSequenceShim.MakeGenericMethod(typeof(TResult)),
                        _expression);
            }

            return Expression
                .Lambda<Func<QueryContext, IAsyncEnumerable<TResult>>>(_expression, _queryContextParameter)
                .Compile();
        }

        private static readonly MethodInfo _taskToSequenceShim
            = typeof(AsyncEntityQueryModelVisitor)
                .GetMethod("TaskToSequenceShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static IAsyncEnumerable<T> TaskToSequenceShim<T>(Task<T> task)
        {
            return new TaskResultAsyncEnumerable<T>(task);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            var streamedDataInfo
                = resultOperator.GetOutputDataInfo(_streamedSequenceInfo);

            _expression
                = ProcessResultOperator(_expression, _streamedSequenceInfo.ResultItemType, (dynamic)resultOperator);

            _streamedSequenceInfo = streamedDataInfo as StreamedSequenceInfo;
        }

        private static Expression ProcessResultOperator(Expression expression, Type expressionItemType, AnyResultOperator _)
        {
            return Expression.Call(_anyAsyncShim.MakeGenericMethod(expressionItemType), expression);
        }

        private static readonly MethodInfo _anyAsyncShim
            = typeof(AsyncEntityQueryModelVisitor)
                .GetMethod("AnyAsyncShim", BindingFlags.NonPublic | BindingFlags.Static);

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
            = typeof(AsyncEntityQueryModelVisitor)
                .GetMethod("CountAsyncShim", BindingFlags.NonPublic | BindingFlags.Static);

        [UsedImplicitly]
        private static Task<int> CountAsyncShim<TSource>(IAsyncEnumerable<TSource> source)
        {
            return source.Count();
        }

        private static Expression ProcessResultOperator(Expression _, Type __, ResultOperatorBase ___)
        {
            throw new NotImplementedException();
        }
    }
}
