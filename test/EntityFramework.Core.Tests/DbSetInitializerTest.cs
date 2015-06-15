// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DbSetInitializerTest
    {
        [Fact]
        public void Initializes_all_entity_set_properties_with_setters()
        {
            var setFinderMock = new Mock<IDbSetFinder>();
            setFinderMock.Setup(m => m.FindSets(It.IsAny<DbContext>())).Returns(
                new[]
                    {
                        new DbSetProperty(typeof(JustAContext), "One", typeof(string), hasSetter: true),
                        new DbSetProperty(typeof(JustAContext), "Two", typeof(object), hasSetter: true),
                        new DbSetProperty(typeof(JustAContext), "Three", typeof(string), hasSetter: true),
                        new DbSetProperty(typeof(JustAContext), "Four", typeof(string), hasSetter: false)
                    });

            var customServices = new ServiceCollection()
                .AddInstance<IDbSetInitializer>(new DbSetInitializer(setFinderMock.Object, new ClrPropertySetterSource(), new DbSetSource()));

            var serviceProvider = TestHelpers.Instance.CreateServiceProvider(customServices);

            using (var context = new JustAContext(serviceProvider, new DbContextOptionsBuilder().Options))
            {
                Assert.NotNull(context.One);
                Assert.NotNull(context.GetTwo());
                Assert.NotNull(context.Three);
                Assert.Null(context.Four);
            }
        }

        public class JustAContext : DbContext
        {
            public JustAContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
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
