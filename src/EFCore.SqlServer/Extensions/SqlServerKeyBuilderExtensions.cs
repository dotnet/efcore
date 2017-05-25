// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="KeyBuilder" />.
    /// </summary>
    public static class SqlServerKeyBuilderExtensions
    {
        /// <summary>
        ///     Configures whether the key is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static KeyBuilder ForSqlServerIsClustered([NotNull] this KeyBuilder keyBuilder, bool clustered = true)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            keyBuilder.Metadata.SqlServer().IsClustered = clustered;

            return keyBuilder;
        }
    }
}
