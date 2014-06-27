// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDataStoreCreatorTests
    {
        private readonly AtsDataStoreCreator _creator;
        private readonly FakeConnection _connection;
        private readonly Model _model;

        public AtsDataStoreCreatorTests()
        {
            _connection = new FakeConnection();
            _creator = new AtsDataStoreCreator(_connection);
            _model = new Model();
            var builder = new ModelBuilder(_model);
            builder.Entity("Test1");
            builder.Entity("Test2");
            builder.Entity("Test3");
        }

        [Fact]
        public void Ensure_creation()
        {
            Assert.True(_creator.EnsureCreated(_model));
            AssertLists(_model.EntityTypes.Select(s => s.Name), _connection.Tables.Keys);
        }

        [Fact]
        public void Ensures_creation_async()
        {
            Assert.True(_creator.EnsureCreatedAsync(_model).Result);
            AssertLists(_model.EntityTypes.Select(s => s.Name), _connection.Tables.Keys);
        }

        [Fact]
        public void Ensures_deletion()
        {
            _creator.EnsureCreated(_model);
            Assert.True(_creator.EnsureDeleted(_model));
            Assert.Equal(0, _connection.Tables.Keys.Count);
        }

        [Fact]
        public void Ensures_deletion_async()
        {
            _creator.EnsureCreated(_model);
            Assert.True(_creator.EnsureDeletedAsync(_model).Result);
            Assert.Equal(0, _connection.Tables.Keys.Count);
        }

        [Fact]
        public void Only_deletes_tables_in_model()
        {
            _connection.Tables.TryAdd("Invariant", new FakeConnection.TestCloudTable(_connection, "Invariant"));
            _creator.EnsureCreated(_model);
            Assert.True(_creator.EnsureDeleted(_model));
            Assert.Equal(1, _connection.Tables.Keys.Count);
            Assert.Equal("Invariant", _connection.Tables.Keys.First());
        }

        [Fact]
        public void Only_deletes_async_tables_in_model()
        {
            _connection.Tables.TryAdd("Invariant", new FakeConnection.TestCloudTable(_connection, "Invariant"));
            _creator.EnsureCreated(_model);
            Assert.True(_creator.EnsureDeletedAsync(_model).Result);
            Assert.Equal(1, _connection.Tables.Keys.Count);
            Assert.Equal("Invariant", _connection.Tables.Keys.First());
        }

        private static void AssertLists(IEnumerable<string> expected, IEnumerable<string> actual)
        {
            Assert.Equal(expected.ToList().OrderBy(s => s), actual.ToList().OrderBy(s => s));
        }
    }
}
