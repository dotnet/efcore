// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class ComparisonTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public ComparisonTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method.ReturnType == typeof(int))
            {
                SqlExpression left = null;
                SqlExpression right = null;
                if (method.Name == nameof(string.Compare)
                    && arguments.Count == 2
                    && arguments[0].Type.UnwrapNullableType() == arguments[1].Type.UnwrapNullableType())
                {
                    left = arguments[0];
                    right = arguments[1];
                }
                else if (method.Name == nameof(string.CompareTo)
                    && arguments.Count == 1
                    && instance != null
                    && instance.Type.UnwrapNullableType() == arguments[0].Type.UnwrapNullableType())
                {
                    left = instance;
                    right = arguments[0];
                }

                if (left != null && right != null)
                {
                    return _sqlExpressionFactory.Case(
                        new CaseWhenClause[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(left, right), _sqlExpressionFactory.Constant(0)),
                            new CaseWhenClause(
                                _sqlExpressionFactory.GreaterThan(left, right), _sqlExpressionFactory.Constant(1)),
                            new CaseWhenClause(
                                _sqlExpressionFactory.LessThan(left, right), _sqlExpressionFactory.Constant(-1)),
                        },
                        null);
                }
            }

            return null;
        }
    }
}
