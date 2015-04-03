// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal static class ColumnReader
    {
        public static object Read(SqliteType sqliteType, StatementHandle handle, int ordinal)
        {
            Debug.Assert(handle != null && !handle.IsInvalid, "handle is null.");

            if (sqliteType == SqliteType.Null
                || NativeMethods.sqlite3_column_type(handle, ordinal) == Constants.SQLITE_NULL)
            {
                return DBNull.Value;
            }

            switch (sqliteType)
            {
                case SqliteType.Integer:
                    return NativeMethods.sqlite3_column_int64(handle, ordinal);

                case SqliteType.Float:
                    return NativeMethods.sqlite3_column_double(handle, ordinal);

                case SqliteType.Text:
                    return NativeMethods.sqlite3_column_text(handle, ordinal);

                default:
                    Debug.Assert(sqliteType == SqliteType.Blob, "_sqliteType is not Blob.");
                    return NativeMethods.sqlite3_column_blob(handle, ordinal);
            }
        }
    }
}
