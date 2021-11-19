// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosStringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _indexOfMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.IndexOf), typeof(string));

        private static readonly MethodInfo _indexOfMethodInfoWithStartingPosition
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.IndexOf), typeof(string), typeof(int));

        private static readonly MethodInfo _replaceMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Replace), typeof(string), typeof(string));

        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Contains), typeof(string));

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.StartsWith), typeof(string));

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.EndsWith), typeof(string));

        private static readonly MethodInfo _toLowerMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.ToLower), Array.Empty<Type>());

        private static readonly MethodInfo _toUpperMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.ToUpper), Array.Empty<Type>());

        private static readonly MethodInfo _trimStartMethodInfoWithoutArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimStart), Array.Empty<Type>());

        private static readonly MethodInfo _trimEndMethodInfoWithoutArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimEnd), Array.Empty<Type>());

        private static readonly MethodInfo _trimMethodInfoWithoutArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Trim), Array.Empty<Type>());

        private static readonly MethodInfo _trimStartMethodInfoWithCharArrayArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimStart), typeof(char[]));

        private static readonly MethodInfo _trimEndMethodInfoWithCharArrayArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.TrimEnd), typeof(char[]));

        private static readonly MethodInfo _trimMethodInfoWithCharArrayArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Trim), typeof(char[]));

        private static readonly MethodInfo _substringMethodInfoWithOneArg
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Substring), typeof(int));

        private static readonly MethodInfo _substringMethodInfoWithTwoArgs
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Substring), typeof(int), typeof(int));

        private static readonly MethodInfo _firstOrDefaultMethodInfoWithoutArgs
            = typeof(Enumerable).GetRuntimeMethods().Single(
                m => m.Name == nameof(Enumerable.FirstOrDefault)
                    && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

        private static readonly MethodInfo _lastOrDefaultMethodInfoWithoutArgs
            = typeof(Enumerable).GetRuntimeMethods().Single(
                m => m.Name == nameof(Enumerable.LastOrDefault)
                    && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

        private static readonly MethodInfo _stringConcatWithTwoArguments =
            typeof(string).GetRequiredRuntimeMethod(nameof(string.Concat), typeof(string), typeof(string));

        private static readonly MethodInfo _stringConcatWithThreeArguments =
            typeof(string).GetRequiredRuntimeMethod(nameof(string.Concat), typeof(string), typeof(string), typeof(string));

        private static readonly MethodInfo _stringConcatWithFourArguments =
            typeof(string).GetRequiredRuntimeMethod(nameof(string.Concat), typeof(string), typeof(string), typeof(string), typeof(string));

        private static readonly MethodInfo _stringComparisonWithComparisonTypeArgumentInstance
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Equals), typeof(string), typeof(StringComparison));

        private static readonly MethodInfo _stringComparisonWithComparisonTypeArgumentStatic
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Equals), typeof(string), typeof(string), typeof(StringComparison));

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosStringMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            if (instance != null)
            {
                if (_indexOfMethodInfo.Equals(method))
                {
                    return TranslateSystemFunction("INDEX_OF", typeof(int), instance, arguments[0]);
                }

                if (_indexOfMethodInfoWithStartingPosition.Equals(method))
                {
                    return TranslateSystemFunction("INDEX_OF", typeof(int), instance, arguments[0], arguments[1]);
                }

                if (_replaceMethodInfo.Equals(method))
                {
                    return TranslateSystemFunction("REPLACE", method.ReturnType, instance, arguments[0], arguments[1]);
                }

                if (_containsMethodInfo.Equals(method))
                {
                    return TranslateSystemFunction("CONTAINS", typeof(bool), instance, arguments[0]);
                }

                if (_startsWithMethodInfo.Equals(method))
                {
                    return TranslateSystemFunction("STARTSWITH", typeof(bool), instance, arguments[0]);
                }

                if (_endsWithMethodInfo.Equals(method))
                {
                    return TranslateSystemFunction("ENDSWITH", typeof(bool), instance, arguments[0]);
                }

                if (_toLowerMethodInfo.Equals(method))
                {
                    return TranslateSystemFunction("LOWER", method.ReturnType, instance);
                }

                if (_toUpperMethodInfo.Equals(method))
                {
                    return TranslateSystemFunction("UPPER", method.ReturnType, instance);
                }

                if (_trimStartMethodInfoWithoutArgs?.Equals(method) == true
                    || (_trimStartMethodInfoWithCharArrayArg.Equals(method)
                        // Cosmos DB LTRIM does not take arguments
                        && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
                {
                    return TranslateSystemFunction("LTRIM", method.ReturnType, instance);
                }

                if (_trimEndMethodInfoWithoutArgs?.Equals(method) == true
                    || (_trimEndMethodInfoWithCharArrayArg.Equals(method)
                        // Cosmos DB RTRIM does not take arguments
                        && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
                {
                    return TranslateSystemFunction("RTRIM", method.ReturnType, instance);
                }

                if (_trimMethodInfoWithoutArgs?.Equals(method) == true
                    || (_trimMethodInfoWithCharArrayArg.Equals(method)
                        // Cosmos DB TRIM does not take arguments
                        && ((arguments[0] as SqlConstantExpression)?.Value as Array)?.Length == 0))
                {
                    return TranslateSystemFunction("TRIM", method.ReturnType, instance);
                }

                if (_substringMethodInfoWithOneArg.Equals(method))
                {
                    return TranslateSystemFunction(
                        "SUBSTRING",
                        method.ReturnType,
                        instance,
                        arguments[0],
                        TranslateSystemFunction("LENGTH", typeof(int), instance));
                }

                if (_substringMethodInfoWithTwoArgs.Equals(method))
                {
                    return arguments[0] is SqlConstantExpression constant
                        && constant.Value is int intValue
                        && intValue == 0
                            ? TranslateSystemFunction("LEFT", method.ReturnType, instance, arguments[1])
                            : TranslateSystemFunction("SUBSTRING", method.ReturnType, instance, arguments[0], arguments[1]);
                }
            }

            if (_firstOrDefaultMethodInfoWithoutArgs.Equals(method))
            {
                return TranslateSystemFunction("LEFT", typeof(char), arguments[0], _sqlExpressionFactory.Constant(1));
            }

            if (_lastOrDefaultMethodInfoWithoutArgs.Equals(method))
            {
                return TranslateSystemFunction("RIGHT", typeof(char), arguments[0], _sqlExpressionFactory.Constant(1));
            }

            if (_stringConcatWithTwoArguments.Equals(method))
            {
                return _sqlExpressionFactory.Add(
                    arguments[0],
                    arguments[1]);
            }

            if (_stringConcatWithThreeArguments.Equals(method))
            {
                return _sqlExpressionFactory.Add(
                    arguments[0],
                    _sqlExpressionFactory.Add(
                        arguments[1],
                        arguments[2]));
            }

            if (_stringConcatWithFourArguments.Equals(method))
            {
                return _sqlExpressionFactory.Add(
                    arguments[0],
                    _sqlExpressionFactory.Add(
                        arguments[1],
                        _sqlExpressionFactory.Add(
                            arguments[2],
                            arguments[3])));
            }

            if (_stringComparisonWithComparisonTypeArgumentInstance.Equals(method)
                || _stringComparisonWithComparisonTypeArgumentStatic.Equals(method))
            {
                var comparisonTypeArgument = arguments[^1];

                if (comparisonTypeArgument is SqlConstantExpression constantComparisonTypeArgument
                    && constantComparisonTypeArgument.Value is StringComparison comparisonTypeArgumentValue
                    && (comparisonTypeArgumentValue == StringComparison.OrdinalIgnoreCase
                        || comparisonTypeArgumentValue == StringComparison.Ordinal))
                {
                    return _stringComparisonWithComparisonTypeArgumentInstance.Equals(method)
                        ? comparisonTypeArgumentValue == StringComparison.OrdinalIgnoreCase
                            ? TranslateSystemFunction(
                                "STRINGEQUALS", typeof(bool), instance!, arguments[0], _sqlExpressionFactory.Constant(true))
                            : TranslateSystemFunction("STRINGEQUALS", typeof(bool), instance!, arguments[0])
                        : comparisonTypeArgumentValue == StringComparison.OrdinalIgnoreCase
                            ? TranslateSystemFunction(
                                "STRINGEQUALS", typeof(bool), arguments[0], arguments[1], _sqlExpressionFactory.Constant(true))
                            : TranslateSystemFunction("STRINGEQUALS", typeof(bool), arguments[0], arguments[1]);
                }
            }

            return null;
        }

        private SqlExpression TranslateSystemFunction(string function, Type returnType, params SqlExpression[] arguments)
            => _sqlExpressionFactory.Function(function, arguments, returnType);
    }
}
