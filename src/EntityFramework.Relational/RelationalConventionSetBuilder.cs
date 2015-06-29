// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Relational.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalConventionSetBuilder : IConventionSetBuilder
    {
        public virtual ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            conventionSet.PropertyAddedConventions.Add(new RelationalColumnAttributeConvention());

            conventionSet.EntityTypeAddedConventions.Add(new RelationalTableAttributeConvention());

            return conventionSet;
        }
    }
}
