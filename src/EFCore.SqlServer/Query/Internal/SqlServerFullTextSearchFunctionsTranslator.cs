// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerFullTextSearchFunctionsTranslator : IMethodCallTranslator
    {
        private const string FreeTextFunctionName = "FREETEXT";
        private const string ContainsFunctionName = "CONTAINS";

        private static readonly MethodInfo _freeTextMethodInfo
            = typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.FreeText),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _freeTextMethodInfoWithLanguage
            = typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.FreeText),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int) });

        private static readonly MethodInfo _containsMethodInfo
            = typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.Contains),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _containsMethodInfoWithLanguage
            = typeof(SqlServerDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.Contains),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int) });

        private static readonly IDictionary<MethodInfo, string> _functionMapping
            = new Dictionary<MethodInfo, string>
            {
                { _freeTextMethodInfo, FreeTextFunctionName },
                { _freeTextMethodInfoWithLanguage, FreeTextFunctionName },
                { _containsMethodInfo, ContainsFunctionName },
                { _containsMethodInfoWithLanguage, ContainsFunctionName }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerFullTextSearchFunctionsTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            if (_functionMapping.TryGetValue(method, out var functionName))
            {
                var propertyReference = arguments[1];
                if (!(propertyReference is ColumnExpression))
                {
                    throw new InvalidOperationException(SqlServerStrings.InvalidColumnNameForFreeText);
                }

                var typeMapping = propertyReference.TypeMapping;
                var freeText = _sqlExpressionFactory.ApplyTypeMapping(arguments[2], typeMapping);

                var functionArguments = new List<SqlExpression> { propertyReference, freeText };

                if (arguments.Count == 4)
                {
                    functionArguments.Add(
                        _sqlExpressionFactory.Fragment($"LANGUAGE {((SqlConstantExpression)arguments[3]).Value}"));
                }

                return _sqlExpressionFactory.Function(
                    functionName,
                    functionArguments,
                    nullResultAllowed: true,
                    // TODO: don't propagate for now
                    argumentsPropagateNullability: functionArguments.Select(a => false).ToList(),
                    typeof(bool));
            }

            return null;
        }
    }
}
