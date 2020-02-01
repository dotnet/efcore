// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IKey" /> for SQL Server-specific metadata.
    /// </summary>
    public static class SqlServerKeyExtensions
    {
        /// <summary>
        ///     Returns a value indicating whether the key is clustered.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> <c>true</c> if the key is clustered. </returns>
        public static bool? IsClustered([NotNull] this IKey key)
            => (bool?)key[SqlServerAnnotationNames.Clustered] ?? GetDefaultIsClustered(key);

        private static bool? GetDefaultIsClustered(IKey key)
        {
            var sharedTablePrincipalPrimaryKeyProperty = key.Properties[0].FindSharedTableRootPrimaryKeyProperty();
            return sharedTablePrincipalPrimaryKeyProperty?.FindContainingPrimaryKey().IsClustered();
        }

        /// <summary>
        ///     Sets a value indicating whether the key is clustered.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="clustered"> The value to set. </param>
        public static void SetIsClustered([NotNull] this IMutableKey key, bool? clustered)
            => key.SetOrRemoveAnnotation(SqlServerAnnotationNames.Clustered, clustered);

        /// <summary>
        ///     Sets a value indicating whether the key is clustered.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <param name="clustered"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetIsClustered([NotNull] this IConventionKey key, bool? clustered, bool fromDataAnnotation = false)
            => key.SetOrRemoveAnnotation(SqlServerAnnotationNames.Clustered, clustered, fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for whether the key is clustered.
        /// </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for whether the key is clustered. </returns>
        public static ConfigurationSource? GetIsClusteredConfigurationSource([NotNull] this IConventionKey key)
            => key.FindAnnotation(SqlServerAnnotationNames.Clustered)?.GetConfigurationSource();
    }
}
