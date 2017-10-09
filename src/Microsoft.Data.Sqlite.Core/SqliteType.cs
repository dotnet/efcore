// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents the type affinities used by columns in SQLite tables.
    /// </summary>
    /// <seealso href="http://sqlite.org/datatype3.html">Datatypes In SQLite Version 3</seealso>
    public enum SqliteType
    {
        /// <summary>
        ///     A signed integer.
        /// </summary>
        Integer = raw.SQLITE_INTEGER,

        /// <summary>
        ///     A floating point value.
        /// </summary>
        Real = raw.SQLITE_FLOAT,

        /// <summary>
        ///     A text string.
        /// </summary>
        Text = raw.SQLITE_TEXT,

        /// <summary>
        ///     A blob of data.
        /// </summary>
        Blob = raw.SQLITE_BLOB
    }
}
