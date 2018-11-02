// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal
{
    public class StoreKeyConvention :
        IEntityTypeAddedConvention,
        IForeignKeyOwnershipChangedConvention,
        IEntityTypeAnnotationChangedConvention,
        IBaseTypeChangedConvention
    {
        public static readonly string IdPropertyName = "id";
        public static readonly string JObjectPropertyName = "__jObject";

        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (entityType.BaseType == null
                && entityType.IsDocumentRoot()
                && !entityType.IsQueryType)
            {
                var idProperty = entityTypeBuilder.Property(IdPropertyName, typeof(string), ConfigurationSource.Convention);
                idProperty.HasValueGenerator((_, __) => new StringValueGenerator(generateTemporaryValues: false), ConfigurationSource.Convention);
                entityTypeBuilder.HasKey(new[] { idProperty.Metadata }, ConfigurationSource.Convention);

                var jObjectProperty = entityTypeBuilder.Property(JObjectPropertyName, typeof(JObject), ConfigurationSource.Convention);
            }
            else
            {
                var idProperty = entityType.FindDeclaredProperty(IdPropertyName);
                if (idProperty != null)
                {
                    var key = entityType.FindKey(idProperty);
                    if (key != null)
                    {
                        entityType.Builder.RemoveKey(key, ConfigurationSource.Convention);
                    }
                }

                var jObjectProperty = entityType.FindDeclaredProperty(JObjectPropertyName);
                if (jObjectProperty != null)
                {
                    entityType.Builder.RemoveShadowPropertiesIfUnused(new[] { jObjectProperty });
                }
            }

            return entityTypeBuilder;
        }

        public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            Apply(relationshipBuilder.Metadata.DeclaringEntityType.Builder);

            return relationshipBuilder;
        }

        public Annotation Apply(InternalEntityTypeBuilder entityTypeBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if(name == CosmosAnnotationNames.ContainerName)
            {
                Apply(entityTypeBuilder);
            }

            return annotation;
        }

        public bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            Apply(entityTypeBuilder);

            return true;
        }
    }
}
