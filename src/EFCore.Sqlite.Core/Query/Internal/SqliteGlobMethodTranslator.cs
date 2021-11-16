// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteGlobMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo = typeof(SqliteDbFunctionsExtensions)
            .GetRequiredMethod(nameof(SqliteDbFunctionsExtensions.Glob), typeof(DbFunctions), typeof(string), typeof(string));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteGlobMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

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
            if (method.Equals(_methodInfo))
            {
                var matchExpression = arguments[1];
                var pattern = arguments[2];
                var stringTypeMapping = ExpressionExtensions.InferTypeMapping(matchExpression, pattern);

                return _sqlExpressionFactory.Function(
                    "glob",
                    new[]
                    {
                        _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping),
                        _sqlExpressionFactory.ApplyTypeMapping(matchExpression, stringTypeMapping)
                    },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    typeof(bool));
            }

            return null;
        }
    }
}
