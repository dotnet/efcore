// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Data.SQLite.Interop;

namespace Microsoft.Data.SQLite.Utilities
{
    internal static class ColumnReader
    {
        public static object Read(SQLiteType sqliteType, StatementHandle handle, int ordinal)
        {
            Debug.Assert(handle != null && !handle.IsInvalid, "handle is null.");

            switch (sqliteType)
            {
                case SQLiteType.Null:
                    return DBNull.Value;

                case SQLiteType.Integer:
                    return NativeMethods.sqlite3_column_int64(handle, ordinal);

                case SQLiteType.Float:
                    return NativeMethods.sqlite3_column_double(handle, ordinal);

                case SQLiteType.Text:
                    return NativeMethods.sqlite3_column_text(handle, ordinal);

                default:
                    Debug.Assert(sqliteType == SQLiteType.Blob, "_sqliteType is not Blob.");
                    return NativeMethods.sqlite3_column_blob(handle, ordinal);
            }
        }
    }
}
