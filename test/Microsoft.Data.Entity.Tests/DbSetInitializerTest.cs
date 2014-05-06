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
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbSetInitializerTest
    {
        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "setFinder",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new DbSetInitializer(null, new ClrPropertySetterSource())).ParamName);

            var initializer = new DbSetInitializer(new Mock<DbSetFinder>().Object, new ClrPropertySetterSource());

            Assert.Equal(
                "context",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => initializer.InitializeSets(null)).ParamName);
        }

        [Fact]
        public void Initializes_all_entity_set_properties_with_setters()
        {
            var setFinderMock = new Mock<DbSetFinder>();
            setFinderMock.Setup(m => m.FindSets(It.IsAny<DbContext>())).Returns(
                new[]
                    {
                        new DbSetFinder.DbSetProperty(typeof(JustAContext), "One", typeof(string), hasSetter: true),
                        new DbSetFinder.DbSetProperty(typeof(JustAContext), "Two", typeof(object), hasSetter: true),
                        new DbSetFinder.DbSetProperty(typeof(JustAContext), "Three", typeof(string), hasSetter: true),
                        new DbSetFinder.DbSetProperty(typeof(JustAContext), "Four", typeof(string), hasSetter: false)
                    });

            var serviceProvider = new ServiceCollection()
                .AddEntityFramework(s => s.UseDbSetInitializer(new DbSetInitializer(setFinderMock.Object, new ClrPropertySetterSource())))
                .BuildServiceProvider();

            var configuration = new DbContextOptions().BuildConfiguration();

            using (var context = new JustAContext(serviceProvider, configuration))
            {
                Assert.NotNull(context.One);
                Assert.NotNull(context.GetTwo());
                Assert.NotNull(context.Three);
                Assert.Null(context.Four);
            }
        }

        public class JustAContext : DbContext
        {
            public JustAContext(IServiceProvider serviceProvider, ImmutableDbContextOptions configuration)
                : base(serviceProvider, configuration)
            {
            }

            public DbSet<string> One { get; set; }
            private DbSet<object> Two { get; set; }
            public DbSet<string> Three { get; private set; }

            public DbSet<string> Four
            {
                get { return null; }
            }

            public DbSet<object> GetTwo()
            {
                return Two;
            }
        }
    }
}
