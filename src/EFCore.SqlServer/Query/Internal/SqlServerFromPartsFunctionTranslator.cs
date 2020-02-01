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
    public class SqlServerFromPartsFunctionTranslator : IMethodCallTranslator
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        private static readonly MethodInfo _dateFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.DateFromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _dateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.DateTimeFromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _dateTime2FromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.DateTime2FromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _dateTimeOffsetFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.DateTimeOffsetFromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _smallDateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.SmallDateTimeFromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _timeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(nameof(SqlServerDbFunctionsExtensions.TimeFromParts), new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        public SqlServerFromPartsFunctionTranslator(
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (_dateFromPartsMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "DATEFROMPARTS",
                    arguments.Skip(1),
                    nullResultAllowed: true,
                    argumentsPropagateNullability: arguments.Skip(1).Select(a => true),
                    _dateFromPartsMethodInfo.ReturnType,
                    _typeMappingSource.FindMapping(typeof(DateTime), "date"));
            }

            if (_dateTimeFromPartsMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "DATETIMEFROMPARTS",
                    arguments.Skip(1),
                    nullResultAllowed: true,
                    argumentsPropagateNullability: arguments.Skip(1).Select(a => true),
                    _dateTimeFromPartsMethodInfo.ReturnType,
                    _typeMappingSource.FindMapping(typeof(DateTime), "datetime"));
            }

            if (_dateTime2FromPartsMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "DATETIME2FROMPARTS",
                    arguments.Skip(1),
                    nullResultAllowed: true,
                    argumentsPropagateNullability: arguments.Skip(1).Select(a => true),
                    _dateTime2FromPartsMethodInfo.ReturnType,
                    _typeMappingSource.FindMapping(typeof(DateTime), "datetime2"));
            }

            if (_dateTimeOffsetFromPartsMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "DATETIMEOFFSETFROMPARTS",
                    arguments.Skip(1),
                    nullResultAllowed: true,
                    argumentsPropagateNullability: arguments.Skip(1).Select(a => true),
                    _dateTimeOffsetFromPartsMethodInfo.ReturnType,
                    _typeMappingSource.FindMapping(typeof(DateTimeOffset), "datetimeoffset"));
            }

            if (_smallDateTimeFromPartsMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "SMALLDATETIMEFROMPARTS",
                    arguments.Skip(1),
                    nullResultAllowed: true,
                    argumentsPropagateNullability: arguments.Skip(1).Select(a => true),
                    _smallDateTimeFromPartsMethodInfo.ReturnType,
                    _typeMappingSource.FindMapping(typeof(DateTime), "smalldatetime"));
            }

            if (_timeFromPartsMethodInfo.Equals(method))
            {
                return _sqlExpressionFactory.Function(
                    "TIMEFROMPARTS",
                    arguments.Skip(1),
                    nullResultAllowed: true,
                    argumentsPropagateNullability: arguments.Skip(1).Select(a => true),
                    _timeFromPartsMethodInfo.ReturnType,
                    _typeMappingSource.FindMapping(typeof(TimeSpan), "time"));
            }

            return null;
        }
    }
}
