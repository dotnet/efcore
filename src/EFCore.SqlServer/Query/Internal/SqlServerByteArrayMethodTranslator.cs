// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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
                var source = arguments[0];
                var sourceTypeMapping = source.TypeMapping;

                var value = arguments[1] is SqlConstantExpression constantValue
                    ? (SqlExpression)_sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value }, sourceTypeMapping)
                    : _sqlExpressionFactory.Convert(arguments[1], typeof(byte[]), sourceTypeMapping);

                return _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.Function("CHARINDEX", new[] { value, source }, typeof(int)),
                    _sqlExpressionFactory.Constant(0));
            }

            return null;
        }
    }
}
