// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
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

        [Theory]
        [InlineData("NeverThrowBeforeUseAfter")]
        [InlineData("NeverThrowBeforeIgnoreAfter")]
        [InlineData("NeverThrowBeforeThrowAfter")]
        [InlineData("OnAddThrowBeforeUseAfter")]
        [InlineData("OnAddThrowBeforeIgnoreAfter")]
        [InlineData("OnAddThrowBeforeThrowAfter")]
        [InlineData("OnAddOrUpdateThrowBeforeUseAfter")]
        [InlineData("OnAddOrUpdateThrowBeforeIgnoreAfter")]
        [InlineData("OnAddOrUpdateThrowBeforeThrowAfter")]
        [InlineData("OnUpdateThrowBeforeUseAfter")]
        [InlineData("OnUpdateThrowBeforeIgnoreAfter")]
        [InlineData("OnUpdateThrowBeforeThrowAfter")]
        public virtual void Before_save_throw_always_throws_if_value_set(string propertyName)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        context.Add(WithValue(propertyName));

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyBeforeSave(propertyName, "Anais"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Theory]
        [InlineData("NeverThrowBeforeUseAfter", null)]
        [InlineData("NeverThrowBeforeIgnoreAfter", null)]
        [InlineData("NeverThrowBeforeThrowAfter", null)]
        [InlineData("OnAddThrowBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddThrowBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddThrowBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateThrowBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateThrowBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateThrowBeforeThrowAfter", "Rabbit")]
        [InlineData("OnUpdateThrowBeforeUseAfter", "Rabbit")]
        [InlineData("OnUpdateThrowBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnUpdateThrowBeforeThrowAfter", "Rabbit")]
        public virtual void Before_save_throw_ignores_value_if_not_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("Never")]
        [InlineData("OnAdd")]
        [InlineData("OnUpdate")]
        [InlineData("NeverUseBeforeUseAfter")]
        [InlineData("NeverUseBeforeIgnoreAfter")]
        [InlineData("NeverUseBeforeThrowAfter")]
        [InlineData("OnAddUseBeforeUseAfter")]
        [InlineData("OnAddUseBeforeIgnoreAfter")]
        [InlineData("OnAddUseBeforeThrowAfter")]
        [InlineData("OnAddOrUpdateUseBeforeUseAfter")]
        [InlineData("OnAddOrUpdateUseBeforeIgnoreAfter")]
        [InlineData("OnAddOrUpdateUseBeforeThrowAfter")]
        [InlineData("OnUpdateUseBeforeUseAfter")]
        [InlineData("OnUpdateUseBeforeIgnoreAfter")]
        [InlineData("OnUpdateUseBeforeThrowAfter")]
        public virtual void Before_save_use_always_uses_value_if_set(string propertyName)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(WithValue(propertyName)).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context => { Assert.Equal("Pink", GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("Never", null)]
        [InlineData("OnAdd", "Rabbit")]
        [InlineData("OnUpdate", null)]
        [InlineData("NeverUseBeforeUseAfter", null)]
        [InlineData("NeverUseBeforeIgnoreAfter", null)]
        [InlineData("NeverUseBeforeThrowAfter", null)]
        [InlineData("OnAddUseBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddUseBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddUseBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateUseBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateUseBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateUseBeforeThrowAfter", "Rabbit")]
        [InlineData("OnUpdateUseBeforeUseAfter", null)]
        [InlineData("OnUpdateUseBeforeIgnoreAfter", null)]
        [InlineData("OnUpdateUseBeforeThrowAfter", null)]
        public virtual void Before_save_use_ignores_value_if_not_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("OnAddOrUpdate", "Rabbit")]
        [InlineData("NeverIgnoreBeforeUseAfter", null)]
        [InlineData("NeverIgnoreBeforeIgnoreAfter", null)]
        [InlineData("NeverIgnoreBeforeThrowAfter", null)]
        [InlineData("OnAddIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeThrowAfter", "Rabbit")]
        [InlineData("OnUpdateIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnUpdateIgnoreBeforeThrowAfter", "Rabbit")]
        public virtual void Before_save_ignore_ignores_value_if_not_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("OnAddOrUpdate", "Rabbit")]
        [InlineData("NeverIgnoreBeforeUseAfter", null)]
        [InlineData("NeverIgnoreBeforeIgnoreAfter", null)]
        [InlineData("NeverIgnoreBeforeThrowAfter", null)]
        [InlineData("OnAddIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeThrowAfter", "Rabbit")]
        [InlineData("OnUpdateIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnUpdateIgnoreBeforeThrowAfter", "Rabbit")]
        public virtual void Before_save_ignore_ignores_value_even_if_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(WithValue(propertyName)).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("NeverUseBeforeThrowAfter")]
        [InlineData("NeverIgnoreBeforeThrowAfter")]
        [InlineData("NeverThrowBeforeThrowAfter")]
        [InlineData("OnAddUseBeforeThrowAfter")]
        [InlineData("OnAddIgnoreBeforeThrowAfter")]
        [InlineData("OnAddThrowBeforeThrowAfter")]
        [InlineData("OnAddOrUpdateUseBeforeThrowAfter")]
        [InlineData("OnAddOrUpdateIgnoreBeforeThrowAfter")]
        [InlineData("OnAddOrUpdateThrowBeforeThrowAfter")]
        [InlineData("OnUpdateUseBeforeThrowAfter")]
        [InlineData("OnUpdateIgnoreBeforeThrowAfter")]
        [InlineData("OnUpdateThrowBeforeThrowAfter")]
        public virtual void After_save_throw_always_throws_if_value_modified(string propertyName)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        context.Attach(WithValue(propertyName, 1)).Property(propertyName).IsModified = true;

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyAfterSave(propertyName, "Anais"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Theory]
        [InlineData("NeverUseBeforeThrowAfter", null)]
        [InlineData("NeverIgnoreBeforeThrowAfter", null)]
        [InlineData("NeverThrowBeforeThrowAfter", null)]
        [InlineData("OnAddUseBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddThrowBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateUseBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeThrowAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateThrowBeforeThrowAfter", "Rabbit")]
        [InlineData("OnUpdateUseBeforeThrowAfter", null)]
        [InlineData("OnUpdateIgnoreBeforeThrowAfter", "Rabbit")]
        [InlineData("OnUpdateThrowBeforeThrowAfter", "Rabbit")]
        public virtual void After_save_throw_ignores_value_if_not_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context =>
                    {
                        var entry = context.Entry(context.Anaises.Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = false;

                        context.SaveChanges();
                    },
                context =>
                    {
                        Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName));
                    });
        }

        [Theory]
        [InlineData("OnAddOrUpdate", "Rabbit")]
        [InlineData("OnUpdate", null)]
        [InlineData("NeverUseBeforeIgnoreAfter", null)]
        [InlineData("NeverIgnoreBeforeIgnoreAfter", null)]
        [InlineData("NeverThrowBeforeIgnoreAfter", null)]
        [InlineData("OnAddUseBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddThrowBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateUseBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateThrowBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnUpdateUseBeforeIgnoreAfter", null)]
        [InlineData("OnUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnUpdateThrowBeforeIgnoreAfter", "Rabbit")]
        public virtual void After_save_ignore_ignores_value_if_not_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context =>
                    {
                        var entry = context.Entry(context.Anaises.Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = false;

                        context.SaveChanges();
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("OnAddOrUpdate", "Rabbit")]
        [InlineData("OnUpdate", null)]
        [InlineData("NeverUseBeforeIgnoreAfter", null)]
        [InlineData("NeverIgnoreBeforeIgnoreAfter", null)]
        [InlineData("NeverThrowBeforeIgnoreAfter", null)]
        [InlineData("OnAddUseBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddThrowBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateUseBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateThrowBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnUpdateUseBeforeIgnoreAfter", null)]
        [InlineData("OnUpdateIgnoreBeforeIgnoreAfter", "Rabbit")]
        [InlineData("OnUpdateThrowBeforeIgnoreAfter", "Rabbit")]
        public virtual void After_save_ignore_ignores_value_even_if_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context =>
                    {
                        var entry = context.Entry(context.Anaises.Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = true;

                        context.SaveChanges();
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("Never", null)]
        [InlineData("OnAdd", "Rabbit")]
        [InlineData("OnAddOrUpdate", "Rabbit")]
        [InlineData("OnUpdate", null)]
        [InlineData("NeverUseBeforeUseAfter", null)]
        [InlineData("NeverIgnoreBeforeUseAfter", null)]
        [InlineData("NeverThrowBeforeUseAfter", null)]
        [InlineData("OnAddUseBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddThrowBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateUseBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnAddOrUpdateThrowBeforeUseAfter", "Rabbit")]
        [InlineData("OnUpdateUseBeforeUseAfter", null)]
        [InlineData("OnUpdateIgnoreBeforeUseAfter", "Rabbit")]
        [InlineData("OnUpdateThrowBeforeUseAfter", "Rabbit")]
        public virtual void After_save_use_ignores_value_if_not_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context =>
                    {
                        var entry = context.Entry(context.Anaises.Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = false;

                        context.SaveChanges();
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        [Theory]
        [InlineData("Never", "Daisy")]
        [InlineData("OnAdd", "Daisy")]
        [InlineData("NeverUseBeforeUseAfter", "Daisy")]
        [InlineData("NeverIgnoreBeforeUseAfter", "Daisy")]
        [InlineData("NeverThrowBeforeUseAfter", "Daisy")]
        [InlineData("OnAddUseBeforeUseAfter", "Daisy")]
        [InlineData("OnAddIgnoreBeforeUseAfter", "Daisy")]
        [InlineData("OnAddThrowBeforeUseAfter", "Daisy")]
        [InlineData("OnAddOrUpdateUseBeforeUseAfter", "Daisy")]
        [InlineData("OnAddOrUpdateIgnoreBeforeUseAfter", "Daisy")]
        [InlineData("OnAddOrUpdateThrowBeforeUseAfter", "Daisy")]
        [InlineData("OnUpdateUseBeforeUseAfter", "Daisy")]
        [InlineData("OnUpdateIgnoreBeforeUseAfter", "Daisy")]
        [InlineData("OnUpdateThrowBeforeUseAfter", "Daisy")]
        public virtual void After_save_use_uses_value_if_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Anais()).Entity;

                        context.SaveChanges();

                        id = entity.Id;
                    },
                context =>
                    {
                        var entry = context.Entry(context.Anaises.Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";

                        context.SaveChanges();
                    },
                context => { Assert.Equal(expectedValue, GetValue(context.Anaises.Find(id), propertyName)); });
        }

        private static Anais WithValue(string propertyName, int id = 0)
            => SetValue(new Anais { Id = id }, propertyName);

        private static Anais SetValue(Anais entity, string propertyName)
        {
            entity.GetType().GetTypeInfo().GetDeclaredProperty(propertyName).SetValue(entity, "Pink");
            return entity;
        }

        private static string GetValue(Anais entity, string propertyName)
            => (string)entity.GetType().GetTypeInfo().GetDeclaredProperty(propertyName).GetValue(entity);

        [Fact]
        public virtual void Identity_key_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        context.Add(new Gumball { Id = 1 });

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyBeforeSave("Id", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Add(new Gumball { Identity = "Masami" });
                        entry.Property(e => e.Identity).IsTemporary = true;

                        context.SaveChanges();
                        id = entry.Entity.Id;

                        Assert.Equal("Banana Joe", entry.Entity.Identity);
                    },
                context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).Identity); });
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Banana Joe", entity.Identity);
                    },
                context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).Identity); });
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        context.Add(new Gumball { IdentityReadOnlyBeforeSave = "Masami" });

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyBeforeSave("IdentityReadOnlyBeforeSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_can_have_value_set_explicitly()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball { Identity = "Masami" }).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Masami", entity.Identity);
                    },
                context => { Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Identity); });
        }

        [Fact]
        public virtual void Identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Anton", gumball.IdentityReadOnlyAfterSave);

                        gumball.IdentityReadOnlyAfterSave = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyAfterSave("IdentityReadOnlyAfterSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Identity_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.Identity);

                        gumball.Identity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.Identity);
                    },
                context => { Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Identity); });
        }

        [Fact]
        public virtual void Identity_property_on_Modified_entity_is_not_included_in_update_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.Identity);

                        gumball.Identity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.Identity).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.Identity).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.Identity);
                    },
                context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).Identity); });
        }

        [Fact]
        public virtual void Always_identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Add(new Gumball { AlwaysIdentity = "Masami" });
                        entry.Property(e => e.AlwaysIdentity).IsTemporary = true;

                        context.SaveChanges();
                        id = entry.Entity.Id;

                        Assert.Equal("Banana Joe", entry.Entity.AlwaysIdentity);
                    },
                context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity); });
        }

        [Fact]
        public virtual void Always_identity_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Banana Joe", entity.AlwaysIdentity);
                    },
                context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity); });
        }

        [Fact]
        public virtual void Always_identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        context.Add(new Gumball { AlwaysIdentityReadOnlyBeforeSave = "Masami" });

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyBeforeSave("AlwaysIdentityReadOnlyBeforeSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Always_identity_property_on_Added_entity_gets_store_value_even_when_set_explicitly()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball { AlwaysIdentity = "Masami" }).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Banana Joe", entity.AlwaysIdentity);
                    },
                context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity); });
        }

        [Fact]
        public virtual void Always_identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Anton", gumball.AlwaysIdentityReadOnlyAfterSave);

                        gumball.AlwaysIdentityReadOnlyAfterSave = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyAfterSave("AlwaysIdentityReadOnlyAfterSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Always_identity_property_on_Modified_entity_is_not_included_in_update_when_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                        gumball.AlwaysIdentity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.AlwaysIdentity);
                    },
                context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity); });
        }

        [Fact]
        public virtual void Always_identity_property_on_Modified_entity_is_not_included_in_the_update_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                        gumball.AlwaysIdentity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.AlwaysIdentity).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.AlwaysIdentity).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.AlwaysIdentity);
                    }, context => { Assert.Equal("Banana Joe", context.Gumballs.Single(e => e.Id == id).AlwaysIdentity); });
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Add(new Gumball { Computed = "Masami" });
                        entry.Property(e => e.Computed).IsTemporary = true;

                        context.SaveChanges();
                        id = entry.Entity.Id;

                        Assert.Equal("Alan", entry.Entity.Computed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).Computed); });
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Alan", entity.Computed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).Computed); });
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        context.Add(new Gumball { ComputedReadOnlyBeforeSave = "Masami" });

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyBeforeSave("ComputedReadOnlyBeforeSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Computed_property_on_Added_entity_can_have_value_set_explicitly()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball { Computed = "Masami" }).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Masami", entity.Computed);
                    },
                context => { Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Computed); });
        }

        [Fact]
        public virtual void Computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Tina Rex", gumball.ComputedReadOnlyAfterSave);

                        gumball.ComputedReadOnlyAfterSave = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyAfterSave("ComputedReadOnlyAfterSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Computed_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.Computed);

                        gumball.Computed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.Computed);
                    },
                context => { Assert.Equal("Masami", context.Gumballs.Single(e => e.Id == id).Computed); });
        }

        [Fact]
        public virtual void Computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.Computed);

                        gumball.Computed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.Computed).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.Computed).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Alan", gumball.Computed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).Computed); });
        }

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Add(new Gumball { AlwaysComputed = "Masami" });
                        entry.Property(e => e.AlwaysComputed).IsTemporary = true;

                        context.SaveChanges();
                        id = entry.Entity.Id;

                        Assert.Equal("Alan", entry.Entity.AlwaysComputed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed); });
        }

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Alan", entity.AlwaysComputed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed); });
        }

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        context.Add(new Gumball { AlwaysComputedReadOnlyBeforeSave = "Masami" });

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyBeforeSave("AlwaysComputedReadOnlyBeforeSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Always_computed_property_on_Added_entity_cannot_have_value_set_explicitly()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball { AlwaysComputed = "Masami" }).Entity;

                        context.SaveChanges();
                        id = entity.Id;

                        Assert.Equal("Alan", entity.AlwaysComputed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed); });
        }

        [Fact]
        public virtual void Always_computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Tina Rex", gumball.AlwaysComputedReadOnlyAfterSave);

                        gumball.AlwaysComputedReadOnlyAfterSave = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        Assert.Equal(
                            CoreStrings.PropertyReadOnlyAfterSave("AlwaysComputedReadOnlyAfterSave", "Gumball"),
                            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                    });
        }

        [Fact]
        public virtual void Always_computed_property_on_Modified_entity_is_not_included_in_update_even_when_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.AlwaysComputed);

                        gumball.AlwaysComputed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Alan", gumball.AlwaysComputed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed); });
        }

        [Fact]
        public virtual void Always_computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entity = context.Add(new Gumball()).Entity;

                        context.SaveChanges();
                        id = entity.Id;
                    },
                context =>
                    {
                        var gumball = context.Gumballs.Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.AlwaysComputed);

                        gumball.AlwaysComputed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.AlwaysComputed).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.AlwaysComputed).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Alan", gumball.AlwaysComputed);
                    },
                context => { Assert.Equal("Alan", context.Gumballs.Single(e => e.Id == id).AlwaysComputed); });
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

        protected class Anais
        {
            public int Id { get; set; }
            public string Never { get; set; }
            public string NeverUseBeforeUseAfter { get; set; }
            public string NeverIgnoreBeforeUseAfter { get; set; }
            public string NeverThrowBeforeUseAfter { get; set; }
            public string NeverUseBeforeIgnoreAfter { get; set; }
            public string NeverIgnoreBeforeIgnoreAfter { get; set; }
            public string NeverThrowBeforeIgnoreAfter { get; set; }
            public string NeverUseBeforeThrowAfter { get; set; }
            public string NeverIgnoreBeforeThrowAfter { get; set; }
            public string NeverThrowBeforeThrowAfter { get; set; }

            public string OnAdd { get; set; }
            public string OnAddUseBeforeUseAfter { get; set; }
            public string OnAddIgnoreBeforeUseAfter { get; set; }
            public string OnAddThrowBeforeUseAfter { get; set; }
            public string OnAddUseBeforeIgnoreAfter { get; set; }
            public string OnAddIgnoreBeforeIgnoreAfter { get; set; }
            public string OnAddThrowBeforeIgnoreAfter { get; set; }
            public string OnAddUseBeforeThrowAfter { get; set; }
            public string OnAddIgnoreBeforeThrowAfter { get; set; }
            public string OnAddThrowBeforeThrowAfter { get; set; }

            public string OnAddOrUpdate { get; set; }
            public string OnAddOrUpdateUseBeforeUseAfter { get; set; }
            public string OnAddOrUpdateIgnoreBeforeUseAfter { get; set; }
            public string OnAddOrUpdateThrowBeforeUseAfter { get; set; }
            public string OnAddOrUpdateUseBeforeIgnoreAfter { get; set; }
            public string OnAddOrUpdateIgnoreBeforeIgnoreAfter { get; set; }
            public string OnAddOrUpdateThrowBeforeIgnoreAfter { get; set; }
            public string OnAddOrUpdateUseBeforeThrowAfter { get; set; }
            public string OnAddOrUpdateIgnoreBeforeThrowAfter { get; set; }
            public string OnAddOrUpdateThrowBeforeThrowAfter { get; set; }

            public string OnUpdate { get; set; }
            public string OnUpdateUseBeforeUseAfter { get; set; }
            public string OnUpdateIgnoreBeforeUseAfter { get; set; }
            public string OnUpdateThrowBeforeUseAfter { get; set; }
            public string OnUpdateUseBeforeIgnoreAfter { get; set; }
            public string OnUpdateIgnoreBeforeIgnoreAfter { get; set; }
            public string OnUpdateThrowBeforeIgnoreAfter { get; set; }
            public string OnUpdateUseBeforeThrowAfter { get; set; }
            public string OnUpdateIgnoreBeforeThrowAfter { get; set; }
            public string OnUpdateThrowBeforeThrowAfter { get; set; }
        }

        protected class StoreGeneratedContext : DbContext
        {
            public StoreGeneratedContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Gumball> Gumballs { get; set; }
            public DbSet<Darwin> Darwins { get; set; }
            public DbSet<Anais> Anaises { get; set; }
        }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<StoreGeneratedContext> testOperation,
            Action<StoreGeneratedContext> nestedTestOperation1 = null,
            Action<StoreGeneratedContext> nestedTestOperation2 = null)
            => DbContextHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
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
                modelBuilder.Entity<Gumball>(
                    b =>
                        {
                            var property = b.Property(e => e.Id).ValueGeneratedOnAdd().Metadata;
#pragma warning disable 618
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
                            property.IsStoreGeneratedAlways = false;
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
#pragma warning restore 618
                        });

                modelBuilder.Entity<Anais>(
                    b =>
                        {
                            b.Property(e => e.Never).ValueGeneratedNever();

                            var property = b.Property(e => e.NeverUseBeforeUseAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.NeverIgnoreBeforeUseAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.NeverThrowBeforeUseAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.NeverUseBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.NeverIgnoreBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.NeverThrowBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.NeverUseBeforeThrowAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.NeverIgnoreBeforeThrowAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.NeverThrowBeforeThrowAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            b.Property(e => e.OnAdd).ValueGeneratedOnAdd();

                            property = b.Property(e => e.OnAddUseBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnAddIgnoreBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnAddThrowBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnAddUseBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnAddThrowBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnAddUseBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.OnAddIgnoreBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.OnAddThrowBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            b.Property(e => e.OnAddOrUpdate).ValueGeneratedOnAddOrUpdate();

                            property = b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            b.Property(e => e.OnUpdate).ValueGeneratedOnUpdate();

                            property = b.Property(e => e.OnUpdateUseBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnUpdateThrowBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.UseValue;

                            property = b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Ignore;

                            property = b.Property(e => e.OnUpdateUseBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.UseValue;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Ignore;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;

                            property = b.Property(e => e.OnUpdateThrowBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertyValueBehavior.Throw;
                            property.AfterSaveBehavior = PropertyValueBehavior.Throw;
                        });
            }
        }
    }
}
