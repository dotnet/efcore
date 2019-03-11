// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    /// <summary>
    ///     <para>
    ///         A SelectExpression factory.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class SelectExpressionFactory : ISelectExpressionFactory
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.Expressions.SelectExpressionFactory class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public SelectExpressionFactory([NotNull] SelectExpressionDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="SelectExpression" />
        /// </summary>
        protected virtual SelectExpressionDependencies Dependencies { get; }

        /// <summary>
        ///     Creates a new SelectExpression.
        /// </summary>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        public virtual SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext)
            => new SelectExpression(Dependencies, queryCompilationContext);

        /// <summary>
        ///     Creates a new SelectExpression.
        /// </summary>
        /// <param name="queryCompilationContext"> Context for the query compilation. </param>
        /// <param name="alias"> The alias of this SelectExpression. </param>
        /// <returns>
        ///     A SelectExpression.
        /// </returns>
        public virtual SelectExpression Create(RelationalQueryCompilationContext queryCompilationContext, string alias)
            => new SelectExpression(
                Dependencies,
                queryCompilationContext,
                Check.NotEmpty(alias, nameof(alias)));
    }
}
