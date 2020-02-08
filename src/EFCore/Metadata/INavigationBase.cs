// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property which can be used to navigate a relationship.
    /// </summary>
    public interface INavigationBase : IPropertyBase
    {
        /// <summary>
        ///     Gets the entity type that this navigation property belongs to.
        /// </summary>
        IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        IEntityType TargetEntityType { get; }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        INavigationBase Inverse { get; }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is a collection property.
        /// </summary>
        bool IsCollection { get; }

        /// <summary>
        ///     Gets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        bool IsEagerLoaded
            => (bool?)this[CoreAnnotationNames.EagerLoaded] ?? false;

        /// <summary>
        ///     Gets the <see cref="IClrCollectionAccessor" /> for this navigation property, if it's a collection
        ///     navigation.
        /// </summary>
        /// <returns> The accessor. </returns>
        IClrCollectionAccessor GetCollectionAccessor();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for this property.
        ///         <c>null</c> indicates that the default property access mode is being used.
        ///     </para>
        /// </summary>
        /// <returns> The access mode being used, or <c>null</c> if the default access mode is being used. </returns>
        PropertyAccessMode IPropertyBase.GetPropertyAccessMode()
            => (PropertyAccessMode)(this[CoreAnnotationNames.PropertyAccessMode]
                ?? DeclaringType.GetNavigationAccessMode());
    }
}
