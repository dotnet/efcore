// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalResultOperatorHandler : IResultOperatorHandler
    {
        private static readonly Dictionary<Type, Func<SelectExpression, ResultOperatorBase, bool>>
            _resultHandlers = new Dictionary<Type, Func<SelectExpression, ResultOperatorBase, bool>>
                {
                    { typeof(TakeResultOperator), (s, r) => ProcessTake(s, (TakeResultOperator)r) },
                    //{ typeof(SingleResultOperator), (s, r) => ProcessSingle(s) },
                    //{ typeof(FirstResultOperator), (s, r) => ProcessFirst(s) },
                    { typeof(DistinctResultOperator), (s, r) => ProcessDistinct(s) }
                };

        private readonly IResultOperatorHandler _resultOperatorHandler;

        public RelationalResultOperatorHandler([NotNull] IResultOperatorHandler resultOperatorHandler)
        {
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");

            _resultOperatorHandler = resultOperatorHandler;
        }

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

            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)entityQueryModelVisitor;

            var selectExpression
                = relationalQueryModelVisitor.TryGetSelectExpression(queryModel.MainFromClause);

            Func<SelectExpression, ResultOperatorBase, bool> resultHandler;
            if (!_resultHandlers.TryGetValue(resultOperator.GetType(), out resultHandler)
                || selectExpression == null
                || resultHandler(selectExpression, resultOperator))
            {
                return _resultOperatorHandler
                    .HandleResultOperator(
                        entityQueryModelVisitor,
                        streamedDataInfo,
                        resultOperator,
                        queryModel);
            }

            return null;
        }

        private static bool ProcessTake(
            SelectExpression selectExpression, TakeResultOperator takeResultOperator)
        {
            selectExpression.AddLimit(takeResultOperator.GetConstantCount());

            return false;
        }

        //        private static bool ProcessSingle(SelectExpression selectExpression)
//        {
        //            selectExpression.AddLimit(2);
//
//            return false;
//        }
//
        //        private static bool ProcessFirst(SelectExpression selectExpression)
//        {
        //            selectExpression.AddLimit(1);
//
//            return false;
//        }

        private static bool ProcessDistinct(SelectExpression selectExpression)
        {
            return !selectExpression.TryMakeDistinct();
        }
    }
}
