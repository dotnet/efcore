// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore.Query.Expressions;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.IMethodCallTranslator" />
    public class SqlServerObjectToStringTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _typeMapping = new Dictionary<string, string>
        {
            [nameof(Int64)] = "VARCHAR(20)",
            [nameof(Int32)] = "VARCHAR(10)",
            [nameof(Int16)] = "VARCHAR(5)"
        };

        /// <summary>
        ///     Translates the given method call expression.
        /// </summary>
        /// <param name="methodCallExpression">The method call expression.</param>
        /// <returns>
        ///     A SQL expression representing the translated MethodCallExpression.
        /// </returns>
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(ToString))
            {
                return new SqlFunctionExpression(
                    functionName: "CONVERT",
                    returnType: methodCallExpression.Type,
                    arguments: new[]
                    {
                        new SqlFragmentExpression(GetConvertBase(methodCallExpression.Object?.Type.UnwrapEnumType().Name)),
                        methodCallExpression.Object
                    });
            }
            return null;
        }

        private string GetConvertBase(string typeName)
           => _typeMapping.ContainsKey(typeName) ? _typeMapping[typeName] : "VARCHAR(MAX)";
    }
}