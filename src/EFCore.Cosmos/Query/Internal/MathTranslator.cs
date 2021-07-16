// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class MathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethodTranslations = new()
        {
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) }), "ABS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) }), "ABS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) }), "ABS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) }), "ABS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) }), "ABS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) }), "ABS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) }), "ABS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) }), "CEILING" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) }), "CEILING" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) }), "FLOOR" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) }), "FLOOR" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) }), "POWER" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) }), "EXP" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) }), "LOG10" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Log), new[] { typeof(double) }), "LOG" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) }), "LOG" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) }), "SQRT" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) }), "ACOS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) }), "ASIN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) }), "ATAN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) }), "ATN2" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) }), "COS" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) }), "SIN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) }), "TAN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) }), "SIGN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) }), "SIGN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) }), "SIGN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) }), "SIGN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) }), "SIGN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) }), "SIGN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) }), "SIGN" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) }), "TRUNC" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) }), "TRUNC" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) }), "ROUND" },
            { typeof(Math).GetRequiredRuntimeMethod(nameof(Math.Round), new[] { typeof(double) }), "ROUND" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Abs), new[] { typeof(float) }), "ABS" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Ceiling), new[] { typeof(float) }), "CEILING" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Floor), new[] { typeof(float) }), "FLOOR" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Pow), new[] { typeof(float), typeof(float) }), "POWER" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Exp), new[] { typeof(float) }), "EXP" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Log10), new[] { typeof(float) }), "LOG10" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Log), new[] { typeof(float) }), "LOG" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Log), new[] { typeof(float), typeof(float) }), "LOG" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Sqrt), new[] { typeof(float) }), "SQRT" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Acos), new[] { typeof(float) }), "ACOS" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Asin), new[] { typeof(float) }), "ASIN" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Atan), new[] { typeof(float) }), "ATAN" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Atan2), new[] { typeof(float), typeof(float) }), "ATN2" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Cos), new[] { typeof(float) }), "COS" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Sin), new[] { typeof(float) }), "SIN" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Tan), new[] { typeof(float) }), "TAN" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Sign), new[] { typeof(float) }), "SIGN" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Truncate), new[] { typeof(float) }), "TRUNC" },
            { typeof(MathF).GetRequiredRuntimeMethod(nameof(MathF.Round), new[] { typeof(float) }), "ROUND" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public MathTranslator(ISqlExpressionFactory sqlExpressionFactory)
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

            if (_supportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
            {
                var typeMapping = arguments.Count == 1
                    ? ExpressionExtensions.InferTypeMapping(arguments[0])
                    : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

                var newArguments = arguments.Select(e => _sqlExpressionFactory.ApplyTypeMapping(e, typeMapping!));

                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    newArguments,
                    method.ReturnType,
                    typeMapping);
            }

            return null;
        }
    }
}
