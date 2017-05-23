// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class DbSetAsTableNameTest
    {
        [Fact]
        public virtual void DbSet_names_are_used_as_table_names()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Cheeses", GetTableName<Cheese>(context));
            }
        }

        [Fact]
        public virtual void DbSet_name_of_base_type_is_used_as_table_name_for_TPH()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Chocolates", GetTableName<Chocolate>(context));
                Assert.Equal("Chocolates", GetTableName<Galaxy>(context));
                Assert.Equal("Chocolates", GetTableName<DairyMilk>(context));
            }
        }

        [Fact]
        public virtual void Type_name_of_base_type_is_used_as_table_name_for_TPH_if_not_added_as_set()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Fruit", GetTableName<Fruit>(context));
                Assert.Equal("Fruit", GetTableName<Apple>(context));
                Assert.Equal("Fruit", GetTableName<Banana>(context));
            }
        }

        [Fact]
        public virtual void DbSet_names_of_derived_types_are_used_as_table_names_when_base_type_not_mapped()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Triskets", GetTableName<Trisket>(context));
                Assert.Equal("WheatThins", GetTableName<WheatThin>(context));
            }
        }

        [Fact]
        public virtual void Name_of_duplicate_DbSet_is_not_used_as_table_name()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Marmite", GetTableName<Marmite>(context));
            }
        }

        [Fact]
        public virtual void Explicit_names_can_be_used_as_table_names()
        {
            using (var context = CreateNamedTablesContext())
            {
                Assert.Equal("YummyCheese", GetTableName<Cheese>(context));
            }
        }

        [Fact]
        public virtual void Explicit_name_of_base_type_can_be_used_as_table_name_for_TPH()
        {
            using (var context = CreateNamedTablesContext())
            {
                Assert.Equal("YummyChocolate", GetTableName<Chocolate>(context));
                Assert.Equal("YummyChocolate", GetTableName<Galaxy>(context));
                Assert.Equal("YummyChocolate", GetTableName<DairyMilk>(context));
            }
        }

        [Fact]
        public virtual void Explicit_name_of_base_type_can_be_used_as_table_name_for_TPH_if_not_added_as_set()
        {
            using (var context = CreateNamedTablesContext())
            {
                Assert.Equal("YummyFruit", GetTableName<Fruit>(context));
                Assert.Equal("YummyFruit", GetTableName<Apple>(context));
                Assert.Equal("YummyFruit", GetTableName<Banana>(context));
            }
        }

        [Fact]
        public virtual void Explicit_names_of_derived_types_can_be_used_as_table_names_when_base_type_not_mapped()
        {
            using (var context = CreateNamedTablesContext())
            {
                Assert.Equal("YummyTriskets", GetTableName<Trisket>(context));
                Assert.Equal("YummyWheatThins", GetTableName<WheatThin>(context));
            }
        }

        [Fact]
        public virtual void Explicit_name_can_be_used_for_type_with_duplicated_sets()
        {
            using (var context = CreateNamedTablesContext())
            {
                Assert.Equal("YummyMarmite", GetTableName<Marmite>(context));
            }
        }

        protected abstract string GetTableName<TEntity>(DbContext context);

        protected abstract SetsContext CreateContext();

        protected abstract class SetsContext : DbContext
        {
            public DbSet<Cheese> Cheeses { get; set; }
            public DbSet<Chocolate> Chocolates { get; set; }
            public DbSet<Galaxy> Galaxies { get; set; }
            public DbSet<DairyMilk> DairyMilks { get; set; }
            public DbSet<Apple> Apples { get; set; }
            public DbSet<Trisket> Triskets { get; set; }
            public DbSet<WheatThin> WheatThins { get; set; }
            public DbSet<Marmite> Food { get; set; }
            public DbSet<Marmite> Beverage { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Fruit>();
                modelBuilder.Entity<Banana>();
            }
        }

        protected abstract SetsContext CreateNamedTablesContext();

        protected abstract class NamedTablesContext : SetsContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Cheese>().ToTable("YummyCheese");
                modelBuilder.Entity<Chocolate>().ToTable("YummyChocolate");
                modelBuilder.Entity<Fruit>().ToTable("YummyFruit");
                modelBuilder.Entity<Trisket>().ToTable("YummyTriskets");
                modelBuilder.Entity<WheatThin>().ToTable("YummyWheatThins");
                modelBuilder.Entity<Marmite>().ToTable("YummyMarmite");
            }
        }

        protected class Cheese
        {
            public int Id { get; set; }
        }

        protected class Chocolate
        {
            public int Id { get; set; }
        }

        protected class Galaxy : Chocolate
        {
        }

        protected class DairyMilk : Chocolate
        {
        }

        protected class Fruit
        {
            public int Id { get; set; }
        }

        protected class Apple : Fruit
        {
        }

        protected class Banana : Fruit
        {
        }

        protected class Cracker
        {
            public int Id { get; set; }
        }

        protected class Trisket : Cracker
        {
        }

        protected class WheatThin : Cracker
        {
        }

        protected class Marmite
        {
            public int Id { get; set; }
        }
    }
}
