// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a type in an <see cref="IConventionModel" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IReadOnlyTypeBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">EF Core model building conventions</see> for more information.
    /// </remarks>
    public interface IConventionTypeBase : IReadOnlyTypeBase, IConventionAnnotatable
    {
        /// <summary>
        ///     Gets the model that this type belongs to.
        /// </summary>
        new IConventionModel Model { get; }

        /// <summary>
        ///     Marks the given member name as ignored, preventing conventions from adding a matching property
        ///     or navigation to the type.
        /// </summary>
        /// <param name="memberName"> The name of the member to be ignored. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The ignored member name. </returns>
        string? AddIgnored(string memberName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the ignored member name.
        /// </summary>
        /// <param name="memberName"> The name of the member to be removed. </param>
        /// <returns> The removed ignored member name. </returns>
        string? RemoveIgnored(string memberName);

        /// <summary>
        ///     Indicates whether the given member name is ignored.
        /// </summary>
        /// <param name="memberName"> The name of the member to be ignored. </param>
        /// <returns>
        ///     The configuration source if the given member name is ignored,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        ConfigurationSource? FindIgnoredConfigurationSource(string memberName);

        /// <summary>
        ///     Gets all the ignored members.
        /// </summary>
        /// <returns> The list of ignored member names. </returns>
        IEnumerable<string> GetIgnoredMembers();

        /// <summary>
        ///     Indicates whether the given member name is ignored.
        /// </summary>
        /// <param name="memberName"> The name of the member that might be ignored. </param>
        /// <returns> <see langword="true" /> if the given member name is ignored. </returns>
        bool IsIgnored(string memberName)
            => FindIgnoredConfigurationSource(memberName) != null;

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for properties of this type.
        ///     </para>
        ///     <para>
        ///         Note that individual properties and navigations can override this access mode. The value set here will
        ///         be used for any property or navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set.</param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        PropertyAccessMode? SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false)
            => (PropertyAccessMode?)SetOrRemoveAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, fromDataAnnotation)?.Value;

        /// <summary>
        ///     Returns the configuration source for <see cref="IReadOnlyTypeBase.GetPropertyAccessMode" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IReadOnlyTypeBase.GetPropertyAccessMode" />. </returns>
        ConfigurationSource? GetPropertyAccessModeConfigurationSource()
            => FindAnnotation(CoreAnnotationNames.PropertyAccessMode)?.GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for navigations of this entity type.
        ///     </para>
        ///     <para>
        ///         Note that individual navigations can override this access mode. The value set here will
        ///         be used for any navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" />, or <see langword="null" /> to clear the mode set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        PropertyAccessMode? SetNavigationAccessMode(PropertyAccessMode? propertyAccessMode, bool fromDataAnnotation = false)
            => (PropertyAccessMode?)SetOrRemoveAnnotation(
                CoreAnnotationNames.NavigationAccessMode, propertyAccessMode, fromDataAnnotation)?.Value;

        /// <summary>
        ///     Returns the configuration source for <see cref="IReadOnlyTypeBase.GetNavigationAccessMode" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="IReadOnlyTypeBase.GetNavigationAccessMode" />. </returns>
        ConfigurationSource? GetNavigationAccessModeConfigurationSource()
            => FindAnnotation(CoreAnnotationNames.NavigationAccessMode)?.GetConfigurationSource();
    }
}
