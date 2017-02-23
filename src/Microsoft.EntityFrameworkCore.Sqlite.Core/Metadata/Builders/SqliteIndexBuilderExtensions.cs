// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite specific extension methods for <see cref="IndexBuilder" />.
    /// </summary>
    public static class SqliteIndexBuilderExtensions
    {
        /// <summary>
        ///     Configures the name of the index in the database when targeting SQLite.
        /// </summary>
        /// <param name="builder"> The builder for the index being configured. </param>
        /// <param name="name"> The name of the index. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static IndexBuilder ForSqliteHasName([NotNull] this IndexBuilder builder, [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().Name = name;

            return builder;
        }
    }
}
