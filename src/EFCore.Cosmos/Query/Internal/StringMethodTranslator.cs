// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class StringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly MethodInfo _toLowerMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.ToLower), Array.Empty<Type>());
            
        private static readonly MethodInfo _toUpperMethodInfo
            = typeof(string).GetRequiredRuntimeMethod(nameof(string.ToUpper), Array.Empty<Type>());

        private static readonly MethodInfo _firstOrDefaultMethodInfoWithoutArgs
            = typeof(Enumerable).GetRuntimeMethods().Single(
                m => m.Name == nameof(Enumerable.FirstOrDefault)
                    && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

        private static readonly MethodInfo _lastOrDefaultMethodInfoWithoutArgs
            = typeof(Enumerable).GetRuntimeMethods().Single(
                m => m.Name == nameof(Enumerable.LastOrDefault)
                    && m.GetParameters().Length == 1).MakeGenericMethod(typeof(char));

        private static readonly MethodInfo _stringConcatWithTwoArguments =
            typeof(String).GetRequiredRuntimeMethod(nameof(string.Concat),
                new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo _stringConcatWithThreeArguments =
            typeof(String).GetRequiredRuntimeMethod(nameof(string.Concat),
                new[] { typeof(string), typeof(string), typeof(string) });

        private static readonly MethodInfo _stringConcatWithFourArguments =
            typeof(String).GetRequiredRuntimeMethod(nameof(string.Concat),
                new[] { typeof(string), typeof(string), typeof(string), typeof(string) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public StringMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
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

            if (instance != null)
            {
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

            return null;
        }

        private SqlExpression TranslateSystemFunction(string function, Type returnType, params SqlExpression[] arguments)
            => _sqlExpressionFactory.Function(function, arguments, returnType);
    }
}
