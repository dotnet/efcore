// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
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

            var modelSource = new DefaultModelSource(new Mock<DbSetFinder>().Object);

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

            var model = new DefaultModelSource(setFinderMock.Object).GetModel(new Mock<DbContext>().Object);

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
            var modelSource = new DefaultModelSource(new DbSetFinder());

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
