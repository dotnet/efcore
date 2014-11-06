// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Extensions
{
    public class AtsDatabaseExtensionTests
    {
        [Fact]
        public void Returns_typed_database_object()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new AtsDatabase(configurationMock.Object, new LoggerFactory());

            Assert.Same(database, database.AsAzureTableStorageDatabase());
        }

        [Fact]
        public void Throws_when_non_ats_provider_is_in_use()
        {
            var configurationMock = new Mock<DbContextConfiguration>();
            var database = new Database(configurationMock.Object, new LoggerFactory());

            Assert.Equal(
                Strings.AtsDatabaseNotInUse,
                Assert.Throws<InvalidOperationException>(() => database.AsAzureTableStorageDatabase()).Message);
        }
    }
}
