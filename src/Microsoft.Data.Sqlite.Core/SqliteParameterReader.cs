// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteParameterReader(string function, sqlite3_value[] values) : SqliteValueReader
    {
        public override int FieldCount
            => values.Length;

        protected override string GetOnNullErrorMsg(int ordinal)
            => Resources.UDFCalledWithNull(function, ordinal);

        protected override double GetDoubleCore(int ordinal)
            => sqlite3_value_double(values[ordinal]);

        protected override long GetInt64Core(int ordinal)
            => sqlite3_value_int64(values[ordinal]);

        protected override string GetStringCore(int ordinal)
            => sqlite3_value_text(values[ordinal]).utf8_to_string();

        protected override byte[] GetBlobCore(int ordinal)
            => sqlite3_value_blob(values[ordinal]).ToArray();

        protected override int GetSqliteType(int ordinal)
            => sqlite3_value_type(values[ordinal]);
    }
}
