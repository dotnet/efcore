// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerObjectToStringTranslator : IMethodCallTranslator
    {
        private const int DefaultLength = 100;

        private static readonly Dictionary<Type, string> _typeMapping
            = new Dictionary<Type, string>
            {
                { typeof(sbyte), "VARCHAR(4)" },
                { typeof(byte), "VARCHAR(3)" },
                { typeof(short), "VARCHAR(6)" },
                { typeof(ushort), "VARCHAR(5)" },
                { typeof(int), "VARCHAR(11)" },
                { typeof(uint), "VARCHAR(10)" },
                { typeof(long), "VARCHAR(20)" },
                { typeof(ulong), "VARCHAR(20)" },
                { typeof(float), $"VARCHAR({DefaultLength})" },
                { typeof(double), $"VARCHAR({DefaultLength})" },
                { typeof(decimal), $"VARCHAR({DefaultLength})" },
                { typeof(char), "VARCHAR(1)" },
                { typeof(DateTime), $"VARCHAR({DefaultLength})" },
                { typeof(DateTimeOffset), $"VARCHAR({DefaultLength})" },
                { typeof(TimeSpan), $"VARCHAR({DefaultLength})" },
                { typeof(Guid), "VARCHAR(36)" },
                { typeof(byte[]), $"VARCHAR({DefaultLength})" },
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerObjectToStringTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            return method.Name == nameof(ToString)
                && arguments.Count == 0
                && instance != null
                && _typeMapping.TryGetValue(instance.Type.UnwrapNullableType(), out var storeType)
                ? _sqlExpressionFactory.Function(
                    "CONVERT",
                    new[] { _sqlExpressionFactory.Fragment(storeType), instance },
                    nullable: true,
                    argumentsPropagateNullability: new bool[] { false, true },
                    typeof(string))
                : null;
        }
    }
}
