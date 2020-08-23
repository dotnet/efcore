// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerObjectToStringTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            return method.Name == nameof(ToString)
                && arguments.Count == 0
                && instance != null
                && _typeMapping.TryGetValue(instance.Type, out var storeType)
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
