// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the discriminator value for entity types in a hierarchy as the entity type name.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class DiscriminatorConvention :
    IEntityTypeBaseTypeChangedConvention,
    IEntityTypeRemovedConvention,
    IDiscriminatorPropertySetConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="DiscriminatorConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public DiscriminatorConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc/>
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (oldBaseType is { IsInModel: true, BaseType: null }
            && !oldBaseType.GetDirectlyDerivedTypes().Any())
        {
            oldBaseType.Builder.HasNoDiscriminator();
        }

        var entityType = entityTypeBuilder.Metadata;
        if (newBaseType == null)
        {
            if (!entityType.GetDerivedTypes().Any())
            {
                entityTypeBuilder.HasNoDiscriminator();
                return;
            }

            entityTypeBuilder.HasDiscriminator(typeof(string));
        }
        else
        {
            if (entityTypeBuilder.HasNoDiscriminator() == null)
            {
                return;
            }

            var rootType = entityType.GetRootType();
            if (rootType.FindDiscriminatorProperty() == null)
            {
                rootType.Builder.HasDiscriminator(typeof(string));
            }
            else
            {
                var discriminator = entityTypeBuilder.HasDiscriminator(typeof(string));
                if (discriminator != null)
                {
                    SetDefaultDiscriminatorValues(entityTypeBuilder.Metadata.GetDerivedTypesInclusive(), discriminator);
                }
            }
        }
    }

    /// <inheritdoc/>
    public virtual void ProcessDiscriminatorPropertySet(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        IConventionContext<string> context)
    {
        if (name == null)
        {
            return;
        }

        var discriminator = entityTypeBuilder.HasDiscriminator(typeof(string));
        if (discriminator != null)
        {
            SetDefaultDiscriminatorValues(entityTypeBuilder.Metadata.GetDerivedTypesInclusive(), discriminator);
        }
    }

    /// <inheritdoc/>
    public virtual void ProcessEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType entityType,
        IConventionContext<IConventionEntityType> context)
    {
        var oldBaseType = entityType.BaseType;
        if (oldBaseType != null
            && oldBaseType.IsInModel
            && oldBaseType.BaseType == null
            && !oldBaseType.GetDirectlyDerivedTypes().Any())
        {
            oldBaseType.Builder.HasNoDiscriminator();
        }
    }

    /// <summary>
    ///     Configures the discriminator values for the given entity types.
    /// </summary>
    /// <param name="entityTypes">The entity types to configure.</param>
    /// <param name="discriminatorBuilder">The discriminator builder.</param>
    protected virtual void SetDefaultDiscriminatorValues(
        IEnumerable<IConventionEntityType> entityTypes,
        IConventionDiscriminatorBuilder discriminatorBuilder)
    {
        foreach (var entityType in entityTypes)
        {
            discriminatorBuilder.HasValue(entityType, entityType.GetDefaultDiscriminatorValue());
        }
    }
}
