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
            var relationalTypeMapper = new SqliteTypeMapper(
                new CoreTypeMapperDependencies(
                    new ValueConverterSelector(
                        new ValueConverterSelectorDependencies())),
                new RelationalTypeMapperDependencies());

            return new SqliteConventionSetBuilder(
                new RelationalConventionSetBuilderDependencies(relationalTypeMapper, null, null, null))
                .AddConventions(
                    new CoreConventionSetBuilder(
                        new CoreConventionSetBuilderDependencies(relationalTypeMapper, null, null))
                        .CreateConventionSet());
        }
    }
}
