// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerFromPartsFunctionTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _dateFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateFromParts), typeof(DbFunctions), typeof(int), typeof(int), typeof(int));

        private static readonly MethodInfo _dateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateTimeFromParts), typeof(DbFunctions), typeof(int), typeof(int), typeof(int),
                typeof(int), typeof(int), typeof(int), typeof(int));

        private static readonly MethodInfo _dateTime2FromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateTime2FromParts), typeof(DbFunctions), typeof(int), typeof(int), typeof(int),
                typeof(int), typeof(int), typeof(int), typeof(int), typeof(int));

        private static readonly MethodInfo _dateTimeOffsetFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateTimeOffsetFromParts), typeof(DbFunctions), typeof(int), typeof(int), typeof(int),
                typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int));

        private static readonly MethodInfo _smallDateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.SmallDateTimeFromParts), typeof(DbFunctions), typeof(int), typeof(int), typeof(int),
                typeof(int), typeof(int));

        private static readonly MethodInfo _timeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.TimeFromParts), typeof(DbFunctions), typeof(int), typeof(int), typeof(int),
                typeof(int), typeof(int));

        private static readonly IDictionary<MethodInfo, (string FunctionName, string ReturnType)> _methodFunctionMapping
            = new Dictionary<MethodInfo, (string, string)>
            {
                { _dateFromPartsMethodInfo, ("DATEFROMPARTS", "date") },
                { _dateTimeFromPartsMethodInfo, ("DATETIMEFROMPARTS", "datetime") },
                { _dateTime2FromPartsMethodInfo, ("DATETIME2FROMPARTS", "datetime2") },
                { _dateTimeOffsetFromPartsMethodInfo, ("DATETIMEOFFSETFROMPARTS", "datetimeoffset") },
                { _smallDateTimeFromPartsMethodInfo, ("SMALLDATETIMEFROMPARTS", "smalldatetime") },
                { _timeFromPartsMethodInfo, ("TIMEFROMPARTS", "time") },
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerFromPartsFunctionTranslator(
            ISqlExpressionFactory sqlExpressionFactory,
            IRelationalTypeMappingSource typeMappingSource)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (_methodFunctionMapping.TryGetValue(method, out var value))
            {
                return _sqlExpressionFactory.Function(
                    value.FunctionName,
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: arguments.Skip(1).Select(a => true),
                    _dateFromPartsMethodInfo.ReturnType,
                    _typeMappingSource.FindMapping(_dateFromPartsMethodInfo.ReturnType, value.ReturnType));
            }

            return null;
        }
    }
}
