// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
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
    public interface IMutableSkipNavigation : ISkipNavigation, IMutableNavigationBase
    {
        /// <inheritdoc cref="INavigationBase.DeclaringEntityType" />
        new IMutableEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((INavigationBase)this).DeclaringEntityType;
        }

        /// <inheritdoc cref="INavigationBase.TargetEntityType" />
        new IMutableEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((INavigationBase)this).TargetEntityType;
        }

        /// <inheritdoc cref="ISkipNavigation.JoinEntityType" />
        new IMutableEntityType JoinEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((ISkipNavigation)this).JoinEntityType;
        }

        /// <inheritdoc cref="ISkipNavigation.ForeignKey" />
        new IMutableForeignKey ForeignKey
        {
            [DebuggerStepThrough]
            get => (IMutableForeignKey)((ISkipNavigation)this).ForeignKey;
        }

        /// <summary>
        ///     Sets the foreign key.
        /// </summary>
        /// <param name="foreignKey">
        ///     The foreign key. Passing <see langword="null" /> will result in there being no foreign key associated.
        /// </param>
        void SetForeignKey([CanBeNull] IMutableForeignKey foreignKey);

        /// <inheritdoc cref="ISkipNavigation.Inverse" />
        new IMutableSkipNavigation Inverse
        {
            [DebuggerStepThrough]
            get => (IMutableSkipNavigation)((ISkipNavigation)this).Inverse;
        }

        /// <summary>
        ///     Sets the inverse skip navigation.
        /// </summary>
        /// <param name="inverse">
        ///     The inverse skip navigation. Passing <see langword="null" /> will result in there being no inverse navigation property defined.
        /// </param>
        [DebuggerStepThrough]
        IMutableSkipNavigation SetInverse([CanBeNull] IMutableSkipNavigation inverse);
    }
}
