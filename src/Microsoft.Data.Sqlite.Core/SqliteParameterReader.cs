// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteParameterReader : SqliteValueReader
    {
        private readonly sqlite3_value[] _values;

        public SqliteParameterReader(sqlite3_value[] values, string name)
        {
            _values = values;
            OnNullErrorMsg = Resources.UDFCalledWithNull(name);
        }

        public override int FieldCount
            => _values.Length;

        protected override string OnNullErrorMsg { get; }

        protected override double GetDoubleCore(int ordinal)
            => raw.sqlite3_value_double(_values[ordinal]);

        protected override long GetInt64Core(int ordinal)
            => raw.sqlite3_value_int64(_values[ordinal]);

        protected override string GetStringCore(int ordinal)
            => raw.sqlite3_value_text(_values[ordinal]);

        protected override byte[] GetBlobCore(int ordinal)
            => raw.sqlite3_value_blob(_values[ordinal]);

        protected override int GetSqliteType(int ordinal)
            => raw.sqlite3_value_type(_values[ordinal]);
    }
}
