// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
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
        {
            SqliteException.ThrowExceptionForRC(rc, null);
        }
    }
}
