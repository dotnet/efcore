// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

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
        }
    }
}
