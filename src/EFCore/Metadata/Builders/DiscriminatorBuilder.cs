// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API surface for setting discriminator values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public class DiscriminatorBuilder : IConventionDiscriminatorBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public DiscriminatorBuilder(IMutableEntityType entityType)
    {
        EntityTypeBuilder = ((EntityType)entityType).Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalEntityTypeBuilder EntityTypeBuilder { get; }

    /// <summary>
    ///     Configures if the discriminator mapping is complete.
    /// </summary>
    /// <param name="complete">The value indicating if this discriminator mapping is complete.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual DiscriminatorBuilder IsComplete(bool complete = true)
        => IsComplete(complete, ConfigurationSource.Explicit)!;

    private DiscriminatorBuilder? IsComplete(bool complete, ConfigurationSource configurationSource)
    {
        if (configurationSource == ConfigurationSource.Explicit)
        {
            ((IMutableEntityType)EntityTypeBuilder.Metadata).SetDiscriminatorMappingComplete(complete);
        }
        else
        {
            if (!((IConventionEntityTypeBuilder)EntityTypeBuilder).CanSetAnnotation(
                    CoreAnnotationNames.DiscriminatorMappingComplete, complete,
                    configurationSource == ConfigurationSource.DataAnnotation))
            {
                return null;
            }

            ((IConventionEntityType)EntityTypeBuilder.Metadata).SetDiscriminatorMappingComplete(
                complete, configurationSource == ConfigurationSource.DataAnnotation);
        }

        return this;
    }

    /// <summary>
    ///     Configures the default discriminator value to use.
    /// </summary>
    /// <param name="value">The discriminator value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual DiscriminatorBuilder HasValue(object? value)
        => HasValue(EntityTypeBuilder, value, ConfigurationSource.Explicit)!;

    /// <summary>
    ///     Configures the discriminator value to use for entities of the given generic type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for which a discriminator value is being set.</typeparam>
    /// <param name="value">The discriminator value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual DiscriminatorBuilder HasValue<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>(
        object? value)
        => HasValue(typeof(TEntity), value);

    /// <summary>
    ///     Configures the discriminator value to use for entities of the given type.
    /// </summary>
    /// <param name="entityType">The entity type for which a discriminator value is being set.</param>
    /// <param name="value">The discriminator value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual DiscriminatorBuilder HasValue(
        [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type entityType,
        object? value)
    {
        var entityTypeBuilder = EntityTypeBuilder.ModelBuilder.Entity(
            entityType, ConfigurationSource.Explicit);

        return HasValue(entityTypeBuilder, value, ConfigurationSource.Explicit)!;
    }

    /// <summary>
    ///     Configures the discriminator value to use for entities of the given type.
    /// </summary>
    /// <param name="entityTypeName">The name of the entity type for which a discriminator value is being set.</param>
    /// <param name="value">The discriminator value.</param>
    /// <returns>The same builder so that multiple calls can be chained.</returns>
    public virtual DiscriminatorBuilder HasValue(string entityTypeName, object? value)
    {
        var entityTypeBuilder = EntityTypeBuilder.ModelBuilder.Entity(
            entityTypeName, ConfigurationSource.Explicit);

        return HasValue(entityTypeBuilder, value, ConfigurationSource.Explicit)!;
    }

    private DiscriminatorBuilder? HasValue(
        InternalEntityTypeBuilder? entityTypeBuilder,
        object? value,
        ConfigurationSource configurationSource)
    {
        if (entityTypeBuilder == null)
        {
            return null;
        }

        var baseEntityTypeBuilder = EntityTypeBuilder;
        if (!baseEntityTypeBuilder.Metadata.IsAssignableFrom(entityTypeBuilder.Metadata)
            && (!baseEntityTypeBuilder.Metadata.ClrType.IsAssignableFrom(entityTypeBuilder.Metadata.ClrType)
                || entityTypeBuilder.HasBaseType(baseEntityTypeBuilder.Metadata, configurationSource) == null))
        {
            throw new InvalidOperationException(
                CoreStrings.DiscriminatorEntityTypeNotDerived(
                    entityTypeBuilder.Metadata.DisplayName(),
                    baseEntityTypeBuilder.Metadata.DisplayName()));
        }

        if (configurationSource == ConfigurationSource.Explicit)
        {
            ((IMutableEntityType)entityTypeBuilder.Metadata).SetDiscriminatorValue(value);
        }
        else
        {
            if (!((IConventionDiscriminatorBuilder)this).CanSetValue(
                    entityTypeBuilder.Metadata, value, configurationSource == ConfigurationSource.DataAnnotation))
            {
                return null;
            }

            ((IConventionEntityType)entityTypeBuilder.Metadata)
                .SetDiscriminatorValue(value, configurationSource == ConfigurationSource.DataAnnotation);
        }

        return this;
    }

    /// <inheritdoc />
    IConventionEntityType IConventionDiscriminatorBuilder.EntityType
    {
        [DebuggerStepThrough]
        get => EntityTypeBuilder.Metadata;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionDiscriminatorBuilder.IsComplete(bool complete, bool fromDataAnnotation)
        => IsComplete(complete, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    bool IConventionDiscriminatorBuilder.CanSetIsComplete(bool complete, bool fromDataAnnotation)
        => ((IConventionEntityTypeBuilder)EntityTypeBuilder).CanSetAnnotation(
            CoreAnnotationNames.DiscriminatorMappingComplete, fromDataAnnotation);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionDiscriminatorBuilder.HasValue(object? value, bool fromDataAnnotation)
        => HasValue(
            EntityTypeBuilder, value,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    [DebuggerStepThrough]
    IConventionDiscriminatorBuilder? IConventionDiscriminatorBuilder.HasValue(
        IConventionEntityType entityType,
        object? value,
        bool fromDataAnnotation)
        => HasValue(
            (InternalEntityTypeBuilder?)entityType.Builder, value,
            fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

    /// <inheritdoc />
    bool IConventionDiscriminatorBuilder.CanSetValue(object? value, bool fromDataAnnotation)
        => ((IConventionDiscriminatorBuilder)this).CanSetValue(EntityTypeBuilder.Metadata, value, fromDataAnnotation);

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
