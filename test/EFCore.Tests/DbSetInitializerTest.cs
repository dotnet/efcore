// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DbSetInitializerTest
    {
        [ConditionalFact]
        public void Initializes_all_entity_set_properties_with_setters()
        {
            var setFinder = new FakeSetFinder();
            var setSource = new DbSetSource();

            var customServices = new ServiceCollection()
                .AddSingleton<IDbSetInitializer>(
                    new DbSetInitializer(setFinder, setSource));

            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider(customServices);

            using var context = new JustAContext(
                new DbContextOptionsBuilder().UseInternalServiceProvider(serviceProvider).Options);
            Assert.NotNull(context.One);
            Assert.NotNull(context.GetTwo());
            Assert.NotNull(context.Three);
            Assert.Null(context.Four);
        }

        private class FakeSetFinder : IDbSetFinder
        {
            public IReadOnlyList<DbSetProperty> FindSets(Type contextType)
            {
                var setterFactory = new ClrPropertySetterFactory();

                return new[]
                {
                    new DbSetProperty("One", typeof(string), setterFactory.Create(typeof(JustAContext).GetAnyProperty("One"))),
                    new DbSetProperty("Two", typeof(object), setterFactory.Create(typeof(JustAContext).GetAnyProperty("Two"))),
                    new DbSetProperty("Three", typeof(string), setterFactory.Create(typeof(JustAContext).GetAnyProperty("Three"))),
                    new DbSetProperty("Four", typeof(string), null)
                };
            }
        }

        private class JustAContext : DbContext
        {
            public JustAContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<string> One { get; set; }
            private DbSet<object> Two { get; set; }
            public DbSet<string> Three { get; private set; }

            public DbSet<string> Four
                => null;

            public DbSet<object> GetTwo()
                => Two;
        }
    }
}
