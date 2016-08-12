// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
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
                    CoreStrings.PropertyReadOnlyBeforeSave("Id", "Gumball"),
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
                entry.Property(e => e.Identity).IsTemporary = true;

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
        public virtual void Identity_property_on_Added_entity_with_default_value_gets_value_from_store()
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
                    CoreStrings.PropertyReadOnlyBeforeSave("IdentityReadOnlyBeforeSave", "Gumball"),
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
                    CoreStrings.PropertyReadOnlyAfterSave("IdentityReadOnlyAfterSave", "Gumball"),
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
        public virtual void Always_identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entry = context.Add(new Gumball { AlwaysIdentity = "Masami" });
                entry.Property(e => e.AlwaysIdentity).IsTemporary = true;

                context.SaveChanges();
                id = entry.Entity.Id;

                Assert.Equal("Banana Joe", entry.Entity.AlwaysIdentity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity);
            }
        }

        [Fact]
        public virtual void Always_identity_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Banana Joe", entity.AlwaysIdentity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity);
            }
        }

        [Fact]
        public virtual void Always_identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            using (var context = CreateContext())
            {
                context.Add(new Gumball { AlwaysIdentityReadOnlyBeforeSave = "Masami" });

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave("AlwaysIdentityReadOnlyBeforeSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Always_identity_property_on_Added_entity_gets_store_value_even_when_set_explicitly()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball { AlwaysIdentity = "Masami" }).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Banana Joe", entity.AlwaysIdentity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity);
            }
        }

        [Fact]
        public virtual void Always_identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
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

                Assert.Equal("Anton", gumball.AlwaysIdentityReadOnlyAfterSave);

                gumball.AlwaysIdentityReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave("AlwaysIdentityReadOnlyAfterSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Always_identity_property_on_Modified_entity_is_not_included_in_update_when_modified()
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

                Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                gumball.AlwaysIdentity = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.SaveChanges();

                Assert.Equal("Masami", gumball.AlwaysIdentity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity);
            }
        }

        [Fact]
        public virtual void Always_identity_property_on_Modified_entity_is_not_included_in_the_update_when_not_modified()
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

                Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                gumball.AlwaysIdentity = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.AlwaysIdentity).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.AlwaysIdentity).IsModified = false;

                context.SaveChanges();

                Assert.Equal("Masami", gumball.AlwaysIdentity);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity);
            }
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entry = context.Add(new Gumball { Computed = "Masami" });
                entry.Property(e => e.Computed).IsTemporary = true;

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
        public virtual void Computed_property_on_Added_entity_with_default_value_gets_value_from_store()
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
                    CoreStrings.PropertyReadOnlyBeforeSave("ComputedReadOnlyBeforeSave", "Gumball"),
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
                    CoreStrings.PropertyReadOnlyAfterSave("ComputedReadOnlyAfterSave", "Gumball"),
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

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entry = context.Add(new Gumball { AlwaysComputed = "Masami" });
                entry.Property(e => e.AlwaysComputed).IsTemporary = true;

                context.SaveChanges();
                id = entry.Entity.Id;

                Assert.Equal("Alan", entry.Entity.AlwaysComputed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed);
            }
        }

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball()).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Alan", entity.AlwaysComputed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed);
            }
        }

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            using (var context = CreateContext())
            {
                context.Add(new Gumball { AlwaysComputedReadOnlyBeforeSave = "Masami" });

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave("AlwaysComputedReadOnlyBeforeSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_cannot_have_value_set_explicitly()
        {
            int id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new Gumball { AlwaysComputed = "Masami" }).Entity;

                context.SaveChanges();
                id = entity.Id;

                Assert.Equal("Alan", entity.AlwaysComputed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed);
            }
        }

        [Fact]
        public virtual void Always_computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
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

                Assert.Equal("Tina Rex", gumball.AlwaysComputedReadOnlyAfterSave);

                gumball.AlwaysComputedReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave("AlwaysComputedReadOnlyAfterSave", "Gumball"),
                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
            }
        }

        [Fact]
        public virtual void Always_computed_property_on_Modified_entity_is_not_included_in_update_even_when_modified()
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

                Assert.Equal("Alan", gumball.AlwaysComputed);

                gumball.AlwaysComputed = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.SaveChanges();

                Assert.Equal("Alan", gumball.AlwaysComputed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed);
            }
        }

        [Fact]
        public virtual void Always_computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
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

                Assert.Equal("Alan", gumball.AlwaysComputed);

                gumball.AlwaysComputed = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.AlwaysComputed).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.AlwaysComputed).IsModified = false;

                context.SaveChanges();

                Assert.Equal("Alan", gumball.AlwaysComputed);
            }

            using (var context = CreateContext())
            {
                Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed);
            }
        }

        protected class Darwin
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        protected class Gumball
        {
            public int Id { get; set; }
            public string NotStoreGenerated { get; set; }

            public string Identity { get; set; }
            public string IdentityReadOnlyBeforeSave { get; set; }
            public string IdentityReadOnlyAfterSave { get; set; }

            public string AlwaysIdentity { get; set; }
            public string AlwaysIdentityReadOnlyBeforeSave { get; set; }
            public string AlwaysIdentityReadOnlyAfterSave { get; set; }

            public string Computed { get; set; }
            public string ComputedReadOnlyBeforeSave { get; set; }
            public string ComputedReadOnlyAfterSave { get; set; }

            public string AlwaysComputed { get; set; }
            public string AlwaysComputedReadOnlyBeforeSave { get; set; }
            public string AlwaysComputedReadOnlyAfterSave { get; set; }
        }

        protected class StoreGeneratedContext : DbContext
        {
            public StoreGeneratedContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Gumball> Gumballs { get; set; }
            public DbSet<Darwin> Darwins { get; set; }
        }

        protected StoreGeneratedContext CreateContext()
            => (StoreGeneratedContext)Fixture.CreateContext(TestStore);

        public void Dispose()
            => TestStore.Dispose();

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
                        var property = b.Property(e => e.Id).ValueGeneratedOnAdd().Metadata;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.Identity).ValueGeneratedOnAdd().Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.IdentityReadOnlyBeforeSave).ValueGeneratedOnAdd().Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.IdentityReadOnlyAfterSave).ValueGeneratedOnAdd().Metadata;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.AlwaysIdentity).ValueGeneratedOnAdd().Metadata;
                        property.IsStoreGeneratedAlways = true;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).ValueGeneratedOnAdd().Metadata;
                        property.IsStoreGeneratedAlways = true;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).ValueGeneratedOnAdd().Metadata;
                        property.IsStoreGeneratedAlways = true;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.Computed).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.ComputedReadOnlyBeforeSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.ComputedReadOnlyAfterSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.AlwaysComputed).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.IsStoreGeneratedAlways = true;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = false;

                        property = b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.IsStoreGeneratedAlways = true;
                        property.IsReadOnlyAfterSave = false;
                        property.IsReadOnlyBeforeSave = true;

                        property = b.Property(e => e.AlwaysComputedReadOnlyAfterSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.IsStoreGeneratedAlways = true;
                        property.IsReadOnlyAfterSave = true;
                        property.IsReadOnlyBeforeSave = false;
                    });
            }
        }
    }
}
