// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerDateTimeFromPartsFunctionTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        private static readonly MethodInfo _methodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.DateTimeFromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        public SqlServerDateTimeFromPartsFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            return _methodInfo.Equals(method)
                ? _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function(
                        "DATETIMEFROMPARTS",
                        new[] { arguments[1], arguments[2], arguments[3], arguments[4], arguments[5], arguments[6], arguments[7] },
                        _methodInfo.ReturnType),
                    _methodInfo.ReturnType)
                : null;
        }
    }
}

//
