// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that adds partition key properties to the EF primary key and adds the '__jObject' containing the JSON
///     object returned by the store.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public class CosmosPartitionKeyInPrimaryKeyConvention :
    IEntityTypeAddedConvention,
    IPropertyAnnotationChangedConvention,
    IForeignKeyOwnershipChangedConvention,
    IForeignKeyRemovedConvention,
    IKeyAddedConvention,
    IKeyRemovedConvention,
    IEntityTypePrimaryKeyChangedConvention,
    IEntityTypeAnnotationChangedConvention,
    IEntityTypeBaseTypeChangedConvention
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static readonly string JObjectPropertyName = "__jObject";

    /// <summary>
    ///     Creates a new instance of <see cref="CosmosPartitionKeyInPrimaryKeyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CosmosPartitionKeyInPrimaryKeyConvention(ProviderConventionSetBuilderDependencies dependencies)
        => Dependencies = dependencies;

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    private static void ProcessIdProperty(IConventionEntityTypeBuilder entityTypeBuilder)
    {
        var entityType = entityTypeBuilder.Metadata;
        var primaryKey = entityType.FindPrimaryKey();

        if (entityType.BaseType == null
            && entityType.IsDocumentRoot()
            && primaryKey != null
            && ConfigurationSource.Convention.Overrides(primaryKey.GetConfigurationSource()))
        {
            // Add partition key properties to the primary key, unless they are already in the primary key.
            var partitionKeyProperties = entityType.GetPartitionKeyProperties();
            var primaryKeyProperties = primaryKey.Properties.ToList();
            var keyContainsPartitionProperties = false;
            if (partitionKeyProperties.Any()
                && partitionKeyProperties.All(p => p != null))
            {
                foreach (var partitionKeyProperty in partitionKeyProperties)
                {
                    if (!primaryKeyProperties.Contains(partitionKeyProperty!))
                    {
                        primaryKeyProperties.Add(partitionKeyProperty!);
                        keyContainsPartitionProperties = true;
                    }
                }

                if (keyContainsPartitionProperties)
                {
                    primaryKey.DeclaringEntityType.Builder.HasNoKey(primaryKey);
                    entityTypeBuilder.PrimaryKey(primaryKeyProperties);
                }
            }
        }
    }

    private static void ProcessJObjectProperty(IConventionEntityTypeBuilder entityTypeBuilder)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (entityType.BaseType == null
            && !entityType.IsKeyless)
        {
            var jObjectProperty = entityTypeBuilder.Property(typeof(JObject), JObjectPropertyName);
            jObjectProperty?.ToJsonProperty("");
            jObjectProperty?.ValueGenerated(ValueGenerated.OnAddOrUpdate);
        }
        else
        {
            var jObjectProperty = entityType.FindDeclaredProperty(JObjectPropertyName);
            if (jObjectProperty != null)
            {
                entityType.Builder.RemoveUnusedImplicitProperties(new[] { jObjectProperty });
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        ProcessIdProperty(entityTypeBuilder);
        ProcessJObjectProperty(entityTypeBuilder);
    }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyOwnershipChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
        => ProcessIdProperty(relationshipBuilder.Metadata.DeclaringEntityType.Builder);

    /// <inheritdoc />
    public virtual void ProcessForeignKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionForeignKey foreignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (entityTypeBuilder.Metadata.IsInModel
            && foreignKey.IsOwnership)
        {
            ProcessIdProperty(foreignKey.DeclaringEntityType.Builder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessKeyAdded(
        IConventionKeyBuilder keyBuilder,
        IConventionContext<IConventionKeyBuilder> context)
    {
        var entityTypeBuilder = keyBuilder.Metadata.DeclaringEntityType.Builder;
        if (entityTypeBuilder.Metadata.GetKeys().Count() == 1)
        {
            ProcessIdProperty(entityTypeBuilder);
            ProcessJObjectProperty(entityTypeBuilder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey key,
        IConventionContext<IConventionKey> context)
    {
        if (!entityTypeBuilder.Metadata.IsInModel)
        {
            return;
        }

        if (entityTypeBuilder.Metadata.IsKeyless)
        {
            ProcessIdProperty(entityTypeBuilder);
            ProcessJObjectProperty(entityTypeBuilder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypePrimaryKeyChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey? newPrimaryKey,
        IConventionKey? previousPrimaryKey,
        IConventionContext<IConventionKey> context)
    {
        if ((newPrimaryKey != null
                && newPrimaryKey.Properties
                    .Any(p => p.GetJsonPropertyName() == CosmosJsonIdConvention.IdPropertyJsonName))
            || (previousPrimaryKey != null
                && previousPrimaryKey.Properties
                    .Any(p => p.GetJsonPropertyName() == CosmosJsonIdConvention.IdPropertyJsonName)))
        {
            ProcessIdProperty(entityTypeBuilder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (entityTypeBuilder.Metadata.BaseType == newBaseType)
        {
            ProcessIdProperty(entityTypeBuilder);
            ProcessJObjectProperty(entityTypeBuilder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == CosmosAnnotationNames.ContainerName
            && (annotation?.Value == null
                || oldAnnotation?.Value == null))
        {
            ProcessIdProperty(entityTypeBuilder);
        }
        else if (name == CosmosAnnotationNames.PartitionKeyNames)
        {
            var oldNames = (IReadOnlyList<string>?)oldAnnotation?.Value;
            if (oldNames != null)
            {
                var newNames = (IReadOnlyList<string>?)annotation?.Value;
                foreach (var oldName in oldNames)
                {
                    if (newNames?.Contains(oldName) != true)
                    {
                        var oldPartitionKeyProperty = entityTypeBuilder.Metadata.FindProperty(oldName);
                        if (oldPartitionKeyProperty != null)
                        {
                            foreach (var key in oldPartitionKeyProperty.GetContainingKeys().ToList())
                            {
                                key.DeclaringEntityType.Builder.HasNoKey(key);
                            }
                        }
                    }
                }
            }

            ProcessIdProperty(entityTypeBuilder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == CosmosAnnotationNames.PropertyName
            && (string?)annotation?.Value == CosmosJsonIdConvention.IdPropertyJsonName
            && propertyBuilder.Metadata.Name != CosmosJsonIdConvention.DefaultIdPropertyName)
        {
            var declaringType = propertyBuilder.Metadata.DeclaringType;

            var idProperty = declaringType.FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName);
            if (idProperty != null)
            {
                foreach (var key in idProperty.GetContainingKeys().ToList())
                {
                    key.DeclaringEntityType.Builder.HasNoKey(key);
                }
            }

            ProcessIdProperty(declaringType.ContainingEntityType.Builder);
        }
    }
}
