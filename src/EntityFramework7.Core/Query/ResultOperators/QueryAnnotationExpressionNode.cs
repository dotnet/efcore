// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.Data.Entity.Query.ResultOperators
{
    public class QueryAnnotationExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods = { QueryAnnotationExtensions.QueryAnnotationMethodInfo };

        private readonly ConstantExpression _annotationExpression;

        public QueryAnnotationExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] ConstantExpression annotationExpression)
            : base(
                Check.NotNull(parseInfo, nameof(parseInfo)),
                null,
                null)
        {
            Check.NotNull(annotationExpression, nameof(annotationExpression));

            _annotationExpression = annotationExpression;
        }

        protected override ResultOperatorBase CreateResultOperator([NotNull] ClauseGenerationContext clauseGenerationContext)
        {
            Check.NotNull(clauseGenerationContext, nameof(clauseGenerationContext));

            return new QueryAnnotationResultOperator(_annotationExpression);
        }

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            Check.NotNull(inputParameter, nameof(inputParameter));
            Check.NotNull(expressionToBeResolved, nameof(expressionToBeResolved));

            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }
    }
}
