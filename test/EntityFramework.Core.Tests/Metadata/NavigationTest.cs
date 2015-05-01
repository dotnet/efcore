// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class NavigationTest
    {
        [Fact]
        public void Can_create_navigation_to_principal()
        {
            var foreignKey = CreateForeignKey();
            foreignKey.IsUnique = false;

            var navigation = new Navigation("Deception", foreignKey, pointsToPrincipal: true);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.NotNull(navigation.EntityType);
            Assert.True(navigation.PointsToPrincipal());
            Assert.False(navigation.IsCollection());
        }

        [Fact]
        public void Can_create_navigation_to_unique_dependent()
        {
            var foreignKey = CreateForeignKey();
            foreignKey.IsUnique = true;

            var navigation = new Navigation("Deception", foreignKey, pointsToPrincipal: false);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.NotNull(navigation.EntityType);
            Assert.False(navigation.PointsToPrincipal());
            Assert.False(navigation.IsCollection());
        }

        [Fact]
        public void Can_create_navigation_to_collection_of_dependents()
        {
            var foreignKey = CreateForeignKey();
            foreignKey.IsUnique = false;

            var navigation = new Navigation("Deception", foreignKey, pointsToPrincipal: false);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.NotNull(navigation.EntityType);
            Assert.False(navigation.PointsToPrincipal());
            Assert.True(navigation.IsCollection());
        }

        private ForeignKey CreateForeignKey()
        {
            var model = new Model();
            var entityType = model.AddEntityType("E");
            var property = entityType.AddProperty("p", typeof(int), shadowProperty: true);
            var key = entityType.SetPrimaryKey(property);
            return new ForeignKey(new[] { property }, key);
        }
    }
}
