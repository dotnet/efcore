// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.Data.Entity.Query.ResultOperators
{
    public class IncludeExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
            {
                QueryableExtensions.IncludeMethodInfo
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

            var includeResultOperator = new IncludeResultOperator(navigationPropertyPath);

            clauseGenerationContext.AddContextInfo(this, includeResultOperator);

            return includeResultOperator;
        }

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);
        }
    }
}
