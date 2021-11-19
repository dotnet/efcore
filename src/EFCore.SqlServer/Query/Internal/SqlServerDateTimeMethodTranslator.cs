// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
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
    public class SqlServerDateTimeMethodTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDatePartMapping = new()
        {
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddYears), typeof(int)), "year" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMonths), typeof(int)), "month" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddDays), typeof(double)), "day" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddHours), typeof(double)), "hour" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMinutes), typeof(double)), "minute" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddSeconds), typeof(double)), "second" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMilliseconds), typeof(double)), "millisecond" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddYears), typeof(int)), "year" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddMonths), typeof(int)), "month" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddDays), typeof(double)), "day" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddHours), typeof(double)), "hour" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddMinutes), typeof(double)), "minute" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddSeconds), typeof(double)), "second" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), typeof(double)), "millisecond" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerDateTimeMethodTranslator(
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
            if (_methodInfoDatePartMapping.TryGetValue(method, out var datePart)
                && instance != null)
            {
                // DateAdd does not accept number argument outside of int range
                // AddYears/AddMonths take int argument so no need to check for range
                if (datePart != "year"
                    && datePart != "month"
                    && arguments[0] is SqlConstantExpression sqlConstant
                    && sqlConstant.Value is double doubleValue
                    && (doubleValue >= int.MaxValue
                        || doubleValue <= int.MinValue))
                {
                    return null;
                }

                if (instance is SqlConstantExpression instanceConstant)
                {
                    instance = instanceConstant.ApplyTypeMapping(_typeMappingSource.FindMapping(typeof(DateTime), "datetime"));
                }

                return _sqlExpressionFactory.Function(
                    "DATEADD",
                    new[]
                    {
                        _sqlExpressionFactory.Fragment(datePart),
                        _sqlExpressionFactory.Convert(arguments[0], typeof(int)),
                        instance
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true, true },
                    instance.Type,
                    instance.TypeMapping);
            }

            return null;
        }
    }
}
