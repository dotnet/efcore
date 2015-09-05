// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.Data.Entity.Query.ResultOperators.Internal
{
    public class QueryAnnotationExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods = { QueryAnnotationExtensions.QueryAnnotationMethodInfo };

        private readonly ConstantExpression _annotationExpression;

        public QueryAnnotationExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] ConstantExpression annotationExpression)
            : base(parseInfo, null, null)
        {
            _annotationExpression = annotationExpression;
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
            => new QueryAnnotationResultOperator(_annotationExpression);

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
            => Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
    }
}
