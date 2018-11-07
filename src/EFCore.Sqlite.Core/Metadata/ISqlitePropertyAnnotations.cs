// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     API for SQLite-specific annotations accessed through
    ///     <see cref="SqliteMetadataExtensions.Sqlite(IProperty)" />.
    /// </summary>
    public interface ISqlitePropertyAnnotations : IRelationalPropertyAnnotations
    {
        /// <summary>
        ///     Gets the SRID to use when creating a column for this property.
        /// </summary>
        int? Srid { get; }

        /// <summary>
        ///     Gets the dimension to use when creating a column for this property.
        /// </summary>
        string Dimension { get; }
    }
}
