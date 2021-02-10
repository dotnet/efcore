// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property which can be used to navigate a relationship.
    /// </summary>
    public interface IReadOnlyNavigationBase : IReadOnlyPropertyBase
    {
        /// <summary>
        ///     Gets the entity type that this navigation property belongs to.
        /// </summary>
        IReadOnlyEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        IReadOnlyEntityType TargetEntityType { get; }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        IReadOnlyNavigationBase? Inverse { get; }

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
        IClrCollectionAccessor? GetCollectionAccessor();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for this property.
        ///         <see langword="null" /> indicates that the default property access mode is being used.
        ///     </para>
        /// </summary>
        /// <returns> The access mode being used. </returns>
        PropertyAccessMode IReadOnlyPropertyBase.GetPropertyAccessMode()
            => (PropertyAccessMode)(this[CoreAnnotationNames.PropertyAccessMode]
                ?? DeclaringType.GetNavigationAccessMode());
    }
}
