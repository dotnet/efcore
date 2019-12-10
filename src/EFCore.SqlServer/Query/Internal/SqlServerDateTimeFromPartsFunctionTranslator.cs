// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
                ? _sqlExpressionFactory.Function(
                    "DATETIMEFROMPARTS",
                    arguments.Skip(1),
                    _methodInfo.ReturnType,
                    _sqlExpressionFactory.FindMapping(typeof(DateTime)))
                : null;
        }
    }
}

//
