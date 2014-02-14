// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDataStoreTest
    {
        [Fact]
        public void CanInitializeWithNameOrConnectionString()
        {
            var sqlServerDataStore = new SqlServerDataStore("Foo");

            Assert.Equal("Foo", sqlServerDataStore.NameOrConnectionString);
        }
    }
}
