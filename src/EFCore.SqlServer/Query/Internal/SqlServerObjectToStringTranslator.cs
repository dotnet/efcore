// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerObjectToStringTranslator : IMethodCallTranslator
    {
        private const int DefaultLength = 100;

        private static readonly Dictionary<Type, string> _typeMapping
            = new()
            {
                { typeof(sbyte), "varchar(4)" },
                { typeof(byte), "varchar(3)" },
                { typeof(short), "varchar(6)" },
                { typeof(ushort), "varchar(5)" },
                { typeof(int), "varchar(11)" },
                { typeof(uint), "varchar(10)" },
                { typeof(long), "varchar(20)" },
                { typeof(ulong), "varchar(20)" },
                { typeof(float), $"varchar({DefaultLength})" },
                { typeof(double), $"varchar({DefaultLength})" },
                { typeof(decimal), $"varchar({DefaultLength})" },
                { typeof(char), "varchar(1)" },
                { typeof(DateTime), $"varchar({DefaultLength})" },
                { typeof(DateTimeOffset), $"varchar({DefaultLength})" },
                { typeof(TimeSpan), $"varchar({DefaultLength})" },
                { typeof(Guid), "varchar(36)" },
                { typeof(byte[]), $"varchar({DefaultLength})" },
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerObjectToStringTranslator(ISqlExpressionFactory sqlExpressionFactory)
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

            if (instance == null || method.Name != nameof(ToString) || arguments.Count != 0)
            {
                return null;
            }

            if (instance.Type == typeof(bool))
            {
                if (instance is ColumnExpression columnExpression && columnExpression.IsNullable)
                {
                    return _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                                _sqlExpressionFactory.Constant(false.ToString())),
                            new CaseWhenClause(
                                _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(true)),
                                _sqlExpressionFactory.Constant(true.ToString()))
                        },
                        _sqlExpressionFactory.Constant(null));
                }

                return _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            _sqlExpressionFactory.Equal(instance, _sqlExpressionFactory.Constant(false)),
                            _sqlExpressionFactory.Constant(false.ToString()))
                    },
                    _sqlExpressionFactory.Constant(true.ToString()));
            }

            return _typeMapping.TryGetValue(instance.Type, out var storeType)
                ? _sqlExpressionFactory.Function(
                    "CONVERT",
                    new[] { _sqlExpressionFactory.Fragment(storeType), instance },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true },
                    typeof(string))
                : null;
        }
    }
}
