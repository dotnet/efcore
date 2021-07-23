﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteRandomTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo = typeof(DbFunctionsExtensions).GetRequiredMethod(
            nameof(DbFunctionsExtensions.Random), typeof(DbFunctions));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteRandomTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            // Issue #15586: Query: TypeCompatibility chart for inference.
            return _methodInfo.Equals(method)
                ? _sqlExpressionFactory.Function(
                    "abs",
                    new SqlExpression[]
                    {
                        _sqlExpressionFactory.Divide(
                            _sqlExpressionFactory.Function(
                                "random",
                                Array.Empty<SqlExpression>(),
                                nullable: false,
                                argumentsPropagateNullability: Array.Empty<bool>(),
                                method.ReturnType),
                            _sqlExpressionFactory.Constant(9223372036854780000.0))
                    },
                    nullable: false,
                    argumentsPropagateNullability: Array.Empty<bool>(),
                    method.ReturnType)
                : null;
        }
    }
}
