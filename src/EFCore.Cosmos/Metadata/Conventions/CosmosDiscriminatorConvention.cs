// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the discriminator value for entity types as the entity type name.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public class CosmosDiscriminatorConvention :
    DiscriminatorConvention,
    IForeignKeyOwnershipChangedConvention,
    IForeignKeyRemovedConvention,
    IEntityTypeAddedConvention,
    IEntityTypeAnnotationChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="CosmosDiscriminatorConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CosmosDiscriminatorConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => ProcessEntityType(entityTypeBuilder);

    /// <summary>
    ///     Called after the ownership value for a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyOwnershipChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
    {
        var entityType = relationshipBuilder.Metadata.DeclaringEntityType;

        ProcessEntityType(entityType.Builder);
    }

    /// <summary>
    ///     Called after a foreign key is removed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="foreignKey">The removed foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionForeignKey foreignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (foreignKey.IsOwnership)
        {
            ProcessEntityType(entityTypeBuilder);
        }
    }

    /// <summary>
    ///     Called after an annotation is changed on an entity type.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name != CosmosAnnotationNames.ContainerName
            || (annotation == null) == (oldAnnotation == null))
        {
            return;
        }

        ProcessEntityType(entityTypeBuilder);
    }

    private void ProcessEntityType(IConventionEntityTypeBuilder entityTypeBuilder)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (entityType.BaseType != null)
        {
            return;
        }

        if (entityType.IsDocumentRoot())
        {
            entityTypeBuilder.HasDiscriminator(typeof(string))
                ?.HasValue(entityType, entityType.ShortName());
        }
        else
        {
            entityTypeBuilder.HasNoDiscriminator();
        }
    }

    /// <summary>
    ///     Called after the base type of an entity type changes.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newBaseType">The new base entity type.</param>
    /// <param name="oldBaseType">The old base entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (entityTypeBuilder.Metadata.BaseType != newBaseType)
        {
            return;
        }

        IConventionDiscriminatorBuilder? discriminator = null;
        var entityType = entityTypeBuilder.Metadata;
        if (newBaseType == null)
        {
            if (entityType.IsDocumentRoot())
            {
                discriminator = entityTypeBuilder.HasDiscriminator(typeof(string));
            }
        }
        else
        {
            var rootType = newBaseType.GetRootType();
            discriminator = rootType.IsInModel
                ? rootType.Builder.HasDiscriminator(typeof(string))
                : null;

            if (newBaseType.BaseType == null)
            {
                discriminator?.HasValue(newBaseType, newBaseType.ShortName());
            }
        }

        if (discriminator != null)
        {
            discriminator.HasValue(entityTypeBuilder.Metadata, entityTypeBuilder.Metadata.ShortName());
            SetDefaultDiscriminatorValues(entityType.GetDerivedTypes(), discriminator);
        }
    }

    /// <summary>
    ///     Called after an entity type is removed from the model.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="entityType">The removed entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType entityType,
        IConventionContext<IConventionEntityType> context)
    {
    }
}
