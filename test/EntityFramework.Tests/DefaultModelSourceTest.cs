// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
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
                Assert.Throws<ArgumentNullException>(() => new DefaultModelSource(null, new ModelBuilderSelector())).ParamName);

            var modelSource = new DefaultModelSource(new Mock<DbSetFinder>().Object, new ModelBuilderSelector());

            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => modelSource.GetModel(null)).ParamName);
        }

        [Fact]
        public void Adds_all_entities_based_on_all_distinct_entity_types_found()
        {
            var setFinderMock = new Mock<DbSetFinder>();
            setFinderMock.Setup(m => m.FindSets(It.IsAny<DbContext>())).Returns(
                new[]
                    {
                        new DbSetFinder.DbSetProperty(typeof(JustAClass), "One", typeof(Random), hasSetter: true),
                        new DbSetFinder.DbSetProperty(typeof(JustAClass), "Two", typeof(object), hasSetter: true),
                        new DbSetFinder.DbSetProperty(typeof(JustAClass), "Three", typeof(string), hasSetter: true),
                        new DbSetFinder.DbSetProperty(typeof(JustAClass), "Four", typeof(string), hasSetter: true)
                    });

            var model = new DefaultModelSource(setFinderMock.Object, new ModelBuilderSelector()).GetModel(new Mock<DbContext>().Object);

            Assert.Equal(
                new[] { "Object", "Random", "String" },
                model.EntityTypes.Select(e => e.Name).ToArray());
        }

        private class JustAClass
        {
            public DbSet<Random> One { get; set; }
            protected DbSet<object> Two { get; set; }
            private DbSet<string> Three { get; set; }
            private DbSet<string> Four { get; set; }
        }

        [Fact]
        public void Caches_model_by_context_type()
        {
            var modelSource = new DefaultModelSource(new DbSetFinder(), new ModelBuilderSelector());

            var model1 = modelSource.GetModel(new Context1());
            var model2 = modelSource.GetModel(new Context2());

            Assert.NotSame(model1, model2);
            Assert.Same(model1, modelSource.GetModel(new Context1()));
            Assert.Same(model2, modelSource.GetModel(new Context2()));
        }

        private class Context1 : DbContext
        {
        }

        private class Context2 : DbContext
        {
        }
    }
}
