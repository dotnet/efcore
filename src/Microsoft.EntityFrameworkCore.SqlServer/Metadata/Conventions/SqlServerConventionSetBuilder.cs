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
            [CanBeNull] ICurrentDbContext currentContext,
            [CanBeNull] IDbSetFinder setFinder)
            : base(typeMapper, currentContext, setFinder)
        {
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            conventionSet.ModelInitializedConventions.Add(new SqlServerValueGenerationStrategyConvention());

            var sqlServerInMemoryTablesConvention = new SqlServerInMemoryTablesConvention();
            var cascadeDeleteConvention = new SqlServerCascadeDeleteConvention();
            conventionSet.EntityTypeAnnotationSetConventions.Add(sqlServerInMemoryTablesConvention);
            conventionSet.EntityTypeAnnotationSetConventions.Add(cascadeDeleteConvention);

            conventionSet.KeyAddedConventions.Add(sqlServerInMemoryTablesConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, (CascadeDeleteConvention)cascadeDeleteConvention);

            conventionSet.IndexAddedConventions.Add(sqlServerInMemoryTablesConvention);

            ReplaceConvention(conventionSet.PropertyNullableChangedConventions, (CascadeDeleteConvention)cascadeDeleteConvention);

            return conventionSet;
        }

        public static ConventionSet Build()
            => new SqlServerConventionSetBuilder(new SqlServerTypeMapper(), null, null)
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
