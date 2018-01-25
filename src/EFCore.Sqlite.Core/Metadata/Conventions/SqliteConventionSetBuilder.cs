// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteConventionSetBuilder : RelationalConventionSetBuilder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteConventionSetBuilder([NotNull] RelationalConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ConventionSet Build()
        {
            var coreTypeMapperDependencies = new CoreTypeMapperDependencies(
                new ValueConverterSelector(
                    new ValueConverterSelectorDependencies()));

            var relationalTypeMapper = new SqliteTypeMapper(
                new RelationalTypeMapperDependencies());

            var convertingTypeMapper = new FallbackRelationalCoreTypeMapper(
                coreTypeMapperDependencies,
                new RelationalTypeMapperDependencies(),
                relationalTypeMapper);

            return new SqliteConventionSetBuilder(
                new RelationalConventionSetBuilderDependencies(convertingTypeMapper, null, null, null))
                .AddConventions(
                    new CoreConventionSetBuilder(
                        new CoreConventionSetBuilderDependencies(convertingTypeMapper, null, null))
                        .CreateConventionSet());
        }
    }
}
