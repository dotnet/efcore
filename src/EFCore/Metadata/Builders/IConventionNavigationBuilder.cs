// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     <para>
///         Provides a simple API surface for configuring an <see cref="IConventionNavigation" /> from conventions.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionNavigationBuilder : IConventionPropertyBaseBuilder<IConventionNavigationBuilder>
{
    /// <summary>
    ///     Gets the navigation being configured.
    /// </summary>
    new IConventionNavigation Metadata { get; }

    /// <summary>
    ///     Configures this navigation to be automatically included in a query.
    /// </summary>
    /// <param name="autoInclude">A value indicating whether the navigation should be automatically included.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionNavigationBuilder? AutoInclude(bool? autoInclude, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether this navigation can be configured to be automatically included in a query
    ///     from the current configuration source.
    /// </summary>
    /// <param name="autoInclude">A value indicating whether the navigation should be automatically included.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if automatically included can be set for this navigation.</returns>
    bool CanSetAutoInclude(bool? autoInclude, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures this navigation to be enabled for lazy-loading.
    /// </summary>
    /// <param name="lazyLoadingEnabled">A value indicating whether the navigation should be enabled for lazy-loading.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    IConventionNavigationBuilder? EnableLazyLoading(bool? lazyLoadingEnabled, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether this navigation can be configured to enable lazy-loading
    ///     from the current configuration source.
    /// </summary>
    /// <param name="lazyLoadingEnabled">A value indicating whether the navigation should be enabled for lazy-loading.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if automatically included can be set for this navigation.</returns>
    bool CanSetLazyLoadingEnabled(bool? lazyLoadingEnabled, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures whether this navigation is required.
    /// </summary>
    /// <param name="required">
    ///     A value indicating whether this is a required navigation.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the requiredness was configured, <see langword="null" /> otherwise.
    /// </returns>
    IConventionNavigationBuilder? IsRequired(bool? required, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether this navigation requiredness can be configured
    ///     from the current configuration source.
    /// </summary>
    /// <param name="required">A value indicating whether the navigation should be required.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if requiredness can be set for this navigation.</returns>
    bool CanSetIsRequired(bool? required, bool fromDataAnnotation = false);
}
