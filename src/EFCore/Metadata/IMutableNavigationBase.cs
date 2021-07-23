// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a navigation property which can be used to navigate a relationship.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IReadOnlyNavigationBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableNavigationBase : IReadOnlyNavigationBase, IMutablePropertyBase
    {
        /// <summary>
        ///     Sets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        /// <param name="eagerLoaded"> A value indicating whether this navigation should be eager loaded by default. </param>
        void SetIsEagerLoaded(bool? eagerLoaded)
            => SetOrRemoveAnnotation(CoreAnnotationNames.EagerLoaded, eagerLoaded);
    }
}
