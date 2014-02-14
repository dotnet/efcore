// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class ApiExtensionsTest
    {
        [Fact]
        public void CanCreateContextWithNameOrConnectionString()
        {
            var entityConfiguration = new EntityConfiguration();

            entityConfiguration.CreateContext("Foo");

            Assert.Equal("Foo", ((SqlServerDataStore)entityConfiguration.DataStore).NameOrConnectionString);
        }
    }
}
