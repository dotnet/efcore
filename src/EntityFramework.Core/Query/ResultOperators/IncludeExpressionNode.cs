// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.Data.Entity.Query.ResultOperators
{
    public class IncludeExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            EntityFrameworkQueryableExtensions.IncludeMethodInfo
        };

        private readonly LambdaExpression _navigationPropertyPathLambda;

        public IncludeExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] LambdaExpression navigationPropertyPathLambda)
            : base(parseInfo, null, null)
        {
            Check.NotNull(navigationPropertyPathLambda, nameof(navigationPropertyPathLambda));

            _navigationPropertyPathLambda = navigationPropertyPathLambda;
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            var navigationPropertyPath
                = Source.Resolve(
                    _navigationPropertyPathLambda.Parameters[0],
                    _navigationPropertyPathLambda.Body,
                    clauseGenerationContext);

            var queryAnnotationResultOperator = new QueryAnnotationResultOperator(
                Expression.Constant(
                    new IncludeQueryAnnotation(navigationPropertyPath)));

            clauseGenerationContext.AddContextInfo(this, queryAnnotationResultOperator);

            return queryAnnotationResultOperator;
        }

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
            => Source.Resolve(
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);
    }
}
