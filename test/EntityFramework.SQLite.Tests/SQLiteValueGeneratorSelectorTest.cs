// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.Tests
{
    public class SQLiteValueGeneratorSelectorTest
    {
        [Fact]
        public void Select_returns_tempFactory_when_single_long_key()
        {
            var entityType = new EntityType("Entity");
            var property = entityType.AddProperty("Id", typeof(long));
            property.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            entityType.SetKey(property);

            var result = CreateSelector().Select(property);

            Assert.IsType<SimpleValueGeneratorFactory<TemporaryValueGenerator>>(result);
        }

        [Fact]
        public void Select_returns_tempFactory_when_single_integer_column_key()
        {
            var entityType = new EntityType("Entity");
            var property = entityType.AddProperty("Id", typeof(int));
            property.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            property[MetadataExtensions.Annotations.StorageTypeName] = "INTEGER";
            entityType.SetKey(property);

            var result = CreateSelector().Select(property);

            Assert.IsType<SimpleValueGeneratorFactory<TemporaryValueGenerator>>(result);
        }

        [Fact]
        public void Select_returns_null_when_ValueGenerationOnAdd_is_not_Client()
        {
            var entityType = new EntityType("Entity");
            var property = entityType.AddProperty("Id", typeof(long));
            entityType.SetKey(property);

            var result = CreateSelector().Select(property);

            Assert.Null(result);
        }

        [Fact]
        public void Select_throws_when_composite_key()
        {
            var selector = CreateSelector();
            var entityType = new EntityType("Entity");
            var property = entityType.AddProperty("Id1", typeof(long));
            property.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            entityType.SetKey(property, entityType.AddProperty("Id2", typeof(long)));

            Assert.Throws<NotSupportedException>(() => selector.Select(property));
        }

        [Fact]
        public void Select_throws_when_non_integer_column_key()
        {
            var selector = CreateSelector();
            var entityType = new EntityType("Entity");
            var property = entityType.AddProperty("Id", typeof(int));
            property.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            entityType.SetKey(property);

            Assert.Throws<NotSupportedException>(() => selector.Select(property));
        }

        private SQLiteValueGeneratorSelector CreateSelector()
        {
            return new SQLiteValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>());
        }
    }
}
