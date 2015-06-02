// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class SqliteParameterTest
    {
        [Fact]
        public void Ctor_validates_arguments()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SqliteParameter(null, 1));

            Assert.Equal("name", ex.ParamName);
        }

        [Fact]
        public void Ctor_sets_name_and_value()
        {
            var result = new SqliteParameter("@Parameter", 1);

            Assert.Equal("@Parameter", result.ParameterName);
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void Ctor_sets_other_values()
        {
            var result = new SqliteParameter("@Parameter", SqliteType.Integer, 8, "Column");

            Assert.Equal("@Parameter", result.ParameterName);
            Assert.Equal(SqliteType.Integer, result.SqliteType);
            Assert.Equal(8, result.Size);
            Assert.Equal("Column", result.SourceColumn);
        }

        [Fact]
        public void DbType_defaults_to_string()
        {
            Assert.Equal(DbType.String, new SqliteParameter().DbType);
        }

        [Fact]
        public void SqliteType_defaults_to_text()
        {
            Assert.Equal(SqliteType.Text, new SqliteParameter().SqliteType);
        }

        [Fact]
        public void Direction_input_by_default()
        {
            Assert.Equal(ParameterDirection.Input, new SqliteParameter().Direction);
        }

        [Fact]
        public void Direction_validates_value()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteParameter().Direction = ParameterDirection.Output);
            Assert.Equal(Strings.FormatInvalidParameterDirection(ParameterDirection.Output), ex.Message);
        }

        [Fact]
        public void ParameterName_validates_value()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SqliteParameter().ParameterName = null);
            Assert.Equal("value", ex.ParamName);
        }

        [Fact]
        public void ResetDbType_works()
        {
            var parameter = new SqliteParameter
                {
                    DbType = DbType.Int64,
                    SqliteType = SqliteType.Integer
                };

            parameter.ResetDbType();

            Assert.Equal(DbType.String, parameter.DbType);
            Assert.Equal(SqliteType.Text, parameter.SqliteType);
        }

        [Fact]
        public void ResetSqliteType_works()
        {
            var parameter = new SqliteParameter
                {
                    DbType = DbType.Int64,
                    SqliteType = SqliteType.Integer
                };

            parameter.ResetSqliteType();

            Assert.Equal(DbType.String, parameter.DbType);
            Assert.Equal(SqliteType.Text, parameter.SqliteType);
        }

        [Fact]
        public void Bind_requires_set_name()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @Parameter;";
                command.Parameters.Add(new SqliteParameter { Value = 1 });
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.FormatRequiresSet("ParameterName"), ex.Message);
            }
        }

        [Fact]
        public void Bind_requires_set_value()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @Parameter;";
                command.Parameters.Add(new SqliteParameter { ParameterName = "@Parameter" });
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteNonQuery());

                Assert.Equal(Strings.FormatRequiresSet("Value"), ex.Message);
            }
        }

        [Fact]
        public void Bind_is_noop_on_unknown_parameter()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1;";
                command.Parameters.AddWithValue("@Unknown", 1);
                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        [Theory]
        [InlineData(true, 1L)]
        [InlineData((byte)1, 1L)]
        [InlineData('A', 65L)]
        [InlineData(3.14, 3.14)]
        [InlineData(3f, 3.0)]
        [InlineData(1, 1L)]
        [InlineData(1L, 1L)]
        [InlineData((sbyte)1, 1L)]
        [InlineData((short)1, 1L)]
        [InlineData("test", "test")]
        [InlineData(1u, 1L)]
        [InlineData(1ul, 1L)]
        [InlineData((ushort)1, 1L)]
        public void Bind_works(object value, object coercedValue)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @Parameter;";
                command.Parameters.AddWithValue("@Parameter", value);
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.Equal(coercedValue, result);
            }
        }

        [Fact]
        public void Bind_works_when_byte_array()
        {
            var bytes = new byte[] { 0x7E, 0x57 };
            Bind_works(bytes, bytes);
        }

        [Fact]
        public void Bind_works_when_DateTime() => Bind_works(new DateTime(2014, 4, 14, 11, 13, 59), "2014-04-14 11:13:59");

        [Fact]
        public void Bind_works_when_DateTimeOffset() => Bind_works(new DateTimeOffset(2014, 4, 14, 11, 13, 59, new TimeSpan(-8, 0, 0)), "2014-04-14 11:13:59-08:00");

        [Fact]
        public void Bind_works_when_DBNull() => Bind_works(DBNull.Value, DBNull.Value);

        [Fact]
        public void Bind_works_when_decimal() => Bind_works(3.14m, "3.14");

        [Fact]
        public void Bind_works_when_Enum() => Bind_works(MyEnum.One, 1L);

        [Fact]
        public void Bind_works_when_Guid() =>
            Bind_works(
                new Guid("1c902ddb-f4b6-4945-af38-0dc1b0760465"),
                new byte[] { 0xDB, 0x2D, 0x90, 0x1C, 0xB6, 0xF4, 0x45, 0x49, 0xAF, 0x38, 0x0D, 0xC1, 0xB0, 0x76, 0x04, 0x65 });

        [Fact]
        public void Bind_works_when_Nullable() => Bind_works((int?)1, 1L);

        [Fact]
        public void Bind_works_when_TimeSpan() => Bind_works(new TimeSpan(11, 19, 32), "11:19:32");

        [Fact]
        public void Bind_throws_when_unknown()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @Parameter;";
                command.Parameters.AddWithValue("@Parameter", new object());
                connection.Open();

                var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());

                Assert.Equal(Strings.FormatUnknownDataType(typeof(object)), ex.Message);
            }
        }

        [Fact]
        public void Bind_binds_string_values_without_embedded_nulls()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT @Text || 'ing';";
                command.Parameters.AddWithValue("@Text", "test");
                connection.Open();

                var result = command.ExecuteScalar();

                Assert.Equal("testing", result);
            }
        }

        private enum MyEnum
        {
            One = 1
        }
    }
}
