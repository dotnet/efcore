// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using SQLitePCL;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal class SqliteResultValueStore : SqliteValueStore
    {
        private readonly sqlite3_context _ctx;

        public SqliteResultValueStore(sqlite3_context ctx)
        {
            _ctx = ctx;
        }

        private SqliteResultValueStore()
            => throw new NotSupportedException();

        protected override void StoreBlob(byte[] value)
            => raw.sqlite3_result_blob(_ctx, value);

        protected override void StoreDouble(double value)
            => raw.sqlite3_result_double(_ctx, value);

        protected override void StoreInt64(long value)
            => raw.sqlite3_result_int64(_ctx, value);

        protected override void StoreNull()
            => raw.sqlite3_result_null(_ctx);

        protected override void StoreString(string value)
            => raw.sqlite3_result_text(_ctx, value);
    }
}
