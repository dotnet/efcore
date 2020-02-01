// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Creates keys that uniquely identifies the model for a given context. This is used to store and lookup
    ///         a cached model for a given context.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IModelCacheKeyFactory
    {
        /// <summary>
        ///     Gets the model cache key for a given context.
        /// </summary>
        /// <param name="context">
        ///     The context to get the model cache key for.
        /// </param>
        /// <returns> The created key. </returns>
        object Create([NotNull] DbContext context);
    }
}
