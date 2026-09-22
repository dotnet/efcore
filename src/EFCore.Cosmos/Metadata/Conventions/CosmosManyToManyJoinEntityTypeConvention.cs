// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

// ReSharper disable once CheckNamespace
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
        if (name is CosmosAnnotationNames.PartitionKeyNames or CosmosAnnotationNames.ContainerName)
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
        // The join entity type should belong to the same partition as the entity types on either side.
        var principalProperties = skipNavigation.DeclaringEntityType.GetPartitionKeyProperties();
        if (!principalProperties.Any() || principalProperties.Any(p => p is null))
        {
            return;
        }

        var partitionKeyProperties = principalProperties.Select(p => joinEntityTypeBuilder.Property(p!.ClrType, p.Name)!.Metadata).ToList();
        joinEntityTypeBuilder.HasPartitionKey(partitionKeyProperties.Select(p => p.Name).ToList());

        CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder, partitionKeyProperties);
        CreateSkipNavigationForeignKey(skipNavigation.Inverse!, joinEntityTypeBuilder, partitionKeyProperties);
    }

    private void CreateSkipNavigationForeignKey(
        IConventionSkipNavigation skipNavigation,
        IConventionEntityTypeBuilder joinEntityTypeBuilder,
        List<IConventionProperty> partitionKeyProperties)
    {
        if (skipNavigation.ForeignKey != null
            && !skipNavigation.Builder.CanSetForeignKey(null))
        {
            return;
        }

        var principalKey = skipNavigation.DeclaringEntityType.FindPrimaryKey();
        if (principalKey == null
            || principalKey.Properties.All(p => !partitionKeyProperties.Select(e => e.Name).Contains(p.Name)))
        {
            CreateSkipNavigationForeignKey(skipNavigation, joinEntityTypeBuilder);
            return;
        }

        // Any partition key property that already exists should be used for the FK, otherwise a new property is created.
        var dependentProperties = new IConventionProperty[principalKey.Properties.Count];
        for (var i = 0; i < principalKey.Properties.Count; i++)
        {
            var principalProperty = principalKey.Properties[i];
            var partitionKeyProperty = partitionKeyProperties.FirstOrDefault(p => p.Name == principalProperty.Name);
            if (partitionKeyProperty != null)
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
                var principalPartitionProperties = skipNavigation.DeclaringEntityType.GetPartitionKeyProperties();
                var partitionKeyProperties = joinEntityType.GetPartitionKeyProperties();
                if ((partitionKeyProperties.Any()
                        && (!joinEntityTypeBuilder.CanSetPartitionKey(principalPartitionProperties.Select(p => p!.Name).ToList())
                            || (partitionKeyProperties.All(p => skipNavigation.ForeignKey!.Properties.Contains(p))
                                && partitionKeyProperties.All(p => inverseSkipNavigation.ForeignKey!.Properties.Contains(p)))))
                    || !skipNavigation.Builder.CanSetForeignKey(null)
                    || !inverseSkipNavigation.Builder.CanSetForeignKey(null))
                {
                    return;
                }

                ConfigurePartitionKeyJoinEntityType(skipNavigation, joinEntityTypeBuilder);
            }
            else
            {
                var partitionKeyProperties = joinEntityType.GetPartitionKeyProperties();
                if (partitionKeyProperties.Any()
                    && joinEntityTypeBuilder.HasPartitionKey((IReadOnlyList<string>?)null) != null
                    && ((partitionKeyProperties.Any(p => skipNavigation.ForeignKey!.Properties.Contains(p))
                            && skipNavigation.Builder.CanSetForeignKey(null))
                        || (partitionKeyProperties.Any(p => inverseSkipNavigation.ForeignKey!.Properties.Contains(p))
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
            && skipNavigation.DeclaringEntityType.GetPartitionKeyPropertyNames().Any()
            && (skipNavigation.Inverse?.DeclaringEntityType.GetPartitionKeyPropertyNames()
                    .SequenceEqual(skipNavigation.DeclaringEntityType.GetPartitionKeyPropertyNames(), StringComparer.Ordinal)
                == true);
}
