// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal
{
    public class IncludeExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly IReadOnlyCollection<MethodInfo> SupportedMethods = new[]
        {
            EntityFrameworkQueryableExtensions.IncludeMethodInfo
        };

        private readonly LambdaExpression _navigationPropertyPathLambda;

        public IncludeExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] LambdaExpression navigationPropertyPathLambda)
            : base(parseInfo, null, null)
        {
            _navigationPropertyPathLambda = navigationPropertyPathLambda;
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            var navigationPropertyPath
                = (MemberExpression)Source.Resolve(
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
            => Source.Resolve(
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);
    }
}
