// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API surface for setting discriminator values from conventions.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionDiscriminatorBuilder
{
    /// <summary>
    ///     Gets the entity type on which the discriminator is being configured.
    /// </summary>
    IConventionEntityType EntityType { get; }

    /// <summary>
    ///     Configures if the discriminator mapping is complete.
    /// </summary>
    /// <param name="complete">The value indicating if this discriminator mapping is complete.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    IConventionDiscriminatorBuilder? IsComplete(bool complete, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator mapping is complete can be set from this configuration source.
    /// </summary>
    /// <param name="complete">The value indicating if this discriminator mapping is complete.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the discriminator value can be set from this configuration source.</returns>
    bool CanSetIsComplete(bool complete, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the discriminator value to use.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    IConventionDiscriminatorBuilder? HasValue(object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Configures the discriminator value to use for entities of the given type.
    /// </summary>
    /// <param name="entityType">The entity type for which a discriminator value is being set.</param>
    /// <param name="value">The discriminator value.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    IConventionDiscriminatorBuilder? HasValue(
        IConventionEntityType entityType,
        object? value,
        bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator value can be set from this configuration source.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the discriminator value can be set from this configuration source.</returns>
    bool CanSetValue(object? value, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the discriminator value can be set from this configuration source.
    /// </summary>
    /// <param name="entityType">The entity type for which a discriminator value is being set.</param>
    /// <param name="value">The discriminator value.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the discriminator value can be set from this configuration source.</returns>
    bool CanSetValue(IConventionEntityType entityType, object? value, bool fromDataAnnotation = false)
    {
        if (!EntityType.IsAssignableFrom(entityType)
            && !entityType.Builder.CanSetBaseType(EntityType, fromDataAnnotation))
        {
            return false;
        }

        return entityType.Builder.CanSetAnnotation(CoreAnnotationNames.DiscriminatorValue, value, fromDataAnnotation);
    }
}
