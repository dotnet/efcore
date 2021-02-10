// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a primary or alternate key on an entity type.
    /// </summary>
    public interface IKey : IReadOnlyKey, IAnnotatable
    {
        /// <summary>
        ///     Gets the properties that make up the key.
        /// </summary>
        new IReadOnlyList<IProperty> Properties { get; }

        /// <summary>
        ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        new IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets all foreign keys that target a given primary or alternate key.
        /// </summary>
        /// <returns> The foreign keys that reference the given key. </returns>
        IEnumerable<IForeignKey> GetReferencingForeignKeys()
            => ((IReadOnlyKey)this).GetReferencingForeignKeys().Cast<IForeignKey>();
    }
}
