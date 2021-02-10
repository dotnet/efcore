// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property which can be used to navigate a relationship.
    /// </summary>
    public interface IReadOnlyNavigation : IReadOnlyNavigationBase
    {
        /// <summary>
        ///     Gets the entity type that this navigation property belongs to.
        /// </summary>
        new IReadOnlyEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => IsOnDependent ? ForeignKey.DeclaringEntityType : ForeignKey.PrincipalEntityType;
        }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        new IReadOnlyEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => IsOnDependent ? ForeignKey.PrincipalEntityType : ForeignKey.DeclaringEntityType;
        }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        new IReadOnlyNavigation? Inverse
        {
            [DebuggerStepThrough]
            get => IsOnDependent ? ForeignKey.PrincipalToDependent : ForeignKey.DependentToPrincipal;
        }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is a collection property.
        /// </summary>
        new bool IsCollection
        {
            [DebuggerStepThrough]
            get => !IsOnDependent && !ForeignKey.IsUnique;
        }

        /// <summary>
        ///     Gets the foreign key that defines the relationship this navigation property will navigate.
        /// </summary>
        IReadOnlyForeignKey ForeignKey { get; }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is defined on the dependent side of the underlying foreign key.
        /// </summary>
        bool IsOnDependent
        {
            [DebuggerStepThrough]
            get => ForeignKey.DependentToPrincipal == this;
        }

        /// <summary>
        ///     Gets the <see cref="IClrCollectionAccessor" /> for this navigation property, if it's a collection
        ///     navigation.
        /// </summary>
        /// <returns> The accessor. </returns>
        [DebuggerStepThrough]
        new IClrCollectionAccessor? GetCollectionAccessor()
            => ((Navigation)this).CollectionAccessor;

        /// <summary>
        ///     Gets the entity type that this navigation property belongs to.
        /// </summary>
        IReadOnlyEntityType IReadOnlyNavigationBase.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        IReadOnlyEntityType IReadOnlyNavigationBase.TargetEntityType
        {
            [DebuggerStepThrough]
            get => TargetEntityType;
        }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        IReadOnlyNavigationBase? IReadOnlyNavigationBase.Inverse
        {
            [DebuggerStepThrough]
            get => Inverse;
        }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is a collection property.
        /// </summary>
        bool IReadOnlyNavigationBase.IsCollection
        {
            [DebuggerStepThrough]
            get => IsCollection;
        }

        /// <summary>
        ///     Gets the <see cref="IClrCollectionAccessor" /> for this navigation property, if it's a collection
        ///     navigation.
        /// </summary>
        /// <returns> The accessor. </returns>
        [DebuggerStepThrough]
        IClrCollectionAccessor? IReadOnlyNavigationBase.GetCollectionAccessor()
            => GetCollectionAccessor();
    }
}
