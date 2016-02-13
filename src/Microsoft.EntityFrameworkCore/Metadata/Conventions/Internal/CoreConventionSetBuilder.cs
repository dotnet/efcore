// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class CoreConventionSetBuilder : ICoreConventionSetBuilder
    {
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = new ConventionSet();

            var propertyDiscoveryConvention = new PropertyDiscoveryConvention();
            var keyDiscoveryConvention = new KeyDiscoveryConvention();
            var relationshipDiscoveryConvention = new RelationshipDiscoveryConvention();
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedEntityTypeAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedMemberAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new BaseTypeDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(propertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(new InversePropertyAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(new DerivedTypeDiscoveryConvention());

            conventionSet.BaseEntityTypeSetConventions.Add(propertyDiscoveryConvention);
            conventionSet.BaseEntityTypeSetConventions.Add(keyDiscoveryConvention);
            conventionSet.BaseEntityTypeSetConventions.Add(relationshipDiscoveryConvention);

            // An ambiguity might have been resolved
            conventionSet.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);

            var keyAttributeConvention = new KeyAttributeConvention();
            var foreignKeyPropertyDiscoveryConvention = new ForeignKeyPropertyDiscoveryConvention();
            conventionSet.PropertyAddedConventions.Add(new ConcurrencyCheckAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new DatabaseGeneratedAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new RequiredPropertyAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new MaxLengthAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new StringLengthAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new TimestampAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.PropertyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.PropertyAddedConventions.Add(keyAttributeConvention);

            var keyConvention = new KeyConvention();
            conventionSet.KeyAddedConventions.Add(keyConvention);

            conventionSet.PrimaryKeySetConventions.Add(keyConvention);

            var cascadeDeleteConvention = new CascadeDeleteConvention();
            conventionSet.ForeignKeyAddedConventions.Add(new ForeignKeyAttributeConvention());
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(keyConvention);
            conventionSet.ForeignKeyAddedConventions.Add(cascadeDeleteConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(keyConvention);

            conventionSet.ModelBuiltConventions.Add(new ModelCleanupConvention());
            conventionSet.ModelBuiltConventions.Add(keyAttributeConvention);
            conventionSet.ModelBuiltConventions.Add(keyConvention);
            conventionSet.ModelBuiltConventions.Add(new PropertyMappingValidationConvention());
            conventionSet.ModelBuiltConventions.Add(new RelationshipValidationConvention());

            conventionSet.NavigationAddedConventions.Add(new RequiredNavigationAttributeConvention());
            conventionSet.NavigationAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.NavigationAddedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.PropertyNullableChangedConventions.Add(cascadeDeleteConvention);

            conventionSet.PrincipalEndSetConventions.Add(foreignKeyPropertyDiscoveryConvention);

            return conventionSet;
        }
    }
}
