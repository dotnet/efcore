// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a navigation property which can be used to navigate a relationship.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="INavigation" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableNavigation : INavigation, IMutableNavigationBase
    {
        /// <inheritdoc cref="INavigation.DeclaringEntityType" />
        new IMutableEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((INavigationBase)this).DeclaringEntityType;
        }

        /// <inheritdoc cref="INavigation.TargetEntityType" />
        new IMutableEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((INavigationBase)this).TargetEntityType;
        }

        /// <inheritdoc cref="INavigation.ForeignKey" />
        new IMutableForeignKey ForeignKey
        {
            [DebuggerStepThrough]
            get => (IMutableForeignKey)((INavigation)this).ForeignKey;
        }

        /// <inheritdoc cref="INavigation.Inverse" />
        new IMutableNavigation Inverse
        {
            [DebuggerStepThrough]
            get => (IMutableNavigation)((INavigation)this).Inverse;
        }

        /// <summary>
        ///     Sets the inverse navigation.
        /// </summary>
        /// <param name="inverseName">
        ///     The name of the inverse navigation property. Passing <see langword="null" /> will result in there being
        ///     no inverse navigation property defined.
        /// </param>
        /// <returns> The inverse navigation. </returns>
        IMutableNavigation SetInverse([CanBeNull] string inverseName);

        /// <summary>
        ///     Sets the inverse navigation.
        /// </summary>
        /// <param name="inverse">
        ///     The inverse navigation property. Passing <see langword="null" /> will result in there being
        ///     no inverse navigation property defined.
        /// </param>
        /// <returns> The inverse navigation. </returns>
        IMutableNavigation SetInverse([CanBeNull] MemberInfo inverse);
    }
}
