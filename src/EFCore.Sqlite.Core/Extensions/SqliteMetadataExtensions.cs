// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite specific extension methods for metadata.
    /// </summary>
    public static class SqliteMetadataExtensions
    {
        /// <summary>
        ///     Gets the SQLite specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the property. </returns>
        public static SqlitePropertyAnnotations Sqlite([NotNull] this IMutableProperty property)
            => (SqlitePropertyAnnotations)Sqlite((IProperty)property);

        /// <summary>
        ///     Gets the SQLite specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The SQLite specific metadata for the property. </returns>
        public static ISqlitePropertyAnnotations Sqlite([NotNull] this IProperty property)
            => new SqlitePropertyAnnotations(Check.NotNull(property, nameof(property)));
    }
}
