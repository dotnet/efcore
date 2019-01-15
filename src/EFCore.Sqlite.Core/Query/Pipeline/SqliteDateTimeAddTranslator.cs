// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteDateTimeAddTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _addMilliseconds
            = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) });

        private static readonly MethodInfo _addTicks
            = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddTicks), new[] { typeof(long) });

        private readonly Dictionary<MethodInfo, string> _methodInfoToUnitSuffix = new Dictionary<MethodInfo, string>
        {
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) }), " years" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) }), " months" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) }), " days" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) }), " hours" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) }), " minutes" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) }), " seconds" }
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;
        private readonly RelationalTypeMapping _stringTypeMapping;
        private readonly RelationalTypeMapping _doubleTypeMapping;

        public SqliteDateTimeAddTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
            _doubleTypeMapping = _typeMappingSource.FindMapping(typeof(double));
            _stringTypeMapping = _typeMappingSource.FindMapping(typeof(string));
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            SqlExpression modifier = null;
            if (_addMilliseconds.Equals(method))
            {
                var argument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[0], _doubleTypeMapping);

                modifier = new SqlBinaryExpression(
                    ExpressionType.Add,
                    new SqlCastExpression(
                        new SqlBinaryExpression(
                            ExpressionType.Divide,
                            argument,
                            new SqlConstantExpression(Expression.Constant(1000), _typeMappingSource.FindMapping(typeof(int))),
                            typeof(double),
                            _doubleTypeMapping),
                        typeof(string),
                        _stringTypeMapping),
                    new SqlConstantExpression(Expression.Constant(" seconds"), _stringTypeMapping),
                    typeof(string),
                    _stringTypeMapping);
            }
            else if (_addTicks.Equals(method))
            {
                var argument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    arguments[0], _typeMappingSource.FindMapping(arguments[0].Type));

                modifier = new SqlBinaryExpression(
                    ExpressionType.Add,
                    new SqlCastExpression(
                        new SqlBinaryExpression(
                            ExpressionType.Divide,
                            argument,
                            new SqlConstantExpression(Expression.Constant((double)TimeSpan.TicksPerDay), _doubleTypeMapping),
                            typeof(double),
                            _doubleTypeMapping),
                        typeof(string),
                        _stringTypeMapping),
                    new SqlConstantExpression(Expression.Constant(" seconds"), _stringTypeMapping),
                    typeof(string),
                    _stringTypeMapping);
            }
            else if (_methodInfoToUnitSuffix.TryGetValue(method, out var unitSuffix))
            {
                var argument = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                    arguments[0], _typeMappingSource.FindMapping(arguments[0].Type));

                modifier = new SqlBinaryExpression(
                    ExpressionType.Add,
                    new SqlCastExpression(
                        argument,
                        typeof(string),
                        _stringTypeMapping),
                    new SqlConstantExpression(Expression.Constant(unitSuffix), _stringTypeMapping),
                    typeof(string),
                    _stringTypeMapping);
            }

            if (modifier != null)
            {
                var typeMapping = _typeMappingSource.FindMapping(instance.Type);
                instance = _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(instance, typeMapping);

                return new SqlFunctionExpression(
                    "rtrim",
                    new SqlExpression[]
                    {
                        new SqlFunctionExpression(
                            "rtrim",
                            new SqlExpression[]
                            {
                                SqliteExpression.Strftime(
                                    method.ReturnType,
                                    typeMapping,
                                    CreateStringConstant("%Y-%m-%d %H:%M:%f"),
                                    instance,
                                    new [] { modifier }),
                                CreateStringConstant("0")
                            },
                            method.ReturnType,
                            typeMapping,
                            false),
                        CreateStringConstant(".")
                    },
                    method.ReturnType,
                    typeMapping,
                    false);
            }

            return null;
        }


        private SqlConstantExpression CreateStringConstant(string value)
        {
            return new SqlConstantExpression(Expression.Constant(value), _stringTypeMapping);
        }
    }
}
