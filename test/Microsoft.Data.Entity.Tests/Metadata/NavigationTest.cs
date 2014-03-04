// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
                Assert.Throws<ArgumentNullException>(() => new Navigation(null, "Handlebars")).ParamName);
            Assert.Equal(
                Strings.ArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new Navigation(new Mock<ForeignKey>().Object, "")).Message);

            var navigation = new Navigation(new Mock<ForeignKey>().Object, "Handlebars");

            Assert.Equal(
                "value",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => navigation.EntityType = null).ParamName);

            Assert.Equal(
                "ownerEntity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => navigation.SetOrAddEntity(null, new Random())).ParamName);

            Assert.Equal(
                "relatedEntity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => navigation.SetOrAddEntity(new Random(), null)).ParamName);
        }

        [Fact]
        public void Can_create_navigation()
        {
            var foreignKey = new Mock<ForeignKey>().Object;

            var navigation = new Navigation(foreignKey, "Deception");

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
            Assert.Null(navigation.EntityType);

            Assert.Same(foreignKey, ((INavigation)navigation).ForeignKey);
            Assert.Null(((INavigation)navigation).EntityType);
        }

        [Fact]
        public void Can_set_entity_type()
        {
            var navigation = new Navigation(new Mock<ForeignKey>().Object, "TheBattle");
            var entityType = new Mock<EntityType>().Object;

            navigation.EntityType = entityType;

            Assert.Same(entityType, navigation.EntityType);
            Assert.Same(entityType, ((INavigation)navigation).EntityType);
        }

        [Fact]
        public void Can_set_value()
        {
            var entityTypeMock = new Mock<EntityType>();
            entityTypeMock.Setup(m => m.Type).Returns(typeof(CutChemist));

            var navigation = new Navigation(new Mock<ForeignKey>().Object, "BigBreak") { EntityType = entityTypeMock.Object };
            var entity = new CutChemist();

            navigation.SetOrAddEntity(entity, "In case of nuclear attack, protection of records is essential.");

            Assert.Equal("In case of nuclear attack, protection of records is essential.", entity.BigBreak);
        }

        private class CutChemist
        {
            internal string BigBreak { get; set; }
        }
    }
}
