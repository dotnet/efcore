// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A key that uniquely identifies the model for a given context. This is used to store and lookup
    ///         a cached model for a given context. This default implementation uses the context type as they key, thus
    ///         assuming that all contexts of a given type have the same model.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ModelCacheKey
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelCacheKey" /> class.
        /// </summary>
        /// <param name="context">
        ///     The context instance that this key is for.
        /// </param>
        public ModelCacheKey([NotNull] DbContext context)
        {
            _dbContextType = context.GetType();
        }

        private readonly Type _dbContextType;

        /// <summary>
        ///     Determines if this key is equivalent to a given key (i.e. if they are for the same context type).
        /// </summary>
        /// <param name="other">
        ///     The key to compare this key to.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the key is for the same context type, otherwise <see langword="false" />.
        /// </returns>
        protected virtual bool Equals([NotNull] ModelCacheKey other)
            => _dbContextType == other._dbContextType;

        /// <summary>
        ///     Determines if this key is equivalent to a given object (i.e. if they are keys for the same context type).
        /// </summary>
        /// <param name="obj">
        ///     The object to compare this key to.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the object is a <see cref="ModelCacheKey" /> and is for the same context type, otherwise
        ///     <see langword="false" />.
        /// </returns>
        public override bool Equals(object obj)
            => (obj is ModelCacheKey otherAsKey) && Equals(otherAsKey);

        /// <summary>
        ///     Gets the hash code for the key.
        /// </summary>
        /// <returns>
        ///     The hash code for the key.
        /// </returns>
        public override int GetHashCode()
            => _dbContextType?.GetHashCode() ?? 0;
    }
}
