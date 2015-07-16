// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.Data.Entity.Query.ResultOperators
{
    public class ThenIncludeExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            EntityFrameworkQueryableExtensions.ThenIncludeAfterCollectionMethodInfo,
            EntityFrameworkQueryableExtensions.ThenIncludeAfterReferenceMethodInfo
        };

        private readonly LambdaExpression _navigationPropertyPathLambda;

        public ThenIncludeExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] LambdaExpression navigationPropertyPathLambda)
            : base(parseInfo, null, null)
        {
            Check.NotNull(navigationPropertyPathLambda, nameof(navigationPropertyPathLambda));

            _navigationPropertyPathLambda = navigationPropertyPathLambda;
        }

        protected override QueryModel ApplyNodeSpecificSemantics(QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
        {
            var queryAnnotationResultOperator
                = (QueryAnnotationResultOperator)clauseGenerationContext.GetContextInfo(Source);

            ((IncludeQueryAnnotation)queryAnnotationResultOperator.Annotation)
                .AppendToNavigationPath(_navigationPropertyPathLambda.GetComplexPropertyAccess());

            clauseGenerationContext.AddContextInfo(this, queryAnnotationResultOperator);

            return queryModel;
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext) => null;

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
