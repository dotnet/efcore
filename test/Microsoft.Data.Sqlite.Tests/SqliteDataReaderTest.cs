// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Sqlite.TestUtilities;
using Microsoft.Data.Sqlite.Utilities;
using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class SqliteDataReaderTest
    {
        [Fact]
        public void Depth_returns_zero()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    Assert.Equal(0, reader.Depth);
                }
            }
        }

        [Fact]
        public void FieldCount_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    Assert.Equal(1, reader.FieldCount);
                }
            }
        }

        [Fact]
        public void FieldCount_throws_when_closed() => X_throws_when_closed(r => { var x = r.FieldCount; }, "FieldCount");

        [Fact]
        public void GetBoolean_works() =>
            GetX_works(
                "SELECT 1;",
                r => r.GetBoolean(0),
                true);

        [Fact]
        public void GetByte_works() =>
            GetX_works(
                "SELECT 1;",
                r => r.GetByte(0),
                (byte)1);

        [Fact]
        public void GetBytes_not_supported()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT x'7E57';"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    var buffer = new byte[2];
                    Assert.Throws<NotSupportedException>(() => reader.GetBytes(0, 0, buffer, 0, buffer.Length));
                }
            }
        }

        [Fact]
        public void GetChar_works() =>
            GetX_works(
                "SELECT 1;",
                r => r.GetChar(0),
                (char)1);

        [Fact]
        public void GetChars_not_supported()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 'test';"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    var buffer = new char[4];
                    Assert.Throws<NotSupportedException>(() => reader.GetChars(0, 0, buffer, 0, buffer.Length));
                }
            }
        }

        [Fact]
        public void GetDateTime_works() =>
            GetX_works(
                "SELECT '2014-04-15 10:47:16';",
                r => r.GetDateTime(0),
                new DateTime(2014, 4, 15, 10, 47, 16));

        [Theory]
        [InlineData("SELECT 1;", "INTEGER")]
        [InlineData("SELECT 3.14;", "REAL")]
        [InlineData("SELECT 'test';", "TEXT")]
        [InlineData("SELECT X'7E57';", "BLOB")]
        [InlineData("SELECT NULL;", "INTEGER")]
        public void GetDataTypeName_works(string sql, string expected)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader(sql))
                {
                    Assert.Equal(expected, reader.GetDataTypeName(0));
                }
            }
        }

        [Fact]
        public void GetDataTypeName_works_when_column()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE Person ( Name nvarchar(4000) );");

                using (var reader = connection.ExecuteReader("SELECT Name FROM Person;"))
                {
                    Assert.Equal("nvarchar", reader.GetDataTypeName(0));
                }
            }
        }

        [Fact]
        public void GetDataTypeName_throws_when_ordinal_out_of_range()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetDataTypeName(1));

                    Assert.Equal("ordinal", ex.ParamName);
                    Assert.Equal(1, ex.ActualValue);
                }
            }
        }

        [Fact]
        public void GetDataTypeName_throws_when_closed() => X_throws_when_closed(r => r.GetDataTypeName(0), "GetDataTypeName");

        [Fact]
        public void GetDecimal_works() =>
            GetX_works(
                "SELECT '3.14';",
                r => r.GetDecimal(0),
                3.14m);

        [Fact]
        public void GetDouble_works() =>
            GetX_works(
                "SELECT 3.14;",
                r => r.GetDouble(0),
                3.14);

        [Fact]
        public void GetDouble_throws_when_null() =>
            GetX_throws_when_null(
                r => r.GetDouble(0));

#if DNXCORE50
        [Fact]
        public void GetEnumerator_not_implemented()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.Throws<NotImplementedException>(() => reader.GetEnumerator());
                }
            }
        }
#else
        [Fact]
        public void GetEnumerator_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.NotNull(reader.GetEnumerator());
                }
            }
        }
#endif

        [Theory]
        [InlineData("SELECT 1;", true)]
        [InlineData("SELECT 1;", (byte)1)]
        [InlineData("SELECT 1;", (char)1)]
        [InlineData("SELECT 3.14;", 3.14)]
        [InlineData("SELECT 3;", 3f)]
        [InlineData("SELECT 1;", 1)]
        [InlineData("SELECT 1;", 1L)]
        [InlineData("SELECT 1;", (sbyte)1)]
        [InlineData("SELECT 1;", (short)1)]
        [InlineData("SELECT 'test';", "test")]
        [InlineData("SELECT 1;", 1u)]
        [InlineData("SELECT 1;", 1ul)]
        [InlineData("SELECT 1;", (ushort)1)]
        public void GetFieldValue_works<T>(string sql, T expected)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader(sql))
                {
                    var hasData = reader.Read();

                    Assert.True(hasData);
                    Assert.Equal(expected, reader.GetFieldValue<T>(0));
                }
            }
        }

        [Fact]
        public void GetFieldValue_of_byteArray_works() =>
            GetFieldValue_works(
                "SELECT X'7E57';",
                new byte[] { 0x7e, 0x57 });

        [Fact]
        public void GetFieldValue_of_byteArray_throws_when_null() =>
            GetX_throws_when_null(
                r => r.GetFieldValue<byte[]>(0));

        [Fact]
        public void GetFieldValue_of_DateTime_works() =>
            GetFieldValue_works(
                "SELECT '2014-04-15 11:58:13';",
                new DateTime(2014, 4, 15, 11, 58, 13));

        [Fact]
        public void GetFieldValue_of_DateTimeOffset_works() =>
            GetFieldValue_works(
                "SELECT '2014-04-15 11:58:13-08:00';",
                new DateTimeOffset(2014, 4, 15, 11, 58, 13, new TimeSpan(-8, 0, 0)));

        [Fact]
        public void GetFieldValue_of_DBNull_works() =>
            GetFieldValue_works(
                "SELECT NULL;",
                DBNull.Value);

        [Fact]
        public void GetFieldValue_of_decimal_works() =>
            GetFieldValue_works(
                "SELECT '3.14';",
                3.14m);

        [Fact]
        public void GetFieldValue_of_Enum_works() =>
            GetFieldValue_works(
                "SELECT 1;",
                MyEnum.One);

        [Fact]
        public void GetFieldValue_of_Guid_works() =>
            GetFieldValue_works(
                "SELECT X'0E7E0DDC5D364849AB9B8CA8056BF93A';",
                new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

        [Fact]
        public void GetFieldValue_of_Nullable_works() =>
            GetFieldValue_works(
                "SELECT 1;",
                (int?)1);

        [Fact]
        public void GetFieldValue_of_TimeSpan_works() =>
            GetFieldValue_works(
                "SELECT '12:06:29';",
                new TimeSpan(12, 6, 29));

        [Fact]
        public void GetFieldValue_throws_before_read() => X_throws_before_read(r => r.GetFieldValue<DBNull>(0));

        [Fact]
        public void GetFieldValue_throws_when_done() => X_throws_when_done(r => r.GetFieldValue<DBNull>(0));

        [Theory]
        [InlineData("SELECT 1;", typeof(long))]
        [InlineData("SELECT 3.14;", typeof(double))]
        [InlineData("SELECT 'test';", typeof(string))]
        [InlineData("SELECT X'7E57';", typeof(byte[]))]
        [InlineData("SELECT NULL;", typeof(int))]
        public void GetFieldType_works(string sql, Type expected)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader(sql))
                {
                    Assert.Equal(expected, reader.GetFieldType(0));
                }
            }
        }

        [Fact]
        public void GetFieldType_throws_when_ordinal_out_of_range()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetFieldType(1));

                    Assert.Equal("ordinal", ex.ParamName);
                    Assert.Equal(1, ex.ActualValue);
                }
            }
        }

        [Fact]
        public void GetFieldType_throws_when_closed() => X_throws_when_closed(r => r.GetFieldType(0), "GetFieldType");

        [Fact]
        public void GetFloat_works() =>
            GetX_works(
                "SELECT 3;",
                r => r.GetFloat(0),
                3f);

        [Fact]
        public void GetGuid_works() =>
            GetX_works(
                "SELECT X'0E7E0DDC5D364849AB9B8CA8056BF93A';",
                r => r.GetGuid(0),
                new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

        [Fact]
        public void GetInt16_works() =>
            GetX_works(
                "SELECT 1;",
                r => r.GetInt16(0),
                (short)1);

        [Fact]
        public void GetInt32_works() =>
            GetX_works(
                "SELECT 1;",
                r => r.GetInt32(0),
                1);

        [Fact]
        public void GetInt64_works() =>
            GetX_works(
                "SELECT 1;",
                r => r.GetInt64(0),
                1L);

        [Fact]
        public void GetInt64_throws_when_null() =>
            GetX_throws_when_null(
                r => r.GetInt64(0));

        [Fact]
        public void GetName_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1 AS Id;"))
                {
                    Assert.Equal("Id", reader.GetName(0));
                }
            }
        }

        [Fact]
        public void GetName_throws_when_ordinal_out_of_range()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetName(1));

                    Assert.Equal("ordinal", ex.ParamName);
                    Assert.Equal(1, ex.ActualValue);
                }
            }
        }

        [Fact]
        public void GetName_throws_when_closed() => X_throws_when_closed(r => r.GetName(0), "GetName");

        [Fact]
        public void GetOrdinal_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1 AS Id;"))
                {
                    Assert.Equal(0, reader.GetOrdinal("Id"));
                }
            }
        }

        [Fact]
        public void GetOrdinal_throws_when_out_of_range()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var ex = Assert.Throws<ArgumentOutOfRangeException>(() => reader.GetOrdinal("Name"));
                    Assert.NotNull(ex.Message);
                    Assert.Equal("name", ex.ParamName);
                    Assert.Equal("Name", ex.ActualValue);
                }
            }
        }

        [Fact]
        public void GetString_works() =>
            GetX_works(
                "SELECT 'test';",
                r => r.GetString(0),
                "test");

        [Fact]
        public void GetString_throws_when_null() =>
            GetX_throws_when_null(
                r => r.GetString(0));

        [Theory]
        [InlineData("SELECT 1;", 1L)]
        [InlineData("SELECT 3.14;", 3.14)]
        [InlineData("SELECT 'test';", "test")]
        public void GetValue_works(string sql, object expected)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader(sql))
                {
                    var hasData = reader.Read();

                    Assert.True(hasData);
                    Assert.Equal(expected, reader.GetValue(0));
                }
            }
        }

        [Fact]
        public void GetValue_works_when_blob() =>
            GetValue_works(
                "SELECT X'7E57';",
                new byte[] { 0x7e, 0x57 });

        [Fact]
        public void GetValue_works_when_null() =>
            GetValue_works(
                "SELECT NULL;",
                DBNull.Value);

        [Fact]
        public void GetValue_throws_before_read() => X_throws_before_read(r => r.GetValue(0));

        [Fact]
        public void GetValue_throws_when_done() => X_throws_when_done(r => r.GetValue(0));

        [Fact]
        public void GetValue_throws_when_closed() => X_throws_when_closed(r => r.GetValue(0), "GetValue");

        [Fact]
        public void GetValues_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    // Array may be wider than row
                    var values = new object[2];
                    var result = reader.GetValues(values);

                    Assert.Equal(1, result);
                    Assert.Equal(1L, values[0]);
                }
            }
        }

        [Fact]
        public void GetValues_throws_when_too_narrow()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    var values = new object[0];
                    Assert.Throws<IndexOutOfRangeException>(() => reader.GetValues(values));
                }
            }
        }

        [Fact]
        public void HasRows_returns_true_when_rows()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    Assert.True(reader.HasRows);
                }
            }
        }

        [Fact]
        public void HasRows_returns_false_when_no_rows()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1 WHERE 0 = 1;"))
                {
                    Assert.False(reader.HasRows);
                }
            }
        }

        [Fact]
        public void HasRows_works_when_batching()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1 WHERE 0 = 1; SELECT 1;"))
                {
                    Assert.False(reader.HasRows);

                    reader.NextResult();

                    Assert.True(reader.HasRows);
                }
            }
        }

        [Fact]
        public void IsClosed_returns_false_when_active()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    Assert.False(reader.IsClosed);
                }
            }
        }

        [Fact]
        public void IsClosed_returns_true_when_closed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var reader = connection.ExecuteReader("SELECT 1;");
#if DNX451
                reader.Close();
#else
                ((IDisposable)reader).Dispose();
#endif

                Assert.True(reader.IsClosed);
            }
        }

        [Fact]
        public void IsDBNull_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT NULL;"))
                {
                    var hasData = reader.Read();

                    Assert.True(hasData);
                    Assert.True(reader.IsDBNull(0));
                }
            }
        }

        [Fact]
        public void IsDBNull_throws_before_read() => X_throws_before_read(r => r.IsDBNull(0));

        [Fact]
        public void IsDBNull_throws_when_done() => X_throws_when_done(r => r.IsDBNull(0));

        [Fact]
        public void IsDBNull_throws_when_closed() => X_throws_when_closed(r => r.IsDBNull(0), "IsDBNull");

        [Fact]
        public void Item_by_ordinal_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.Equal(1L, reader[0]);
                }
            }
        }

        [Fact]
        public void Item_by_name_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1 AS Id;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.Equal(1L, reader["Id"]);
                }
            }
        }

        [Fact]
        public void NextResult_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1; SELECT 2;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);
                    Assert.Equal(1L, reader.GetInt64(0));

                    var hasResults = reader.NextResult();
                    Assert.True(hasResults);

                    hasData = reader.Read();
                    Assert.True(hasData);
                    Assert.Equal(2L, reader.GetInt64(0));

                    hasResults = reader.NextResult();
                    Assert.False(hasResults);
                }
            }
        }

        [Fact]
        public void NextResult_can_be_called_more_than_once()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1;"))
                {
                    var hasResults = reader.NextResult();
                    Assert.False(hasResults);

                    hasResults = reader.NextResult();
                    Assert.False(hasResults);
                }
            }
        }

        [Fact]
        public void NextResult_skips_DML_statements()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE Test(Value);");

                var sql = @"
                    SELECT 1;
                    INSERT INTO Test VALUES(1);
                    SELECT 2;";
                using (var reader = connection.ExecuteReader(sql))
                {
                    var hasResults = reader.NextResult();
                    Assert.True(hasResults);

                    var hasData = reader.Read();
                    Assert.True(hasData);

                    Assert.Equal(2L, reader.GetInt64(0));
                }
            }
        }

        [Fact]
        public void Read_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT 1 UNION SELECT 2;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);
                    Assert.Equal(1L, reader.GetInt64(0));

                    hasData = reader.Read();
                    Assert.True(hasData);
                    Assert.Equal(2L, reader.GetInt64(0));

                    hasData = reader.Read();
                    Assert.False(hasData);
                }
            }
        }

        [Fact]
        public void Read_throws_when_closed() => X_throws_when_closed(r => r.Read(), "Read");

        [Fact]
        public void RecordsAffected_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                connection.ExecuteNonQuery("CREATE TABLE Test(Value);");

                var reader = connection.ExecuteReader("INSERT INTO Test VALUES(1);");
                ((IDisposable)reader).Dispose();

                Assert.Equal(1, reader.RecordsAffected);
            }
        }

        [Fact]
        public void RecordsAffected_works_when_no_DDL()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var reader = connection.ExecuteReader("SELECT 1;");
                ((IDisposable)reader).Dispose();

                Assert.Equal(-1, reader.RecordsAffected);
            }
        }

        private static void GetX_works<T>(string sql, Func<DbDataReader, T> action, T expected)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader(sql))
                {
                    var hasData = reader.Read();

                    Assert.True(hasData);
                    Assert.Equal(expected, action(reader));
                }
            }
        }

        private static void GetX_throws_when_null(Action<DbDataReader> action)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT NULL;"))
                {
                    var hasData = reader.Read();

                    Assert.True(hasData);
                    Assert.Throws<InvalidCastException>(() => action(reader));
                }
            }
        }

        private static void X_throws_before_read(Action<DbDataReader> action)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT NULL;"))
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => action(reader));

                    Assert.Equal(Strings.NoData, ex.Message);
                }
            }
        }

        private static void X_throws_when_done(Action<DbDataReader> action)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var reader = connection.ExecuteReader("SELECT NULL;"))
                {
                    var hasData = reader.Read();
                    Assert.True(hasData);

                    hasData = reader.Read();
                    Assert.False(hasData);

                    var ex = Assert.Throws<InvalidOperationException>(() => action(reader));
                    Assert.Equal(Strings.NoData, ex.Message);
                }
            }
        }

        private static void X_throws_when_closed(Action<DbDataReader> action, string operation)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var reader = connection.ExecuteReader("SELECT 1;");
                ((IDisposable)reader).Dispose();

                var ex = Assert.Throws<InvalidOperationException>(() => action(reader));
                Assert.Equal(Strings.FormatDataReaderClosed(operation), ex.Message);
            }
        }

        private enum MyEnum
        {
            One = 1
        }
    }
}
