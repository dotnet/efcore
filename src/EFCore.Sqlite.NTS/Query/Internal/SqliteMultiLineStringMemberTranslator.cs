// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteMultiLineStringMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _isClosed = typeof(MultiLineString).GetRuntimeProperty(nameof(MultiLineString.IsClosed));
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteMultiLineStringMemberTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
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

            if (Equals(member, _isClosed))
            {
                return _sqlExpressionFactory.Case(
                    new[]
                    {
                        new CaseWhenClause(
                            _sqlExpressionFactory.IsNotNull(instance),
                            _sqlExpressionFactory.Function(
                                "IsClosed",
                                new[] { instance },
                                nullable: false,
                                argumentsPropagateNullability: new[] { false },
                                returnType))
                    },
                    null);
            }

            return null;
        }
    }
}
