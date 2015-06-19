// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class StoreGeneratedTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : StoreGeneratedTestBase<TTestStore, TFixture>.StoreGeneratedFixtureBase, new()
    {
        protected StoreGeneratedTestBase(TFixture fixture)
        {
            Fixture = fixture;
            TestStore = Fixture.CreateTestStore();
        }

        [Fact]
        public virtual void Identity_key_with_read_only_before_save_throws_if_explicit_values_set()
        {
            using (var context = CreateContext())
            {
                context.Add(new Gumball { Id = 1 });

                Assert.Equal(
                    Strings.PropertyReadOnlyBeforeSave("Id", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entry = context.Add(new Gumball { Identity = "Masami" });
                entry.GetService().MarkAsTemporary(entry.Property(e => e.Identity).Metadata);

                context.SaveChanges();
                id = entry.Entity.Id;

                Assert.Equal("Banana Joe", entry.Entity.Identity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).Identity);
            }
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_with_sentinal_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Banana Joe", entity.Identity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).Identity);
            }
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            using (var context = CreateContext())
            {
                context.Add(new Gumball { IdentityReadOnlyBeforeSave = "Masami" });

                Assert.Equal(
                    Strings.PropertyReadOnlyBeforeSave("IdentityReadOnlyBeforeSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_can_have_value_set_explicitly()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball { Identity = "Masami" }).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Masami", entity.Identity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Identity);
            }
        }

        [Fact]
        public virtual void Identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;
            }

            using (var context = CreateContext())
            {
                var gumball = context.Gumballs.Single(e => e.Id == id);

                Assert.Equal("Anton", gumball.IdentityReadOnlyAfterSave);

                gumball.IdentityReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    Strings.PropertyReadOnlyAfterSave("IdentityReadOnlyAfterSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Identity_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;
            }

            using (var context = CreateContext())
            {
                var gumball = context.Gumballs.Single(e => e.Id == id);

                Assert.Equal("Banana Joe", gumball.Identity);

                gumball.Identity = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.SaveChanges();

                Assert.Equal("Masami", gumball.Identity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Identity);
            }
        }

        [Fact]
        public virtual void Identity_property_on_Modified_entity_is_not_included_in_update_when_not_modified()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;
            }

            using (var context = CreateContext())
            {
                var gumball = context.Gumballs.Single(e => e.Id == id);

                Assert.Equal("Banana Joe", gumball.Identity);

                gumball.Identity = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.Identity).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.Identity).IsModified = false;

                context.SaveChanges();

                Assert.Equal("Masami", gumball.Identity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).Identity);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entry = context.Add(new Gumball { Computed = "Masami" });
                entry.GetService().MarkAsTemporary(entry.Property(e => e.Computed).Metadata);

                context.SaveChanges();
                id = entry.Entity.Id;

                Assert.Equal("Alan", entry.Entity.Computed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).Computed);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_with_sentinal_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Alan", entity.Computed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).Computed);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            using (var context = CreateContext())
            {
                context.Add(new Gumball { ComputedReadOnlyBeforeSave = "Masami" });

                Assert.Equal(
                    Strings.PropertyReadOnlyBeforeSave("ComputedReadOnlyBeforeSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_can_have_value_set_explicitly()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball { Computed = "Masami" }).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Masami", entity.Computed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Computed);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;
            }

            using (var context = CreateContext())
            {
                var gumball = context.Gumballs.Single(e => e.Id == id);

                Assert.Equal("Tina Rex", gumball.ComputedReadOnlyAfterSave);

                gumball.ComputedReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    Strings.PropertyReadOnlyAfterSave("ComputedReadOnlyAfterSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;
            }

            using (var context = CreateContext())
            {
                var gumball = context.Gumballs.Single(e => e.Id == id);

                Assert.Equal("Alan", gumball.Computed);

                gumball.Computed = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.SaveChanges();

                Assert.Equal("Masami", gumball.Computed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Computed);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;
            }

            using (var context = CreateContext())
            {
                var gumball = context.Gumballs.Single(e => e.Id == id);

                Assert.Equal("Alan", gumball.Computed);

                gumball.Computed = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.Computed).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.Computed).IsModified = false;

                context.SaveChanges();

                Assert.Equal("Alan", gumball.Computed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).Computed);
            }
        }

        protected class Gumball
        {
            public int Id { get; set; }
            public string NotStoreGenerated { get; set; }

            public string Identity { get; set; }
            public string IdentityReadOnlyBeforeSave { get; set; }
            public string IdentityReadOnlyAfterSave { get; set; }

            public string Computed { get; set; }
            public string ComputedReadOnlyBeforeSave { get; set; }
            public string ComputedReadOnlyAfterSave { get; set; }
        }

        protected class StoreGeneratedContext : DbContext
        {
            public StoreGeneratedContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Gumball> Gumballs { get; set; }
        }

        protected StoreGeneratedContext CreateContext()
        {
            return (StoreGeneratedContext)Fixture.CreateContext(TestStore);
        }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        public abstract class StoreGeneratedFixtureBase
        {
            public abstract TTestStore CreateTestStore();

            public abstract DbContext CreateContext(TTestStore testStore);

            protected virtual void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Gumball>(b =>
                    {
                        var property = b.Property(e => e.Id)
                            .StoreGeneratedPattern(StoreGeneratedPattern.Identity)
                            .Metadata;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.Identity)
                            .StoreGeneratedPattern(StoreGeneratedPattern.Identity)
                            .Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.IdentityReadOnlyBeforeSave)
                            .StoreGeneratedPattern(StoreGeneratedPattern.Identity)
                            .Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.IdentityReadOnlyAfterSave)
                            .StoreGeneratedPattern(StoreGeneratedPattern.Identity)
                            .Metadata;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.Computed)
                            .StoreGeneratedPattern(StoreGeneratedPattern.Computed)
                            .Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.ComputedReadOnlyBeforeSave)
                            .StoreGeneratedPattern(StoreGeneratedPattern.Computed)
                            .Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.ComputedReadOnlyAfterSave)
                            .StoreGeneratedPattern(StoreGeneratedPattern.Computed)
                            .Metadata;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = false;
                    });
            }
        }
    }
}
