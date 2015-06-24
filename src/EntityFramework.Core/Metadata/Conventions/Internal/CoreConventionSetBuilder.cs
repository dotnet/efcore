// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class CoreConventionSetBuilder : ICoreConventionSetBuilder
    {
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = new ConventionSet();

            var relationshipDiscoveryConvention = new RelationshipDiscoveryConvention();
            var keyDiscoveryConvention = new KeyDiscoveryConvention();
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedEntityTypeAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedMemberAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(new InversePropertyAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.BaseEntityTypeSetConventions.Add(relationshipDiscoveryConvention);

            // An ambiguity might have been resolved
            conventionSet.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);

            var foreignKeyPropertyDiscoveryConvention = new ForeignKeyPropertyDiscoveryConvention();
            conventionSet.PropertyAddedConventions.Add(new ConcurrencyCheckAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new DatabaseGeneratedAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new RequiredPropertyAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new MaxLengthAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new StringLengthAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new TimestampAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.PropertyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            var keyAttributeConvention = new KeyAttributeConvention();
            conventionSet.PropertyAddedConventions.Add(keyAttributeConvention);

            var keyConvention = new KeyConvention();
            conventionSet.KeyAddedConventions.Add(keyConvention);

            conventionSet.PrimaryKeySetConventions.Add(keyConvention);

            var cascadeDeleteConvention = new CascadeDeleteConvention();
            conventionSet.ForeignKeyAddedConventions.Add(new ForeignKeyAttributeConvention());
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(cascadeDeleteConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(keyConvention);

            conventionSet.ModelBuiltConventions.Add(new ModelCleanupConvention());
            conventionSet.ModelBuiltConventions.Add(keyAttributeConvention);

            conventionSet.NavigationAddedConventions.Add(new RequiredNavigationAttributeConvention());
            conventionSet.NavigationAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.PropertyNullableChangedConventions.Add(cascadeDeleteConvention);

            return conventionSet;
        }
    }
}
