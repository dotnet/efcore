// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
