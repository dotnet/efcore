// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class OracleConventionSetBuilder : RelationalConventionSetBuilder
    {
        public OracleConventionSetBuilder(
            [NotNull] RelationalConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        public override ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            base.AddConventions(conventionSet);

            var valueGenerationStrategyConvention = new OracleValueGenerationStrategyConvention();
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            conventionSet.ModelInitializedConventions.Add(new RelationalMaxIdentifierLengthConvention(128));

            ValueGeneratorConvention valueGeneratorConvention = new OracleValueGeneratorConvention();
            ReplaceConvention(conventionSet.BaseEntityTypeChangedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.PrimaryKeyChangedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGeneratorConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGeneratorConvention);

            conventionSet.PropertyAnnotationChangedConventions.Add((OracleValueGeneratorConvention)valueGeneratorConvention);

            return conventionSet;
        }

        public static ConventionSet Build()
        {
            var oracleTypeMapper = new OracleTypeMapper(
                new CoreTypeMapperDependencies(
                    new ValueConverterSelector(
                        new ValueConverterSelectorDependencies())),
                new RelationalTypeMapperDependencies());

            return new OracleConventionSetBuilder(
                new RelationalConventionSetBuilderDependencies(oracleTypeMapper, null, null, null))
                .AddConventions(
                    new CoreConventionSetBuilder(
                        new CoreConventionSetBuilderDependencies(oracleTypeMapper, null, null))
                        .CreateConventionSet());
        }
    }
}
