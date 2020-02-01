// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DefaultValuesTest : IDisposable
    {
        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider();

        [ConditionalFact]
        public void Can_use_SQL_Server_default_values()
        {
            using (var context = new ChipsContext(_serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreatedResiliently();

                context.Chippers.Add(
                    new Chipper { Id = "Default" });

                context.SaveChanges();

                var honeyDijon = context.Add(
                    new KettleChips { Name = "Honey Dijon" }).Entity;
                var buffaloBleu = context.Add(
                    new KettleChips { Name = "Buffalo Bleu", BestBuyDate = new DateTime(2111, 1, 11) }).Entity;

                context.SaveChanges();

                Assert.Equal(new DateTime(2035, 9, 25), honeyDijon.BestBuyDate);
                Assert.Equal(new DateTime(2111, 1, 11), buffaloBleu.BestBuyDate);
            }

            using (var context = new ChipsContext(_serviceProvider, TestStore.Name))
            {
                Assert.Equal(new DateTime(2035, 9, 25), context.Chips.Single(c => c.Name == "Honey Dijon").BestBuyDate);
                Assert.Equal(new DateTime(2111, 1, 11), context.Chips.Single(c => c.Name == "Buffalo Bleu").BestBuyDate);
            }
        }

        private class ChipsContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly string _databaseName;

            public ChipsContext(IServiceProvider serviceProvider, string databaseName)
            {
                _serviceProvider = serviceProvider;
                _databaseName = databaseName;
            }

            public DbSet<KettleChips> Chips { get; set; }
            public DbSet<Chipper> Chippers { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<KettleChips>(
                    b =>
                    {
                        b.Property(e => e.BestBuyDate)
                            .ValueGeneratedOnAdd()
                            .HasDefaultValue(new DateTime(2035, 9, 25));

                        b.Property(e => e.ChipperId)
                            .IsRequired()
                            .HasDefaultValue("Default");
                    });
        }

        private class KettleChips
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime BestBuyDate { get; set; }
            public string ChipperId { get; set; }

            public Chipper Manufacturer { get; set; }
        }

        private class Chipper
        {
            public string Id { get; set; }
        }

        public DefaultValuesTest()
        {
            TestStore = SqlServerTestStore.CreateInitialized("DefaultValuesTest");
        }

        protected SqlServerTestStore TestStore { get; }

        public virtual void Dispose() => TestStore.Dispose();
    }
}
