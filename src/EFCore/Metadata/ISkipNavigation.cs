// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property that is part of a relationship
    ///     that is forwarded through a third entity type.
    /// </summary>
    public interface ISkipNavigation : IPropertyBase
    {
        /// <summary>
        ///     Gets the entity type that this navigation belongs to.
        /// </summary>
        IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        IEntityType TargetEntityType { get; }

        /// <summary>
        ///     Gets the association type used by the foreign key.
        /// </summary>
        IEntityType AssociationEntityType => IsOnPrincipal ? ForeignKey?.DeclaringEntityType : ForeignKey?.PrincipalEntityType;

        /// <summary>
        ///     Gets the foreign key to the association type.
        /// </summary>
        IForeignKey ForeignKey { get; }

        /// <summary>
        ///     Gets the inverse skip navigation.
        /// </summary>
        ISkipNavigation Inverse { get; }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is a collection property.
        /// </summary>
        bool IsCollection { get; }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is defined on the principal side of the underlying foreign key.
        /// </summary>
        bool IsOnPrincipal { get; }

        /// <summary>
        ///     Gets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        bool IsEagerLoaded
            => (bool?)this[CoreAnnotationNames.EagerLoaded] ?? false;

        /// <summary>
        ///     Gets the <see cref="IClrCollectionAccessor" /> for this navigation property, which must be a collection
        ///     navigation.
        /// </summary>
        /// <returns> The accessor. </returns>
        IClrCollectionAccessor GetCollectionAccessor()
            => new ClrCollectionAccessorFactory().Create(this);
    }
}
