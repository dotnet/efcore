// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.SQLite.Utilities;
using Xunit;

namespace Microsoft.Data.SQLite
{
    public class SQLiteDataReaderTest
    {
        [Fact]
        public void Depth_returns_zero()
        {
            Assert.Equal(0, CreateReader().Depth);
        }

        [Fact]
        public void FieldCount_throws_when_closed()
        {
            var reader = CreateReader();

            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.FieldCount);
            Assert.Equal(Strings.FormatDataReaderClosed("FieldCount"), ex.Message);
        }

        [Fact]
        public void FieldCount_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                    Assert.Equal(1, reader.FieldCount);
            }
        }

        [Fact]
        public void IsClosed_works()
        {
            var reader = CreateReader();
            Assert.False(reader.IsClosed);

            reader.Close();

            Assert.True(reader.IsClosed);
        }

        [Fact]
        public void RecordsAffected_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(0, reader.RecordsAffected);
                }
            }
        }

        [Fact]
        public void Item_string_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1 AS Column1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(1L, reader["Column1"]);
                }
            }
        }

        [Fact]
        public void Item_int_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(1L, reader[0]);
                }
            }
        }

        [Fact]
        public void Read_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.Read());
            Assert.Equal(Strings.FormatDataReaderClosed("Read"), ex.Message);
        }

        [Fact]
        public void Read_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.False(reader.Read());
                }
            }
        }

        [Fact]
        public void NextResult_not_supported()
        {
            var reader = CreateReader();

            Assert.Throws<NotSupportedException>(() => reader.NextResult());
        }

        [Fact]
        public void Close_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1 UNION SELECT 2";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    reader.Read();
                    reader.Close();

                    Assert.Null(command.OpenReader);
                    Assert.True(reader.IsClosed);
                }

                // NOTE: This would equal two if not reset
                Assert.Equal(1L, command.ExecuteScalar());
            }
        }

        [Fact]
        public void Close_can_be_called_more_than_once()
        {
            var reader = CreateReader();

            reader.Close();
            reader.Close();
        }

        [Fact]
        public void GetName_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetName(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetName"), ex.Message);
        }

        [Fact]
        public void GetName_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1 AS Column1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                    Assert.Equal("Column1", reader.GetName(0));
            }
        }

        [Fact]
        public void GetOrdinal_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetOrdinal("Column1"));
            Assert.Equal(Strings.FormatDataReaderClosed("GetOrdinal"), ex.Message);
        }

        [Fact]
        public void GetOrdinal_throws_when_unknown()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    var ex = Assert.Throws<IndexOutOfRangeException>(() => reader.GetOrdinal("Unknown"));
                    Assert.Equal("Unknown", ex.Message);
                }
            }
        }

        [Fact]
        public void GetOrdinal_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1 AS Column1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                    Assert.Equal(0, reader.GetOrdinal("Column1"));
            }
        }

        [Fact]
        public void GetDataTypeName_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetDataTypeName(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetDataTypeName"), ex.Message);
        }

        [Fact]
        public void GetDataTypeName_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT Int32Column FROM TestTable";

                using (var reader = command.ExecuteReader())
                    Assert.Equal("INT", reader.GetDataTypeName(0));
            }
        }

        [Fact]
        public void GetFieldType_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetFieldType(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetFieldType"), ex.Message);
        }

        [Fact]
        public void GetFieldType_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT Int32Column FROM TestTable";

                using (var reader = command.ExecuteReader())
                    Assert.Equal(typeof(int), reader.GetFieldType(0));
            }
        }

        [Fact]
        public void IsDBNull_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.IsDBNull(0));
            Assert.Equal(Strings.FormatDataReaderClosed("IsDBNull"), ex.Message);
        }

        [Fact]
        public void IsDBNull_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT NULL";
                connection.Open();

                using (var reader = command.ExecuteReader())
                    Assert.True(reader.IsDBNull(0));
            }
        }

        [Fact]
        public void GetBoolean_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetBoolean(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetBoolean"), ex.Message);
        }

        [Fact]
        public void GetBoolean_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT BooleanColumn FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.True(reader.GetBoolean(0));
                }
            }
        }

        [Fact]
        public void GetByte_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetByte(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetByte"), ex.Message);
        }

        [Fact]
        public void GetByte_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT ByteColumn FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal((byte)1, reader.GetByte(0));
                }
            }
        }

        [Fact]
        public void GetChar_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetChar(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetChar"), ex.Message);
        }

        [Fact]
        public void GetChar_not_supported()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 't'";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    var ex = Assert.Throws<ArgumentException>(() => reader.GetChar(0));
                    Assert.Equal(Strings.FormatUnknownDataType(typeof(char)), ex.Message);
                }
            }
        }

        [Fact]
        public void GetDateTime_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetDateTime(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetDateTime"), ex.Message);
        }

        [Fact]
        public void GetDateTime_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT DateTimeColumn FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(new DateTime(2014, 4, 1, 14, 45, 0), reader.GetDateTime(0));
                }
            }
        }

        [Fact]
        public void GetDecimal_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetDecimal(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetDecimal"), ex.Message);
        }

        [Fact]
        public void GetDecimal_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT DecimalColumn FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(3.14m, reader.GetDecimal(0));
                }
            }
        }

        [Fact]
        public void GetDouble_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetDouble(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetDouble"), ex.Message);
        }

        [Fact]
        public void GetDouble_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 3.14";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(3.14, reader.GetDouble(0));
                }
            }
        }

        [Fact]
        public void GetFloat_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetFloat(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetFloat"), ex.Message);
        }

        [Fact]
        public void GetFloat_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT FloatColumn FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(3.14f, reader.GetFloat(0));
                }
            }
        }

        [Fact]
        public void GetGuid_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetGuid(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetGuid"), ex.Message);
        }

        [Fact]
        public void GetGuid_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT GuidColumn FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(new Guid("dc13b11c-e6fb-449f-a892-5e2a47b05350"), reader.GetGuid(0));
                }
            }
        }

        [Fact]
        public void GetInt16_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetInt16(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetInt16"), ex.Message);
        }

        [Fact]
        public void GetInt16_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT Int16Column FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal((short)1, reader.GetInt16(0));
                }
            }
        }

        [Fact]
        public void GetInt32_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetInt32(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetInt32"), ex.Message);
        }

        [Fact]
        public void GetInt32_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                CreateTestTable(connection);

                command.CommandText = "SELECT Int32Column FROM TestTable";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(1, reader.GetInt32(0));
                }
            }
        }

        [Fact]
        public void GetInt64_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetInt64(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetInt64"), ex.Message);
        }

        [Fact]
        public void GetInt64_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(1L, reader.GetInt64(0));
                }
            }
        }

        [Fact]
        public void GetString_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetString(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetString"), ex.Message);
        }

        [Fact]
        public void GetString_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 'test'";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal("test", reader.GetString(0));
                }
            }
        }

        [Fact]
        public void GetBytes_not_supported()
        {
            var reader = CreateReader();
            var buffer = new byte[0];

            Assert.Throws<NotSupportedException>(() => reader.GetBytes(0, 0, buffer, 0, buffer.Length));
        }

        [Fact]
        public void GetChars_not_supported()
        {
            var reader = CreateReader();
            var buffer = new char[0];

            Assert.Throws<NotSupportedException>(() => reader.GetChars(0, 0, buffer, 0, buffer.Length));
        }

        [Fact]
        public void GetFieldValue_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetFieldValue<int>(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetFieldValue"), ex.Message);
        }

        [Fact]
        public void GetFieldValue_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(1L, reader.GetFieldValue<long>(0));
                }
            }
        }

        [Fact]
        public void GetValue_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetValue(0));
            Assert.Equal(Strings.FormatDataReaderClosed("GetValue"), ex.Message);
        }

        [Fact]
        public void GetValue_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    Assert.Equal(1L, reader.GetValue(0));
                }
            }
        }

        [Fact]
        public void GetValues_throws_when_closed()
        {
            var reader = CreateReader();
            reader.Close();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.GetValues(new object[0]));
            Assert.Equal(Strings.FormatDataReaderClosed("GetValues"), ex.Message);
        }

        [Fact]
        public void GetValues_works()
        {
            using (var connection = new SQLiteConnection("Filename=:memory:"))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT NULL, 1, 3.14, 'test', x'7e57'";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();

                    var values = new object[5];
                    var count = reader.GetValues(values);

                    Assert.Equal(5, count);
                    Assert.Equal(DBNull.Value, values[0]);
                    Assert.Equal(1L, values[1]);
                    Assert.Equal(3.14, values[2]);
                    Assert.Equal("test", values[3]);
                    Assert.Equal(new byte[] { 0x7e, 0x57 }, values[4]);
                }
            }
        }

        private static SQLiteDataReader CreateReader()
        {
            var command = new SQLiteCommand();
            var reader = new SQLiteDataReader(command);
            command.OpenReader = reader;

            return reader;
        }

        private static void CreateTestTable(SQLiteConnection connection)
        {
            connection.Open();
            connection.ExecuteNonQuery(@"
                CREATE TABLE TestTable (
                    BooleanColumn BIT,
                    ByteColumn TINYINT,
                    DateTimeColumn DATETIME,
                    DecimalColumn DECIMAL,
                    FloatColumn SINGLE,
                    GuidColumn UNIQUEIDENTIFIER,
                    Int16Column SMALLINT,
                    Int32Column INT
                )");
            connection.ExecuteNonQuery(@"
                INSERT INTO TestTable VALUES (
                    1,
                    1,
                    '2014-04-01 14:45:00',
                    '3.14',
                    3.14,
                    x'1cb113dcfbe69f44a8925e2a47b05350',
                    1,
                    1
                )");
        }
    }
}
