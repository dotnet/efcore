// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IReadOnlyNavigationBase" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionNavigationBase : IReadOnlyNavigationBase, IConventionPropertyBase
{
    /// <summary>
    ///     Sets a value indicating whether this navigation should be eager loaded by default.
    /// </summary>
    /// <param name="eagerLoaded">A value indicating whether this navigation should be eager loaded by default.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsEagerLoaded(bool? eagerLoaded, bool fromDataAnnotation = false)
        => (bool?)SetOrRemoveAnnotation(CoreAnnotationNames.EagerLoaded, eagerLoaded, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyNavigationBase.IsEagerLoaded" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyNavigationBase.IsEagerLoaded" />.</returns>
    ConfigurationSource? GetIsEagerLoadedConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.EagerLoaded)?.GetConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether this navigation should be lazy-loaded, if lazy-loading is enabled and in place.
    /// </summary>
    /// <param name="lazyLoadingEnabled">A value indicating whether this navigation should lazy-loaded.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetLazyLoadingEnabled(bool? lazyLoadingEnabled, bool fromDataAnnotation = false)
        => (bool?)SetOrRemoveAnnotation(CoreAnnotationNames.LazyLoadingEnabled, lazyLoadingEnabled, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyNavigationBase.LazyLoadingEnabled" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyNavigationBase.LazyLoadingEnabled" />.</returns>
    ConfigurationSource? GetLazyLoadingEnabledConfigurationSource()
        => FindAnnotation(CoreAnnotationNames.LazyLoadingEnabled)?.GetConfigurationSource();
}
