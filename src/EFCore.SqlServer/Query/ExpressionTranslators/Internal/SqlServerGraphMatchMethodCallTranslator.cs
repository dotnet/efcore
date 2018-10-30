// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerGraphMatchMethodCallTranslator : IMethodCallTranslator
    {
        private const string MatchFunctionName = "MATCH";

        private static readonly MethodInfo _matchMethodInfo
            = typeof(SqlServerDbFunctionsExtensions)
                .GetRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.Match),
                    new[] { typeof(DbFunctions), typeof(object), typeof(object), typeof(object) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (Equals(methodCallExpression.Method, _matchMethodInfo))
            {
                ValidateGraphReference(methodCallExpression.Arguments[1], true);
                ValidateGraphReference(methodCallExpression.Arguments[2], false);
                ValidateGraphReference(methodCallExpression.Arguments[3], true);

                return new SqlFunctionExpression(
                    MatchFunctionName,
                    typeof(bool),
                    new[]
                    {
                        methodCallExpression.Arguments[1],
                        methodCallExpression.Arguments[2],
                        methodCallExpression.Arguments[3]
                    });
            }

            return null;
        }

        private static void ValidateGraphReference(Expression expression, bool node)
        {
            expression = expression.RemoveConvert();

            if (!(expression is QuerySourceReferenceExpression))
            {
                throw new InvalidOperationException(SqlServerStrings.ParameterIsNotAnEntity(node ? "node" : "edge"));
            }
        }
    }
}
