// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class DataStoreCreatorTests : IClassFixture<FakeConnection>
    {
        private readonly AzureTableStorageDataStoreCreator _creator;
        private readonly FakeConnection _fixture;

        public DataStoreCreatorTests(FakeConnection fixture)
        {
            _fixture = fixture;
            _creator = new AzureTableStorageDataStoreCreator(fixture);
        }

        [Fact]
        public void It_requests_create_on_all_models()
        {
            var model = new Mock<IModel>();
            model.Setup(s => s.EntityTypes).Returns(new List<IEntityType>
                {
                    new EntityType("Test1"),
                    new EntityType("Test2")
                });
            _creator.CreateTables(model.Object);
            Assert.Equal(2, _fixture.CreateTableRequests);
            _creator.CreateTablesAsync(model.Object).Wait();
            Assert.Equal(4, _fixture.CreateTableRequests);
        }

        [Fact]
        public void It_cannot_delete_account()
        {
            Assert.Throws<AzureAccountException>(() => _creator.Delete());
            Assert.ThrowsAsync<AzureAccountException>(() => _creator.DeleteAsync());
        }
    }
}
