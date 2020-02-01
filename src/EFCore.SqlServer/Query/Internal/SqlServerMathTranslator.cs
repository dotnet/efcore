// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerMathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethodTranslations = new Dictionary<MethodInfo, string>
        {
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) }), "CEILING" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) }), "CEILING" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) }), "FLOOR" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) }), "FLOOR" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) }), "POWER" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) }), "EXP" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) }), "LOG10" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) }), "LOG" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) }), "LOG" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) }), "SQRT" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) }), "ACOS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) }), "ASIN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) }), "ATAN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) }), "ATN2" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) }), "COS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) }), "SIN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) }), "TAN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) }), "SIGN" }
        };

        private static readonly IEnumerable<MethodInfo> _truncateMethodInfos = new[]
        {
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) })
        };

        private static readonly IEnumerable<MethodInfo> _roundMethodInfos = new[]
        {
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerMathTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (_supportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
            {
                var typeMapping = arguments.Count == 1
                    ? ExpressionExtensions.InferTypeMapping(arguments[0])
                    : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

                var newArguments = new SqlExpression[arguments.Count];
                newArguments[0] = _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);

                if (arguments.Count == 2)
                {
                    newArguments[1] = _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);
                }

                return _sqlExpressionFactory.Function(
                    sqlFunctionName,
                    newArguments,
                    nullResultAllowed: true,
                    argumentsPropagateNullability: newArguments.Select(a => true).ToArray(),
                    method.ReturnType,
                    sqlFunctionName == "SIGN" ? null : typeMapping);
            }

            if (_truncateMethodInfos.Contains(method))
            {
                var argument = arguments[0];

                return _sqlExpressionFactory.Function(
                    "ROUND",
                    new[] { argument, _sqlExpressionFactory.Constant(0), _sqlExpressionFactory.Constant(1) },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true, false, false },
                    method.ReturnType,
                    argument.TypeMapping);
            }

            if (_roundMethodInfos.Contains(method))
            {
                var argument = arguments[0];
                var digits = arguments.Count == 2 ? arguments[1] : _sqlExpressionFactory.Constant(0);

                return _sqlExpressionFactory.Function(
                    "ROUND",
                    new[] { argument, digits },
                    nullResultAllowed: true,
                    argumentsPropagateNullability: new[] { true, true },
                    method.ReturnType,
                    argument.TypeMapping);
            }

            return null;
        }
    }
}
