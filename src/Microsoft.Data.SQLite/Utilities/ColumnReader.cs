// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
