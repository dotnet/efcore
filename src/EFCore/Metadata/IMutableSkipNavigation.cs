// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a navigation property that is part of a relationship
    ///         that is forwarded through a third entity type.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="ISkipNavigation" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableSkipNavigation : ISkipNavigation, IMutablePropertyBase
    {
        /// <summary>
        ///     Gets the type that this navigation property belongs to.
        /// </summary>
        new IMutableEntityType DeclaringEntityType => (IMutableEntityType)((ISkipNavigation)this).DeclaringEntityType;

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        new IMutableEntityType TargetEntityType => (IMutableEntityType)((ISkipNavigation)this).TargetEntityType;

        /// <summary>
        ///     Gets the association type used by the foreign key.
        /// </summary>
        new IMutableEntityType AssociationEntityType => (IMutableEntityType)((ISkipNavigation)this).AssociationEntityType;

        /// <summary>
        ///     Gets the foreign key to the association type.
        /// </summary>
        new IMutableForeignKey ForeignKey => (IMutableForeignKey)((ISkipNavigation)this).ForeignKey;

        /// <summary>
        ///     Sets the foreign key.
        /// </summary>
        /// <param name="foreignKey">
        ///     The foreign key. Passing <c>null</c> will result in there being no foreign key associated.
        /// </param>
        IMutableForeignKey SetForeignKey([CanBeNull] IMutableForeignKey foreignKey);

        /// <summary>
        ///     Gets the inverse skip navigation.
        /// </summary>
        new IMutableSkipNavigation Inverse => (IMutableSkipNavigation)((ISkipNavigation)this).Inverse;

        /// <summary>
        ///     Sets the inverse skip navigation.
        /// </summary>
        /// <param name="inverse">
        ///     The inverse skip navigation. Passing <c>null</c> will result in there being no inverse navigation property defined.
        /// </param>
        IConventionSkipNavigation SetInverse([CanBeNull] IMutableSkipNavigation inverse);
    }
}
