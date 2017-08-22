// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class OracleConvertTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _typeMapping = new Dictionary<string, string>
        {
            [nameof(Convert.ToByte)] = "NUMBER(3)",
            [nameof(Convert.ToDecimal)] = "DECIMAL(29,4)",
            [nameof(Convert.ToDouble)] = "NUMBER",
            [nameof(Convert.ToInt16)] = "NUMBER(6)",
            [nameof(Convert.ToInt32)] = "NUMBER(10)",
            [nameof(Convert.ToInt64)] = "NUMBER(19)",
            [nameof(Convert.ToString)] = "NVARCHAR2(2000)"
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

        private static readonly IEnumerable<MethodInfo> _supportedMethods
            = _typeMapping.Keys
                .SelectMany(
                    t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                        .Where(
                            m => m.GetParameters().Length == 1
                                 && _supportedTypes.Contains(m.GetParameters().First().ParameterType)));

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
            => _supportedMethods.Contains(methodCallExpression.Method)
                ? new SqlFunctionExpression(
                    "CAST",
                    methodCallExpression.Type,
                    new[]
                    {
                        methodCallExpression.Arguments[0],
                        new SqlFragmentExpression(_typeMapping[methodCallExpression.Method.Name])
                    })
                : null;
    }
}
