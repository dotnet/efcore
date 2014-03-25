// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
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
                        new EntitySetFinder.EntitySetProperty(typeof(JustAClass), "One", typeof(Random), hasSetter: true),
                        new EntitySetFinder.EntitySetProperty(typeof(JustAClass), "Two", typeof(object), hasSetter: true),
                        new EntitySetFinder.EntitySetProperty(typeof(JustAClass), "Three", typeof(string), hasSetter: true),
                        new EntitySetFinder.EntitySetProperty(typeof(JustAClass), "Four", typeof(string), hasSetter: true)
                    });

            var model = new DefaultModelSource(setFinderMock.Object).GetModel(new Mock<EntityContext>().Object);

            Assert.Equal(
                new[] { "Object", "Random", "String" },
                model.EntityTypes.Select(e => e.Name).ToArray());
        }

        private class JustAClass
        {
            public EntitySet<Random> One { get; set; }
            protected EntitySet<object> Two { get; set; }
            private EntitySet<string> Three { get; set; }
            private EntitySet<string> Four { get; set; }
        }

        [Fact]
        public void Caches_model_by_context_type()
        {
            var modelSource = new DefaultModelSource(new EntitySetFinder());

            var model1 = modelSource.GetModel(new Context1());
            var model2 = modelSource.GetModel(new Context2());

            Assert.NotSame(model1, model2);
            Assert.Same(model1, modelSource.GetModel(new Context1()));
            Assert.Same(model2, modelSource.GetModel(new Context2()));
        }

        private class Context1 : EntityContext
        {
        }

        private class Context2 : EntityContext
        {
        }
    }
}
