// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Tests
{
    public class SqliteValueGeneratorSelectorTest
    {
        [Fact]
        public void Select_returns_tempFactory_when_single_long_key()
        {
            var entityType = new Model().AddEntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(long), shadowProperty: true);
            property.GenerateValueOnAdd = true;
            entityType.GetOrSetPrimaryKey(property);

            var result = CreateSelector().Select(property);

            Assert.IsType<SimpleValueGeneratorFactory<TemporaryValueGenerator>>(result);
        }

        [Fact]
        public void Select_returns_tempFactory_when_single_integer_column_key()
        {
            var entityType = new Model().AddEntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            property.GenerateValueOnAdd = true;
            // TODO: SQLite-specific. Issue #875
            property.Relational().ColumnType = "INTEGER";
            entityType.GetOrSetPrimaryKey(property);

            var result = CreateSelector().Select(property);

            Assert.IsType<SimpleValueGeneratorFactory<TemporaryValueGenerator>>(result);
        }

        [Fact]
        public void Select_returns_null_when_ValueGeneration_is_not_set()
        {
            var entityType = new Model().AddEntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(long), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(property);

            var result = CreateSelector().Select(property);

            Assert.Null(result);
        }

        [Fact]
        public void Select_throws_when_composite_key()
        {
            var selector = CreateSelector();
            var entityType = new Model().AddEntityType("Entity");
            var property = entityType.GetOrAddProperty("Id1", typeof(long), shadowProperty: true);
            property.GenerateValueOnAdd = true;
            entityType.GetOrSetPrimaryKey(new[] { property, entityType.GetOrAddProperty("Id2", typeof(long), shadowProperty: true) });

            Assert.Throws<NotSupportedException>(() => selector.Select(property));
        }

        [Fact]
        public void Select_throws_when_non_integer_column_key()
        {
            var selector = CreateSelector();
            var entityType = new Model().AddEntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            property.GenerateValueOnAdd = true;
            entityType.GetOrSetPrimaryKey(property);

            Assert.Throws<NotSupportedException>(() => selector.Select(property));
        }

        private SqliteValueGeneratorSelector CreateSelector()
        {
            return new SqliteValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>());
        }
    }
}
