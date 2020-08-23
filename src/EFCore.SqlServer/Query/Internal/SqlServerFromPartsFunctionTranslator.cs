// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

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
            .GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateFromParts),
                new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _dateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateTimeFromParts),
                new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _dateTime2FromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateTime2FromParts),
                new[]
                {
                    typeof(DbFunctions),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int)
                });

        private static readonly MethodInfo _dateTimeOffsetFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.DateTimeOffsetFromParts),
                new[]
                {
                    typeof(DbFunctions),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int)
                });

        private static readonly MethodInfo _smallDateTimeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.SmallDateTimeFromParts),
                new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

        private static readonly MethodInfo _timeFromPartsMethodInfo = typeof(SqlServerDbFunctionsExtensions)
            .GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.TimeFromParts),
                new[] { typeof(DbFunctions), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

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
            [NotNull] ISqlExpressionFactory sqlExpressionFactory,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
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
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

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
