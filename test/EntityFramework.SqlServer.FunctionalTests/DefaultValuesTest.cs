// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
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

        public class Level1
        {
            public int Id { get; set; }
            public string Name { get; set; }

            //public Level2 OneToOne_Required_PK { get; set; } // remove to repro
            public Level2 OneToOne_Optional_PK { get; set; }

            public Level2 OneToOne_Optional_FK { get; set; }

            public ICollection<Level2> OneToMany_Optional { get; set; }
            public ICollection<Level1> OneToMany_Optional_Self { get; set; }
            public Level1 OneToMany_Optional_Self_Inverse { get; set; }
        }

        public class Level2
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public int Level1_Required_Id { get; set; }
            public int? Level1_Optional_Id { get; set; }

            public Level1 OneToOne_Required_PK_Inverse { get; set; }
            public Level1 OneToOne_Optional_PK_Inverse { get; set; }
            public Level1 OneToOne_Required_FK_Inverse { get; set; }
            public Level1 OneToOne_Optional_FK_Inverse { get; set; }

            public Level1 OneToMany_Optional_Inverse { get; set; } //
        }

        public class MyContext : DbContext
        {
            public DbSet<Level1> LevelOne { get; set; }
            public DbSet<Level2> LevelTwo { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Level1>().Property(e => e.Id).GenerateValueOnAdd(false);
                modelBuilder.Entity<Level2>().Property(e => e.Id).GenerateValueOnAdd(false);

                //modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).ReferencedKey<Level1>(e => e.Id).Required(true);  // remove to repro
                modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).ReferencedKey<Level1>(e => e.Id).Required(false);
                modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).ForeignKey<Level2>(e => e.Level1_Optional_Id).Required(false);
                modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).Required(false);
                modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).Required(false);

                modelBuilder.Entity<Level2>().Property(e => e.Id).GenerateValueOnAdd(false);
                modelBuilder.Entity<Level2>().Property(e => e.Id).GenerateValueOnAdd(false);
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestStore.CreateConnectionString("Repro1479"));
            }
        }

        [Fact]
        public void Can_use_SQL_Server_default_values()
        {
            using (var context = new MyContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            using (var context = new ChipsContext(_serviceProvider, "DefaultKettleChips"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var honeyDijon = context.Add(new KettleChips { Name = "Honey Dijon" }).Entity;

                context.SaveChanges();

                Assert.Equal(new DateTime(2035, 9, 25), honeyDijon.BestBuyDate);

                var buffaloBleu = context.Add(new KettleChips { Name = "Buffalo Bleu", BestBuyDate = new DateTime(2111, 1, 11) }).Entity;

                context.SaveChanges();

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

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<KettleChips>()
                    .Property(e => e.BestBuyDate)
                    .UseStoreDefault()
                    .ForRelational().DefaultValue(new DateTime(2035, 9, 25));
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
