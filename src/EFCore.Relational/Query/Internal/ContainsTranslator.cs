// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class ContainsTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public ContainsTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains)
                && ValidateValues(arguments[0]))
            {
                return _sqlExpressionFactory.In(RemoveObjectConvert(arguments[1]), arguments[0], negated: false);
            }

            if (arguments.Count == 1
                && method.IsContainsMethod()
                && ValidateValues(instance))
            {
                return _sqlExpressionFactory.In(RemoveObjectConvert(arguments[0]), instance, negated: false);
            }

            return null;
        }

        private bool ValidateValues(SqlExpression values)
            => values is SqlConstantExpression || values is SqlParameterExpression;

        private SqlExpression RemoveObjectConvert(SqlExpression expression)
            => expression is SqlUnaryExpression sqlUnaryExpression
                && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                && sqlUnaryExpression.Type == typeof(object)
                ? sqlUnaryExpression.Operand
                : expression;
    }
}
