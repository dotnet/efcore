// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using Microsoft.Data.Sqlite.Properties;
using Xunit;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite;

public class SqliteBlobTest : IDisposable
{
    private const string Table = "data";
    private const string Column = "value";
    private const long Rowid = 1;

    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteBlobTest()
    {
        _connection.Open();
        _connection.ExecuteNonQuery(
            "CREATE TABLE "
            + Table
            + " ("
            + Column
            + " BLOB);"
            + "INSERT INTO "
            + Table
            + " (rowid, "
            + Column
            + ") VALUES ("
            + Rowid
            + ", X'0102');");
    }

    [Fact]
    public void Ctor_throws_when_connection_closed()
    {
        var connection = new SqliteConnection();

        var ex = Assert.Throws<InvalidOperationException>(
            () => new SqliteBlob(connection, Table, Column, Rowid));
        Assert.Equal(Resources.SqlBlobRequiresOpenConnection, ex.Message);
    }

    [Fact]
    public void Ctor_throws_when_error()
    {
        var ex = Assert.Throws<SqliteException>(
            () => new SqliteBlob(_connection, "UnknownTable", Column, Rowid));
        Assert.Equal(SQLITE_ERROR, ex.SqliteErrorCode);
    }

    [Fact]
    public void Ctor_throws_when_table_null()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new SqliteBlob(_connection, null!, Column, Rowid));
        Assert.Equal("tableName", ex.ParamName);
    }

    [Fact]
    public void Ctor_throws_when_column_null()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new SqliteBlob(_connection, Table, null!, Rowid));
        Assert.Equal("columnName", ex.ParamName);
    }

    [Fact]
    public void CanRead_works()
    {
        using (var stream = CreateStream())
        {
            Assert.True(stream.CanRead);
        }
    }

    [Fact]
    public void CanSeek_works()
    {
        using (var stream = CreateStream())
        {
            Assert.True(stream.CanSeek);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CanWrite_works(bool readOnly)
    {
        using (var stream = CreateStream(readOnly))
        {
            Assert.Equal(!readOnly, stream.CanWrite);
        }
    }

    [Fact]
    public void Length_works()
    {
        using (var stream = CreateStream())
        {
            Assert.Equal(2, stream.Length);
        }
    }

    [Fact]
    public void Position_throws_when_negative()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => stream.Position = -1);
            Assert.Equal("value", ex.ParamName);
            Assert.Equal(-1L, ex.ActualValue);
        }
    }

    [Fact]
    public void Flush_works()
    {
        using (var stream = CreateStream())
        {
            stream.Flush();
        }
    }

    [Theory]
    [InlineData(0, new byte[] { }, 0, 0, 0)]
    [InlineData(0, new byte[] { 0 }, 0, 0, 0)]
    [InlineData(0, new byte[] { 0 }, 2, 0, 1)]
    [InlineData(0, new byte[] { 0 }, 3, 0, 1)]
    [InlineData(1, new byte[] { 1 }, 0, 0, 1)]
    [InlineData(1, new byte[] { 1, 0 }, 0, 0, 1)]
    [InlineData(1, new byte[] { 2 }, 1, 0, 1)]
    [InlineData(1, new byte[] { 2, 0 }, 1, 0, 2)]
    [InlineData(1, new byte[] { 0, 1 }, 0, 1, 1)]
    public void Read_works(
        int expectedBytesRead,
        byte[] expectedBuffer,
        long initialPosition,
        int offset,
        int count)
    {
        using (var stream = CreateStream())
        {
            stream.Position = initialPosition;
            var buffer = new byte[expectedBuffer.Length];

            var bytesRead = stream.Read(buffer, offset, count);

            Assert.Equal(expectedBytesRead, bytesRead);
            Assert.Equal(expectedBuffer, buffer);
            Assert.Equal(initialPosition + bytesRead, stream.Position);
        }
    }

    [Fact]
    public void Read_throws_when_buffer_null()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => stream.Read(null!, 0, 1));

            Assert.Equal("buffer", ex.ParamName);
        }
    }

    [Fact]
    public void Read_throws_when_offset_negative()
    {
        using (var stream = CreateStream())
        {
            var buffer = new byte[1];

            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => stream.Read(buffer, -1, 1));
            Assert.Equal("offset", ex.ParamName);
            Assert.Equal(-1, ex.ActualValue);
        }
    }

    [Fact]
    public void Read_throws_when_offset_out_of_range()
    {
        using (var stream = CreateStream())
        {
            var buffer = new byte[1];

            var ex = Assert.Throws<ArgumentException>(
                () => stream.Read(buffer, 1, 1));
            Assert.Null(ex.ParamName);
            Assert.Equal(Resources.InvalidOffsetAndCount, ex.Message);
        }
    }

    [Fact]
    public void Read_throws_when_count_negative()
    {
        using (var stream = CreateStream())
        {
            var buffer = new byte[1];

            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => stream.Read(buffer, 0, -1));
            Assert.Equal("count", ex.ParamName);
            Assert.Equal(-1, ex.ActualValue);
        }
    }

    [Fact]
    public void Read_throws_when_count_out_of_range()
    {
        using (var stream = CreateStream())
        {
            var buffer = new byte[1];

            var ex = Assert.Throws<ArgumentException>(
                () => stream.Read(buffer, 0, 2));

            Assert.Null(ex.ParamName);
            Assert.Equal(Resources.InvalidOffsetAndCount, ex.Message);
        }
    }

    [Fact]
    public void Read_throws_when_disposed()
    {
        var buffer = new byte[1];

        var stream = CreateStream();
        stream.Dispose();

        var ex = Assert.Throws<ObjectDisposedException>(
            () => stream.Read(buffer, 0, 1));
    }

    [Theory]
    [InlineData(0, 1, 0, SeekOrigin.Begin)]
    [InlineData(1, 1, 1, SeekOrigin.Begin)]
    [InlineData(3, 1, 3, SeekOrigin.Begin)]
    [InlineData(0, 1, -1, SeekOrigin.Current)]
    [InlineData(1, 1, 0, SeekOrigin.Current)]
    [InlineData(2, 1, 1, SeekOrigin.Current)]
    [InlineData(3, 1, 2, SeekOrigin.Current)]
    [InlineData(1, 1, -1, SeekOrigin.End)]
    [InlineData(2, 1, 0, SeekOrigin.End)]
    [InlineData(3, 1, 1, SeekOrigin.End)]
    public void Seek_works(
        long expected,
        long initialPosition,
        long offset,
        SeekOrigin origin)
    {
        using (var stream = CreateStream())
        {
            stream.Position = initialPosition;

            var position = stream.Seek(offset, origin);

            Assert.Equal(expected, stream.Position);
            Assert.Equal(stream.Position, position);
        }
    }

    [Theory]
    [InlineData(1, -1, SeekOrigin.Begin)]
    [InlineData(1, -2, SeekOrigin.Current)]
    [InlineData(1, -3, SeekOrigin.End)]
    public void Seek_throws_when_negative(
        long initialPosition,
        long offset,
        SeekOrigin origin)
    {
        using (var stream = CreateStream())
        {
            stream.Position = initialPosition;

            var ex = Assert.Throws<IOException>(
                () => stream.Seek(offset, origin));
            Assert.Equal(Resources.SeekBeforeBegin, ex.Message);
        }
    }

    [Fact]
    public void Seek_validates_origin()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentException>(
                () => stream.Seek(0, (SeekOrigin)(-1)));
            Assert.Equal("origin", ex.ParamName);
            Assert.Contains(Resources.InvalidEnumValue(typeof(SeekOrigin), -1), ex.Message);
        }
    }

    [Fact]
    public void SetLength_throws()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => stream.SetLength(1));
            Assert.Equal(Resources.ResizeNotSupported, ex.Message);
        }
    }

    [Theory]
    [InlineData(new byte[] { 3, 4 }, 0, new byte[] { 3, 4 }, 0, 2)]
    [InlineData(new byte[] { 3, 2 }, 0, new byte[] { 3, 4 }, 0, 1)]
    [InlineData(new byte[] { 4, 2 }, 0, new byte[] { 3, 4 }, 1, 1)]
    [InlineData(new byte[] { 1, 3 }, 1, new byte[] { 3 }, 0, 1)]
    [InlineData(new byte[] { 1, 2 }, 0, new byte[] { }, 0, 0)]
    [InlineData(new byte[] { 1, 2 }, 2, new byte[] { }, 0, 0)]
    [InlineData(new byte[] { 1, 2 }, 3, new byte[] { }, 0, 0)]
    [InlineData(new byte[] { 1, 2 }, 0, new byte[] { 3 }, 1, 0)]
    public void Write_works(
        byte[] expected,
        long initialPosition,
        byte[] buffer,
        int offset,
        int count)
    {
        using (var stream = CreateStream())
        {
            stream.Position = initialPosition;
            stream.Write(buffer, offset, count);

            Assert.Equal(initialPosition + count, stream.Position);
        }

        Assert.Equal(
            expected,
            _connection.ExecuteScalar<byte[]>(
                $"SELECT {Column} FROM {Table} WHERE rowid = {Rowid}"));
    }

    [Fact]
    public void Write_throws_when_buffer_null()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => stream.Write(null!, 0, 0));
            Assert.Equal("buffer", ex.ParamName);
        }
    }

    [Fact]
    public void Write_throws_when_count_out_of_range()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentException>(
                () => stream.Write([3], 0, 2));
            Assert.Null(ex.ParamName);
            Assert.Equal(Resources.InvalidOffsetAndCount, ex.Message);
        }
    }

    [Fact]
    public void Write_throws_when_count_negative()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => stream.Write([], 0, -1));
            Assert.Equal("count", ex.ParamName);
            Assert.Equal(-1, ex.ActualValue);
        }
    }

    [Fact]
    public void Write_throws_when_offset_out_of_range()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentException>(
                () => stream.Write([3], 1, 1));
            Assert.Null(ex.ParamName);
            Assert.Equal(Resources.InvalidOffsetAndCount, ex.Message);
        }
    }

    [Fact]
    public void Write_throws_when_offset_negative()
    {
        using (var stream = CreateStream())
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => stream.Write([3, 4], -1, 2));
            Assert.Equal("offset", ex.ParamName);
        }
    }

    [Fact]
    public void Write_throws_when_position_at_end_of_stream()
    {
        using (var stream = CreateStream())
        {
            stream.Position = 2;
            var ex = Assert.Throws<NotSupportedException>(
                () => stream.Write([3], 0, 1));
            Assert.Equal(Resources.ResizeNotSupported, ex.Message);
        }
    }

    [Fact]
    public void Write_throws_when_readOnly()
    {
        using (var stream = CreateStream(readOnly: true))
        {
            var ex = Assert.Throws<NotSupportedException>(
                () => stream.Write([1], 0, 1));

            Assert.Equal(Resources.WriteNotSupported, ex.Message);
        }
    }

    [Fact]
    public void Write_throws_when_disposed()
    {
        var stream = CreateStream();
        stream.Dispose();

        var ex = Assert.Throws<ObjectDisposedException>(
            () => stream.Write([3], 0, 1));
    }

    [Fact]
    public void Empty_works()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        connection.ExecuteNonQuery(
            @"
                CREATE TABLE """" ("""" BLOB);
                INSERT INTO """" (rowid, """") VALUES(1, X'02');
            ");

        using var stream = new SqliteBlob(connection, "", "", 1);
        Assert.Equal(2, stream.ReadByte());
    }

    protected Stream CreateStream(bool readOnly = false)
        => new SqliteBlob(_connection, Table, Column, Rowid, readOnly);

    public void Dispose()
        => _connection.Dispose();
}
