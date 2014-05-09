// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.SQLite.Interop;

namespace Microsoft.Data.SQLite
{
    internal enum SQLiteType
    {
        Integer = Constants.SQLITE_INTEGER,
        Float = Constants.SQLITE_FLOAT,
        Text = Constants.SQLITE_TEXT,
        Blob = Constants.SQLITE_BLOB,
        Null = Constants.SQLITE_NULL
    }
}
