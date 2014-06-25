// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDatabaseTests : IClassFixture<FakeConnection>
    {
        private readonly FakeConnection _fixture;
        private Mock<IModel> _model = new Mock<IModel>();
        private Mock<DbContextConfiguration> _config = new Mock<DbContextConfiguration>();

        public AtsDatabaseTests(FakeConnection fixture)
        {
            _fixture = fixture;
            _config.SetupGet(s => s.Model).Returns(_model.Object);
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
            var config = new Mock<DbContextConfiguration>();
            config.SetupGet(s => s.Model).Returns(model.Object);
            var database = new AtsDatabase(config.Object, _fixture);

            database.CreateTables();
            Assert.Equal(2, _fixture.CreateTableRequests);

            database.CreateTablesAsync().Wait();
            Assert.Equal(4, _fixture.CreateTableRequests);
        }

        [Fact]
        public void Has_no_tables()
        {
            var model = new Model();
            var config = new Mock<DbContextConfiguration>();
            config.SetupGet(s => s.Model).Returns(model);
            var database = new AtsDatabase(config.Object, new FakeConnection());

            Assert.False(database.HasTables());
        }

      
    }
}
