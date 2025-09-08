// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGObjectToStringTranslator : IMethodCallTranslator
    {
        private static readonly List<Type> _supportedTypes = new List<Type>
        {
            typeof(int),
            typeof(long),
            typeof(DateTime),
            typeof(Guid),
            typeof(bool),
            typeof(byte),
            typeof(byte[]),
            typeof(double),
            typeof(DateTimeOffset),
            typeof(char),
            typeof(short),
            typeof(float),
            typeof(decimal),
            typeof(TimeSpan),
            typeof(uint),
            typeof(ushort),
            typeof(ulong),
            typeof(sbyte),
        };
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public XGObjectToStringTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (instance == null || method.Name != nameof(ToString) || arguments.Count != 0)
            {
                return null;
            }

            if (instance.TypeMapping?.ClrType == typeof(string))
            {
                return instance;
            }

            if (instance.Type == typeof(bool))
            {
                if (instance is not ColumnExpression { IsNullable: false })
                {
                    return _sqlExpressionFactory.Case(
                        instance,
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.Constant(false),
                                _sqlExpressionFactory.Constant(false.ToString())),
                            new CaseWhenClause(
                                _sqlExpressionFactory.Constant(true),
                                _sqlExpressionFactory.Constant(true.ToString()))
                        },
                        _sqlExpressionFactory.Constant(string.Empty));
                }

                return _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            instance,
                            _sqlExpressionFactory.Constant(true.ToString()))
                    },
                    _sqlExpressionFactory.Constant(false.ToString()));
            }

            // Enums are handled by EnumMethodTranslator.
            return _supportedTypes.Contains(instance.Type)
                ? _sqlExpressionFactory.Coalesce(
                    _sqlExpressionFactory.Convert(instance, typeof(string)),
                    _sqlExpressionFactory.Constant(string.Empty))
                : null;
        }
    }
}
