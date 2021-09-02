// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Factory for <see cref="IExecutionStrategy" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IExecutionStrategyFactory
    {
        /// <summary>
        ///     Creates a new <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <returns>An instance of <see cref="IExecutionStrategy" />.</returns>
        IExecutionStrategy Create();

        /// <summary>
        ///     Returns the <see cref="IExecutionStrategy" /> instance to the pool
        /// </summary>
        /// <param name="executionStrategy"> The <see cref="IExecutionStrategy" /> instance. </param>
        void Return(IExecutionStrategy executionStrategy);
    }
}
