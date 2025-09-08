// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal
{
    public class XGByteArrayMethodTranslator : IMethodCallTranslator
    {
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        private static readonly MethodInfo _containsMethod = typeof(Enumerable)
            .GetTypeInfo()
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

        private static readonly MethodInfo _firstWithoutPredicate = typeof(Enumerable)
            .GetTypeInfo()
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Single(mi => mi.Name == nameof(Enumerable.First) && mi.GetParameters().Length == 1);

        public XGByteArrayMethodTranslator([NotNull] XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (method.IsGenericMethod &&
                arguments[0].TypeMapping is XGByteArrayTypeMapping)
            {
                if (method.GetGenericMethodDefinition().Equals(_containsMethod))
                {
                    var source = arguments[0];
                    var sourceTypeMapping = source.TypeMapping;

                    var value = arguments[1] is SqlConstantExpression constantValue
                        ? (SqlExpression)_sqlExpressionFactory.Constant(new[] {(byte)constantValue.Value}, sourceTypeMapping)
                        : _sqlExpressionFactory.Convert(arguments[1], typeof(byte[]), sourceTypeMapping);

                    return _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.NullableFunction(
                            "LOCATE",
                            new[] {value, source},
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0));
                }

                if (method.GetGenericMethodDefinition().Equals(_firstWithoutPredicate))
                {
                    return _sqlExpressionFactory.NullableFunction(
                        "ASCII",
                        new[] {arguments[0]},
                        typeof(byte));
                }
            }

            return null;
        }
    }
}
