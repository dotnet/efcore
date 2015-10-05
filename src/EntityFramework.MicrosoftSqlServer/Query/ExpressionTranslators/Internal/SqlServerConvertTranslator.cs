// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTranslators.Internal
{
    public class SqlServerConvertTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, DbType> _typeMapping = new Dictionary<string, DbType>
            {
                [nameof(Convert.ToByte)] = DbType.Byte,
                [nameof(Convert.ToDecimal)] = DbType.Decimal,
                [nameof(Convert.ToDouble)] = DbType.Double,
                [nameof(Convert.ToInt16)] = DbType.Int16,
                [nameof(Convert.ToInt32)] = DbType.Int32,
                [nameof(Convert.ToInt64)] = DbType.Int64,
                [nameof(Convert.ToString)] = DbType.String
            };

        private static readonly List<Type> _supportedTypes = new List<Type>
            {
                typeof(bool),
                typeof(byte),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(int),
                typeof(long),
                typeof(short),
                typeof(string)
            };

        private static readonly IEnumerable<MethodInfo> _supportedMethods;

        static SqlServerConvertTranslator()
        {
            _supportedMethods = _typeMapping.Keys
                .SelectMany(t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(m => m.GetParameters().Count() == 1
                                && _supportedTypes.Contains(m.GetParameters().First().ParameterType)));
        }

        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
            => _supportedMethods.Contains(methodCallExpression.Method)
                ? new SqlFunctionExpression(
                    "CONVERT",
                    methodCallExpression.Type,
                    new[]
                        {
                            Expression.Constant(
                                _typeMapping[methodCallExpression.Method.Name]),
                            methodCallExpression.Arguments[0]
                        })
                : null;
    }
}
