// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CoreConventionSetBuilder : ICoreConventionSetBuilder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = new ConventionSet();

            var propertyDiscoveryConvention = new PropertyDiscoveryConvention();
            var keyDiscoveryConvention = new KeyDiscoveryConvention();
            var inversePropertyAttributeConvention = new InversePropertyAttributeConvention();
            var relationshipDiscoveryConvention = new RelationshipDiscoveryConvention();
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedEntityTypeAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedMemberAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new BaseTypeDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(propertyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(keyDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeAddedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.EntityTypeAddedConventions.Add(new DerivedTypeDiscoveryConvention());

            conventionSet.EntityTypeIgnoredConventions.Add(inversePropertyAttributeConvention);

            var foreignKeyIndexConvention = new ForeignKeyIndexConvention();
            conventionSet.BaseEntityTypeSetConventions.Add(propertyDiscoveryConvention);
            conventionSet.BaseEntityTypeSetConventions.Add(keyDiscoveryConvention);
            conventionSet.BaseEntityTypeSetConventions.Add(inversePropertyAttributeConvention);
            conventionSet.BaseEntityTypeSetConventions.Add(relationshipDiscoveryConvention);
            conventionSet.BaseEntityTypeSetConventions.Add(foreignKeyIndexConvention);

            // An ambiguity might have been resolved
            conventionSet.EntityTypeMemberIgnoredConventions.Add(inversePropertyAttributeConvention);
            conventionSet.EntityTypeMemberIgnoredConventions.Add(relationshipDiscoveryConvention);

            var keyAttributeConvention = new KeyAttributeConvention();
            var foreignKeyPropertyDiscoveryConvention = new ForeignKeyPropertyDiscoveryConvention();
            var backingFieldConvention = new BackingFieldConvention();
            conventionSet.PropertyAddedConventions.Add(backingFieldConvention);
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
            conventionSet.PrimaryKeySetConventions.Add(keyConvention);

            conventionSet.KeyAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.KeyRemovedConventions.Add(foreignKeyIndexConvention);

            var cascadeDeleteConvention = new CascadeDeleteConvention();
            conventionSet.ForeignKeyAddedConventions.Add(new ForeignKeyAttributeConvention());
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.ForeignKeyAddedConventions.Add(keyConvention);
            conventionSet.ForeignKeyAddedConventions.Add(cascadeDeleteConvention);
            conventionSet.ForeignKeyAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyRemovedConventions.Add(keyConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(relationshipDiscoveryConvention);
            conventionSet.ForeignKeyRemovedConventions.Add(foreignKeyIndexConvention);

            conventionSet.ForeignKeyUniquenessConventions.Add(foreignKeyIndexConvention);

            conventionSet.ModelBuiltConventions.Add(new ModelCleanupConvention());
            conventionSet.ModelBuiltConventions.Add(keyAttributeConvention);
            conventionSet.ModelBuiltConventions.Add(keyConvention);
            conventionSet.ModelBuiltConventions.Add(new IgnoredMembersValidationConvention());
            conventionSet.ModelBuiltConventions.Add(new PropertyMappingValidationConvention());
            conventionSet.ModelBuiltConventions.Add(new RelationshipValidationConvention());

            conventionSet.NavigationAddedConventions.Add(backingFieldConvention);
            conventionSet.NavigationAddedConventions.Add(new RequiredNavigationAttributeConvention());
            conventionSet.NavigationAddedConventions.Add(inversePropertyAttributeConvention);
            conventionSet.NavigationAddedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.NavigationAddedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.NavigationRemovedConventions.Add(relationshipDiscoveryConvention);

            conventionSet.IndexAddedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexRemovedConventions.Add(foreignKeyIndexConvention);

            conventionSet.IndexUniquenessConventions.Add(foreignKeyIndexConvention);

            conventionSet.PropertyNullableChangedConventions.Add(cascadeDeleteConvention);

            conventionSet.PrincipalEndSetConventions.Add(foreignKeyPropertyDiscoveryConvention);

            conventionSet.PropertyFieldChangedConventions.Add(keyDiscoveryConvention);
            conventionSet.PropertyFieldChangedConventions.Add(foreignKeyPropertyDiscoveryConvention);
            conventionSet.PropertyFieldChangedConventions.Add(keyAttributeConvention);

            return conventionSet;
        }
    }
}
