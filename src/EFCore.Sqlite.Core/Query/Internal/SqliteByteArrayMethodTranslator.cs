// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    public class SqliteByteArrayMethodTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqliteByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method.IsGenericMethod
                && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains)
                && arguments[0].Type == typeof(byte[]))
            {
                instance = arguments[0];
                var typeMapping = instance.TypeMapping;

                var pattern = arguments[1] is SqlConstantExpression constantPattern
                    ? (SqlExpression)_sqlExpressionFactory.Constant(new[] { (byte)constantPattern.Value }, typeMapping)
                    : _sqlExpressionFactory.Function(
                        "char",
                        new[] { arguments[1] },
                        typeof(string));

                return _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function(
                        "instr",
                        new[] { instance, pattern },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));
            }

            return null;
        }
    }
}
