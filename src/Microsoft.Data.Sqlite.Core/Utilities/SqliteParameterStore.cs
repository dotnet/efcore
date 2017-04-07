// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal class SqliteParameterStore : SqliteValueStore
    {
        private readonly Action _storeAction;

        private sqlite3_stmt _stmt;
        private int _index;

        public SqliteParameterStore(Type type, object val)
        {
            _storeAction = GetStoreValueAction(type, val);
        }

        public void BindParameter(sqlite3_stmt stmt, int index)
        {
            _stmt = stmt;
            _index = index;
            _storeAction();
        }

        private SqliteParameterStore() => throw new NotSupportedException();
        
        public override void StoreValue(Type type, object val)
             => throw new NotSupportedException();

        protected override void StoreBlob(byte[] value)
            => raw.sqlite3_bind_blob(_stmt, _index, value);

        protected override void StoreDouble(double value)
        {
            if (double.IsNaN(value))
            {
                throw new InvalidOperationException(Resources.CannotStoreNaN);
            }

            raw.sqlite3_bind_double(_stmt, _index, value);
        }

        protected override void StoreInt64(long value)
            => raw.sqlite3_bind_int64(_stmt, _index, value);

        protected override void StoreNull()
            => raw.sqlite3_bind_null(_stmt, _index);

        protected override void StoreString(string value)
            => raw.sqlite3_bind_text(_stmt, _index, value);
    }
}
