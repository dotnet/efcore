// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TableSplittingTestBase<TTestStore>
        where TTestStore : TestStore
    {
        [Fact(Skip = "#8973")]
        public void Can_query_shared()
        {
            using (var store = CreateTestStore(OnModelCreating))
            {
                using (var context = CreateContext(store, OnModelCreating))
                {
                    Assert.Equal(4, context.Set<Operator>().ToList().Count);
                }
            }
        }

        [Fact(Skip = "#8973")]
        public void Can_query_shared_derived()
        {
            using (var store = CreateTestStore(OnModelCreating))
            {
                using (var context = CreateContext(store, OnModelCreating))
                {
                    Assert.Equal(1, context.Set<FuelTank>().ToList().Count);
                }
            }
        }

        [Fact(Skip = "#8973")]
        public void Can_use_with_redundant_relationships()
        {
            Test_roundtrip(OnModelCreating);
        }

        [Fact(Skip = "#8973")]
        public void Can_use_with_chained_relationships()
        {
            Test_roundtrip(modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<FuelTank>(eb =>
                        {
                            eb.Ignore(e => e.Vehicle);
                        });
                });
        }

        [Fact(Skip = "#8973")]
        public void Can_use_with_fanned_relationships()
        {
            Test_roundtrip(modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<FuelTank>(eb =>
                        {
                            eb.Ignore(e => e.Engine);
                        });
                    modelBuilder.Entity<CombustionEngine>(eb =>
                        {
                            eb.Ignore(e => e.FuelTank);
                        });
                });
        }

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            TransportationContext.OnModelCreatingBase(modelBuilder);

            modelBuilder.Entity<Vehicle>(eb =>
                {
                    eb.HasDiscriminator<string>("Discriminator");
                    eb.Property<string>("Discriminator").HasColumnName("Discriminator");
                    eb.ToTable("Vehicles");
                });

            modelBuilder.Entity<Engine>().ToTable("Vehicles");
            modelBuilder.Entity<Operator>().ToTable("Vehicles");
            modelBuilder.Entity<FuelTank>().ToTable("Vehicles");
        }

        protected void Test_roundtrip(Action<ModelBuilder> onModelCreating)
        {
            using (var store = CreateTestStore(onModelCreating))
            {
                using (var context = CreateContext(store, onModelCreating))
                {
                    context.AssertSeeded();
                }
            }
        }

        protected static readonly string DatabaseName = "TableSplittingTest";
        public abstract TTestStore CreateTestStore(Action<ModelBuilder> onModelCreating);
        public abstract TransportationContext CreateContext(TTestStore testStore, Action<ModelBuilder> onModelCreating);
    }
}
