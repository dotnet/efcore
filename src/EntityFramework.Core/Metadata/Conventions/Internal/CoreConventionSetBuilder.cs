// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class CoreConventionSetBuilder : ICoreConventionSetBuilder
    {
        public virtual ConventionSet CreateConventionSet()
        {
            var conventionSet = new ConventionSet();

            conventionSet.EntityTypeAddedConventions.Add(new NotMappedEntityTypeAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(new NotMappedNavigationAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new InversePropertyAttributeConvention());
            conventionSet.EntityTypeAddedConventions.Add(new RelationshipDiscoveryConvention());

            conventionSet.PropertyAddedConventions.Add(new NotMappedPropertyAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new ConcurrencyCheckAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new DatabaseGeneratedAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new RequiredPropertyAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new MaxLengthAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new StringLengthAttributeConvention());
            conventionSet.PropertyAddedConventions.Add(new TimestampAttributeConvention());

            var keyAttributeConvention = new KeyAttributeConvention();
            conventionSet.PropertyAddedConventions.Add(keyAttributeConvention);

            var keyConvention = new KeyConvention();
            conventionSet.KeyAddedConventions.Add(keyConvention);

            conventionSet.ForeignKeyAddedConventions.Add(new ForeignKeyAttributeConvention());
            conventionSet.ForeignKeyAddedConventions.Add(new ForeignKeyPropertyDiscoveryConvention());

            conventionSet.ForeignKeyRemovedConventions.Add(keyConvention);

            conventionSet.ModelBuiltConventions.Add(keyAttributeConvention);

            conventionSet.NavigationAddedConventions.Add(new RequiredNavigationAttributeConvention());

            return conventionSet;
        }
    }
}
