// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerByteArrayMethodTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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

                var pattern = arguments[1] is SqlConstantExpression constantExpression
                    ? (SqlExpression)_sqlExpressionFactory.Constant(new[] { (byte)constantExpression.Value }, typeMapping)
                    : _sqlExpressionFactory.Convert(arguments[1], typeof(byte[]), typeMapping);

                return _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function(
                        "CHARINDEX",
                        new[] { pattern, instance },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));
            }

            return null;
        }
    }
}
