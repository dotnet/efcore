// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

#nullable enable

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
    public interface IMutableSkipNavigation : IReadOnlySkipNavigation, IMutableNavigationBase
    {
        /// <summary>
        ///     Gets the type that this navigation property belongs to.
        /// </summary>
        new IMutableEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((IReadOnlyNavigationBase)this).DeclaringEntityType;
        }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        new IMutableEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((IReadOnlyNavigationBase)this).TargetEntityType;
        }

        /// <summary>
        ///     Gets the join type used by the foreign key.
        /// </summary>
        new IMutableEntityType? JoinEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType?)((IReadOnlySkipNavigation)this).JoinEntityType;
        }

        /// <summary>
        ///     Gets the foreign key to the join type.
        /// </summary>
        new IMutableForeignKey? ForeignKey
        {
            [DebuggerStepThrough]
            get => (IMutableForeignKey?)((IReadOnlySkipNavigation)this).ForeignKey;
        }

        /// <summary>
        ///     Sets the foreign key.
        /// </summary>
        /// <param name="foreignKey">
        ///     The foreign key. Passing <see langword="null" /> will result in there being no foreign key associated.
        /// </param>
        void SetForeignKey([CanBeNull] IMutableForeignKey? foreignKey);

        /// <summary>
        ///     Gets the inverse skip navigation.
        /// </summary>
        new IMutableSkipNavigation? Inverse
        {
            [DebuggerStepThrough]
            get => (IMutableSkipNavigation?)((IReadOnlySkipNavigation)this).Inverse;
        }

        /// <summary>
        ///     Sets the inverse skip navigation.
        /// </summary>
        /// <param name="inverse">
        ///     The inverse skip navigation. Passing <see langword="null" /> will result in there being no inverse navigation property defined.
        /// </param>
        [DebuggerStepThrough]
        IMutableSkipNavigation? SetInverse([CanBeNull] IMutableSkipNavigation? inverse);
    }
}
