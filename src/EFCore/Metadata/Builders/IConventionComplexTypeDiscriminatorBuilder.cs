// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API surface for setting discriminator values from conventions.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionComplexTypeDiscriminatorBuilder
{
    /// <summary>
    ///     Gets the complex type on which the discriminator is being configured.
    /// </summary>
    IConventionComplexType ComplexType { get; }

    /// <summary>
    ///     Configures the discriminator value to use.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    IConventionComplexTypeDiscriminatorBuilder? HasValue(object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator value can be set from this configuration source.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the discriminator value can be set from this configuration source.</returns>
    bool CanSetValue(object? value, bool fromDataAnnotation = false);
}
