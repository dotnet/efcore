// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class KeyConvention
        : IKeyConvention, IPrimaryKeyConvention, IForeignKeyConvention, IForeignKeyRemovedConvention, IModelConvention
    {
        public virtual InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            SetValueGeneration(keyBuilder.Metadata.Properties);

            return keyBuilder;
        }

        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            foreach (var property in relationshipBuilder.Metadata.Properties)
            {
                var propertyBuilder = property.Builder;
                propertyBuilder.RequiresValueGenerator(false, ConfigurationSource.Convention);
                propertyBuilder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
            }

            return relationshipBuilder;
        }

        public virtual void Apply(InternalEntityTypeBuilder entityTypeBuilder, ForeignKey foreignKey)
        {
            var properties = foreignKey.Properties;
            SetValueGeneration(properties.Where(property => property.IsKey()));
            SetIdentity(properties, entityTypeBuilder.Metadata);
        }

        public virtual bool Apply(InternalKeyBuilder keyBuilder, Key previousPrimaryKey)
        {
            if (previousPrimaryKey != null)
            {
                foreach (var property in previousPrimaryKey.Properties)
                {
                    property.Builder?.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Convention);
                    property.Builder?.RequiresValueGenerator(false, ConfigurationSource.Convention);
                }
            }

            SetIdentity(keyBuilder.Metadata.Properties, keyBuilder.Metadata.DeclaringEntityType);

            return true;
        }

        private static void SetValueGeneration(IEnumerable<Property> properties)
        {
            var generatingProperties = properties.Where(property =>
                !property.IsForeignKey()
                && property.ValueGenerated == ValueGenerated.OnAdd);
            foreach (var propertyBuilder in generatingProperties)
            {
                propertyBuilder.Builder?.RequiresValueGenerator(true, ConfigurationSource.Convention);
            }
        }

        public virtual Property FindValueGeneratedOnAddProperty(
             [NotNull] IReadOnlyList<Property> properties, [NotNull] EntityType entityType)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            if (entityType.FindPrimaryKey(properties) != null
                && properties.Count == 1)
            {
                var property = properties.First();
                if (!property.IsForeignKey())
                {
                    var propertyType = property.ClrType.UnwrapNullableType();
                    if (propertyType.IsInteger()
                        || propertyType == typeof(Guid))
                    {
                        return property;
                    }
                }
            }
            return null;
        }

        private void SetIdentity(IReadOnlyList<Property> properties, EntityType entityType)
        {
            var candidateIdentityProperty = FindValueGeneratedOnAddProperty(properties, entityType);
            if (candidateIdentityProperty != null)
            {
                var propertyBuilder = candidateIdentityProperty.Builder;
                propertyBuilder?.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
                propertyBuilder?.RequiresValueGenerator(true, ConfigurationSource.Convention);
            }
        }

        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var key in entityType.GetDeclaredKeys())
                {
                    if (key.Properties.Any(p => p.IsShadowProperty
                                                && ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())))
                    {
                        string message;
                        var referencingFk = key.FindReferencingForeignKeys().FirstOrDefault();
                        if (referencingFk != null)
                        {
                            message = CoreStrings.ReferencedShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties),
                                Property.Format(referencingFk.Properties),
                                referencingFk.DeclaringEntityType.Name);
                        }
                        else
                        {
                            message = CoreStrings.ShadowKey(
                                Property.Format(key.Properties),
                                entityType.Name,
                                Property.Format(key.Properties));
                        }

                        throw new InvalidOperationException(message);
                    }
                }
            }

            return modelBuilder;
        }
    }
}
