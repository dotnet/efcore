// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class EqualsTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public EqualsTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            SqlExpression left = null;
            SqlExpression right = null;

            if (method.Name == nameof(object.Equals)
                && instance != null
                && arguments.Count == 1)
            {
                left = instance;
                right = RemoveObjectConvert(arguments[0]);
            }
            else if (instance == null
                && method.Name == nameof(object.Equals)
                && arguments.Count == 2)
            {
                left = RemoveObjectConvert(arguments[0]);
                right = RemoveObjectConvert(arguments[1]);
            }

            if (left != null
                && right != null)
            {
                if (left.Type.UnwrapNullableType() == right.Type.UnwrapNullableType())
                {
                    return _sqlExpressionFactory.Equal(left, right);
                }

                return _sqlExpressionFactory.Constant(false);
            }

            return null;
        }

        private SqlExpression RemoveObjectConvert(SqlExpression expression)
        {
            if (expression is SqlUnaryExpression sqlUnaryExpression
                && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                && sqlUnaryExpression.Type == typeof(object))
            {
                return sqlUnaryExpression.Operand;
            }

            return expression;
        }
    }
}
