// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that creates a join entity type for a many-to-many relationship
///     and adds a partition key to it if the related types share one.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public class CosmosManyToManyJoinEntityTypeConvention :
    ManyToManyJoinEntityTypeConvention,
    IEntityTypeAnnotationChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="CosmosManyToManyJoinEntityTypeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CosmosManyToManyJoinEntityTypeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
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
        if (name is CosmosAnnotationNames.PartitionKeyName or CosmosAnnotationNames.ContainerName)
        {
            foreach (var skipNavigation in entityTypeBuilder.Metadata.GetSkipNavigations())
            {
                ProcessJoinPartitionKey(skipNavigation);
            }
        }
    }

    /// <inheritdoc />
    public override void ProcessSkipNavigationForeignKeyChanged(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionForeignKey? foreignKey,
        IConventionForeignKey? oldForeignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        base.ProcessSkipNavigationForeignKeyChanged(skipNavigationBuilder, foreignKey, oldForeignKey, context);

        if (oldForeignKey != null)
        {
            ProcessJoinPartitionKey(skipNavigationBuilder.Metadata);
        }
    }

    /// <inheritdoc />
    protected override void CreateJoinEntityType(string joinEntityTypeName, IConventionSkipNavigation skipNavigation)
    {
        if (ShouldSharePartitionKey(skipNavigation))
        {
            var model = skipNavigation.DeclaringEntityType.Model;
            var joinEntityTypeBuilder = model.Builder.SharedTypeEntity(joinEntityTypeName, typeof(Dictionary<string, object>))!;
            ConfigurePartitionKeyJoinEntityType(skipNavigation, joinEntityTypeBuilder);
        }
        else
        {
            base.CreateJoinEntityType(joinEntityTypeName, skipNavigation);
        }
    }

    private void ConfigurePartitionKeyJoinEntityType(
        IConventionSkipNavigation skipNavigation,
        IConventionEntityTypeBuilder joinEntityTypeBuilder)
    {
        var principalPartitionKey = skipNavigation.DeclaringEntityType.GetPartitionKeyProperty()!;
        var partitionKey = joinEntityTypeBuilder.Property(principalPartitionKey.ClrType, principalPartitionKey.Name)!.Metadata;
        joinEntityTypeBuilder.HasPartitionKey(partitionKey.Name);

        CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder, partitionKey);
        CreateSkipNavigationForeignKey(skipNavigation.Inverse!, joinEntityTypeBuilder, partitionKey);
    }

    private void CreateSkipNavigationForeignKey(
        IConventionSkipNavigation skipNavigation,
        IConventionEntityTypeBuilder joinEntityTypeBuilder,
        IConventionProperty partitionKeyProperty)
    {
        if (skipNavigation.ForeignKey != null
            && !skipNavigation.Builder.CanSetForeignKey(null))
        {
            return;
        }

        var principalKey = skipNavigation.DeclaringEntityType.FindPrimaryKey();
        if (principalKey == null
            || principalKey.Properties.All(p => p.Name != partitionKeyProperty.Name))
        {
            CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder);
            return;
        }

        if (skipNavigation.ForeignKey?.Properties.Contains(partitionKeyProperty) == true)
        {
            return;
        }

        var dependentProperties = new IConventionProperty[principalKey.Properties.Count];
        for (var i = 0; i < principalKey.Properties.Count; i++)
        {
            var principalProperty = principalKey.Properties[i];
            if (principalProperty.Name == partitionKeyProperty.Name)
            {
                dependentProperties[i] = partitionKeyProperty;
            }
            else
            {
                dependentProperties[i] = joinEntityTypeBuilder.CreateUniqueProperty(
                    principalProperty.ClrType, principalProperty.Name, required: true)!.Metadata;
            }
        }

        var foreignKey = joinEntityTypeBuilder.HasRelationship(skipNavigation.DeclaringEntityType, dependentProperties, principalKey)!
            .IsUnique(false)!
            .Metadata;

        skipNavigation.Builder.HasForeignKey(foreignKey);
    }

    private void ProcessJoinPartitionKey(IConventionSkipNavigation skipNavigation)
    {
        var inverseSkipNavigation = skipNavigation.Inverse;
        if (skipNavigation is { JoinEntityType: not null, IsCollection: true }
            && inverseSkipNavigation is { IsCollection: true }
            && inverseSkipNavigation.JoinEntityType == skipNavigation.JoinEntityType)
        {
            var joinEntityType = skipNavigation.JoinEntityType;
            var joinEntityTypeBuilder = joinEntityType.Builder;
            if (ShouldSharePartitionKey(skipNavigation))
            {
                var principalPartitionKey = skipNavigation.DeclaringEntityType.GetPartitionKeyProperty()!;
                var partitionKey = joinEntityType.GetPartitionKeyProperty();
                if ((partitionKey != null
                        && (!joinEntityTypeBuilder.CanSetPartitionKey(principalPartitionKey.Name)
                            || (skipNavigation.ForeignKey!.Properties.Contains(partitionKey)
                                && inverseSkipNavigation.ForeignKey!.Properties.Contains(partitionKey))))
                    || !skipNavigation.Builder.CanSetForeignKey(null)
                    || !inverseSkipNavigation.Builder.CanSetForeignKey(null))
                {
                    return;
                }

                ConfigurePartitionKeyJoinEntityType(skipNavigation, joinEntityTypeBuilder);
            }
            else
            {
                var partitionKey = joinEntityType.GetPartitionKeyProperty();
                if (partitionKey != null
                    && joinEntityTypeBuilder.HasPartitionKey(null) != null
                    && ((skipNavigation.ForeignKey!.Properties.Contains(partitionKey)
                            && skipNavigation.Builder.CanSetForeignKey(null))
                        || (inverseSkipNavigation.ForeignKey!.Properties.Contains(partitionKey)
                            && inverseSkipNavigation.Builder.CanSetForeignKey(null))))
                {
                    CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder);
                    CreateSkipNavigationForeignKey(inverseSkipNavigation, joinEntityTypeBuilder);
                }
            }
        }
    }

    private static bool ShouldSharePartitionKey(IConventionSkipNavigation skipNavigation)
        => skipNavigation.DeclaringEntityType.GetContainer() == skipNavigation.TargetEntityType.GetContainer()
            && skipNavigation.DeclaringEntityType.GetPartitionKeyPropertyName() != null
            && skipNavigation.Inverse?.DeclaringEntityType.GetPartitionKeyPropertyName()
            == skipNavigation.DeclaringEntityType.GetPartitionKeyPropertyName();
}
