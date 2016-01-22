// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Commands.FunctionalTests
{
    public class RuntimeTypeDiscovererTest
    {
        private readonly IServiceProvider _services;

        [Fact]
        public void DiscoversRuntimeTypes()
        {
            using (var context = _services.GetRequiredService<PeopleContext>())
            {
                var discoverer = new RuntimeTypeDiscoverer(
                    context.Model,
                    context.GetService<IStateManager>(),
                    context.GetService<IInternalEntityEntryFactory>());

                var runtimeTypes = discoverer.Discover(typeof(EntityType).GetTypeInfo().Assembly, typeof(RelationalDatabase).GetTypeInfo().Assembly);

                // Original values snapshot: Snapshot<Id, AlternateId, Birthday, Name>
                Assert.Contains(typeof(Snapshot<int, Guid, DateTime, string>).GetTypeInfo(), runtimeTypes);

                // Relationalship snapshot
                Assert.Contains(typeof(Snapshot<int>).GetTypeInfo(), runtimeTypes);

                // Shadow values snapshot
                Assert.Contains(typeof(Snapshot<Guid>).GetTypeInfo(), runtimeTypes);

                // Query entry method
                Assert.Contains(typeof(IDatabase).GetTypeInfo().GetDeclaredMethod(nameof(IDatabase.CompileQuery)).MakeGenericMethod(typeof(Person)), runtimeTypes);
            }
        }

        public RuntimeTypeDiscovererTest()
        {
            _services = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<PeopleContext>(o => o.UseInMemoryDatabase())
                .GetInfrastructure()
                .BuildServiceProvider();
        }

        public class PeopleContext : DbContext
        {
            public DbSet<Person> People { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Person>().Property<Guid>("AlternateId");
            }
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthday { get; set; }
        }
    }
}
