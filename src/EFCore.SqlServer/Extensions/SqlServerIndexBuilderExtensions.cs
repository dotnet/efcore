// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class SqlServerIndexBuilderExtensions
    {
        /// <summary>
        ///     Configures the name of the index in the database when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForSqlServerHasName([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string name)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            indexBuilder.Metadata.SqlServer().Name = name;

            return indexBuilder;
        }

        /// <summary>
        ///     Determines whether the specified index has filter expression when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="filterExpression"> The filter expression for the index. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForSqlServerHasFilter([NotNull] this IndexBuilder indexBuilder, [CanBeNull] string filterExpression)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NullButNotEmpty(filterExpression, nameof(filterExpression));

            indexBuilder.Metadata.SqlServer().Filter = filterExpression;

            return indexBuilder;
        }

        /// <summary>
        ///     Configures whether the index is clustered when targeting SQL Server.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index being configured. </param>
        /// <param name="clustered"> A value indicating whether the index is clustered. </param>
        /// <returns> A builder to further configure the index. </returns>
        public static IndexBuilder ForSqlServerIsClustered([NotNull] this IndexBuilder indexBuilder, bool clustered = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SqlServer().IsClustered = clustered;

            return indexBuilder;
        }
    }
}
