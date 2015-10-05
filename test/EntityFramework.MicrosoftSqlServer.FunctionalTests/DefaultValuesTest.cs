// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class DefaultValuesTest : IDisposable
    {
        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddEntityFramework()
            .AddSqlServer()
            .ServiceCollection()
            .BuildServiceProvider();

        [Fact]
        public void Can_use_SQL_Server_default_values()
        {
            using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var honeyDijon = context.Add(new KettleChips { Name = "Honey Dijon" }).Entity;
                var buffaloBleu = context.Add(new KettleChips { Name = "Buffalo Bleu", BestBuyDate = new DateTime(2111, 1, 11) }).Entity;

                context.SaveChanges();

                Assert.Equal(new DateTime(2035, 9, 25), honeyDijon.BestBuyDate);
                Assert.Equal(new DateTime(2111, 1, 11), buffaloBleu.BestBuyDate);
            }

            using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
            {
                Assert.Equal(new DateTime(2035, 9, 25), context.Chips.Single(c => c.Name == "Honey Dijon").BestBuyDate);
                Assert.Equal(new DateTime(2111, 1, 11), context.Chips.Single(c => c.Name == "Buffalo Bleu").BestBuyDate);
            }
        }

        public void Dispose()
        {
            using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
            {
                context.Database.EnsureDeleted();
            }
        }

        private class ChipsContext : DbContext
        {
            private readonly string _databaseName;

            public ChipsContext(IServiceProvider serviceProvider, string databaseName)
                : base(serviceProvider)
            {
                _databaseName = databaseName;
            }

            public DbSet<KettleChips> Chips { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<KettleChips>()
                    .Property(e => e.BestBuyDate)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValue(new DateTime(2035, 9, 25));
            }
        }

        private class KettleChips
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime BestBuyDate { get; set; }
        }
    }
}
