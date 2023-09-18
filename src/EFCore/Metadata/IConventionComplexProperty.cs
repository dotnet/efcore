// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a complex property of a structural type.
/// </summary>
/// <remarks>
///     <para>
///         This interface is used during model creation and allows the metadata to be modified.
///         Once the model is built, <see cref="IComplexProperty" /> represents a read-only view of the same metadata.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public interface IConventionComplexProperty : IReadOnlyComplexProperty, IConventionPropertyBase
{
    /// <summary>
    ///     Gets the builder that can be used to configure this property.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the property has been removed from the model.</exception>
    new IConventionComplexPropertyBuilder Builder { get; }

    /// <summary>
    ///     Gets the associated complex type.
    /// </summary>
    new IConventionComplexType ComplexType { get; }

    /// <summary>
    ///     Sets a value indicating whether this property can contain <see langword="null" />.
    /// </summary>
    /// <param name="nullable">
    ///     A value indicating whether this property can contain <see langword="null" />.
    ///     <see langword="null" /> to reset to default.
    /// </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsNullable(bool? nullable, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyProperty.IsNullable" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyProperty.IsNullable" />.</returns>
    ConfigurationSource? GetIsNullableConfigurationSource();
}
