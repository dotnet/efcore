// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.SQLite
{
    public class SQLiteExceptionTest
    {
        [Fact]
        public void Ctor_sets_message_and_errorCode()
        {
            var ex = new SQLiteException("test", 1);

            Assert.Equal("test", ex.Message);
            Assert.Equal(1, ex.ErrorCode);
        }
    }
}
