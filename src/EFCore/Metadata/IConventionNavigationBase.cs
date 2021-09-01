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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">EF Core model building conventions</see> for more information.
    /// </remarks>
    public interface IConventionNavigationBase : IReadOnlyNavigationBase, IConventionPropertyBase
    {
        /// <summary>
        ///     Sets a value indicating whether this navigation should be eager loaded by default.
        /// </summary>
        /// <param name="eagerLoaded"> A value indicating whether this navigation should be eager loaded by default. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        bool? SetIsEagerLoaded(bool? eagerLoaded, bool fromDataAnnotation = false)
        {
            SetOrRemoveAnnotation(CoreAnnotationNames.EagerLoaded, eagerLoaded, fromDataAnnotation);
            return eagerLoaded;
        }

        /// <summary>
        ///     Returns the configuration source for <see cref="NavigationExtensions.IsEagerLoaded" />.
        /// </summary>
        /// <returns> The configuration source for <see cref="NavigationExtensions.IsEagerLoaded" />. </returns>
        ConfigurationSource? GetIsEagerLoadedConfigurationSource()
            => FindAnnotation(CoreAnnotationNames.EagerLoaded)?.GetConfigurationSource();
    }
}
