// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Creates keys that uniquely identifies the model for a given context. This is used to store and lookup
    ///         a cached model for a given context. This default implementation uses the context type as they key, thus
    ///         assuming that all contexts of a given type have the same model.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ModelCacheKeyFactory : IModelCacheKeyFactory
    {
        /// <summary>
        ///     Gets the model cache key for a given context.
        /// </summary>
        /// <param name="context">
        ///     The context to get the model cache key for.
        /// </param>
        /// <returns> The created key. </returns>
        public virtual object Create(DbContext context) => new ModelCacheKey(context);
    }
}
