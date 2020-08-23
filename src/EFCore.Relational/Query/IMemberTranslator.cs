// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A SQL translator for LINQ <see cref="MemberExpression" /> expression.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IMemberTranslator
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
