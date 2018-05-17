// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Metadata
{
    public class CosmosSqlConventionSetBuilder : IConventionSetBuilder
    {
        public ConventionSet AddConventions(ConventionSet conventionSet)
        {
            var discriminatorConvention = new DiscriminatorConvention();

            conventionSet.EntityTypeAddedConventions.Add(discriminatorConvention);

            conventionSet.BaseEntityTypeChangedConventions.Add(discriminatorConvention);

            return conventionSet;
        }
    }
}
