// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     <para>
    ///         Keeps a cache of value generators for properties.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IValueGeneratorCache
    {
        /// <summary>
        ///     Gets the existing value generator from the cache, or creates a new one if one is not present in
        ///     the cache.
        /// </summary>
        /// <param name="property"> The property to get the value generator for. </param>
        /// <param name="entityType">
        ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
        ///     this entity type may be different from the declared entity type on <paramref name="property" />
        /// </param>
        /// <param name="factory"> Factory to create a new value generator if one is not present in the cache. </param>
        /// <returns> The existing or newly created value generator. </returns>
        ValueGenerator GetOrAdd(
            [NotNull] IProperty property,
            [NotNull] IEntityType entityType,
            [NotNull] Func<IProperty, IEntityType, ValueGenerator> factory);
    }
}
