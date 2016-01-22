// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a primary or alternate key on an entity.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IKey" /> represents a ready-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableKey : IMutableAnnotatable, IKey
    {
        /// <summary>
        ///     Gets the properties that make up the key.
        /// </summary>
        new IReadOnlyList<IMutableProperty> Properties { get; }

        /// <summary>
        ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="Properties" />
        ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
        ///     may be defined on a base type).
        /// </summary>
        new IMutableEntityType DeclaringEntityType { get; }
    }
}
