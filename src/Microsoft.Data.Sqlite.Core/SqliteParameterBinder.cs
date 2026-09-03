// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteParameterBinder(sqlite3_stmt stmt, sqlite3 handle, int index, object value, int? size, SqliteType? sqliteType)
        : SqliteValueBinder(value, sqliteType)
    {
        protected override void BindBlob(byte[] value)
        {
            var blob = value;
            if (ShouldTruncate(value.Length))
            {
                blob = new byte[size!.Value];
                Array.Copy(value, blob, size.Value);
            }

            var rc = sqlite3_bind_blob(stmt, index, blob);
            SqliteException.ThrowExceptionForRC(rc, handle);
        }

        protected override void BindDoubleCore(double value)
        {
            var rc = sqlite3_bind_double(stmt, index, value);

            SqliteException.ThrowExceptionForRC(rc, handle);
        }

        protected override void BindInt64(long value)
        {
            var rc = sqlite3_bind_int64(stmt, index, value);

            SqliteException.ThrowExceptionForRC(rc, handle);
        }

        protected override void BindNull()
        {
            var rc = sqlite3_bind_null(stmt, index);

            SqliteException.ThrowExceptionForRC(rc, handle);
        }

        protected override void BindText(string value)
        {
            var rc = sqlite3_bind_text(
                stmt,
                index,
                ShouldTruncate(value.Length)
                    ? value.Substring(0, size!.Value)
                    : value);

            SqliteException.ThrowExceptionForRC(rc, handle);
        }

        private bool ShouldTruncate(int length)
            => size.HasValue
                && length > size.Value
                && size.Value != -1;
    }
}
