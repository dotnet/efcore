// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata.Conventions.Internal
{
    public class StoreKeyConvention : IEntityTypeAddedConvention, IForeignKeyOwnershipChangedConvention
    {
        public static readonly string IdPropertyName = "id";
        public static readonly string JObjectPropertyName = "__jObject";

        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder.Metadata.BaseType == null
                && entityTypeBuilder.Metadata.IsDocumentRoot())
            {
                var idProperty = entityTypeBuilder.Property(IdPropertyName, typeof(string), ConfigurationSource.Convention);
                idProperty.HasValueGenerator((_, __) => new StringValueGenerator(generateTemporaryValues: false), ConfigurationSource.Convention);
                entityTypeBuilder.HasKey(new[] { idProperty.Metadata }, ConfigurationSource.Convention);

                var jObjectProperty = entityTypeBuilder.Property(JObjectPropertyName, typeof(JObject), ConfigurationSource.Convention);
            }

            return entityTypeBuilder;
        }

        public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            if (relationshipBuilder.Metadata.IsOwnership)
            {
                var ownedType = relationshipBuilder.Metadata.DeclaringEntityType;
                var idProperty = ownedType.FindProperty(IdPropertyName);
                if (idProperty != null)
                {
                    var key = ownedType.FindKey(idProperty);
                    if (key != null)
                    {
                        ownedType.Builder.RemoveKey(key, ConfigurationSource.Convention);
                    }
                }

                var jObjectProperty = ownedType.FindProperty(JObjectPropertyName);
                if (jObjectProperty != null)
                {
                    ownedType.Builder.RemoveShadowPropertiesIfUnused(new[] { jObjectProperty });
                }
            }

            return relationshipBuilder;
        }
    }
}
