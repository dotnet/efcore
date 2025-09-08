// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGMathMethodTranslator : IMethodCallTranslator
    {
        private static readonly IDictionary<MethodInfo, (string Name, bool OnlyNullByArgs, bool ReverseArgs)> _methodToFunctionName = new Dictionary<MethodInfo, (string Name, bool OnlyNullByArgs, bool ReverseArgs)>
        {
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) }), ("ABS", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) }), ("ABS", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) }), ("ABS", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) }), ("ABS", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) }), ("ABS", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) }), ("ABS", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Abs), new[] { typeof(float) }), ("ABS", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) }), ("ACOS", false, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Acos), new[] { typeof(float) }), ("ACOS", false, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) }), ("ASIN", false, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Asin), new[] { typeof(float) }), ("ASIN", false, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) }), ("ATAN", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan), new[] { typeof(float) }), ("ATAN", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) }), ("ATAN2", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan2), new[] { typeof(float), typeof(float) }), ("ATAN2", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) }), ("CEILING", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) }), ("CEILING", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Ceiling), new[] { typeof(float) }), ("CEILING", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) }), ("COS", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Cos), new[] { typeof(float) }), ("COS", true, false) },

            { typeof(double).GetRuntimeMethod(nameof(double.DegreesToRadians), new[] { typeof(double) })!, ("RADIANS", true, false) },
            { typeof(float).GetRuntimeMethod(nameof(float.DegreesToRadians), new[] { typeof(float) })!, ("RADIANS", true, false) },
            { typeof(double).GetRuntimeMethod(nameof(double.RadiansToDegrees), new[] { typeof(double) })!, ("DEGREES", true, false) },
            { typeof(float).GetRuntimeMethod(nameof(float.RadiansToDegrees), new[] { typeof(float) })!, ("DEGREES", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) }), ("EXP", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Exp), new[] { typeof(float) }), ("EXP", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) }), ("FLOOR", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) }), ("FLOOR", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Floor), new[] { typeof(float) }), ("FLOOR", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) }), ("LOG", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) }), ("LOG", false, true) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float) }), ("LOG", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float), typeof(float) }), ("LOG", false, true) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) }), ("LOG10", false, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log10), new[] { typeof(float) }), ("LOG10", false, false) },

            // GREATEST is handled in XGSqlTranslatingExpressionVisitor.GenerateGreatest():
            // LEAST is handled in XGSqlTranslatingExpressionVisitor.GenerateLeast():

            { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) }), ("POWER", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Pow), new[] { typeof(float), typeof(float) }), ("POWER", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) }), ("ROUND", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) }), ("ROUND", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) }), ("ROUND", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) }), ("ROUND", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float) }), ("ROUND", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float), typeof(int) }), ("ROUND", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) }), ("SIGN", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) }), ("SIGN", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) }), ("SIGN", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) }), ("SIGN", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) }), ("SIGN", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) }), ("SIGN", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) }), ("SIGN", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sign), new[] { typeof(float) }), ("SIGN", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) }), ("SIN", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sin), new[] { typeof(float) }), ("SIN", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) }), ("SQRT", false, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sqrt), new[] { typeof(float) }), ("SQRT", false, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) }), ("TAN", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Tan), new[] { typeof(float) }), ("TAN", true, false) },

            { typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) }), ("TRUNCATE", true, false) },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) }), ("TRUNCATE", true, false) },
            { typeof(MathF).GetRuntimeMethod(nameof(MathF.Truncate), new[] { typeof(float) }), ("TRUNCATE", true, false) },
        };

        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGMathMethodTranslator(XGSqlExpressionFactory sqlExpressionFactory)
            => _sqlExpressionFactory = sqlExpressionFactory;

        /// <inheritdoc />
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (_methodToFunctionName.TryGetValue(method, out var mapping))
            {
                var targetArgumentsCount = arguments.Count;

                if (mapping.Name == "TRUNCATE")
                {
                    targetArgumentsCount = 2;
                }

                Debug.Assert(targetArgumentsCount is >= 1 and <= 2);

                var newArguments = new SqlExpression[targetArgumentsCount];
                newArguments[0] = arguments[0];

                if (targetArgumentsCount == 2)
                {
                    newArguments[1] = arguments.Count == 2
                        ? arguments[1]
                        : _sqlExpressionFactory.Constant(0);

                    if (mapping.ReverseArgs)
                    {
                        (newArguments[0], newArguments[1]) = (newArguments[1], newArguments[0]);
                    }
                }

                return _sqlExpressionFactory.NullableFunction(
                    mapping.Name,
                    newArguments,
                    method.ReturnType,
                    mapping.OnlyNullByArgs);
            }

            return null;
        }
    }
}
