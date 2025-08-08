// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API surface for setting discriminator values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class ComplexTypeDiscriminatorBuilder : IConventionComplexTypeDiscriminatorBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ComplexTypeDiscriminatorBuilder(IMutableComplexType complexType)
        => ComplexTypeBuilder = ((ComplexType)complexType).Builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalComplexTypeBuilder ComplexTypeBuilder { get; }

    /// <summary>
    ///     Configures the default discriminator value to use.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual ComplexTypeDiscriminatorBuilder HasValue(object? value)
        => HasValue(ComplexTypeBuilder, value, ConfigurationSource.Explicit)!;

    private ComplexTypeDiscriminatorBuilder? HasValue(
        InternalComplexTypeBuilder? complexTypeBuilder,
        object? value,
        ConfigurationSource configurationSource)
    {
        if (complexTypeBuilder == null)
        {
            return null;
        }

        var baseComplexTypeBuilder = ComplexTypeBuilder;
        if (!baseComplexTypeBuilder.Metadata.IsAssignableFrom(complexTypeBuilder.Metadata)
            && (!baseComplexTypeBuilder.Metadata.ClrType.IsAssignableFrom(complexTypeBuilder.Metadata.ClrType)
                || complexTypeBuilder.HasBaseType(baseComplexTypeBuilder.Metadata, configurationSource) == null))
        {
            throw new InvalidOperationException(
                CoreStrings.DiscriminatorEntityTypeNotDerived(
                    complexTypeBuilder.Metadata.DisplayName(),
                    baseComplexTypeBuilder.Metadata.DisplayName()));
        }

        if (configurationSource == ConfigurationSource.Explicit)
        {
            ((IMutableComplexType)complexTypeBuilder.Metadata).SetDiscriminatorValue(value);
        }
        else
        {
            if (!((IConventionComplexTypeDiscriminatorBuilder)this).CanSetValue(
                    value, configurationSource == ConfigurationSource.DataAnnotation))
            {
                return null;
            }

            ((IConventionComplexType)complexTypeBuilder.Metadata)
                .SetDiscriminatorValue(value, configurationSource == ConfigurationSource.DataAnnotation);
        }

        return this;
    }

    /// <inheritdoc />
    IConventionComplexType IConventionComplexTypeDiscriminatorBuilder.ComplexType
    {
        [DebuggerStepThrough]
        get => ComplexTypeBuilder.Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionComplexTypeDiscriminatorBuilder? IConventionComplexTypeDiscriminatorBuilder.HasValue(object? value, bool fromDataAnnotation)
        => HasValue(
            ComplexTypeBuilder, value,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    bool IConventionComplexTypeDiscriminatorBuilder.CanSetValue(object? value, bool fromDataAnnotation)
        => ((IConventionComplexTypeBuilder)ComplexTypeBuilder).CanSetAnnotation(CoreAnnotationNames.DiscriminatorValue, value, fromDataAnnotation);

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
