// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class DefaultModelSourceTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "setFinder",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new DefaultModelSource(null)).ParamName);

            var modelSource = new DefaultModelSource(new Mock<EntitySetFinder>().Object);

            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => modelSource.GetModel(null)).ParamName);
        }

        [Fact]
        public void Adds_all_entities_based_on_all_distinct_entity_types_found()
        {
            var setFinderMock = new Mock<EntitySetFinder>();
            setFinderMock.Setup(m => m.FindSets(It.IsAny<EntityContext>())).Returns(
                new[]
                    {
                        typeof(JustAClass).GetAnyProperty("One"),
                        typeof(JustAClass).GetAnyProperty("Two"),
                        typeof(JustAClass).GetAnyProperty("Three"),
                        typeof(JustAClass).GetAnyProperty("Four")
                    });

            var model = new DefaultModelSource(setFinderMock.Object).GetModel(new Mock<EntityContext>().Object);

            Assert.Equal(
                new[] { "Object", "Random", "String" },
                model.EntityTypes.Select(e => e.Name).ToArray());
        }

        public class JustAClass
        {
            public EntitySet<Random> One { get; set; }
            protected EntitySet<object> Two { get; set; }
            private EntitySet<string> Three { get; set; }
            private EntitySet<string> Four { get; set; }
        }
    }
}
