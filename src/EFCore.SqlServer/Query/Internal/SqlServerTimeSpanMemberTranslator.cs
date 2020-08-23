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
    public class SqlServerTimeSpanMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMappings = new Dictionary<string, string>
        {
            { nameof(TimeSpan.Hours), "hour" },
            { nameof(TimeSpan.Minutes), "minute" },
            { nameof(TimeSpan.Seconds), "second" },
            { nameof(TimeSpan.Milliseconds), "millisecond" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerTimeSpanMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
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
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            if (member.DeclaringType == typeof(TimeSpan) && _datePartMappings.TryGetValue(member.Name, out string value))
            {
                return _sqlExpressionFactory.Function(
                    "DATEPART", new[] { _sqlExpressionFactory.Fragment(value), instance },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true },
                    returnType);
            }

            return null;
        }
    }
}
