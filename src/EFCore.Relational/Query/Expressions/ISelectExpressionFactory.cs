// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     A factory for SelectExpression instances.
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
