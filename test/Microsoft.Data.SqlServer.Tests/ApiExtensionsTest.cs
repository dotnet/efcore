// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
{
    public class ApiExtensionsTest
    {
        [Fact]
        public void Can_create_context_with_name_or_connection_string()
        {
            var entityConfiguration = new EntityConfiguration();

            entityConfiguration.CreateContext("Foo");

            Assert.Equal("Foo", ((SqlServerDataStore)entityConfiguration.DataStore).NameOrConnectionString);
        }
    }
}
