// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.SqlServer
{
    using Xunit;

    public class SqlServerDataStoreFacts
    {
        [Fact]
        public void Can_initialize_with_name_or_connection_string()
        {
            var sqlServerDataStore = new SqlServerDataStore("Foo");

            Assert.Equal("Foo", sqlServerDataStore.NameOrConnectionString);
        }
    }
}
