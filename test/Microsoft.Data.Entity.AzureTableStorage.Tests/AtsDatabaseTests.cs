// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDatabaseTests : IClassFixture<FakeConnection>
    {
        private readonly FakeConnection _fixture;
        private AtsDatabase _creator;

        public AtsDatabaseTests(FakeConnection fixture)
        {
            _fixture = fixture;
            var model = new Mock<IModel>();
            model.Setup(s => s.EntityTypes).Returns(new List<IEntityType>
                {
                    new EntityType("Test1"),
                    new EntityType("Test2")
                });
            var config = new Mock<DbContextConfiguration>();
            config.SetupGet(s => s.Model).Returns(model.Object);
            _creator = new AtsDatabase(config.Object, _fixture);
        }

        [Fact]
        public void It_requests_create_on_all_models()
        {
            _creator.CreateTables();
            Assert.Equal(2, _fixture.CreateTableRequests);
            _creator.CreateTablesAsync().Wait();
            Assert.Equal(4, _fixture.CreateTableRequests);
        }
    }
}
