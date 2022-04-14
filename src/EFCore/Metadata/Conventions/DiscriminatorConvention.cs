// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the discriminator value for entity types in a hierarchy as the entity type name.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class DiscriminatorConvention : IEntityTypeBaseTypeChangedConvention, IEntityTypeRemovedConvention
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

    /// <summary>
    ///     Called after the base type of an entity type changes.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newBaseType">The new base entity type.</param>
    /// <param name="oldBaseType">The old base entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (oldBaseType != null
            && oldBaseType.IsInModel
            && oldBaseType.BaseType == null
            && !oldBaseType.GetDirectlyDerivedTypes().Any())
        {
            oldBaseType.Builder.HasNoDiscriminator();
        }

        var entityType = entityTypeBuilder.Metadata;
        var derivedEntityTypes = entityType.GetDerivedTypes().ToList();

        IConventionDiscriminatorBuilder? discriminator;
        if (newBaseType == null)
        {
            if (derivedEntityTypes.Count == 0)
            {
                entityTypeBuilder.HasNoDiscriminator();
                return;
            }

            discriminator = entityTypeBuilder.HasDiscriminator(typeof(string));
        }
        else
        {
            if (entityTypeBuilder.HasNoDiscriminator() == null)
            {
                return;
            }

            var rootTypeBuilder = entityType.GetRootType().Builder;
            discriminator = rootTypeBuilder.HasDiscriminator(typeof(string));

            if (newBaseType.BaseType == null)
            {
                discriminator?.HasValue(newBaseType, newBaseType.ShortName());
            }
        }

        if (discriminator != null)
        {
            discriminator.HasValue(entityTypeBuilder.Metadata, entityTypeBuilder.Metadata.ShortName());
            SetDefaultDiscriminatorValues(derivedEntityTypes, discriminator);
        }
    }

    /// <summary>
    ///     Called after an entity type is removed from the model.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="entityType">The removed entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
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
            discriminatorBuilder.HasValue(entityType, entityType.ShortName());
        }
    }
}
