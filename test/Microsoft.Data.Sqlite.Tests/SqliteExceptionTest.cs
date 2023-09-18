// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite;

public class SqliteExceptionTest
{
    [Fact]
    public void Ctor_sets_message_and_errorCode()
    {
        var ex = new SqliteException("test", 1);

        Assert.Equal("test", ex.Message);
        Assert.Equal(1, ex.SqliteErrorCode);
        Assert.Equal(1, ex.SqliteExtendedErrorCode);
    }

    [Fact]
    public void Ctor_sets_extendedErrorCode()
    {
        var ex = new SqliteException("test", 1, 2);

        Assert.Equal("test", ex.Message);
        Assert.Equal(1, ex.SqliteErrorCode);
        Assert.Equal(2, ex.SqliteExtendedErrorCode);
    }

    [Theory]
    [InlineData(SQLITE_OK)]
    [InlineData(SQLITE_ROW)]
    [InlineData(SQLITE_DONE)]
    public void ThrowExceptionForRC_does_nothing_when_non_error(int rc)
        => SqliteException.ThrowExceptionForRC(rc, null);
}
