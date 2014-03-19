// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class EntitySetInitializerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "setFinder",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new EntitySetInitializer(null, new ClrPropertySetterSource())).ParamName);

            var initializer = new EntitySetInitializer(new Mock<EntitySetFinder>().Object, new ClrPropertySetterSource());

            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => initializer.InitializeSets(null)).ParamName);
        }

        [Fact]
        public void Initializes_all_entity_set_properties_with_setters()
        {
            var setFinderMock = new Mock<EntitySetFinder>();
            setFinderMock.Setup(m => m.FindSets(It.IsAny<EntityContext>())).Returns(
                new[]
                    {
                        new EntitySetFinder.EntitySetProperty(typeof(JustAContext), "One", typeof(string), hasSetter: true),
                        new EntitySetFinder.EntitySetProperty(typeof(JustAContext), "Two", typeof(object), hasSetter: true),
                        new EntitySetFinder.EntitySetProperty(typeof(JustAContext), "Three", typeof(string), hasSetter: true),
                        new EntitySetFinder.EntitySetProperty(typeof(JustAContext), "Four", typeof(string), hasSetter: false)
                    });

            var configuration = new EntityConfigurationBuilder()
                .UseEntitySetInitializer(new EntitySetInitializer(setFinderMock.Object, new ClrPropertySetterSource()))
                .BuildConfiguration();

            using (var context = new JustAContext(configuration))
            {
                Assert.NotNull(context.One);
                Assert.NotNull(context.GetTwo());
                Assert.NotNull(context.Three);
                Assert.Null(context.Four);
            }
        }

        public class JustAContext : EntityContext
        {
            public JustAContext(EntityConfiguration configuration)
                : base(configuration)
            {
            }

            public EntitySet<string> One { get; set; }
            private EntitySet<object> Two { get; set; }
            public EntitySet<string> Three { get; private set; }

            public EntitySet<string> Four
            {
                get { return null; }
            }

            public EntitySet<object> GetTwo()
            {
                return Two;
            }
        }
    }
}
