// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
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

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;
        private readonly RelationalTypeMapping _intTypeMapping;

        public SqlServerMathTranslator(IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
            _intTypeMapping = _typeMappingSource.FindMapping(typeof(int));
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (_supportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
            {
                var typeMapping = (arguments.Count == 1
                    ? ExpressionExtensions.InferTypeMapping(arguments[0])
                    : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]))
                    ?? _typeMappingSource.FindMapping(arguments[0].Type);

                var newArguments = new SqlExpression[arguments.Count];
                newArguments[0] = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[0], typeMapping);

                if (arguments.Count == 2)
                {
                    newArguments[1] = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[1], typeMapping);
                }

                return new SqlFunctionExpression(
                    sqlFunctionName,
                    newArguments,
                    method.ReturnType,
                    sqlFunctionName == "SIGN" ? _intTypeMapping : typeMapping,
                    false);
            }

            if (_truncateMethodInfos.Contains(method))
            {
                var argument = arguments[0];
                var typeMapping = ExpressionExtensions.InferTypeMapping(argument) ?? _typeMappingSource.FindMapping(arguments[0].Type);
                argument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(argument, typeMapping);

                return new SqlFunctionExpression(
                    "ROUND",
                    new[] {
                            argument,
                            MakeSqlConstant(0),
                            MakeSqlConstant(1)
                    },
                    method.ReturnType,
                    typeMapping,
                    false);
            }

            if (_roundMethodInfos.Contains(method))
            {
                var argument = arguments[0];
                var typeMapping = ExpressionExtensions.InferTypeMapping(argument) ?? _typeMappingSource.FindMapping(arguments[0].Type);
                argument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(argument, typeMapping);

                var digits = arguments.Count == 2
                    ? _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[1], _intTypeMapping)
                    : MakeSqlConstant(0);

                return new SqlFunctionExpression(
                    "ROUND",
                    new[] { argument, digits },
                    method.ReturnType,
                    typeMapping,
                    false);
            }

            return null;
        }

        private SqlExpression MakeSqlConstant(int value)
        {
            return new SqlConstantExpression(Expression.Constant(value), _intTypeMapping);
        }
    }
}
