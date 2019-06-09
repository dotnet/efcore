// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class IntegerValueGeneratorTest
    {
        [ConditionalFact]
        public void Each_property_gets_its_own_generator()
        {
            var olives = new Olive[4];
            var toasts = new Toast[4];

            using (var context = new PetsContext("Dance"))
            {
                olives[0] = context.Add(new Olive()).Entity;
                toasts[0] = context.Add(new Toast()).Entity;

                Assert.Equal(1, olives[0].Id);
                Assert.Equal(1, toasts[0].Id);

                olives[1] = context.Add(new Olive()).Entity;
                toasts[1] = context.Add(new Toast()).Entity;

                Assert.Equal(2, olives[1].Id);
                Assert.Equal(2, toasts[1].Id);

                context.SaveChanges();

                Assert.Equal(1, olives[0].Id);
                Assert.Equal(1, toasts[0].Id);
                Assert.Equal(2, olives[1].Id);
                Assert.Equal(2, toasts[1].Id);

                olives[2] = context.Add(new Olive()).Entity;
                toasts[2] = context.Add(new Toast()).Entity;

                Assert.Equal(3, olives[2].Id);
                Assert.Equal(3, toasts[2].Id);

                context.SaveChanges();
            }

            using (var context = new PetsContext("Dance"))
            {
                olives[3] = context.Add(new Olive()).Entity;
                toasts[3] = context.Add(new Toast()).Entity;

                Assert.Equal(4, olives[3].Id);
                Assert.Equal(4, toasts[3].Id);

                context.SaveChanges();
            }

            Assert.Equal(1, olives[0].Id);
            Assert.Equal(1, toasts[0].Id);
            Assert.Equal(2, olives[1].Id);
            Assert.Equal(2, toasts[1].Id);
            Assert.Equal(3, olives[2].Id);
            Assert.Equal(3, toasts[2].Id);
            Assert.Equal(4, olives[3].Id);
            Assert.Equal(4, toasts[3].Id);
        }

        [ConditionalFact]
        public void Generators_are_associated_with_database_root()
        {
            var serviceProvider1 = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var serviceProvider2 = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var root = new InMemoryDatabaseRoot();

            var olives = new Olive[2];
            var toasts = new Toast[2];

            using (var context = new PetsContext("Drink", root, serviceProvider1))
            {
                olives[0] = context.Add(new Olive()).Entity;
                toasts[0] = context.Add(new Toast()).Entity;

                Assert.Equal(1, olives[0].Id);
                Assert.Equal(1, toasts[0].Id);

                context.SaveChanges();
            }

            using (var context = new PetsContext("Drink", root, serviceProvider2))
            {
                olives[1] = context.Add(new Olive()).Entity;
                toasts[1] = context.Add(new Toast()).Entity;

                Assert.Equal(2, olives[1].Id);
                Assert.Equal(2, toasts[1].Id);

                context.SaveChanges();
            }

            Assert.Equal(1, olives[0].Id);
            Assert.Equal(1, toasts[0].Id);
            Assert.Equal(2, olives[1].Id);
            Assert.Equal(2, toasts[1].Id);
        }

        [ConditionalFact]
        public void Mixing_explicit_values_with_generated_values_with_care_works()
        {
            var olives = new Olive[4];
            var toasts = new Toast[4];

            using (var context = new PetsContext("Wercs"))
            {
                olives[0] = context.Add(new Olive { Id = 10 }).Entity;
                toasts[0] = context.Add(new Toast { Id = 100 }).Entity;

                context.SaveChanges();

                olives[1] = context.Add(new Olive()).Entity;
                toasts[1] = context.Add(new Toast()).Entity;

                context.SaveChanges();

                Assert.Equal(10, olives[0].Id);
                Assert.Equal(100, toasts[0].Id);
                Assert.Equal(11, olives[1].Id);
                Assert.Equal(101, toasts[1].Id);

                olives[2] = context.Add(new Olive { Id = 20 }).Entity;
                toasts[2] = context.Add(new Toast { Id = 200 }).Entity;

                context.SaveChanges();

                olives[3] = context.Add(new Olive()).Entity;
                toasts[3] = context.Add(new Toast()).Entity;

                context.SaveChanges();

                Assert.Equal(20, olives[2].Id);
                Assert.Equal(200, toasts[2].Id);
                Assert.Equal(21, olives[3].Id);
                Assert.Equal(201, toasts[3].Id);
            }
        }

        [ConditionalFact]
        public void Each_database_gets_its_own_generators()
        {
            var olives = new List<Olive>();
            var toasts = new List<Toast>();

            using (var context = new PetsContext("Nothing"))
            {
                olives.Add(context.Add(new Olive()).Entity);
                toasts.Add(context.Add(new Toast()).Entity);

                Assert.Equal(1, olives[0].Id);
                Assert.Equal(1, toasts[0].Id);

                context.SaveChanges();
            }

            using (var context = new PetsContext("Else"))
            {
                olives.Add(context.Add(new Olive()).Entity);
                toasts.Add(context.Add(new Toast()).Entity);

                Assert.Equal(1, olives[1].Id);
                Assert.Equal(1, toasts[1].Id);

                context.SaveChanges();
            }

            Assert.Equal(1, olives[0].Id);
            Assert.Equal(1, toasts[0].Id);
            Assert.Equal(1, olives[1].Id);
            Assert.Equal(1, toasts[1].Id);
        }

        [ConditionalFact]
        public void Each_root_gets_its_own_generators()
        {
            var olives = new List<Olive>();
            var toasts = new List<Toast>();

            using (var context = new PetsContext("To", new InMemoryDatabaseRoot()))
            {
                olives.Add(context.Add(new Olive()).Entity);
                toasts.Add(context.Add(new Toast()).Entity);

                Assert.Equal(1, olives[0].Id);
                Assert.Equal(1, toasts[0].Id);

                context.SaveChanges();
            }

            using (var context = new PetsContext("To", new InMemoryDatabaseRoot()))
            {
                olives.Add(context.Add(new Olive()).Entity);
                toasts.Add(context.Add(new Toast()).Entity);

                Assert.Equal(1, olives[1].Id);
                Assert.Equal(1, toasts[1].Id);

                context.SaveChanges();
            }

            Assert.Equal(1, olives[0].Id);
            Assert.Equal(1, toasts[0].Id);
            Assert.Equal(1, olives[1].Id);
            Assert.Equal(1, toasts[1].Id);
        }

        [ConditionalFact]
        public void EnsureDeleted_resets_generators()
        {
            var olives = new List<Olive>();
            var toasts = new List<Toast>();

            using (var context = new PetsContext("Do"))
            {
                olives.Add(context.Add(new Olive()).Entity);
                toasts.Add(context.Add(new Toast()).Entity);

                Assert.Equal(1, olives[0].Id);
                Assert.Equal(1, toasts[0].Id);

                context.SaveChanges();
            }

            using (var context = new PetsContext("Do"))
            {
                context.Database.EnsureDeleted();

                olives.Add(context.Add(new Olive()).Entity);
                toasts.Add(context.Add(new Toast()).Entity);

                Assert.Equal(1, olives[1].Id);
                Assert.Equal(1, toasts[1].Id);

                context.SaveChanges();
            }

            Assert.Equal(1, olives[0].Id);
            Assert.Equal(1, toasts[0].Id);
            Assert.Equal(1, olives[1].Id);
            Assert.Equal(1, toasts[1].Id);
        }

        private class PetsContext : DbContext
        {
            private readonly string _databaseName;
            private readonly InMemoryDatabaseRoot _root;
            private readonly IServiceProvider _internalServiceProvider;

            public PetsContext(
                string databaseName,
                InMemoryDatabaseRoot root = null,
                IServiceProvider internalServiceProvider = null)
            {
                _databaseName = databaseName;
                _root = root;
                _internalServiceProvider = internalServiceProvider;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInternalServiceProvider(_internalServiceProvider);

                if (_root == null)
                {
                    optionsBuilder.UseInMemoryDatabase(_databaseName);
                }
                else
                {
                    optionsBuilder.UseInMemoryDatabase(_databaseName, _root);
                }
            }

            public DbSet<Toast> CookedBreads { get; set; }
            public DbSet<Olive> Olives { get; set; }
        }

        private class Toast
        {
            public int Id { get; set; }
        }

        private class Olive
        {
            public int Id { get; set; }
        }
    }
}
