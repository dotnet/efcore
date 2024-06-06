// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions;

/// <summary>
///     A convention that builds an <see cref="JsonIdDefinition"/> for each top-level entity type. This is used
///     to build JSON `id` property values from combinations of other property values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public class JsonIdConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="JsonIdConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public JsonIdConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (entityType.IsOwned() || primaryKey == null)
            {
                entityType.RemoveAnnotation(CosmosAnnotationNames.JsonIdDefinition);
                continue;
            }

            // Remove properties that are also partition keys, since Cosmos handles those separately, and so they should not be in `id`.
            var partitionKeyNames = entityType.GetPartitionKeyPropertyNames();
            var primaryKeyProperties = new List<IConventionProperty>(primaryKey.Properties.Count);
            foreach (var property in primaryKey.Properties)
            {
                if (!partitionKeyNames.Contains(property.Name))
                {
                    primaryKeyProperties.Add(property);
                }
            }

            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.GetJsonPropertyName() == StoreKeyConvention.IdPropertyJsonName);

            var properties = new List<IConventionProperty>();
            // If the property mapped to the JSON id is simply the primary key, or is the primary key without partition keys, then use
            // it directly.
            if ((primaryKeyProperties.Count == 1
                    && primaryKeyProperties[0] == idProperty)
                || (primaryKey.Properties.Count == 1
                    && primaryKey.Properties[0] == idProperty))
            {
                properties.Add(idProperty);
            }
            // Otherwise, if the property mapped to the JSON id doesn't have a generator, then we can't use ReadItem.
            else if (idProperty != null && idProperty.GetValueGeneratorFactory() == null)
            {
                entityType.RemoveAnnotation(CosmosAnnotationNames.JsonIdDefinition);
                continue;
            }
            else
            {
                var discriminator = entityType.GetDiscriminatorValue();
                // If the discriminator is not part of the primary key already, then add it to the Cosmos `id`.
                if (discriminator != null)
                {
                    var discriminatorProperty = entityType.FindDiscriminatorProperty();
                    if (!primaryKey.Properties.Contains(discriminatorProperty))
                    {
                        properties.Add(discriminatorProperty!);
                    }
                }

                // Next add all primary key properties, except for those that are also partition keys, which were removed above.
                foreach (var property in primaryKeyProperties)
                {
                    properties.Add(property);
                }
            }

            entityType.SetAnnotation(CosmosAnnotationNames.JsonIdDefinition, new JsonIdDefinition(properties));
        }
    }
}
