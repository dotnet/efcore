// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal
{
    public class CosmosConventionSetBuilder : IConventionSetBuilder
    {
        public ConventionSet AddConventions(ConventionSet conventionSet)
        {
            var discriminatorConvention = new DiscriminatorConvention();
            var storeKeyConvention = new StoreKeyConvention();
            conventionSet.EntityTypeAddedConventions.Add(storeKeyConvention);
            conventionSet.EntityTypeAddedConventions.Add(discriminatorConvention);

            conventionSet.BaseEntityTypeChangedConventions.Add(storeKeyConvention);
            conventionSet.BaseEntityTypeChangedConventions.Add(discriminatorConvention);

            conventionSet.ForeignKeyOwnershipChangedConventions.Add(storeKeyConvention);

            conventionSet.EntityTypeAnnotationChangedConventions.Add(storeKeyConvention);

            return conventionSet;
        }
    }
}
