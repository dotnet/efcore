// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property which can be used to navigate a relationship.
    /// </summary>
    public interface INavigation : IReadOnlyNavigation, INavigationBase
    {
        /// <summary>
        ///     Gets the entity type that this navigation property belongs to.
        /// </summary>
        new IEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (IEntityType)((IReadOnlyNavigationBase)this).DeclaringEntityType;
        }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        new IEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (IEntityType)((IReadOnlyNavigationBase)this).TargetEntityType;
        }

        /// <summary>
        ///     Gets the foreign key that defines the relationship this navigation property will navigate.
        /// </summary>
        new IForeignKey ForeignKey
        {
            [DebuggerStepThrough]
            get => (IForeignKey)((IReadOnlyNavigation)this).ForeignKey;
        }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        new INavigation? Inverse
        {
            [DebuggerStepThrough]
            get => (INavigation?)((IReadOnlyNavigation)this).Inverse;
        }
    }
}
