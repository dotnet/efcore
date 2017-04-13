// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class SqlServerConventionSetBuilder : RelationalConventionSetBuilder
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        public SqlServerConventionSetBuilder(
            [NotNull] RelationalConventionSetBuilderDependencies dependencies,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(dependencies)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new SqlServerValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);

            ValueGeneratorConvention valueGeneratorConvention = new SqlServerValueGeneratorConvention(Dependencies.AnnotationProvider);
            ReplaceConvention(conventionSet.BaseEntityTypeSetConventions, valueGeneratorConvention);

            var sqlServerInMemoryTablesConvention = new SqlServerMemoryOptimizedTablesConvention();
            conventionSet.EntityTypeAnnotationSetConventions.Add(sqlServerInMemoryTablesConvention);

            ReplaceConvention(conventionSet.PrimaryKeySetConventions, valueGeneratorConvention);

            conventionSet.KeyAddedConventions.Add(sqlServerInMemoryTablesConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);

            var sqlServerIndexConvention = new SqlServerIndexConvention(_sqlGenerationHelper);
            conventionSet.IndexAddedConventions.Add(sqlServerInMemoryTablesConvention);
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
                new RelationalConventionSetBuilderDependencies(
                    new SqlServerTypeMapper(
                        new RelationalTypeMapperDependencies()),
                    new SqlServerAnnotationProvider(),
                    null,
                    null),
                new SqlServerSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()))
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
