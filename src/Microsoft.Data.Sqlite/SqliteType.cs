// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents the type affinities used by columns in SQLite tables. 
    /// <see href="https://www.sqlite.org/datatype3.html#affinity">See SQLite.org for more details on type affinity</see>
    /// </summary>
    public enum SqliteType
    {
        Integer = Constants.SQLITE_INTEGER,
        Real = Constants.SQLITE_FLOAT,
        Text = Constants.SQLITE_TEXT,
        Blob = Constants.SQLITE_BLOB
    }
}
