// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using static Microsoft.Data.Sqlite.Interop.Constants;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents the type affinities used by columns in SQLite tables.
    /// <see href="https://www.sqlite.org/datatype3.html#affinity">See SQLite.org for more details on type affinity</see>
    /// </summary>
    public enum SqliteType
    {
        Integer = SQLITE_INTEGER,
        Real = SQLITE_FLOAT,
        Text = SQLITE_TEXT,
        Blob = SQLITE_BLOB
    }
}
