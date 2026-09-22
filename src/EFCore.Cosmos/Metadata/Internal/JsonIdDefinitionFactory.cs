// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class JsonIdDefinitionFactory : IJsonIdDefinitionFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IJsonIdDefinition? Create(IEntityType entityType)
    {
        var primaryKey = entityType.FindPrimaryKey();
        if (entityType.GetForeignKeys().Any(fk => fk.IsOwnership) || primaryKey == null)
        {
            return null;
        }

        // Remove properties that are also partition keys, since Cosmos handles those separately, and so they should not be in `id`.
        var partitionKeyNames = entityType.GetPartitionKeyPropertyNames();
        var primaryKeyProperties = new List<IProperty>(primaryKey.Properties.Count);
        foreach (var property in primaryKey.Properties)
        {
            if (!partitionKeyNames.Contains(property.Name))
            {
                primaryKeyProperties.Add(property);
            }
        }

        if (!primaryKeyProperties.Any())
        {
            primaryKeyProperties = entityType.GetPartitionKeyProperties().ToList();
        }

        var properties = new List<IProperty>();

        // Add all primary key properties, except for those that are also partition keys, which were removed above.
        foreach (var property in primaryKeyProperties)
        {
            properties.Add(property);
        }

        var includeDiscriminator = entityType.GetDiscriminatorInKey();

        if (includeDiscriminator is IdDiscriminatorMode.EntityType or IdDiscriminatorMode.RootEntityType)
        {
            var discriminator = entityType.GetDiscriminatorValue();
            // If the discriminator is not part of the primary key already, then add it to the Cosmos `id`.
            if (discriminator != null)
            {
                var discriminatorProperty = entityType.FindDiscriminatorProperty();
                if (!primaryKey.Properties.Contains(discriminatorProperty))
                {
                    // Use the actual type for backwards compat, but the base type to allow lookup using ReadItem.
                    return includeDiscriminator is IdDiscriminatorMode.EntityType
                        ? new JsonIdDefinition(properties, entityType, false)
                        : new JsonIdDefinition(properties, entityType.GetRootType(), true);
                }
            }
        }

        return new JsonIdDefinition(properties);
    }
}
