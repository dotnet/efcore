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
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        public SqlServerConventionSetBuilder(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotationProvider,
            [CanBeNull] ICurrentDbContext currentContext,
            [CanBeNull] IDbSetFinder setFinder,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(typeMapper, annotationProvider, currentContext, setFinder)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new SqlServerValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);

            ValueGeneratorConvention valueGeneratorConvention = new SqlServerValueGeneratorConvention(AnnotationProvider);
            ReplaceConvention(conventionSet.BaseEntityTypeSetConventions, valueGeneratorConvention);

            var sqlServerMemoryOptimizedTablesConvention = new SqlServerMemoryOptimizedTablesConvention();
            conventionSet.EntityTypeAnnotationSetConventions.Add(sqlServerMemoryOptimizedTablesConvention);

            ReplaceConvention(conventionSet.PrimaryKeySetConventions, valueGeneratorConvention);

            conventionSet.KeyAddedConventions.Add(sqlServerMemoryOptimizedTablesConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);

            var sqlServerIndexConvention = new SqlServerIndexConvention(_sqlGenerationHelper);
            conventionSet.IndexAddedConventions.Add(sqlServerMemoryOptimizedTablesConvention);
            conventionSet.IndexAddedConventions.Add(sqlServerIndexConvention);

            conventionSet.IndexUniquenessConventions.Add(sqlServerIndexConvention);

            conventionSet.IndexAnnotationSetConventions.Add(sqlServerIndexConvention);

            conventionSet.PropertyNullableChangedConventions.Add(sqlServerIndexConvention);

            conventionSet.PropertyAnnotationSetConventions.Add(sqlServerIndexConvention);
            conventionSet.PropertyAnnotationSetConventions.Add((SqlServerValueGeneratorConvention)valueGeneratorConvention);

            return conventionSet;
        }

        public static ConventionSet Build()
            => new SqlServerConventionSetBuilder(
                new SqlServerTypeMapper(), new SqlServerAnnotationProvider(), null, null, new SqlServerSqlGenerationHelper())
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
