// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property that is part of a relationship
    ///     that is forwarded through a third entity type.
    /// </summary>
    public interface ISkipNavigation : IReadOnlySkipNavigation, INavigationBase
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
        ///     Gets the join type used by the foreign key.
        /// </summary>
        new IEntityType JoinEntityType
        {
            [DebuggerStepThrough]
            get => (IEntityType)((IReadOnlySkipNavigation)this).JoinEntityType!;
        }

        /// <summary>
        ///     Gets the foreign key to the join type.
        /// </summary>
        new IForeignKey ForeignKey
        {
            [DebuggerStepThrough]
            get => (IForeignKey)((IReadOnlySkipNavigation)this).ForeignKey!;
        }

        /// <summary>
        ///     Gets the inverse skip navigation.
        /// </summary>
        new ISkipNavigation Inverse
        {
            [DebuggerStepThrough]
            get => (ISkipNavigation)((IReadOnlySkipNavigation)this).Inverse!;
        }
    }
}
