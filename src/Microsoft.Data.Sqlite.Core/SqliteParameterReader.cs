// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteParameterReader : SqliteValueReader
    {
        private readonly sqlite3_value[] _values;

        public SqliteParameterReader(sqlite3_value[] values)
        {
            _values = values;
        }

        public override int FieldCount
            => _values.Length;

        public override double GetDouble(int ordinal)
            => raw.sqlite3_value_double(_values[ordinal]);

        public override long GetInt64(int ordinal)
            => raw.sqlite3_value_int64(_values[ordinal]);

        public override string GetString(int ordinal)
            => raw.sqlite3_value_text(_values[ordinal]);

        protected override byte[] GetBlobCore(int ordinal)
            => raw.sqlite3_value_blob(_values[ordinal]);

        protected override int GetSqliteType(int ordinal)
            => raw.sqlite3_value_type(_values[ordinal]);
    }
}
