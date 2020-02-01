// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteResultBinder : SqliteValueBinder
    {
        private readonly sqlite3_context _ctx;

        public SqliteResultBinder(sqlite3_context ctx, object value)
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
