// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite
{
    public enum SqliteType
    {
        Integer = Constants.SQLITE_INTEGER,
        Real = Constants.SQLITE_FLOAT,
        Text = Constants.SQLITE_TEXT,
        Blob = Constants.SQLITE_BLOB
    }
}
