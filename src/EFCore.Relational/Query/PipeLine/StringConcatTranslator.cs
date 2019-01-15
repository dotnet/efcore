// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class StringConcatTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _stringConcatObjectMethodInfo
            = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) });

        private static readonly MethodInfo _stringConcatStringMethodInfo
            = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (_stringConcatStringMethodInfo.Equals(method)
                || _stringConcatObjectMethodInfo.Equals(method))
            {
                var left = arguments[0];
                var right = arguments[1];

                return new SqlBinaryExpression(
                    ExpressionType.Add,
                    left,
                    right,
                    typeof(string),
                    null);
            }

            return null;
        }
    }
}
