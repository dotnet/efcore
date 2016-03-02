// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class SqlServerConventionSetBuilder : RelationalConventionSetBuilder
    {
        public SqlServerConventionSetBuilder(
            [NotNull] IRelationalTypeMapper typeMapper,
            [CanBeNull] DbContext context,
            [CanBeNull] IDbSetFinder setFinder)
            : base(typeMapper, context, setFinder)
        {
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            conventionSet.ModelInitializedConventions.Add(new SqlServerValueGenerationStrategyConvention());

            return conventionSet;
        }

        public static ConventionSet Build()
            => new SqlServerConventionSetBuilder(new SqlServerTypeMapper(), null, null)
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
