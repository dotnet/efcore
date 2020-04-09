// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a navigation property which can be used to navigate a relationship.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="INavigationBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableNavigationBase : INavigationBase, IMutablePropertyBase
    {
        /// <summary>
        ///     Gets the type that this navigation property belongs to.
        /// </summary>
        new IMutableEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((INavigationBase)this).DeclaringEntityType;
        }

        /// <summary>
        ///     Gets the entity type that this navigation property will hold an instance(s) of.
        /// </summary>
        new IMutableEntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (IMutableEntityType)((INavigationBase)this).TargetEntityType;
        }

        /// <summary>
        ///     Gets the inverse skip navigation.
        /// </summary>
        new IMutableNavigationBase Inverse
        {
            [DebuggerStepThrough]
            get => (IMutableNavigationBase)((INavigationBase)this).Inverse;
        }

        /// <summary>
        ///     Sets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        /// <param name="eagerLoaded"> A value indicating whether this navigation should be eager loaded by default. </param>
        void SetIsEagerLoaded(bool? eagerLoaded)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.EagerLoaded, eagerLoaded);
    }
}
