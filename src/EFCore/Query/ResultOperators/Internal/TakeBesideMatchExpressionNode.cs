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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class TakeBesideMatchExpressionNode : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly IReadOnlyCollection<MethodInfo> SupportedMethods = new[]
        {
            EntityFrameworkQueryableExtensions.TakeAfterMatchMethodInfo,
            EntityFrameworkQueryableExtensions.TakeBeforeMatchMethodInfo
        };

        private readonly LambdaExpression _lambdaExpression;

        private readonly Expression _count;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public TakeBesideMatchExpressionNode(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] LambdaExpression predicateLambda,
            [NotNull] Expression count)
            : base(parseInfo, predicateLambda, null)
        {
            _count = count;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            var takeBesideMatchOperator = new TakeBesideMatchResultOperator(
                forward: ParsedExpression.Method.GetGenericMethodDefinition()
                    .Equals(EntityFrameworkQueryableExtensions.TakeAfterMatchMethodInfo)
                );
            // etc...
            return takeBesideMatchOperator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
            => Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
    }
}
