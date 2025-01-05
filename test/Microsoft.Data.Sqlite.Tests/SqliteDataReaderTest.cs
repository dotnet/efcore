// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using Microsoft.Data.Sqlite.Properties;
using Xunit;

namespace Microsoft.Data.Sqlite;

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
    public void FieldCount_throws_when_closed()
        => X_throws_when_closed(
            r =>
            {
                var x = r.FieldCount;
            }, "FieldCount");

    [Fact]
    public void FieldCount_returns_zero_when_non_query()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("CREATE TABLE dual(dummy);"))
            {
                Assert.Equal(0, reader.FieldCount);
            }
        }
    }

    [Fact]
    public void GetBoolean_works()
        => GetX_works(
            "SELECT 1;",
            r => r.GetBoolean(0),
            true);

    [Fact]
    public void GetBoolean_throws_when_closed()
        => X_throws_when_closed(r => r.GetBoolean(0), nameof(SqliteDataReader.GetBoolean));

    [Fact]
    public void GetBoolean_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetBoolean(0));

    [Fact]
    public void GetByte_works()
        => GetX_works(
            "SELECT 1;",
            r => r.GetByte(0),
            (byte)1);

    [Fact]
    public void GetByte_throws_when_closed()
        => X_throws_when_closed(r => r.GetByte(0), nameof(SqliteDataReader.GetByte));

    [Fact]
    public void GetByte_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetByte(0));

    [Fact]
    public void GetBytes_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");
            connection.ExecuteNonQuery("INSERT INTO Test VALUES(x'427E5743');");
            connection.ExecuteNonQuery("INSERT INTO Test VALUES(x'538F6854');");
            connection.ExecuteNonQuery("INSERT INTO Test VALUES(x'649A7965');");

            using (var reader = connection.ExecuteReader("SELECT Value FROM Test;"))
            {
                var list = new List<byte[]>();
                while (reader.Read())
                {
                    var buffer = new byte[6];
                    var bytesRead = reader.GetBytes(0, 0, buffer, 0, buffer.Length);
                    Assert.Equal(4, bytesRead);
                    list.Add(buffer);
                }

                Assert.Equal(3, list.Count);
                Assert.Equal([0x42, 0x7E, 0x57, 0x43, 0, 0], list[0]);
                Assert.Equal([0x53, 0x8F, 0x68, 0x54, 0, 0], list[1]);
                Assert.Equal([0x64, 0x9A, 0x79, 0x65, 0, 0], list[2]);
            }
        }
    }

    [Fact]
    public void GetBytes_works_streaming()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (x'01020304');");

            using (var reader = connection.ExecuteReader("SELECT rowid, Value FROM Data;"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var buffer = new byte[2];
                reader.GetBytes(1, 1, buffer, 0, buffer.Length);
                Assert.Equal([0x02, 0x03], buffer);
            }
        }
    }

    [Fact]
    public void GetBytes_works_streaming_join()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE A (ID INTEGER PRIMARY KEY,VALUE BLOB); INSERT INTO A (ID, VALUE) VALUES (1,x'01020304');");
            connection.ExecuteNonQuery("CREATE TABLE B (ID INTEGER PRIMARY KEY,FATHER_ID INTEGER NOT NULL,VALUE BLOB); INSERT INTO B (ID,FATHER_ID, VALUE) VALUES (1000,1,x'05060708');");

            using (var reader = connection.ExecuteReader(@"SELECT 
                                                A.ID as AID,
                                                A.VALUE as AVALUE,
                                                B.ID as BID,
                                                B.VALUE as BVALUE
                                            FROM 
                                                A JOIN B
                                                ON B.FATHER_ID=A.ID "))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                //reading fields that does not involve blobs should be ok
                Console.WriteLine($"A.ID={reader.GetInt32(0)} B.ID={reader.GetInt32(2)}");

                //get len of abuff
                var abuff = new byte[2];
                reader.GetBytes(1, 1, abuff, 0, abuff.Length);
                Assert.Equal([0x02, 0x03], abuff);

                var bbuff = new byte[2];
                reader.GetBytes(3, 1, bbuff, 0, bbuff.Length);  //this was failing. now should be fixed
                Assert.Equal([0x06, 0x07], bbuff);

            }
        }
    }

    [Fact]
    public void GetBytes_NullBuffer()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT x'427E5743';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var bytesRead = reader.GetBytes(0, 1, null, 0, 3);

                Assert.Equal(4, bytesRead);
            }
        }
    }

    [Fact]
    public void GetBytes_works_with_overflow()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT x'427E5743';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var hugeBuffer = new byte[1024];
                var bytesRead = reader.GetBytes(0, 1, hugeBuffer, 0, hugeBuffer.Length);
                Assert.Equal(3, bytesRead);

                var correctBytes = new byte[3] { 0x7E, 0x57, 0x43 };
                for (var i = 0; i < bytesRead; i++)
                {
                    Assert.Equal(correctBytes[i], hugeBuffer[i]);
                }
            }
        }
    }

    [Fact]
    public void GetBytes_throws_when_closed()
        => X_throws_when_closed(r => r.GetBytes(0, 0, null, 0, 0), nameof(SqliteDataReader.GetBytes));

    [Fact]
    public void GetBytes_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetBytes(0, 0, null, 0, 0));

    [Fact]
    public void GetChar_works()
        => GetX_works(
            "SELECT 1;",
            r => r.GetChar(0),
            (char)1);

    [Fact]
    public void GetChar_works_with_text()
        => GetX_works(
            "SELECT 'A';",
            r => r.GetChar(0),
            'A');

    [Fact]
    public void GetChar_throws_when_closed()
        => X_throws_when_closed(r => r.GetChar(0), nameof(SqliteDataReader.GetChar));

    [Fact]
    public void GetChar_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetChar(0));

    [Fact]
    public void GetChars_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 'têst';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var buffer = new char[2];
                reader.GetChars(0, 1, buffer, 0, buffer.Length);
                Assert.Equal(new[] { 'ê', 's' }, buffer);
            }
        }
    }

    [Fact]
    public void GetChars_works_when_buffer_null()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 'têst';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var bytesRead = reader.GetChars(0, 1, null, 0, 3);

                Assert.Equal(4, bytesRead);
            }
        }
    }

    [Fact]
    public void GetChars_works_with_overflow()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 'têst';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var hugeBuffer = new char[1024];
                var charsRead = reader.GetChars(0, 1, hugeBuffer, 0, hugeBuffer.Length);
                Assert.Equal(3, charsRead);

                var correctBytes = new char[3] { 'ê', 's', 't' };
                for (var i = 0; i < charsRead; i++)
                {
                    Assert.Equal(correctBytes[i], hugeBuffer[i]);
                }
            }
        }
    }

    [Fact]
    public void GetChars_throws_when_dataOffset_out_of_range()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 'têst';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var buffer = new char[1];
                var ex = Assert.Throws<ArgumentOutOfRangeException>(
                    () => reader.GetChars(0, 5, buffer, 0, buffer.Length));
                Assert.Equal("dataOffset", ex.ParamName);
            }
        }
    }

    [Fact]
    public void GetChars_throws_when_closed()
        => X_throws_when_closed(r => r.GetChars(0, 0, null!, 0, 0), nameof(SqliteDataReader.GetChars));

    [Fact]
    public void GetChars_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetChars(0, 0, null!, 0, 0));

    [Fact]
    public void GetChars_works_streaming()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES ('têst');");

            using (var reader = connection.ExecuteReader("SELECT rowid, Value FROM Data;"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var buffer = new char[2];
                reader.GetChars(1, 1, buffer, 0, buffer.Length);
                Assert.Equal(new[] { 'ê', 's' }, buffer);
            }
        }
    }

    [Fact]
    public void GetStream_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT x'427E5743';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var stream = reader.GetStream(0);
                Assert.IsType<MemoryStream>(stream);
                Assert.Equal(0x42, stream.ReadByte());
                var stream2 = reader.GetStream(0);
                Assert.Equal(0x42, stream2.ReadByte());
                Assert.Equal(0x7E, stream.ReadByte());
            }
        }
    }

    [Fact]
    public void GetStream_works_with_text()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 'abcdefghi';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var stream = reader.GetStream(0);
                Assert.Equal((byte)'a', stream.ReadByte());
                var stream2 = reader.GetStream(0);
                Assert.Equal((byte)'a', stream2.ReadByte());
                Assert.Equal((byte)'b', stream.ReadByte());
            }
        }
    }

    [Fact]
    public void GetStream_works_with_int()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 12;"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var stream = reader.GetStream(0);
                Assert.Equal((byte)'1', stream.ReadByte());
                var stream2 = reader.GetStream(0);
                Assert.Equal((byte)'1', stream2.ReadByte());
                Assert.Equal((byte)'2', stream.ReadByte());
            }
        }
    }

    [Fact]
    public void GetStream_works_with_float()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 1.2;"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var stream = reader.GetStream(0);
                Assert.Equal((byte)'1', stream.ReadByte());
                var stream2 = reader.GetStream(0);
                Assert.Equal((byte)'1', stream2.ReadByte());
                Assert.Equal((byte)'.', stream.ReadByte());
                Assert.Equal((byte)'2', stream.ReadByte());
            }
        }
    }

    [Theory]
    [InlineData("CREATE TABLE DataTable (Id INTEGER, Data BLOB);", "SELECT rowid, Data FROM DataTable WHERE Id = 5")]
    [InlineData("CREATE TABLE DataTable (Id INTEGER PRIMARY KEY, Data BLOB);", "SELECT rowid, Data FROM DataTable WHERE Id = 5")]
    [InlineData("CREATE TABLE DataTable (Id INTEGER PRIMARY KEY, Data BLOB);", "SELECT Id, Data FROM DataTable WHERE Id = 5")]
    public void GetStream_Blob_works(string createTableCmd, string selectCmd)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery(
                createTableCmd + "INSERT INTO DataTable VALUES (5, X'01020304');");

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = selectCmd;
            using (var reader = selectCommand.ExecuteReader())
            {
                Assert.True(reader.Read());
                using (var sourceStream = reader.GetStream(1))
                {
                    Assert.IsType<SqliteBlob>(sourceStream);
                    var buffer = new byte[4];
                    var bytesRead = sourceStream.Read(buffer, 0, 4);
                    Assert.Equal(4, bytesRead);
                    Assert.Equal([0x01, 0x02, 0x03, 0x04], buffer);
                }
            }
        }
    }

    [Fact]
    public void GetStream_Blob_works_when_long_pk()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery(
                "CREATE TABLE DataTable (Id INTEGER PRIMARY KEY, Data BLOB);" + "INSERT INTO DataTable VALUES (2147483648, X'01020304');");

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT Id, Data FROM DataTable WHERE Id = 2147483648";
            using (var reader = selectCommand.ExecuteReader())
            {
                Assert.True(reader.Read());
                using (var sourceStream = reader.GetStream(1))
                {
                    Assert.IsType<SqliteBlob>(sourceStream);
                    var buffer = new byte[4];
                    var bytesRead = sourceStream.Read(buffer, 0, 4);
                    Assert.Equal(4, bytesRead);
                    Assert.Equal([0x01, 0x02, 0x03, 0x04], buffer);
                }
            }
        }
    }

    [Fact]
    public void GetStream_works_when_composite_pk()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery(
                @"CREATE TABLE DataTable (Id1 INTEGER, Id2 INTEGER, Data BLOB, PRIMARY KEY (Id1, Id2));
                    INSERT INTO DataTable VALUES (5, 6, X'01020304');");

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT Id1, Id2, Data FROM DataTable WHERE Id1 = 5 AND Id2 = 6";
            using (var reader = selectCommand.ExecuteReader())
            {
                Assert.True(reader.Read());
                using (var sourceStream = reader.GetStream(2))
                {
                    Assert.IsType<MemoryStream>(sourceStream);
                    var buffer = new byte[4];
                    var bytesRead = sourceStream.Read(buffer, 0, 4);
                    Assert.Equal(4, bytesRead);
                    Assert.Equal([0x01, 0x02, 0x03, 0x04], buffer);
                }
            }
        }
    }

    [Fact]
    public void GetStream_works_when_composite_pk_and_rowid()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery(
                @"CREATE TABLE DataTable (Id1 INTEGER, Id2 INTEGER, Data BLOB, PRIMARY KEY (Id1, Id2));
                    INSERT INTO DataTable VALUES (5, 6, X'01020304');");

            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = "SELECT Id1, Id2, rowid, Data FROM DataTable WHERE Id1 = 5 AND Id2 = 6";
            using (var reader = selectCommand.ExecuteReader())
            {
                Assert.True(reader.Read());
                using (var sourceStream = reader.GetStream(3))
                {
                    Assert.IsType<SqliteBlob>(sourceStream);
                    var buffer = new byte[4];
                    var bytesRead = sourceStream.Read(buffer, 0, 4);
                    Assert.Equal(4, bytesRead);
                    Assert.Equal([0x01, 0x02, 0x03, 0x04], buffer);
                }
            }
        }
    }

    [Fact]
    public void GetStream_throws_when_closed()
        => X_throws_when_closed(r => r.GetStream(0), nameof(SqliteDataReader.GetStream));

    [Fact]
    public void GetStream_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetStream(0));

    [Fact]
    public void GetTextReader_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 'test';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                using (var textReader = reader.GetTextReader(0))
                {
                    Assert.IsType<MemoryStream>(Assert.IsType<StreamReader>(textReader).BaseStream);
                    Assert.Equal("test", textReader.ReadToEnd());
                }
            }
        }
    }

    [Fact]
    public void GetTextReader_works_when_null()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT NULL;"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                using (var textReader = reader.GetTextReader(0))
                {
                    Assert.IsType<StringReader>(textReader);
                    Assert.Empty(textReader.ReadToEnd());
                }
            }
        }
    }

    [Fact]
    public void GetTextReader_works_streaming()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES ('test');");

            using (var reader = connection.ExecuteReader("SELECT rowid, Value FROM Data;"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                using (var textReader = reader.GetTextReader(1))
                {
                    Assert.IsType<SqliteBlob>(Assert.IsType<StreamReader>(textReader).BaseStream);
                    Assert.Equal("test", textReader.ReadToEnd());
                }
            }
        }
    }

    [Fact]
    public void GetDateTime_works_with_text()
        => GetX_works(
            "SELECT '2014-04-15 10:47:16';",
            r => r.GetDateTime(0),
            new DateTime(2014, 4, 15, 10, 47, 16));

    [Fact]
    public void GetDateTime_works_with_real()
        => GetX_works(
            "SELECT julianday('2013-10-07 08:23:19.120');",
            r => r.GetDateTime(0),
            new DateTime(2013, 10, 7, 8, 23, 19, 120));

    [Fact]
    public void GetDateTime_works_with_integer()
        => GetX_works(
            "SELECT CAST(julianday('2013-10-07 12:00') AS INTEGER);",
            r => r.GetDateTime(0),
            new DateTime(2013, 10, 7, 12, 0, 0));

    [Fact]
    public void GetDateTime_throws_when_null()
        => GetX_throws_when_null(r => r.GetDateTime(0));

    [Fact]
    public void GetDateTime_throws_when_closed()
        => X_throws_when_closed(r => r.GetDateTime(0), nameof(SqliteDataReader.GetDateTime));

    [Fact]
    public void GetDateTime_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetDateTime(0));

    [Fact]
    public void GetDateTimeOffset_works_with_text()
        => GetX_works(
            "SELECT '2014-04-15 10:47:16';",
            r => ((SqliteDataReader)r).GetDateTimeOffset(0),
            new DateTimeOffset(new DateTime(2014, 4, 15, 10, 47, 16)));

    [Fact]
    public void GetDateTimeOffset_works_with_real()
        => GetX_works(
            "SELECT julianday('2013-10-07 08:23:19.120');",
            r => ((SqliteDataReader)r).GetDateTimeOffset(0),
            new DateTimeOffset(new DateTime(2013, 10, 7, 8, 23, 19, 120)));

    [Fact]
    public void GetDateTimeOffset_works_with_integer()
        => GetX_works(
            "SELECT CAST(julianday('2013-10-07 12:00') AS INTEGER);",
            r => ((SqliteDataReader)r).GetDateTimeOffset(0),
            new DateTimeOffset(new DateTime(2013, 10, 7, 12, 0, 0)));

    [Fact]
    public void GetDateTimeOffset_throws_when_closed()
        => X_throws_when_closed(r => r.GetDateTimeOffset(0), nameof(SqliteDataReader.GetDateTimeOffset));

    [Fact]
    public void GetDateTimeOffset_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetDateTimeOffset(0));

    [Fact]
    public void GetTimeSpan_works_with_text()
        => GetX_works(
            "SELECT '12:06:29';",
            r => ((SqliteDataReader)r).GetTimeSpan(0),
            new TimeSpan(12, 06, 29));

    [Fact]
    public void GetTimeSpan_works_with_real()
        => GetX_works(
            "SELECT julianday('2013-10-12 09:25:22.120') - julianday('2013-10-07 08:23:19');",
            r => ((SqliteDataReader)r).GetTimeSpan(0),
            TimeSpan.FromDays(5.04309166688472));

    [Fact]
    public void GetTimeSpan_works_with_integer()
        => GetX_works(
            "SELECT CAST(julianday('2017-08-31') - julianday('1776-07-04') AS INTEGER);",
            r => ((SqliteDataReader)r).GetTimeSpan(0),
            new TimeSpan(88081, 0, 0, 0));

    [Fact]
    public void GetTimeSpan_throws_when_closed()
        => X_throws_when_closed(r => r.GetTimeSpan(0), nameof(SqliteDataReader.GetTimeSpan));

    [Fact]
    public void GetTimeSpan_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetTimeSpan(0));

    [Fact]
    public void GetDateTimeOffset_throws_when_null()
        => GetX_throws_when_null(r => ((SqliteDataReader)r).GetDateTimeOffset(0));

#if NET6_0_OR_GREATER
    [Fact]
    public void GetFieldValue_of_DateOnly_works()
        => GetFieldValue_works(
            "SELECT '2014-04-15';",
            new DateOnly(2014, 4, 15));

    [Fact]
    public void GetFieldValue_of_DateOnly_works_with_real()
        => GetFieldValue_works(
            "SELECT julianday('2014-04-15');",
            new DateOnly(2014, 4, 15));

    [Fact]
    public void GetFieldValue_of_TimeOnly_works()
        => GetFieldValue_works(
            "SELECT '13:10:15';",
            new TimeOnly(13, 10, 15));

    [Fact]
    public void GetFieldValue_of_TimeOnly_works_with_milliseconds()
        => GetFieldValue_works(
            "SELECT '13:10:15.5';",
            new TimeOnly(13, 10, 15, 500));
#endif

    [Theory]
    [InlineData("SELECT 1;", "INTEGER")]
    [InlineData("SELECT 3.14;", "REAL")]
    [InlineData("SELECT 'test';", "TEXT")]
    [InlineData("SELECT X'7E57';", "BLOB")]
    [InlineData("SELECT NULL;", "BLOB")]
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
    public void GetDataTypeName_throws_when_closed()
        => X_throws_when_closed(r => r.GetDataTypeName(0), "GetDataTypeName");

    [Fact]
    public void GetDataTypeName_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetDataTypeName(0));

    [Theory]
    [InlineData("3.14", 3.14)]
    [InlineData("1.0e-2", 0.01)]
    public void GetDecimal_works(string input, decimal expected)
        => GetX_works(
            "SELECT '" + input + "';",
            r => r.GetDecimal(0),
            expected);

    [Fact]
    public void GetDecimal_throws_when_null()
        => GetX_throws_when_null(r => r.GetDecimal(0));

    [Fact]
    public void GetDecimal_throws_when_closed()
        => X_throws_when_closed(r => r.GetDecimal(0), nameof(SqliteDataReader.GetDecimal));

    [Fact]
    public void GetDecimal_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetDecimal(0));

    [Fact]
    public void GetDouble_throws_when_null()
        => GetX_throws_when_null(
            r => r.GetDouble(0));

    [Fact]
    public void GetDouble_throws_when_closed()
        => X_throws_when_closed(r => r.GetDouble(0), nameof(SqliteDataReader.GetDouble));

    [Fact]
    public void GetDouble_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetDouble(0));

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
    public void GetFieldValue_of_byteArray_works()
        => GetFieldValue_works(
            "SELECT X'7E57';",
            new byte[] { 0x7e, 0x57 });

    [Fact]
    public void GetFieldValue_of_byteArray_empty()
        => GetFieldValue_works(
            "SELECT X'';", Array.Empty<byte>());

    [Fact]
    public void GetFieldValue_of_byteArray_throws_when_null()
        => GetX_throws_when_null(
            r => r.GetFieldValue<byte[]>(0));

    [Fact]
    public void GetFieldValue_of_DateTime_works()
        => GetFieldValue_works(
            "SELECT '2014-04-15 11:58:13';",
            new DateTime(2014, 4, 15, 11, 58, 13));

    [Fact]
    public void GetFieldValue_of_DateTimeOffset_works()
        => GetFieldValue_works(
            "SELECT '2014-04-15 11:58:13-08:00';",
            new DateTimeOffset(2014, 4, 15, 11, 58, 13, new TimeSpan(-8, 0, 0)));

    [Fact]
    public void GetFieldValue_of_DBNull_works()
        => GetFieldValue_works(
            "SELECT NULL;",
            DBNull.Value);

    [Fact]
    public void GetFieldValue_of_DBNull_throws_when_not_null()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 1;"))
            {
                var hasData = reader.Read();

                Assert.True(hasData);
                Assert.Throws<InvalidCastException>(() => reader.GetFieldValue<DBNull>(0));
            }
        }
    }

    [Fact]
    public void GetFieldValue_of_decimal_works()
        => GetFieldValue_works(
            "SELECT '3.14';",
            3.14m);

    [Fact]
    public void GetFieldValue_of_Enum_works()
        => GetFieldValue_works(
            "SELECT 1;",
            MyEnum.One);

    [Fact]
    public void GetFieldValue_of_Guid_works()
        => GetFieldValue_works(
            "SELECT X'0E7E0DDC5D364849AB9B8CA8056BF93A';",
            new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

    [Fact]
    public void GetFieldValue_of_Nullable_works()
        => GetFieldValue_works(
            "SELECT 1;",
            (int?)1);

    [Fact]
    public void GetFieldValue_of_Stream_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT x'7E57';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                var stream = reader.GetFieldValue<Stream>(0);
                Assert.Equal(0x7E, stream.ReadByte());
                Assert.Equal(0x57, stream.ReadByte());
            }
        }
    }

    [Fact]
    public void GetFieldValue_of_TextReader_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 'test';"))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                using (var textReader = reader.GetFieldValue<TextReader>(0))
                {
                    Assert.Equal("test", textReader.ReadToEnd());
                }
            }
        }
    }

    [Fact]
    public void GetFieldValue_of_TimeSpan_works()
        => GetFieldValue_works(
            "SELECT '12:06:29';",
            new TimeSpan(12, 6, 29));

    [Fact]
    public void GetFieldValue_of_TimeSpan_throws_when_null()
        => GetX_throws_when_null(r => r.GetFieldValue<TimeSpan>(0));

    [Fact]
    public void GetFieldValue_throws_before_read()
        => X_throws_before_read(r => r.GetFieldValue<DBNull>(0));

    [Fact]
    public void GetFieldValue_throws_when_done()
        => X_throws_when_done(r => r.GetFieldValue<DBNull>(0));

    [Fact]
    public void GetFieldValue_throws_when_closed()
        => X_throws_when_closed(r => r.GetFieldValue<long>(0), nameof(SqliteDataReader.GetFieldValue));

    [Fact]
    public void GetFieldValue_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetFieldValue<long>(0));

    [Theory]
    [InlineData(byte.MinValue)]
    [InlineData(char.MinValue)]
    [InlineData(int.MinValue)]
    [InlineData(sbyte.MinValue)]
    [InlineData(short.MinValue)]
    [InlineData(uint.MinValue)]
    [InlineData(ushort.MinValue)]
    public void GetFieldValue_throws_on_overflow<T>(T minValue)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader(
                       "SELECT $minValue - 1;",
                       new SqliteParameter("$minValue", minValue)))
            {
                reader.Read();

                Assert.Throws<OverflowException>(() => reader.GetFieldValue<T>(0));
            }
        }
    }

    [Theory]
    [InlineData("SELECT 1;", typeof(long))]
    [InlineData("SELECT 3.14;", typeof(double))]
    [InlineData("SELECT 'test';", typeof(string))]
    [InlineData("SELECT X'7E57';", typeof(byte[]))]
    [InlineData("SELECT NULL;", typeof(byte[]))]
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

    [Theory]
    [InlineData("TEXT", typeof(string))]
    [InlineData("CHARACTER(20)", typeof(string))]
    [InlineData("NVARCHAR(100)", typeof(string))]
    [InlineData("CLOB", typeof(string))]
    [InlineData("INTEGER", typeof(long))]
    [InlineData("BIGINT", typeof(long))]
    [InlineData("UNSIGNED BIG INT", typeof(long))]
    [InlineData("REAL", typeof(double))]
    [InlineData("DOUBLE", typeof(double))]
    [InlineData("FLOAT", typeof(double))]
    [InlineData("BLOB", typeof(byte[]))]
    [InlineData("", typeof(byte[]))]
    [InlineData("NUMERIC", typeof(string))]
    [InlineData("DATETIME", typeof(string))]
    public void GetFieldType_works_on_NULL(string type, Type expected)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery($"CREATE TABLE Test(Value {type});");

            using (var reader = connection.ExecuteReader("SELECT Value FROM Test;"))
            {
                Assert.Equal(expected, reader.GetFieldType(0));
            }
        }
    }

    [Fact]
    public void GetFieldType_works_on_NULL_cached()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Test(Value FOOBAR);");
            connection.ExecuteNonQuery("INSERT INTO Test (Value) VALUES ('test'), (NULL);");

            using (var reader = connection.ExecuteReader("SELECT Value FROM Test;"))
            {
                Assert.True(reader.Read());
                Assert.Equal(typeof(string), reader.GetFieldType(0));
                Assert.Equal("test", reader.GetValue(0));
                Assert.True(reader.Read());
                Assert.Equal(typeof(string), reader.GetFieldType(0));
                Assert.Equal(DBNull.Value, reader.GetValue(0));
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
    public void GetFieldType_throws_when_closed()
        => X_throws_when_closed(r => r.GetFieldType(0), "GetFieldType");

    [Fact]
    public void GetFieldType_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetFieldType(0));

    [Theory]
    [InlineData("3", 3f)]
    [InlineData("9e999", float.PositiveInfinity)]
    [InlineData("-9e999", float.NegativeInfinity)]
    public void GetFloat_works(string val, float result)
        => GetX_works(
            "SELECT " + val,
            r => r.GetFloat(0),
            result);

    [Fact]
    public void GetFloat_throws_when_closed()
        => X_throws_when_closed(r => r.GetFloat(0), nameof(SqliteDataReader.GetFloat));

    [Fact]
    public void GetFloat_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetFloat(0));

    [Theory]
    [InlineData("2.0", 2.0)]
    [InlineData("9e999", double.PositiveInfinity)]
    [InlineData("-9e999", double.NegativeInfinity)]
    [InlineData("'3.14'", 3.14)]
    [InlineData("'1.2e-03'", 0.0012)]
    public void GetDouble_works(string val, double result)
        => GetX_works(
            "SELECT " + val,
            r => r.GetDouble(0),
            result);

    [Fact]
    public void GetGuid_works_when_blob()
        => GetX_works(
            "SELECT X'0E7E0DDC5D364849AB9B8CA8056BF93A';",
            r => r.GetGuid(0),
            new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

    [Fact]
    public void GetGuid_works_when_text_blob()
        => GetX_works(
            "SELECT CAST('dc0d7e0e-365d-4948-ab9b-8ca8056bf93a' AS BLOB);",
            r => r.GetGuid(0),
            new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

    [Fact]
    public void GetGuid_works_when_text()
        => GetX_works(
            "SELECT 'dc0d7e0e-365d-4948-ab9b-8ca8056bf93a';",
            r => r.GetGuid(0),
            new Guid("dc0d7e0e-365d-4948-ab9b-8ca8056bf93a"));

    [Fact]
    public void GetGuid_throws_when_null()
        => GetX_throws_when_null(r => r.GetGuid(0));

    [Fact]
    public void GetGuid_throws_when_closed()
        => X_throws_when_closed(r => r.GetGuid(0), nameof(SqliteDataReader.GetGuid));

    [Fact]
    public void GetGuid_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetGuid(0));

    [Fact]
    public void GetInt16_works()
        => GetX_works(
            "SELECT 1;",
            r => r.GetInt16(0),
            (short)1);

    [Fact]
    public void GetInt16_throws_when_closed()
        => X_throws_when_closed(r => r.GetInt16(0), nameof(SqliteDataReader.GetInt16));

    [Fact]
    public void GetInt16_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetInt16(0));

    [Fact]
    public void GetInt32_works()
        => GetX_works(
            "SELECT 1;",
            r => r.GetInt32(0),
            1);

    [Fact]
    public void GetInt32_throws_when_closed()
        => X_throws_when_closed(r => r.GetInt32(0), nameof(SqliteDataReader.GetInt32));

    [Fact]
    public void GetInt32_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetInt32(0));

    [Fact]
    public void GetInt64_works()
        => GetX_works(
            "SELECT 1;",
            r => r.GetInt64(0),
            1L);

    [Fact]
    public void GetInt64_throws_when_closed()
        => X_throws_when_closed(r => r.GetInt64(0), nameof(SqliteDataReader.GetInt64));

    [Fact]
    public void GetInt64_throws_when_null()
        => GetX_throws_when_null(
            r => r.GetInt64(0));

    [Fact]
    public void GetInt64_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetInt64(0));

    [Fact]
    public void GetName_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 1 AS Id;"))
            {
                Assert.Equal("Id", reader.GetName(0));

                // NB: Repeated to use caching
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
    public void GetName_throws_when_closed()
        => X_throws_when_closed(r => r.GetName(0), "GetName");

    [Fact]
    public void GetName_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetName(0));

    [Fact]
    public void GetOrdinal_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT 1 AS Id;"))
            {
                Assert.Equal(0, reader.GetOrdinal("Id"));

                // NB: Repeated to use caching
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
    public void GetOrdinal_throws_when_ambiguous()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var reader = connection.ExecuteReader("SELECT 1 AS Id, 2 AS ID");
        var ex = Assert.Throws<InvalidOperationException>(() => reader.GetOrdinal("id"));

        Assert.Contains(Resources.AmbiguousColumnName("id", "Id", "ID"), ex.Message);
    }

    [Fact]
    public void GetOrdinal_throws_when_closed()
        => X_throws_when_closed(r => r.GetOrdinal(null!), nameof(SqliteDataReader.GetOrdinal));

    [Fact]
    public void GetOrdinal_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetOrdinal("dummy"));

    [Fact]
    public void GetString_works_utf8()
        => GetX_works(
            "SELECT '测试测试测试';",
            r => r.GetString(0),
            "测试测试测试");

    [Fact]
    public void GetFieldValue_works_utf8()
        => GetX_works(
            "SELECT '测试测试测试';",
            r => r.GetFieldValue<string>(0),
            "测试测试测试");

    [Fact]
    public void GetValue_to_string_works_utf8()
        => GetX_works(
            "SELECT '测试测试测试';",
            r => r.GetValue(0) as string,
            "测试测试测试");

    [Fact]
    public void GetString_works()
        => GetX_works(
            "SELECT 'test';",
            r => r.GetString(0),
            "test");

    [Fact]
    public void GetString_throws_when_null()
        => GetX_throws_when_null(
            r => r.GetString(0));

    [Fact]
    public void GetString_throws_when_closed()
        => X_throws_when_closed(r => r.GetString(0), nameof(SqliteDataReader.GetString));

    [Fact]
    public void GetString_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetString(0));

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
    public void GetValue_works_when_blob()
        => GetValue_works(
            "SELECT X'7E57';",
            new byte[] { 0x7e, 0x57 });

    [Fact]
    public void GetValue_works_when_null()
        => GetValue_works(
            "SELECT NULL;",
            DBNull.Value);

    [Fact]
    public void GetValue_throws_before_read()
        => X_throws_before_read(r => r.GetValue(0));

    [Fact]
    public void GetValue_throws_when_done()
        => X_throws_when_done(r => r.GetValue(0));

    [Fact]
    public void GetValue_throws_when_closed()
        => X_throws_when_closed(r => r.GetValue(0), "GetValue");

    [Fact]
    public void GetValue_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetValue(0));

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

                var values = Array.Empty<object>();
                Assert.Throws<IndexOutOfRangeException>(() => reader.GetValues(values));
            }
        }
    }

    [Fact]
    public void GetValues_throws_when_closed()
        => X_throws_when_closed(r => r.GetValues(null!), nameof(SqliteDataReader.GetValues));

    [Fact]
    public void GetValues_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetValues(null!));

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
            reader.Close();

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
    public void IsDBNull_throws_before_read()
        => X_throws_before_read(r => r.IsDBNull(0));

    [Fact]
    public void IsDBNull_throws_when_done()
        => X_throws_when_done(r => r.IsDBNull(0));

    [Fact]
    public void IsDBNull_throws_when_closed()
        => X_throws_when_closed(r => r.IsDBNull(0), "IsDBNull");

    [Fact]
    public void IsDBNull_throws_when_non_query()
        => X_throws_when_non_query(r => r.IsDBNull(0));

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
    public void Item_by_ordinal_throws_when_non_query()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("CREATE TABLE dual(dummy);"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => reader[0]);
                Assert.Equal(Resources.NoData, ex.Message);
            }
        }
    }

    [Theory]
    [InlineData("SELECT 1 AS Id;", "Id", 1L)]
    [InlineData("SELECT 1 AS Id;", "id", 1L)]
    [InlineData("SELECT 1 AS Id, 2 AS id;", "id", 2L)]
    public void Item_by_name_works(string query, string column, long expected)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader(query))
            {
                var hasData = reader.Read();
                Assert.True(hasData);

                Assert.Equal(expected, reader[column]);
            }
        }
    }

    [Fact]
    public void Item_by_name_throws_when_non_query()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("CREATE TABLE dual(dummy);"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => reader["dummy"]);
                Assert.Equal(Resources.NoData, ex.Message);
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
    public void NextResult_throws_on_error()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");
            connection.CreateFunction<string, long>("throw", message => throw new Exception(message));

            var sql = @"
                    SELECT 1;
                    SELECT throw('An error');
                    INSERT INTO Test VALUES (1);";
            using (var reader = connection.ExecuteReader(sql))
            {
                var ex = Assert.Throws<SqliteException>(() => reader.NextResult());
                Assert.Contains("An error", ex.Message);
            }

            Assert.Equal(0L, connection.ExecuteScalar<long>("SELECT count() FROM Test;"));
        }
    }

    [Fact]
    public void NextResult_throws_when_closed()
        => X_throws_when_closed(r => r.NextResult(), nameof(SqliteDataReader.NextResult));

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

                hasData = reader.Read();
                Assert.False(hasData);
            }
        }
    }

    [Fact]
    public void Read_throws_when_closed()
        => X_throws_when_closed(r => r.Read(), "Read");

    [Fact]
    public void Read_returns_false_when_non_query()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("CREATE TABLE dual(dummy);"))
            {
                Assert.False(reader.Read());
            }
        }
    }

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

    [Fact]
    public void RecordsAffected_works_during_enumeration()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");

            var reader = connection.ExecuteReader(
                @"
                    SELECT 1;
                    INSERT INTO Test VALUES(1);
                    SELECT 1;
                    INSERT INTO Test VALUES(2);");
            using (reader)
            {
                Assert.Equal(-1, reader.RecordsAffected);
                reader.NextResult();
                Assert.Equal(1, reader.RecordsAffected);
            }

            Assert.Equal(2, reader.RecordsAffected);
        }
    }

    [Fact]
    public void RecordsAffected_works_with_returning()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            if (new Version(connection.ServerVersion) < new Version(3, 35, 0))
            {
                // Skip. RETURNING clause not supported
                return;
            }

            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");

            var reader = connection.ExecuteReader("INSERT INTO Test VALUES(1) RETURNING rowid;");
            ((IDisposable)reader).Dispose();

            Assert.Equal(1, reader.RecordsAffected);
        }
    }

    [Fact]
    public void RecordsAffected_works_with_returning_before_dispose_after_draining()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            if (new Version(connection.ServerVersion) < new Version(3, 35, 0))
            {
                // Skip. RETURNING clause not supported
                return;
            }

            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");

            using (var reader = connection.ExecuteReader("INSERT INTO Test VALUES(1) RETURNING rowid;"))
            {
                while (reader.Read())
                {
                }

                Assert.Equal(1, reader.RecordsAffected);
            }
        }
    }

    [Fact]
    public void RecordsAffected_works_with_returning_multiple()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            if (new Version(connection.ServerVersion) < new Version(3, 35, 0))
            {
                // Skip. RETURNING clause not supported
                return;
            }

            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");

            var reader = connection.ExecuteReader("INSERT INTO Test VALUES(1),(2) RETURNING rowid;");
            ((IDisposable)reader).Dispose();

            Assert.Equal(2, reader.RecordsAffected);
        }
    }

    [Fact]
    public void GetSchemaTable_works()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery(
                "CREATE TABLE Person (ID INTEGER PRIMARY KEY, FirstName TEXT, LastName TEXT NOT NULL, Code INT UNIQUE);");
            connection.ExecuteNonQuery("INSERT INTO Person VALUES(101, 'John', 'Dee', 123);");
            connection.ExecuteNonQuery("INSERT INTO Person VALUES(105, 'Jane', 'Doe', 456);");

            using (var reader = connection.ExecuteReader("SELECT LastName, ID, Code, ID+1 AS IncID FROM Person;"))
            {
                var schema = reader.GetSchemaTable();
                Assert.True(schema.Columns.Contains("ColumnName"));
                Assert.True(schema.Columns.Contains("ColumnOrdinal"));
                Assert.True(schema.Columns.Contains("ColumnSize"));
                Assert.True(schema.Columns.Contains("NumericPrecision"));
                Assert.True(schema.Columns.Contains("NumericScale"));
                Assert.True(schema.Columns.Contains("IsUnique"));
                Assert.True(schema.Columns.Contains("IsKey"));
                Assert.True(schema.Columns.Contains("BaseServerName"));
                Assert.True(schema.Columns.Contains("BaseCatalogName"));
                Assert.True(schema.Columns.Contains("BaseColumnName"));
                Assert.True(schema.Columns.Contains("BaseSchemaName"));
                Assert.True(schema.Columns.Contains("BaseTableName"));
                Assert.True(schema.Columns.Contains("DataType"));
                Assert.True(schema.Columns.Contains("DataTypeName"));
                Assert.True(schema.Columns.Contains("AllowDBNull"));
                Assert.True(schema.Columns.Contains("IsAliased"));
                Assert.True(schema.Columns.Contains("IsExpression"));
                Assert.True(schema.Columns.Contains("IsAutoIncrement"));
                Assert.True(schema.Columns.Contains("IsLong"));

                Assert.Equal(4, schema.Rows.Count);

                Assert.Equal("LastName", schema.Rows[0]["ColumnName"]);
                Assert.Equal(0, schema.Rows[0]["ColumnOrdinal"]);
                Assert.Equal(-1, schema.Rows[0]["ColumnSize"]);
                Assert.Equal(DBNull.Value, schema.Rows[0]["NumericPrecision"]);
                Assert.Equal(DBNull.Value, schema.Rows[0]["NumericScale"]);
                Assert.False((bool)schema.Rows[0]["IsUnique"]);
                Assert.False((bool)schema.Rows[0]["IsKey"]);
                Assert.Equal("", schema.Rows[0]["BaseServerName"]);
                Assert.Equal("main", schema.Rows[0]["BaseCatalogName"]);
                Assert.Equal("LastName", schema.Rows[0]["BaseColumnName"]);
                Assert.Equal(DBNull.Value, schema.Rows[0]["BaseSchemaName"]);
                Assert.Equal("Person", schema.Rows[0]["BaseTableName"]);
                Assert.Equal(typeof(string), schema.Rows[0]["DataType"]);
                Assert.Equal("TEXT", schema.Rows[0]["DataTypeName"]);
                Assert.False((bool)schema.Rows[0]["AllowDBNull"]);
                Assert.False((bool)schema.Rows[0]["IsAliased"]);
                Assert.False((bool)schema.Rows[0]["IsExpression"]);
                Assert.False((bool)schema.Rows[0]["IsAutoIncrement"]);
                Assert.Equal(DBNull.Value, schema.Rows[0]["IsLong"]);

                Assert.Equal("ID", schema.Rows[1]["ColumnName"]);
                Assert.Equal(1, schema.Rows[1]["ColumnOrdinal"]);
                Assert.Equal(-1, schema.Rows[1]["ColumnSize"]);
                Assert.Equal(DBNull.Value, schema.Rows[1]["NumericPrecision"]);
                Assert.Equal(DBNull.Value, schema.Rows[1]["NumericScale"]);
                Assert.False((bool)schema.Rows[1]["IsUnique"]);
                Assert.True((bool)schema.Rows[1]["IsKey"]);
                Assert.Equal("", schema.Rows[1]["BaseServerName"]);
                Assert.Equal("main", schema.Rows[1]["BaseCatalogName"]);
                Assert.Equal("ID", schema.Rows[1]["BaseColumnName"]);
                Assert.Equal(DBNull.Value, schema.Rows[1]["BaseSchemaName"]);
                Assert.Equal("Person", schema.Rows[1]["BaseTableName"]);
                Assert.Equal(typeof(long), schema.Rows[1]["DataType"]);
                Assert.Equal("INTEGER", schema.Rows[1]["DataTypeName"]);
                Assert.True((bool)schema.Rows[1]["AllowDBNull"]);
                Assert.False((bool)schema.Rows[1]["IsAliased"]);
                Assert.False((bool)schema.Rows[1]["IsExpression"]);
                Assert.False((bool)schema.Rows[1]["IsAutoIncrement"]);
                Assert.Equal(DBNull.Value, schema.Rows[1]["IsLong"]);

                Assert.Equal("Code", schema.Rows[2]["ColumnName"]);
                Assert.Equal(2, schema.Rows[2]["ColumnOrdinal"]);
                Assert.Equal(-1, schema.Rows[2]["ColumnSize"]);
                Assert.Equal(DBNull.Value, schema.Rows[2]["NumericPrecision"]);
                Assert.Equal(DBNull.Value, schema.Rows[2]["NumericScale"]);
                Assert.True((bool)schema.Rows[2]["IsUnique"]);
                Assert.False((bool)schema.Rows[2]["IsKey"]);
                Assert.Equal("", schema.Rows[2]["BaseServerName"]);
                Assert.Equal("main", schema.Rows[2]["BaseCatalogName"]);
                Assert.Equal("Code", schema.Rows[2]["BaseColumnName"]);
                Assert.Equal(DBNull.Value, schema.Rows[2]["BaseSchemaName"]);
                Assert.Equal("Person", schema.Rows[2]["BaseTableName"]);
                Assert.Equal(typeof(long), schema.Rows[2]["DataType"]);
                Assert.Equal("INT", schema.Rows[2]["DataTypeName"]);
                Assert.True((bool)schema.Rows[2]["AllowDBNull"]);
                Assert.False((bool)schema.Rows[2]["IsAliased"]);
                Assert.False((bool)schema.Rows[2]["IsExpression"]);
                Assert.False((bool)schema.Rows[2]["IsAutoIncrement"]);
                Assert.Equal(DBNull.Value, schema.Rows[2]["IsLong"]);

                Assert.Equal("IncID", schema.Rows[3]["ColumnName"]);
                Assert.Equal(3, schema.Rows[3]["ColumnOrdinal"]);
                Assert.Equal(-1, schema.Rows[3]["ColumnSize"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["NumericPrecision"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["NumericScale"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["IsUnique"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["IsKey"]);
                Assert.Equal("", schema.Rows[3]["BaseServerName"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["BaseCatalogName"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["BaseColumnName"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["BaseSchemaName"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["BaseTableName"]);
                Assert.Equal(typeof(long), schema.Rows[3]["DataType"]);
                Assert.Equal("INTEGER", schema.Rows[3]["DataTypeName"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["AllowDBNull"]);
                Assert.True((bool)schema.Rows[3]["IsAliased"]);
                Assert.True((bool)schema.Rows[3]["IsExpression"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["IsAutoIncrement"]);
                Assert.Equal(DBNull.Value, schema.Rows[3]["IsLong"]);
            }
        }
    }

    [Fact]
    public void GetSchemaTable_works_when_quotes()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery(
                @"CREATE TABLE ""Bad""""Table""(Value);");

            using (var reader = connection.ExecuteReader(@"SELECT * FROM ""Bad""""Table"";"))
            {
                var schemaTable = reader.GetSchemaTable();
                Assert.Equal(1, schemaTable.Rows.Count);
                Assert.Equal(@"Bad""Table", schemaTable.Rows[0][SchemaTableColumn.BaseTableName]);
            }
        }
    }

    [Fact]
    public void GetSchemaTable_works_when_view()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery(
                @"CREATE VIEW dual AS SELECT 'X' AS dummy;");

            using (var reader = connection.ExecuteReader("SELECT * FROM dual;"))
            {
                var schemaTable = reader.GetSchemaTable();
                Assert.Equal(1, schemaTable.Rows.Count);
                Assert.Equal("dummy", schemaTable.Rows[0][SchemaTableColumn.ColumnName]);
            }
        }
    }

    [Fact]
    public void GetSchemaTable_works_when_virtual_table()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery("CREATE VIRTUAL TABLE dual USING fts3(dummy);");

            using (var reader = connection.ExecuteReader("SELECT * FROM dual;"))
            {
                var schemaTable = reader.GetSchemaTable();
                Assert.Equal(1, schemaTable.Rows.Count);
                Assert.Equal("dummy", schemaTable.Rows[0][SchemaTableColumn.ColumnName]);
            }
        }
    }

    [Fact]
    public void GetSchemaTable_works_when_pragma()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("PRAGMA table_info('sqlite_master');"))
            {
                var schemaTable = reader.GetSchemaTable();
                Assert.Equal(6, schemaTable.Rows.Count);
                Assert.Equal("cid", schemaTable.Rows[0][SchemaTableColumn.ColumnName]);
                Assert.Equal("name", schemaTable.Rows[1][SchemaTableColumn.ColumnName]);
                Assert.Equal("type", schemaTable.Rows[2][SchemaTableColumn.ColumnName]);
                Assert.Equal("notnull", schemaTable.Rows[3][SchemaTableColumn.ColumnName]);
                Assert.Equal("dflt_value", schemaTable.Rows[4][SchemaTableColumn.ColumnName]);
                Assert.Equal("pk", schemaTable.Rows[5][SchemaTableColumn.ColumnName]);
            }
        }
    }

    [Fact]
    public void GetSchemaTable_works_when_eponymous_virtual_table()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("SELECT * FROM pragma_table_info('sqlite_master');"))
            {
                var schemaTable = reader.GetSchemaTable();
                Assert.Equal(6, schemaTable.Rows.Count);
                Assert.Equal("cid", schemaTable.Rows[0][SchemaTableColumn.ColumnName]);
                Assert.Equal("name", schemaTable.Rows[1][SchemaTableColumn.ColumnName]);
                Assert.Equal("type", schemaTable.Rows[2][SchemaTableColumn.ColumnName]);
                Assert.Equal("notnull", schemaTable.Rows[3][SchemaTableColumn.ColumnName]);
                Assert.Equal("dflt_value", schemaTable.Rows[4][SchemaTableColumn.ColumnName]);
                Assert.Equal("pk", schemaTable.Rows[5][SchemaTableColumn.ColumnName]);
            }
        }
    }

    [Theory]
    [InlineData("(0), (1), ('A')", typeof(long))]
    [InlineData("('Z'), (1), ('A')", typeof(string))]
    [InlineData("(0.1), (0.01), ('A')", typeof(double))]
    [InlineData("(X'7E57'), (X'577E'), ('A')", typeof(byte[]))]
    [InlineData("(NULL), (NULL), (NULL)", typeof(byte[]))]
    [InlineData("(NULL), ('A'), ('B')", typeof(string))]
    public void GetSchemaTable_DataType_works(string values, Type expectedType)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");
            connection.ExecuteNonQuery($"INSERT INTO Test VALUES {values};");

            using (var reader = connection.ExecuteReader("SELECT Value FROM Test;"))
            {
                var schema = reader.GetSchemaTable();
                Assert.True(schema.Columns.Contains("DataType"));
                Assert.Equal(expectedType, schema.Rows[0]["DataType"]);
            }
        }
    }

    [Theory]
    [InlineData("TEXT", typeof(string))]
    [InlineData("CHARACTER(20)", typeof(string))]
    [InlineData("NVARCHAR(100)", typeof(string))]
    [InlineData("CLOB", typeof(string))]
    [InlineData("INTEGER", typeof(long))]
    [InlineData("BIGINT", typeof(long))]
    [InlineData("UNSIGNED BIG INT", typeof(long))]
    [InlineData("REAL", typeof(double))]
    [InlineData("DOUBLE", typeof(double))]
    [InlineData("FLOAT", typeof(double))]
    [InlineData("BLOB", typeof(byte[]))]
    [InlineData("", typeof(byte[]))]
    [InlineData("NUMERIC", typeof(string))]
    [InlineData("DATETIME", typeof(string))]
    public void GetSchemaTable_DataType_works_on_empty_table(string type, Type expected)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();
            connection.ExecuteNonQuery($"CREATE TABLE Test(Value {type});");

            using (var reader = connection.ExecuteReader("SELECT Value FROM Test;"))
            {
                var schema = reader.GetSchemaTable();
                Assert.True(schema.Columns.Contains("DataType"));
                Assert.Equal(expected, schema.Rows[0]["DataType"]);
            }
        }
    }

    [Fact]
    public void GetSchemaTable_throws_when_closed()
        => X_throws_when_closed(r => r.GetSchemaTable(), nameof(SqliteDataReader.GetSchemaTable));

    [Fact]
    public void GetSchemaTable_throws_when_non_query()
        => X_throws_when_non_query(r => r.GetSchemaTable());

    [Fact]
    public void Dispose_executes_remaining_statements()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");
            connection.CreateFunction<string, long>("throw", message => throw new Exception(message));

            var reader = connection.ExecuteReader(
                @"
                    SELECT 1;
                    INSERT INTO Test VALUES (1);");
            ((IDisposable)reader).Dispose();

            Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT count() FROM Test;"));
        }
    }

    [Fact]
    public void Dispose_doesnt_throw_but_stops_on_error()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery("CREATE TABLE Test(Value);");
            connection.CreateFunction<string, long>("throw", message => throw new Exception(message));

            var reader = connection.ExecuteReader(
                @"
                    SELECT 1;
                    SELECT throw('An error');
                    INSERT INTO Test VALUES (1);");
            ((IDisposable)reader).Dispose();

            Assert.Equal(0L, connection.ExecuteScalar<long>("SELECT count() FROM Test;"));
        }
    }

    [Fact] // Issue #29744
    public void DataTable_load_handles_nulls()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery(
        """
        CREATE TABLE Member (
          ID INTEGER,
          Lastname TEXT NOT NULL,
          Firstname TEXT NOT NULL,
          Type INTEGER,
          Hidden INTEGER,
          PRIMARY KEY (ID AUTOINCREMENT)
        );

        CREATE TABLE Types (
          ID INTEGER,
          Description TEXT NOT NULL,
          Hidden INTEGER,
          PRIMARY KEY (ID AUTOINCREMENT)
        );

        INSERT INTO Types (Description) VALUES ('Administrator');
        INSERT INTO Types (Description) VALUES ('User');

        INSERT INTO Member (Lastname, Firstname, Type, Hidden) VALUES ('Mustermann', 'Max', 1, 0);
        INSERT INTO Member (Lastname, Firstname, Type, Hidden) VALUES ('Weber', 'Max', 2, 0);
        INSERT INTO Member (Lastname, Firstname, Type, Hidden) VALUES ('Müller', 'Willhelm', NULL, 0);
        """);

            string sql =
                """
                SELECT
                  Member.ID AS ID,
                  Member.Lastname,
                  Member.Firstname,
                  Types.ID AS TypeID,
                  Types.Description AS Type,
                  Member.Hidden
                FROM Member
                LEFT OUTER JOIN Types ON Types.ID = Member.Type;
                """;

            var table = new DataTable();
            using (var command = new SqliteCommand(sql, connection))
            {
                using (var dataReader = command.ExecuteReader())
                {
                    table.Load(dataReader);
                }
            }
        }
    }

    [Fact] // Issue #30765
    public void DataTable_load_handles_unique_columns()
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            connection.ExecuteNonQuery(
        """
        CREATE TABLE "characters" (
        	"id"	INTEGER,
        	"name"	TEXT UNIQUE,
        	"guild"	INTEGER
        );

        CREATE TABLE "guilds" (
        	"id"	INTEGER NOT NULL UNIQUE,
        	"name"	TEXT UNIQUE,
        	UNIQUE("name"),
        	PRIMARY KEY("id" AUTOINCREMENT)
        );
        CREATE UNIQUE INDEX guildname
        ON guilds(name);

        INSERT INTO characters (id, name, guild) VALUES (1, 'John', 1);
        INSERT INTO characters (id, name, guild) VALUES (2, 'Jeanette', 1);

        INSERT INTO guilds (id, name) VALUES (1, 'Testers');
        """);

            string sql =
                """
                SELECT guilds.name as guildName, characters.name as charName FROM guilds
                LEFT JOIN characters
                ON guilds.id = characters.guild
                """;

            var table = new DataTable();
            using (var command = new SqliteCommand(sql, connection))
            {
                using (var dataReader = command.ExecuteReader())
                {
                    table.Load(dataReader);
                }
            }
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
                var ex = Assert.Throws<InvalidOperationException>(() => action(reader));
                Assert.Equal(Resources.CalledOnNullValue(0), ex.Message);
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

                Assert.Equal(Resources.NoData, ex.Message);
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
                Assert.Equal(Resources.NoData, ex.Message);
            }
        }
    }

    private static void X_throws_when_closed(Action<SqliteDataReader> action, string operation)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            var reader = connection.ExecuteReader("SELECT 1;");
            ((IDisposable)reader).Dispose();

            var ex = Assert.Throws<InvalidOperationException>(() => action(reader));
            Assert.Equal(Resources.DataReaderClosed(operation), ex.Message);
        }
    }

    private static void X_throws_when_non_query(Action<SqliteDataReader> action)
    {
        using (var connection = new SqliteConnection("Data Source=:memory:"))
        {
            connection.Open();

            using (var reader = connection.ExecuteReader("CREATE TABLE dual(dummy);"))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => action(reader));
                Assert.Equal(Resources.NoData, ex.Message);
            }
        }
    }

    private enum MyEnum
    {
        One = 1
    }
}
