// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     <para>
    ///         A factory for SelectExpression instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public interface ISelectExpressionFactory
    {
        /// <summary>
        ///     Creates a new SelectExpression.
        /// </summary>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        SelectExpression Create([NotNull] RelationalQueryCompilationContext queryCompilationContext);

        /// <summary>
        ///     Creates a new SelectExpression.
        /// </summary>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <param name="alias"> The alias. </param>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        SelectExpression Create(
            [NotNull] RelationalQueryCompilationContext queryCompilationContext, [NotNull] string alias);
    }
}
