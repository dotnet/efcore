// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDatabaseExtensionTests
    {

        [Fact]
        public void Returns_typed_database_object()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new AtsDatabase(configurationMock.Object, Mock.Of<AtsConnection>());

            Assert.Same(database, database.AsAzureTableStorageDatabase());
        }

        [Fact]
        public void Throws_when_non_ats_provider_is_in_use()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new Database(configurationMock.Object);

            Assert.Equal(
                Strings.AtsDatabaseNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsAzureTableStorageDatabase()).Message);
        } 
    }
}