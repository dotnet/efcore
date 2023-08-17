// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that adds the 'id' property - a key required by Azure Cosmos.
/// </summary>
/// <remarks>
///     <para>
///         This convention also adds the '__jObject' containing the JSON object returned by the store.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public class StoreKeyConvention :
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
    public static readonly string IdPropertyJsonName = "id";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static readonly string DefaultIdPropertyName = "__id";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static readonly string JObjectPropertyName = "__jObject";

    /// <summary>
    ///     Creates a new instance of <see cref="StoreKeyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public StoreKeyConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    private static void ProcessIdProperty(IConventionEntityTypeBuilder entityTypeBuilder)
    {
        IConventionKey? newKey = null;
        IConventionProperty? idProperty;
        var entityType = entityTypeBuilder.Metadata;
        if (entityType.BaseType == null
            && entityType.IsDocumentRoot()
            && !entityType.IsKeyless)
        {
            idProperty = entityType.FindDeclaredProperty(DefaultIdPropertyName)
                ?? entityType.GetDeclaredProperties().FirstOrDefault(p => p.GetJsonPropertyName() == IdPropertyJsonName)
                ?? entityTypeBuilder.Property(typeof(string), DefaultIdPropertyName, setTypeConfigurationSource: false)
                    ?.ToJsonProperty(IdPropertyJsonName)?.Metadata;

            if (idProperty != null)
            {
                if (idProperty.ClrType == typeof(string))
                {
                    if (idProperty.IsPrimaryKey())
                    {
                        idProperty.Builder.HasValueGenerator((Type?)null);
                    }
                    else
                    {
                        idProperty.Builder.HasValueGeneratorFactory(typeof(IdValueGeneratorFactory));
                    }
                }

                var partitionKey = entityType.GetPartitionKeyPropertyName();
                if (partitionKey != null)
                {
                    var partitionKeyProperty = entityType.FindProperty(partitionKey);
                    if (partitionKeyProperty == null
                        || partitionKeyProperty == idProperty)
                    {
                        newKey = entityTypeBuilder.HasKey(new[] { idProperty })?.Metadata;
                    }
                    else
                    {
                        if (entityType.FindKey(new[] { partitionKeyProperty, idProperty }) == null)
                        {
                            newKey = entityTypeBuilder.HasKey(new[] { idProperty, partitionKeyProperty })?.Metadata;
                        }

                        entityTypeBuilder.HasNoKey(new[] { idProperty });
                    }
                }
                else
                {
                    newKey = entityTypeBuilder.HasKey(new[] { idProperty })?.Metadata;
                }
            }
        }
        else
        {
            idProperty = entityType.FindDeclaredProperty(DefaultIdPropertyName);
        }

        if (idProperty != null
            && idProperty.GetContainingKeys().Count() > (newKey == null ? 0 : 1))
        {
            foreach (var key in idProperty.GetContainingKeys().ToList())
            {
                if (key != newKey)
                {
                    key.DeclaringEntityType.Builder.HasNoKey(key);
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
        if ((newPrimaryKey != null && newPrimaryKey.Properties.Any(p => p.GetJsonPropertyName() == IdPropertyJsonName))
            || (previousPrimaryKey != null && previousPrimaryKey.Properties.Any(p => p.GetJsonPropertyName() == IdPropertyJsonName)))
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
        else if (name == CosmosAnnotationNames.PartitionKeyName)
        {
            var oldName = (string?)oldAnnotation?.Value;
            if (oldName != null)
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
            && (string?)annotation?.Value == IdPropertyJsonName
            && propertyBuilder.Metadata.Name != DefaultIdPropertyName)
        {
            var declaringType = propertyBuilder.Metadata.DeclaringType;

            var idProperty = declaringType.FindProperty(DefaultIdPropertyName);
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
