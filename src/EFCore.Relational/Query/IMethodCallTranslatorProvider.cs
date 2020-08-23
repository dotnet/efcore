// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Provides translations for LINQ <see cref="MethodCallExpression" /> expressions.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IMethodCallTranslatorProvider
    {
        /// <summary>
        ///     Translates a LINQ <see cref="MethodCallExpression" /> to a SQL equivalent.
        /// </summary>
        /// <param name="model"> A model to use for translation. </param>
        /// <param name="instance"> A SQL representation of <see cref="MethodCallExpression.Object" />. </param>
        /// <param name="method"> The method info from <see cref="MethodCallExpression.Method" />. </param>
        /// <param name="arguments"> SQL representations of <see cref="MethodCallExpression.Arguments" />. </param>
        /// <param name="logger"> The query logger to use. </param>
        /// <returns> A SQL translation of the <see cref="MethodCallExpression" />. </returns>
        SqlExpression Translate(
            [NotNull] IModel model,
            [CanBeNull] SqlExpression instance,
            [NotNull] MethodInfo method,
            [NotNull] IReadOnlyList<SqlExpression> arguments,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger);
    }
}
