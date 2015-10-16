// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Conventions.Internal
{
    public class SqlServerConventionSetBuilder : RelationalConventionSetBuilder
    {
        public SqlServerConventionSetBuilder([NotNull] IRelationalTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            conventionSet.ModelInitializedConventions.Add(new SqlServerValueGenerationStrategyConvention());

            return conventionSet;
        }
    }
}
