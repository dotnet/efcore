// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class SqlServerInternalMetadataBuilderExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SqlServerModelBuilderAnnotations SqlServer(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerModelBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SqlServerPropertyBuilderAnnotations SqlServer(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerPropertyBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SqlServerEntityTypeBuilderAnnotations SqlServer(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerEntityTypeBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SqlServerKeyBuilderAnnotations SqlServer(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerKeyBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static SqlServerIndexBuilderAnnotations SqlServer(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new SqlServerIndexBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static RelationalForeignKeyBuilderAnnotations SqlServer(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource);
    }
}
