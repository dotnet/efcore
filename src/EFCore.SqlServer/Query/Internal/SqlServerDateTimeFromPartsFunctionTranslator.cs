// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerDateTimeFromPartsFunctionTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        private static readonly MethodInfo _methodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.DateTimeFromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        public SqlServerDateTimeFromPartsFunctionTranslator(
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            return _methodInfo.Equals(method)
                ? _sqlExpressionFactory.Function(
                    "DATETIMEFROMPARTS",
                    arguments.Skip(1),
                    _methodInfo.ReturnType,
                    _typeMappingSource.FindMapping(typeof(DateTime), "datetime"))
                : null;
        }
    }
}

//
