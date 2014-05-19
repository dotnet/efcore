// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.Data.Entity.Query
{
    public class ResultOperatorHandler : IResultOperatorHandler
    {
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

            return
                Expression.Call(
                    _executeResultOperatorMethodInfo
                        .MakeGenericMethod(
                            entityQueryModelVisitor.StreamedSequenceInfo.ResultItemType, 
                            streamedDataInfo.DataType),
                    entityQueryModelVisitor.Expression,
                    Expression.Constant(resultOperator),
                    Expression.Constant(entityQueryModelVisitor.StreamedSequenceInfo));
        }

        private static readonly MethodInfo _executeResultOperatorMethodInfo
            = typeof(ResultOperatorHandler)
                .GetTypeInfo().GetDeclaredMethod("ExecuteResultOperator");

        [UsedImplicitly]
        private static TResult ExecuteResultOperator<TSource, TResult>(
            IEnumerable<TSource> source, ResultOperatorBase resultOperator, StreamedSequenceInfo streamedSequenceInfo)
        {
            var streamedData
                = resultOperator.ExecuteInMemory(
                    new StreamedSequence(source, streamedSequenceInfo));

            return (TResult)streamedData.Value;
        }
    }
}
