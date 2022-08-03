// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteParameterBinder : SqliteValueBinder
    {
        private readonly sqlite3_stmt _stmt;
        private readonly int _index;
        private readonly int? _size;

        public SqliteParameterBinder(sqlite3_stmt stmt, int index, object value, int? size, SqliteType? sqliteType)
            : base(value, sqliteType)
        {
            _stmt = stmt;
            _index = index;
            _size = size;
        }

        protected override void BindBlob(byte[] value)
        {
            var blob = value;
            if (ShouldTruncate(value.Length))
            {
                blob = new byte[_size!.Value];
                Array.Copy(value, blob, _size.Value);
            }

            sqlite3_bind_blob(_stmt, _index, blob);
        }

        protected override void BindDoubleCore(double value)
            => sqlite3_bind_double(_stmt, _index, value);

        protected override void BindInt64(long value)
            => sqlite3_bind_int64(_stmt, _index, value);

        protected override void BindNull()
            => sqlite3_bind_null(_stmt, _index);

        protected override void BindText(string value)
            => sqlite3_bind_text(
                _stmt,
                _index,
                ShouldTruncate(value.Length)
                    ? value.Substring(0, _size!.Value)
                    : value);

        private bool ShouldTruncate(int length)
            => _size.HasValue
                && length > _size.Value
                && _size.Value != -1;
    }
}
