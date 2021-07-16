// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteResultBinder : SqliteValueBinder
    {
        private readonly sqlite3_context _ctx;

        public SqliteResultBinder(sqlite3_context ctx, object? value)
            : base(value)
        {
            _ctx = ctx;
        }

        protected override void BindBlob(byte[] value)
            => sqlite3_result_blob(_ctx, value);

        protected override void BindDoubleCore(double value)
            => sqlite3_result_double(_ctx, value);

        protected override void BindInt64(long value)
            => sqlite3_result_int64(_ctx, value);

        protected override void BindNull()
            => sqlite3_result_null(_ctx);

        protected override void BindText(string value)
            => sqlite3_result_text(_ctx, value);
    }
}
