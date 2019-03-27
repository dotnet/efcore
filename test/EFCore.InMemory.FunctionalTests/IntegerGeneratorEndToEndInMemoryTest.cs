// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class IntegerGeneratorEndToEndInMemoryTest
    {
        [Fact]
        public void Can_use_sequence_end_to_end()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            AddEntities(serviceProvider);
            AddEntities(serviceProvider);

            using (var context = new BronieContext(serviceProvider))
            {
                var pegasuses = context.Pegasuses.ToList();

                for (var i = 0; i < 50; i++)
                {
                    Assert.True(pegasuses.All(p => p.Id > 0));
                    Assert.Equal(2, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(2, pegasuses.Count(p => p.Name == "Fluttershy " + i));
                }
            }
        }

        private static void AddEntities(IServiceProvider serviceProvider)
        {
            using (var context = new BronieContext(serviceProvider))
            {
                for (var i = 0; i < 50; i++)
                {
                    context.Add(
                        new Pegasus
                        {
                            Name = "Rainbow Dash " + i
                        });
                    context.Add(
                        new Pegasus
                        {
                            Name = "Fluttershy " + i
                        });
                }

                context.SaveChanges();
            }
        }

        [Fact]
        public async Task Can_use_sequence_end_to_end_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            await AddEntitiesAsync(serviceProvider);
            await AddEntitiesAsync(serviceProvider);

            using (var context = new BronieContext(serviceProvider))
            {
                var pegasuses = await context.Pegasuses.ToListAsync();

                for (var i = 0; i < 50; i++)
                {
                    Assert.True(pegasuses.All(p => p.Id > 0));
                    Assert.Equal(2, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(2, pegasuses.Count(p => p.Name == "Fluttershy " + i));
                }
            }
        }

        private static async Task AddEntitiesAsync(IServiceProvider serviceProvider)
        {
            using (var context = new BronieContext(serviceProvider))
            {
                for (var i = 0; i < 50; i++)
                {
                    context.Add(
                        new Pegasus
                        {
                            Name = "Rainbow Dash " + i
                        });
                    context.Add(
                        new Pegasus
                        {
                            Name = "Fluttershy " + i
                        });
                }

                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task Can_use_sequence_end_to_end_from_multiple_contexts_concurrently_async()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            const int threadCount = 50;

            var tests = new Func<Task>[threadCount];
            for (var i = 0; i < threadCount; i++)
            {
                var closureProvider = serviceProvider;
                tests[i] = () => AddEntitiesAsync(closureProvider);
            }

            var tasks = tests.Select(Task.Run).ToArray();

            foreach (var t in tasks)
            {
                await t;
            }

            using (var context = new BronieContext(serviceProvider))
            {
                var pegasuses = await context.Pegasuses.ToListAsync();

                for (var i = 0; i < 50; i++)
                {
                    Assert.True(pegasuses.All(p => p.Id > 0));
                    Assert.Equal(threadCount, pegasuses.Count(p => p.Name == "Rainbow Dash " + i));
                    Assert.Equal(threadCount, pegasuses.Count(p => p.Name == "Fluttershy " + i));
                }
            }
        }

        private class BronieContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public BronieContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(nameof(BronieContext))
                    .UseInternalServiceProvider(_serviceProvider);

            public DbSet<Pegasus> Pegasuses { get; set; }
        }

        private class Pegasus
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
