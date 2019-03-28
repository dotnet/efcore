// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a navigation property which can be used to navigate a relationship.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="INavigation" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableNavigation : INavigation, IMutablePropertyBase
    {
        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IMutableEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the foreign key that defines the relationship this navigation property will navigate.
        /// </summary>
        new IMutableForeignKey ForeignKey { get; }
    }
}
