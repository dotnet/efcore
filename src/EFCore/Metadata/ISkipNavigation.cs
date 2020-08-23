// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property that is part of a relationship
    ///     that is forwarded through a third entity type.
    /// </summary>
    public interface ISkipNavigation : INavigationBase
    {
        /// <summary>
        ///     Gets the join type used by the foreign key.
        /// </summary>
        IEntityType JoinEntityType
            => IsOnDependent ? ForeignKey?.PrincipalEntityType : ForeignKey?.DeclaringEntityType;

        /// <summary>
        ///     Gets the inverse skip navigation.
        /// </summary>
        new ISkipNavigation Inverse { get; }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        INavigationBase INavigationBase.Inverse
        {
            [DebuggerStepThrough]
            get => Inverse;
        }

        /// <summary>
        ///     Gets the foreign key to the join type.
        /// </summary>
        IForeignKey ForeignKey { get; }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is defined on the dependent side of the underlying foreign key.
        /// </summary>
        bool IsOnDependent { get; }

        /// <summary>
        ///     Gets the <see cref="IClrCollectionAccessor" /> for this navigation property, if it's a collection
        ///     navigation.
        /// </summary>
        /// <returns> The accessor. </returns>
        IClrCollectionAccessor INavigationBase.GetCollectionAccessor()
            => ((SkipNavigation)this).CollectionAccessor;
    }
}
