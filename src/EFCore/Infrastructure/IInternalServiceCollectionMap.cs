// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Exposes methods allowing providers to register EF service dependency objects on the internal service provider.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IInternalServiceCollectionMap
    {
        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Singleton" />  dependency object.
        /// </summary>
        /// <typeparam name="TDependencies"> The dependency type. </typeparam>
        /// <returns> The same collection map so that further methods can be chained. </returns>
        IInternalServiceCollectionMap AddDependencySingleton<TDependencies>();

        /// <summary>
        ///     Adds a <see cref="ServiceLifetime.Scoped" />  dependency object.
        /// </summary>
        /// <typeparam name="TDependencies"> The dependency type. </typeparam>
        /// <returns> The same collection map so that further methods can be chained. </returns>
        IInternalServiceCollectionMap AddDependencyScoped<TDependencies>();
    }
}
