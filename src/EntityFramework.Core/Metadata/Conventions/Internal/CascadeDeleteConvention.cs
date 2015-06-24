// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class CascadeDeleteConvention : IForeignKeyConvention, IPropertyNullableConvention
    {
        public virtual bool Apply(InternalPropertyBuilder propertyBuilder)
        {
            foreach (var foreignKey in propertyBuilder.Metadata.FindContainingForeignKeysInHierarchy())
            {
                Apply(propertyBuilder.ModelBuilder.Entity(foreignKey.DeclaringEntityType.Name, ConfigurationSource.Convention)
                    .Relationship(foreignKey, ConfigurationSource.Convention));
            }

            return true;
        }

        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            relationshipBuilder.DeleteBehavior(
                ((IForeignKey)relationshipBuilder.Metadata).IsRequired
                    ? DeleteBehavior.Cascade
                    : DeleteBehavior.Restrict,
                ConfigurationSource.Convention);

            return relationshipBuilder;
        }
    }
}
