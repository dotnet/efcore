// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.SqlServer.Tests
{
    public class SqlServerDataStoreTest
    {
        [Fact]
        public void Can_initialize_with_name_or_connection_string()
        {
            var sqlServerDataStore = new SqlServerDataStore("Foo");

            Assert.Equal("Foo", sqlServerDataStore.NameOrConnectionString);
        }
    }
}
