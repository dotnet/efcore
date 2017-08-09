// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class IncludeExpressionNodeBase : ResultOperatorExpressionNodeBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Type SourceEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual LambdaExpression NavigationPropertyPathLambda { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected IncludeExpressionNodeBase(
            MethodCallExpressionParseInfo parseInfo,
            [NotNull] LambdaExpression navigationPropertyPathLambda)
            : base(parseInfo, null, null)
        {
            NavigationPropertyPathLambda = navigationPropertyPathLambda;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
            => Source.Resolve(
                inputParameter,
                expressionToBeResolved,
                clauseGenerationContext);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected static IReadOnlyList<PropertyInfo> MatchIncludeLambdaPropertyAccess([NotNull] LambdaExpression includeLambda)
        {
            Check.NotNull(includeLambda, nameof(includeLambda));

            var parameterExpression = includeLambda.Parameters.Single();
            var propertyInfos = new List<PropertyInfo>();

            var expression = includeLambda.Body;
            while (expression != null)
            {
                expression = expression.RemoveConvert();
                if (expression == parameterExpression)
                {
                    break;
                }

                if (expression is MemberExpression memberExpression)
                {
                    propertyInfos.Insert(0, (PropertyInfo)memberExpression.Member);
                    expression = memberExpression.Expression;

                    continue;
                }

                propertyInfos.Clear();
                break;
            }

            if (propertyInfos.Count == 0)
            {
                throw new ArgumentException(
                    CoreStrings.InvalidIncludeLambdaExpression(includeLambda));
            }

            return propertyInfos;
        }
    }
}
