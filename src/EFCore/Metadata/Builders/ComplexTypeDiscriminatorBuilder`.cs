// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API surface for setting discriminator values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
/// <typeparam name="TDiscriminator">The type of the discriminator property.</typeparam>
public class ComplexTypeDiscriminatorBuilder<TDiscriminator>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexTypeDiscriminatorBuilder(ComplexTypeDiscriminatorBuilder builder)
        => Builder = builder;

    private ComplexTypeDiscriminatorBuilder Builder { get; }

    /// <summary>
    ///     Configures the default discriminator value to use.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual ComplexTypeDiscriminatorBuilder<TDiscriminator> HasValue(TDiscriminator value)
        => new(Builder.HasValue(value));
}
