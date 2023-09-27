// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

    /// <inheritdoc/>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => ProcessEntityType(entityTypeBuilder);

    /// <inheritdoc/>
    public virtual void ProcessForeignKeyOwnershipChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
    {
        var entityType = relationshipBuilder.Metadata.DeclaringEntityType;

        ProcessEntityType(entityType.Builder);
    }

    /// <inheritdoc/>
    public virtual void ProcessForeignKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionForeignKey foreignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (entityTypeBuilder.Metadata.IsInModel
            && foreignKey.IsOwnership)
        {
            ProcessEntityType(entityTypeBuilder);
        }
    }

    /// <inheritdoc/>
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

    private static void ProcessEntityType(IConventionEntityTypeBuilder entityTypeBuilder)
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

    /// <inheritdoc/>
    public override void ProcessDiscriminatorPropertySet(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        IConventionContext<string> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (entityType.IsDocumentRoot())
        {
            base.ProcessDiscriminatorPropertySet(entityTypeBuilder, name, context);
        }
    }

    /// <inheritdoc/>
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

        var entityType = entityTypeBuilder.Metadata;
        if (newBaseType == null)
        {
            if (entityType.IsDocumentRoot())
            {
                entityTypeBuilder.HasDiscriminator(typeof(string));
            }
        }
        else
        {
            var rootType = newBaseType.GetRootType();
            if (!rootType.IsInModel
                || !rootType.IsDocumentRoot())
            {
                return;
            }

            var discriminator = rootType.Builder.HasDiscriminator(typeof(string));
            if (discriminator != null)
            {
                SetDefaultDiscriminatorValues(entityTypeBuilder.Metadata.GetDerivedTypesInclusive(), discriminator);
            }
        }
    }

    /// <inheritdoc/>
    protected override void SetDefaultDiscriminatorValues(
        IEnumerable<IConventionEntityType> entityTypes,
        IConventionDiscriminatorBuilder discriminatorBuilder)
    {
        foreach (var entityType in entityTypes)
        {
            discriminatorBuilder.HasValue(entityType, entityType.ShortName());
        }
    }

    /// <inheritdoc/>
    public override void ProcessEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType entityType,
        IConventionContext<IConventionEntityType> context)
    {
    }
}
