// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class NavigationTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "foreignKey",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new Navigation(null, "Handlebars", pointsToPrincipal: true)).ParamName);
            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new Navigation(new Mock<ForeignKey>().Object, "", pointsToPrincipal: true)).Message);
        }

        [Fact]
        public void Can_create_navigation_to_principal()
        {
            var keyMock = new Mock<ForeignKey>();
            keyMock.Setup(m => m.IsUnique).Returns(false);
            var foreignKey = keyMock.Object;

            var navigation = new Navigation(foreignKey, "Deception", pointsToPrincipal: true);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.Null(navigation.EntityType);
            Assert.True(navigation.PointsToPrincipal);
            Assert.False(navigation.IsCollection());

            Assert.Same(foreignKey, ((INavigation)navigation).ForeignKey);
            Assert.Null(((INavigation)navigation).EntityType);
        }

        [Fact]
        public void Can_create_navigation_to_unique_dependent()
        {
            var keyMock = new Mock<ForeignKey>();
            keyMock.Setup(m => m.IsUnique).Returns(true);
            var foreignKey = keyMock.Object;

            var navigation = new Navigation(foreignKey, "Deception", pointsToPrincipal: false);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.Null(navigation.EntityType);
            Assert.False(navigation.PointsToPrincipal);
            Assert.False(navigation.IsCollection());

            Assert.Same(foreignKey, ((INavigation)navigation).ForeignKey);
            Assert.Null(((INavigation)navigation).EntityType);
        }

        [Fact]
        public void Can_create_navigation_to_collection_of_dependents()
        {
            var keyMock = new Mock<ForeignKey>();
            keyMock.Setup(m => m.IsUnique).Returns(false);
            var foreignKey = keyMock.Object;

            var navigation = new Navigation(foreignKey, "Deception", pointsToPrincipal: false);

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.Null(navigation.EntityType);
            Assert.False(navigation.PointsToPrincipal);
            Assert.True(navigation.IsCollection());

            Assert.Same(foreignKey, ((INavigation)navigation).ForeignKey);
            Assert.Null(((INavigation)navigation).EntityType);
        }

        [Fact]
        public void Can_set_entity_type()
        {
            var navigation = new Navigation(new Mock<ForeignKey>().Object, "TheBattle", pointsToPrincipal: true);
            var entityType = new Mock<EntityType>().Object;

            navigation.EntityType = entityType;

            Assert.Same(entityType, navigation.EntityType);
            Assert.Same(entityType, ((INavigation)navigation).EntityType);
        }
    }
}
