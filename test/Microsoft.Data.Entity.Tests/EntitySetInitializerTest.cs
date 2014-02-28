// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Utilities;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class EntitySetInitializerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "setFinder",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntitySetInitializer(null)).ParamName);

            var initializer = new EntitySetInitializer(new Mock<EntitySetFinder>().Object);

            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => initializer.InitializeSets(null)).ParamName);
        }

        [Fact]
        public void Adds_all_entity_sets_with_setters_are_entities_based_on_all_distinct_entity_types_found()
        {
            var setFinderMock = new Mock<EntitySetFinder>();
            setFinderMock.Setup(m => m.FindSets(It.IsAny<EntityContext>())).Returns(
                new[]
                    {
                        typeof(JustAContext).GetAnyProperty("One"),
                        typeof(JustAContext).GetAnyProperty("Two"),
                        typeof(JustAContext).GetAnyProperty("Three"),
                        typeof(JustAContext).GetAnyProperty("Four")
                    });

            using (var context = new JustAContext())
            {
                new EntitySetInitializer(setFinderMock.Object).InitializeSets(context);

                Assert.NotNull(context.One);
                Assert.NotNull(context.GetTwo());
                Assert.NotNull(context.Three);
                Assert.Null(context.Four);
            }
        }

        public class JustAContext : EntityContext
        {
            public EntitySet<string> One { get; set; }
            private EntitySet<string> Two { get; set; }
            public EntitySet<string> Three { get; private set; }

            public EntitySet<string> Four
            {
                get { return null; }
            }

            public EntitySet<string> GetTwo()
            {
                return Two;
            }
        }
    }
}
