// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Pipeline
{
    public class SqlServerConvertTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _typeMapping = new Dictionary<string, string>
        {
            [nameof(Convert.ToByte)] = "tinyint",
            [nameof(Convert.ToDecimal)] = "decimal(18, 2)",
            [nameof(Convert.ToDouble)] = "float",
            [nameof(Convert.ToInt16)] = "smallint",
            [nameof(Convert.ToInt32)] = "int",
            [nameof(Convert.ToInt64)] = "bigint",
            [nameof(Convert.ToString)] = "nvarchar(max)"
        };

        private static readonly List<Type> _supportedTypes = new List<Type>
        {
            typeof(byte),
            typeof(DateTime),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        private static readonly IEnumerable<MethodInfo> _supportedMethods
            = _typeMapping.Keys
                .SelectMany(
                    t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                        .Where(
                            m => m.GetParameters().Length == 1
                                 && _supportedTypes.Contains(m.GetParameters().First().ParameterType)));

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ITypeMappingApplyingExpressionVisitor _typeMappingApplyingExpressionVisitor;

        public SqlServerConvertTranslator(IRelationalTypeMappingSource typeMappingSource,
            ITypeMappingApplyingExpressionVisitor typeMappingApplyingExpressionVisitor)
        {
            _typeMappingSource = typeMappingSource;
            _typeMappingApplyingExpressionVisitor = typeMappingApplyingExpressionVisitor;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IList<SqlExpression> arguments)
        {
            return _supportedMethods.Contains(method)
                ? new SqlFunctionExpression(
                    "CONVERT",
                    new[]
                    {
                        new SqlFragmentExpression(_typeMapping[method.Name]),
                        _typeMappingApplyingExpressionVisitor.ApplyTypeMapping(
                            arguments[0], _typeMappingSource.FindMapping(arguments[0].Type))
                    },
                    method.ReturnType,
                    _typeMappingSource.FindMapping(method.ReturnType),
                    false)
                : null;
        }
    }
}
