// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Provides translations for LINQ <see cref="MemberExpression" /> expressions.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IMemberTranslatorProvider
    {
        /// <summary>
        ///     Translates a LINQ <see cref="MemberExpression" /> to a SQL equivalent.
        /// </summary>
        /// <param name="instance"> A SQL representation of <see cref="MemberExpression.Expression" />. </param>
        /// <param name="member"> The member info from <see cref="MemberExpression.Member" />. </param>
        /// <param name="returnType"> The return type from <see cref="P:MemberExpression.Type" />. </param>
        /// <param name="logger"> The query logger to use. </param>
        /// <returns> A SQL translation of the <see cref="MemberExpression" />. </returns>
        SqlExpression Translate(
            [CanBeNull] SqlExpression instance,
            [NotNull] MemberInfo member,
            [NotNull] Type returnType,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger);
    }
}
