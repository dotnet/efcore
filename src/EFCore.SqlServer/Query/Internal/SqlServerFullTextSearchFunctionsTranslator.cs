// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerFullTextSearchFunctionsTranslator : IMethodCallTranslator
    {
        private const string FreeTextFunctionName = "FREETEXT";
        private const string ContainsFunctionName = "CONTAINS";

        private static readonly MethodInfo _freeTextMethodInfo
            = typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.FreeText), typeof(DbFunctions), typeof(object), typeof(string));

        private static readonly MethodInfo _freeTextMethodInfoWithLanguage
            = typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.FreeText), typeof(DbFunctions), typeof(object), typeof(string), typeof(int));

        private static readonly MethodInfo _containsMethodInfo
            = typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.Contains), typeof(DbFunctions), typeof(object), typeof(string));

        private static readonly MethodInfo _containsMethodInfoWithLanguage
            = typeof(SqlServerDbFunctionsExtensions).GetRequiredRuntimeMethod(
                nameof(SqlServerDbFunctionsExtensions.Contains), typeof(DbFunctions), typeof(object), typeof(string), typeof(int));

        private static readonly IDictionary<MethodInfo, string> _functionMapping
            = new Dictionary<MethodInfo, string>
            {
                { _freeTextMethodInfo, FreeTextFunctionName },
                { _freeTextMethodInfoWithLanguage, FreeTextFunctionName },
                { _containsMethodInfo, ContainsFunctionName },
                { _containsMethodInfoWithLanguage, ContainsFunctionName }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerFullTextSearchFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (_functionMapping.TryGetValue(method, out var functionName))
            {
                var propertyReference = arguments[1];
                if (!(propertyReference is ColumnExpression))
                {
                    throw new InvalidOperationException(SqlServerStrings.InvalidColumnNameForFreeText);
                }

                var typeMapping = propertyReference.TypeMapping;
                var freeText = propertyReference.Type == arguments[2].Type
                    ? _sqlExpressionFactory.ApplyTypeMapping(arguments[2], typeMapping)
                    : _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);

                var functionArguments = new List<SqlExpression> { propertyReference, freeText };

                if (arguments.Count == 4)
                {
                    functionArguments.Add(
                        _sqlExpressionFactory.Fragment($"LANGUAGE {((SqlConstantExpression)arguments[3]).Value}"));
                }

                return _sqlExpressionFactory.Function(
                    functionName,
                    functionArguments,
                    nullable: true,
                    // TODO: don't propagate for now
                    argumentsPropagateNullability: functionArguments.Select(a => false).ToList(),
                    typeof(bool));
            }

            return null;
        }
    }
}
