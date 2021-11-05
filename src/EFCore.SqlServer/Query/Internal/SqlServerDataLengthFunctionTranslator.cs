// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerDataLengthFunctionTranslator : IMethodCallTranslator
    {
        private static readonly List<string> _longReturningTypes = new()
        {
            "nvarchar(max)",
            "varchar(max)",
            "varbinary(max)"
        };

        private static readonly HashSet<MethodInfo> _methodInfoDataLengthMapping
            = new()
            {
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(string)),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(bool?)),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(double?)),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(decimal?)),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(DateTime?)),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(TimeSpan?)),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(DateTimeOffset?)),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(byte[])),
                typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                    nameof(SqlServerDbFunctionsExtensions.DataLength), typeof(DbFunctions), typeof(Guid?))
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerDataLengthFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
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
            if (_methodInfoDataLengthMapping.Contains(method))
            {
                var argument = arguments[1];
                if (argument.TypeMapping == null)
                {
                    argument = _sqlExpressionFactory.ApplyDefaultTypeMapping(argument);
                }

                if (_longReturningTypes.Contains(argument.TypeMapping!.StoreType))
                {
                    var result = _sqlExpressionFactory.Function(
                        "DATALENGTH",
                        arguments.Skip(1),
                        nullable: true,
                        argumentsPropagateNullability: new[] { true },
                        typeof(long));

                    return _sqlExpressionFactory.Convert(result, method.ReturnType.UnwrapNullableType());
                }

                return _sqlExpressionFactory.Function(
                    "DATALENGTH",
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    method.ReturnType.UnwrapNullableType());
            }

            return null;
        }
    }
}
