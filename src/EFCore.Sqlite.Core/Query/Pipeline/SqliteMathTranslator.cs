// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    public class SqliteMathTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<MethodInfo, string> _supportedMethods = new Dictionary<MethodInfo, string>
        {
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(float) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(int) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(long) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(sbyte) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(short) }), "abs" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(byte), typeof(byte) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(sbyte), typeof(sbyte) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(uint), typeof(uint) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(ushort), typeof(ushort) }), "max" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(byte), typeof(byte) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(sbyte), typeof(sbyte) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(uint), typeof(uint) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(ushort), typeof(ushort) }), "min" },
            { typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double) }), "round" },
            { typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) }), "round" }
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public SqliteMathTranslator(IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            if (_supportedMethods.TryGetValue(method, out var sqlFunctionName))
            {
                RelationalTypeMapping typeMapping = null;
                var newArguments = new List<SqlExpression>();

                if (string.Equals(method.Name, nameof(Math.Round)))
                {
                    typeMapping = _typeMappingSource.FindMapping(typeof(double));
                    newArguments.Add(_typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[0], typeMapping));
                    if (arguments.Count == 2)
                    {
                        newArguments.Add(
                            _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                                arguments[1], _typeMappingSource.FindMapping(typeof(int))));
                    }
                }
                else
                {
                    typeMapping = (arguments.Count == 1
                        ? ExpressionExtensions.InferTypeMapping(arguments[0])
                        : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]))
                        ?? _typeMappingSource.FindMapping(arguments[0].Type);

                    newArguments.Add(_typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[0], typeMapping));
                    if (arguments.Count == 2)
                    {
                        newArguments.Add(_typeMappingApplyingExpressionVisitor.ApplyTypeMapping(arguments[1], typeMapping));
                    }
                }

                return new SqlFunctionExpression(
                    sqlFunctionName,
                    newArguments,
                    method.ReturnType,
                    typeMapping,
                    false);
            }

            return null;
        }
    }
}
