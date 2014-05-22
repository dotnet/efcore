// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalResultOperatorHandler : IResultOperatorHandler
    {
        private static readonly Dictionary<Type, Func<EntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression>>
            _resultHandlers = new Dictionary<Type, Func<EntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression>>
                {
                    { typeof(TakeResultOperator), (v, r, q) => ProcessResultOperator(v, (TakeResultOperator)r, q) }
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

            Func<EntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression> resultHandler;
            if (!_resultHandlers.TryGetValue(resultOperator.GetType(), out resultHandler))
            {
                return _resultOperatorHandler
                    .HandleResultOperator(
                        entityQueryModelVisitor,
                        streamedDataInfo,
                        resultOperator,
                        queryModel);
            }

            return resultHandler(entityQueryModelVisitor, resultOperator, queryModel);
        }

        private static Expression ProcessResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            TakeResultOperator takeResultOperator,
            QueryModel queryModel)
        {
            var relationalVisitor = (RelationalQueryModelVisitor)entityQueryModelVisitor;

            relationalVisitor
                .QueryFor(queryModel.MainFromClause)
                .SetTopN(takeResultOperator.GetConstantCount());

            return null;
        }
    }
}
