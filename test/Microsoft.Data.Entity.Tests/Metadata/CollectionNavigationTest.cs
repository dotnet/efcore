// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class CollectionNavigationTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "foreignKey",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new CollectionNavigation(null, "Handlebars")).ParamName);
            Assert.Equal(
                Strings.FormatArgumentIsEmpty("name"),
                Assert.Throws<ArgumentException>(() => new CollectionNavigation(new Mock<ForeignKey>().Object, "")).Message);

            var navigation = new CollectionNavigation(new Mock<ForeignKey>().Object, "Handlebars");

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
        public void Can_create_collection_navigation()
        {
            var foreignKey = new Mock<ForeignKey>().Object;

            var navigation = new CollectionNavigation(foreignKey, "Deception");

            Assert.Same(foreignKey, navigation.ForeignKey);
            Assert.Equal("Deception", navigation.Name);
        }

        [Fact]
        public void Can_add_value_to_collection()
        {
            var entityTypeMock = new Mock<EntityType>();
            entityTypeMock.Setup(m => m.Type).Returns(typeof(Akrobatik));

            var navigation = new CollectionNavigation(new Mock<ForeignKey>().Object, "Newsflash") { EntityType = entityTypeMock.Object };
            var entity = new Akrobatik();

            navigation.SetOrAddEntity(entity, "Newsflash!");
            navigation.SetOrAddEntity(entity, "Coders talk...");
            navigation.SetOrAddEntity(entity, "With the mad arrogance...");
            navigation.SetOrAddEntity(entity, "But when it's time to write the code they just ain't legit.");

            Assert.Equal(
                new[]
                    {
                        "Newsflash!",
                        "Coders talk...",
                        "With the mad arrogance...",
                        "But when it's time to write the code they just ain't legit."
                    },
                entity.Newsflash.ToArray());
        }

        private class Akrobatik
        {
            private readonly ICollection<string> _newsflash = new List<string>();

            internal ICollection<string> Newsflash
            {
                get { return _newsflash; }
            }
        }
    }
}
