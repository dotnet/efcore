// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite.Properties;
using Microsoft.Data.Sqlite.TestUtilities;
using Xunit;

namespace Microsoft.Data.Sqlite;

public class SqliteParameterTest
{
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
    public void ParameterName_defaults_to_empty()
    {
        var parameter = new SqliteParameter();

        Assert.Empty(parameter.ParameterName);
    }

    [Fact]
    public void ParameterName_coalesces_to_empty()
    {
        var parameter = new SqliteParameter { ParameterName = null };

        Assert.NotNull(parameter.ParameterName);
        Assert.Empty(parameter.ParameterName);
    }

    [Fact]
    public void SourceColumn_defaults_to_empty()
    {
        var parameter = new SqliteParameter();

        Assert.Empty(parameter.SourceColumn);
    }

    [Fact]
    public void SourceColumn_coalesces_to_empty()
    {
        var parameter = new SqliteParameter { SourceColumn = null };

        Assert.NotNull(parameter.SourceColumn);
        Assert.Empty(parameter.SourceColumn);
    }

    [Fact]
    public void DbType_defaults_to_string()
        => Assert.Equal(DbType.String, new SqliteParameter().DbType);

    [Fact]
    public void Size_validates_argument()
    {
        var parameter = new SqliteParameter();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => parameter.Size = -2);

        Assert.Equal("value", ex.ParamName);
        Assert.Equal(-2, ex.ActualValue);
    }

    [Fact]
    public void SqliteType_defaults_to_text()
        => Assert.Equal(SqliteType.Text, new SqliteParameter().SqliteType);

    [Theory]
    [MemberData(nameof(TypesData))]
    public void SqliteType_is_inferred_from_value(object value, SqliteType expectedType)
    {
        var parameter = new SqliteParameter { Value = value };
        Assert.Equal(expectedType, parameter.SqliteType);
    }

    [Fact]
    public void SqliteType_overrides_inferred_value()
    {
        var parameter = new SqliteParameter { Value = 'A', SqliteType = SqliteType.Integer };
        Assert.Equal(SqliteType.Integer, parameter.SqliteType);
    }

    [Fact]
    public void Direction_input_by_default()
        => Assert.Equal(ParameterDirection.Input, new SqliteParameter().Direction);

    [Fact]
    public void Direction_validates_value()
    {
        var ex = Assert.Throws<ArgumentException>(() => new SqliteParameter().Direction = ParameterDirection.Output);
        Assert.Equal(Resources.InvalidParameterDirection(ParameterDirection.Output), ex.Message);
    }

    [Fact]
    public void ResetDbType_works()
    {
        var parameter = new SqliteParameter { DbType = DbType.Int64, SqliteType = SqliteType.Integer };

        parameter.ResetDbType();

        Assert.Equal(DbType.String, parameter.DbType);
        Assert.Equal(SqliteType.Text, parameter.SqliteType);
    }

    [Fact]
    public void ResetSqliteType_works()
    {
        var parameter = new SqliteParameter { DbType = DbType.Int64, SqliteType = SqliteType.Integer };

        parameter.ResetSqliteType();

        Assert.Equal(DbType.String, parameter.DbType);
        Assert.Equal(SqliteType.Text, parameter.SqliteType);
    }

    [Fact]
    public void ResetSqliteType_works_when_value()
    {
        var parameter = new SqliteParameter { Value = Array.Empty<byte>(), SqliteType = SqliteType.Text };

        parameter.ResetSqliteType();

        Assert.Equal(SqliteType.Blob, parameter.SqliteType);
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

            Assert.Equal(Resources.RequiresSet("ParameterName"), ex.Message);
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

            Assert.Equal(Resources.RequiresSet("Value"), ex.Message);
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
    [InlineData('A', 65L, SqliteType.Integer)]
    [InlineData('A', "A")]
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
    [InlineData("测试测试测试", "测试测试测试")]
    [InlineData(double.NegativeInfinity, double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
    [InlineData(float.NegativeInfinity, double.NegativeInfinity)]
    [InlineData(float.PositiveInfinity, double.PositiveInfinity)]
    public void Bind_works(object value, object coercedValue, SqliteType? sqliteType = null)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT @Parameter;";
            var sqliteParameter = command.Parameters.AddWithValue("@Parameter", value);
            if (sqliteType.HasValue)
            {
                sqliteParameter.SqliteType = sqliteType.Value;
            }

            connection.Open();

            var result = command.ExecuteScalar();

            Assert.Equal(coercedValue, result);
        }
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(float.NaN)]
    public void Bind_throws_for_nan(object value)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT @Parameter;";
            command.Parameters.AddWithValue("@Parameter", value);
            connection.Open();

            var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());
            Assert.Equal(Resources.CannotStoreNaN, ex.Message);
        }
    }

    [Fact]
    public void Bind_works_when_byte_array()
    {
        var bytes = new byte[] { 0x7E, 0x57 };
        Bind_works(bytes, bytes);
    }

    [Fact]
    public void Bind_works_when_DateTime()
        => Bind_works(new DateTime(2014, 4, 14, 11, 13, 59), "2014-04-14 11:13:59");

    [Fact]
    public void Bind_works_when_DateTime_with_SqliteType_Real()
        => Bind_works(new DateTime(2014, 4, 14, 11, 13, 59), 2456761.9680439816, SqliteType.Real);

    [Fact]
    public void Bind_works_when_DateTimeOffset()
        => Bind_works(new DateTimeOffset(2014, 4, 14, 11, 13, 59, new TimeSpan(-8, 0, 0)), "2014-04-14 11:13:59-08:00");

    [Fact]
    public void Bind_works_when_DateTimeOffset_with_SqliteType_Real()
        => Bind_works(
            new DateTimeOffset(new DateTime(2014, 4, 14, 11, 13, 59)),
            2456761.9680439816,
            SqliteType.Real);

#if NET6_0_OR_GREATER
    [Fact]
    public void Bind_works_when_DateOnly()
        => Bind_works(new DateOnly(2014, 4, 14), "2014-04-14");

    [Fact]
    public void Bind_works_when_DateOnly_with_SqliteType_Real()
        => Bind_works(new DateOnly(2014, 4, 14), 2456761.5, SqliteType.Real);

    [Fact]
    public void Bind_works_when_TimeOnly()
        => Bind_works(new TimeOnly(13, 10, 15), "13:10:15");

    [Fact]
    public void Bind_works_when_TimeOnly_with_milliseconds()
        => Bind_works(new TimeOnly(13, 10, 15, 500), "13:10:15.5000000");

    [Fact]
    public void Bind_works_when_TimeOnly_with_SqliteType_Real()
        => Bind_works(new TimeOnly(13, 10, 15), 0.5487847222222222, SqliteType.Real);
#endif

    [Fact]
    public void Bind_works_when_DBNull()
        => Bind_works(DBNull.Value, DBNull.Value);

    [Fact]
    public void Bind_works_when_decimal()
        => Bind_works(3.14m, "3.14");

    [Fact]
    public void Bind_works_when_decimal_with_integral_value()
        => Bind_works(3m, "3.0");

    [Fact]
    public void Bind_works_when_Enum()
        => Bind_works(MyEnum.One, 1L);

    [Fact]
    public void Bind_works_when_Guid_with_SqliteType_Blob()
        => Bind_works(
            new Guid("1c902ddb-f4b6-4945-af38-0dc1b0760465"),
            new byte[] { 0xDB, 0x2D, 0x90, 0x1C, 0xB6, 0xF4, 0x45, 0x49, 0xAF, 0x38, 0x0D, 0xC1, 0xB0, 0x76, 0x04, 0x65 },
            SqliteType.Blob);

    [Fact]
    public void Bind_works_when_Guid()
        => Bind_works(
            new Guid("1c902ddb-f4b6-4945-af38-0dc1b0760465"),
            "1C902DDB-F4B6-4945-AF38-0DC1B0760465");

    [Fact]
    public void Bind_works_when_Nullable()
        => Bind_works((int?)1, 1L);

    [Fact]
    public void Bind_works_when_TimeSpan()
        => Bind_works(new TimeSpan(11, 19, 32), "11:19:32");

    [Fact]
    public void Bind_works_when_TimeSpan_with_SqliteType_Real()
        => Bind_works(new TimeSpan(11, 19, 32), 0.47189814814814812, SqliteType.Real);

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

            Assert.Equal(Resources.UnknownDataType(typeof(object)), ex.Message);
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

    [Fact]
    public void Bind_with_restricted_size_works_on_string_values()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT @Text;";
            command.Parameters.AddWithValue("@Text", "ABCDE").Size = 3;
            connection.Open();

            var result = command.ExecuteScalar();

            Assert.Equal("ABC", result);
        }
    }

    [Fact]
    public void Bind_with_sentinel_size_works_on_string_values()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT $value;";
            command.Parameters.AddWithValue("$value", "TEST").Size = -1;
            connection.Open();

            var result = command.ExecuteScalar();

            Assert.Equal("TEST", result);
        }
    }

    [Fact]
    public void Bind_with_restricted_size_works_on_blob_values()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT @Blob;";
            command.Parameters.AddWithValue("@Blob", new byte[] { 1, 2, 3, 4, 5 }).Size = 3;
            connection.Open();

            var result = command.ExecuteScalar();

            Assert.Equal(new byte[] { 1, 2, 3 }, result);
        }
    }

    [Fact]
    public void Bind_with_sentinel_size_works_on_blob_values()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT $value;";
            command.Parameters.AddWithValue("$value", new byte[] { 0x7E, 0x57 }).Size = -1;
            connection.Open();

            var result = command.ExecuteScalar();

            Assert.Equal(new byte[] { 0x7E, 0x57 }, result);
        }
    }

    [Theory]
    [InlineData("@Parameter")]
    [InlineData("$Parameter")]
    [InlineData(":Parameter")]
    public void Bind_does_not_require_prefix(string parameterName)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT " + parameterName;
            command.Parameters.AddWithValue("Parameter", "harvest");
            connection.Open();

            var result = command.ExecuteScalar();

            Assert.Equal("harvest", result);
        }
    }

    [Fact]
    public void Bind_throws_for_ambiguous_parameters()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT @Param, $Param";
            command.Parameters.AddWithValue("Param", 1);
            connection.Open();

            var ex = Assert.Throws<InvalidOperationException>(() => command.ExecuteScalar());

            Assert.Equal(Resources.AmbiguousParameterName("Param"), ex.Message);
        }
    }

    [Fact]
    public void Bind_with_prefixed_names()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand();
            command.CommandText = "SELECT @Param, $Param, :Param";
            command.Parameters.AddWithValue("@Param", 1);
            command.Parameters.AddWithValue("$Param", 2);
            command.Parameters.AddWithValue(":Param", 3);
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                Assert.True(reader.Read());
                Assert.Equal(1, reader.GetFieldValue<int>(0));
                Assert.Equal(2, reader.GetFieldValue<int>(1));
                Assert.Equal(3, reader.GetFieldValue<int>(2));
            }
        }
    }

    [Fact]
    [UseCulture("ar-SA")]
    public void Bind_DateTime_with_Arabic_Culture()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Person(DateOfBirth datetime);");

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Person(DateOfBirth) VALUES (@DateOfBirth);";
            var date = new DateTime(2018, 3, 25);
            command.Parameters.AddWithValue("DateOfBirth", date);
            Assert.Equal(1, command.ExecuteNonQuery());

            command.CommandText = "SELECT DateOfBirth FROM Person;";
            var result = command.ExecuteScalar()!;
            Assert.Equal("2018-03-25 00:00:00", (string)result);

            using (var reader = command.ExecuteReader())
            {
                Assert.True(reader.Read());
                Assert.Equal("2018-03-25 00:00:00", reader.GetString(0));
                Assert.Equal(date, reader.GetDateTime(0));
            }
        }
    }

    [Fact]
    [UseCulture("ar-SA")]
    public void Bind_DateTimeOffset_with_Arabic_Culture()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Test(date TEXT);");

            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Test(date) VALUES (@date);";
            var date = new DateTimeOffset(new DateTime(2018, 3, 25), new TimeSpan());
            command.Parameters.AddWithValue("date", date);
            Assert.Equal(1, command.ExecuteNonQuery());

            command.CommandText = "SELECT date FROM Test;";
            var result = command.ExecuteScalar()!;
            Assert.Equal("2018-03-25 00:00:00+00:00", (string)result);

            using (var reader = command.ExecuteReader())
            {
                Assert.True(reader.Read());
                Assert.Equal("2018-03-25 00:00:00+00:00", reader.GetString(0));
                Assert.Equal(date, reader.GetDateTimeOffset(0));
            }
        }
    }

    [Fact]
    public void Add_range_of_parameters_using_DbCommand_base_class()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            var command = connection.CreateCommand() as DbCommand;
            command.CommandText = "SELECT @Value1, @Value2;";

            var parameterValue1 = new SqliteParameter("@Value1", SqliteType.Text);
            parameterValue1.Value = "ABCDE";

            var parameterValue2 = new SqliteParameter("@Value2", SqliteType.Text);
            parameterValue2.Value = "FGHIJ";

            var parameters = new[] { parameterValue1, parameterValue2 };

            command.Parameters.AddRange(parameters);
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                Assert.True(reader.Read());
                Assert.Equal(parameterValue1.Value, reader.GetString(0));
                Assert.Equal(parameterValue2.Value, reader.GetString(1));
            }
        }
    }

    public static IEnumerable<object[]> TypesData
        => new List<object[]>
        {
            new object[] { default(DateTime), SqliteType.Text },
            new object[] { default(DateTimeOffset), SqliteType.Text },
            new object[] { DBNull.Value, SqliteType.Text },
            new object[] { 0m, SqliteType.Text },
            new object[] { default(Guid), SqliteType.Text },
            new object[] { default(TimeSpan), SqliteType.Text },
            new object[] { default(TimeSpan), SqliteType.Text },
#if NET6_0_OR_GREATER
            new object[] { default(DateOnly), SqliteType.Text },
            new object[] { default(TimeOnly), SqliteType.Text },
#endif
            new object[] { 'A', SqliteType.Text },
            new object[] { "", SqliteType.Text },
            new object[] { false, SqliteType.Integer },
            new object[] { (byte)0, SqliteType.Integer },
            new object[] { 0, SqliteType.Integer },
            new object[] { 0L, SqliteType.Integer },
            new object[] { (sbyte)0, SqliteType.Integer },
            new object[] { (short)0, SqliteType.Integer },
            new object[] { 0u, SqliteType.Integer },
            new object[] { 0ul, SqliteType.Integer },
            new object[] { (ushort)0, SqliteType.Integer },
            new object[] { 0.0, SqliteType.Real },
            new object[] { 0f, SqliteType.Real },
            new object[] { Array.Empty<byte>(), SqliteType.Blob },
        };

    private enum MyEnum
    {
        One = 1
    }
}
