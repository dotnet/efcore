// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a type in an <see cref="IMutableModel" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IReadOnlyTypeBase" /> represents a read-only view of the same metadata.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    ///     </para>
    /// </remarks>
    public interface IMutableTypeBase : IReadOnlyTypeBase, IMutableAnnotatable
    {
        /// <summary>
        ///     Gets the model that this type belongs to.
        /// </summary>
        new IMutableModel Model { get; }

        /// <summary>
        ///     Marks the given member name as ignored, preventing conventions from adding a matching property
        ///     or navigation to the type.
        /// </summary>
        /// <param name="memberName">The name of the member to be ignored.</param>
        /// <returns>The name of the ignored member.</returns>
        string? AddIgnored(string memberName);

        /// <summary>
        ///     Removes the ignored member name.
        /// </summary>
        /// <param name="memberName">The name of the member to be removed.</param>
        /// <returns>The removed ignored member name, or <see langword="null" /> if the member name was not found.</returns>
        string? RemoveIgnored(string memberName);

        /// <summary>
        ///     Indicates whether the given member name is ignored.
        /// </summary>
        /// <param name="memberName">The name of the member that might be ignored.</param>
        /// <returns><see langword="true" /> if the given member name is ignored.</returns>
        bool IsIgnored(string memberName);

        /// <summary>
        ///     Gets all the ignored members.
        /// </summary>
        /// <returns>The list of ignored member names.</returns>
        IEnumerable<string> GetIgnoredMembers();

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for properties and navigations of this entity type.
        /// </summary>
        /// <remarks>
        ///     Note that individual properties and navigations can override this access mode. The value set here will
        ///     be used for any property or navigation for which no override has been specified.
        /// </remarks>
        /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
        void SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode)
            => SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for navigations of this entity type.
        /// </summary>
        /// <remarks>
        ///     Note that individual navigations can override this access mode. The value set here will
        ///     be used for any navigation for which no override has been specified.
        /// </remarks>
        /// <param name="propertyAccessMode">The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
        void SetNavigationAccessMode(PropertyAccessMode? propertyAccessMode)
            => SetOrRemoveAnnotation(CoreAnnotationNames.NavigationAccessMode, propertyAccessMode);
    }
}
