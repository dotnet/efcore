// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class StoreGeneratedTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : StoreGeneratedTestBase<TFixture>.StoreGeneratedFixtureBase, new()
    {
        protected StoreGeneratedTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void Value_generation_throws_for_common_cases()
        {
            ValueGenerationNegative<int, IntToString, NumberToStringConverter<int>>();
            ValueGenerationNegative<short, ShortToBytes, NumberToBytesConverter<short>>();
        }

        private void ValueGenerationNegative<TKey, TEntity, TConverter>()
            where TEntity : WithConverter<TKey>, new()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.ValueGenWithConversion(
                        typeof(TEntity).ShortDisplayName(),
                        nameof(WithConverter<int>.Id),
                        typeof(TConverter).ShortDisplayName()),
                    Assert.Throws<NotSupportedException>(() => context.Add(new TEntity())).Message);
            }
        }

        [Fact]
        public virtual void Value_generation_works_for_common_GUID_conversions()
        {
            ValueGenerationPositive<Guid, GuidToString>();
            ValueGenerationPositive<Guid, GuidToBytes>();
        }

        private void ValueGenerationPositive<TKey, TEntity>()
            where TEntity : WithConverter<TKey>, new()
        {
            TKey id;

            using (var context = CreateContext())
            {
                var entity = context.Add(new TEntity()).Entity;

                context.SaveChanges();

                id = entity.Id;
            }

            using (var context = CreateContext())
            {
                Assert.Equal(id, context.Set<TEntity>().Single(e => e.Id.Equals(id)).Id);
            }
        }

        [Theory]
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter))]
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
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter), "Rabbit")]
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
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.Never))]
        [InlineData(nameof(Anais.OnAdd))]
        [InlineData(nameof(Anais.OnUpdate))]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter))]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter))]
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
                context => Assert.Equal("Pink", GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.Never), null)]
        [InlineData(nameof(Anais.OnAdd), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter), null)]
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
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
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
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
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
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter))]
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
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter), "Rabbit")]
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
                        var entry = context.Entry(context.Set<Anais>().Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = false;

                        context.SaveChanges();
                    },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
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
                        var entry = context.Entry(context.Set<Anais>().Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = false;

                        context.SaveChanges();
                    },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
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
                        var entry = context.Entry(context.Set<Anais>().Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = true;

                        context.SaveChanges();
                    },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.Never), null)]
        [InlineData(nameof(Anais.OnAdd), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Rabbit")]
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
                        var entry = context.Entry(context.Set<Anais>().Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";
                        entry.Property(propertyName).IsModified = false;

                        context.SaveChanges();
                    },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [Theory]
        [InlineData(nameof(Anais.Never), "Daisy")]
        [InlineData(nameof(Anais.OnAdd), "Daisy")]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Daisy")]
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
                        var entry = context.Entry(context.Set<Anais>().Find(id));
                        entry.State = EntityState.Modified;
                        entry.Property(propertyName).CurrentValue = "Daisy";

                        context.SaveChanges();
                    },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
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
                        Assert.False(entry.Property(e => e.Identity).IsTemporary);
                    },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
        }

        [Fact]
        public virtual void Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store_even_if_same()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Add(new Gumball { Identity = "Banana Joe" });
                        entry.Property(e => e.Identity).IsTemporary = true;

                        context.SaveChanges();
                        id = entry.Entity.Id;

                        Assert.Equal("Banana Joe", entry.Entity.Identity);
                        Assert.False(entry.Property(e => e.Identity).IsTemporary);
                    },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
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
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
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
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Identity));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.Identity);

                        gumball.Identity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.Identity);
                    },
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Identity));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.Identity);

                        gumball.Identity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.Identity).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.Identity).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.Identity);
                    },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
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
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
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
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
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
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                        gumball.AlwaysIdentity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.AlwaysIdentity);
                    },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                        gumball.AlwaysIdentity = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.AlwaysIdentity).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.AlwaysIdentity).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.AlwaysIdentity);
                    }, context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
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
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).Computed));
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
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).Computed));
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
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Computed));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.Computed);

                        gumball.Computed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Masami", gumball.Computed);
                    },
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Computed));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.Computed);

                        gumball.Computed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.Computed).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.Computed).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Alan", gumball.Computed);
                    },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).Computed));
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
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
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
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
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
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.AlwaysComputed);

                        gumball.AlwaysComputed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.SaveChanges();

                        Assert.Equal("Alan", gumball.AlwaysComputed);
                    },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
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
                        var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                        Assert.Equal("Alan", gumball.AlwaysComputed);

                        gumball.AlwaysComputed = "Masami";
                        gumball.NotStoreGenerated = "Larry Needlemeye";

                        context.Entry(gumball).Property(e => e.AlwaysComputed).OriginalValue = "Masami";
                        context.Entry(gumball).Property(e => e.AlwaysComputed).IsModified = false;

                        context.SaveChanges();

                        Assert.Equal("Alan", gumball.AlwaysComputed);
                    },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
        }

        [Fact]
        public virtual void Fields_used_correctly_for_store_generated_values()
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new WithBackingFields()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var entity = context.Set<WithBackingFields>().Single(e => e.Id.Equals(id));
                    Assert.Equal(1, entity.NullableAsNonNullable);
                    Assert.Equal(1, entity.NonNullableAsNullable);
                });
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

        protected class WithBackingFields
        {
            private int _id;
            public int Id
            {
                get => _id;
                set => _id = value;
            }

            private int? _nullableAsNonNullable = 0;
            public int NullableAsNonNullable
            {
                get => (int)_nullableAsNonNullable;
                set => _nullableAsNonNullable = value;
            }

            private int _nonNullableAsNullable;
            public int? NonNullableAsNullable
            {
                get => _nonNullableAsNullable;
                set => _nonNullableAsNullable = (int)value;
            }
        }

        protected class WithConverter<TKey>
        {
            public TKey Id { get; set; }
        }

        protected class IntToString : WithConverter<int>
        {
        }

        protected class GuidToString: WithConverter<Guid>
        {
        }

        protected class GuidToBytes: WithConverter<Guid>
        {
        }

        protected class ShortToBytes: WithConverter<short>
        {
        }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<DbContext> testOperation,
            Action<DbContext> nestedTestOperation1 = null,
            Action<DbContext> nestedTestOperation2 = null)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected DbContext CreateContext() => Fixture.CreateContext();

        public abstract class StoreGeneratedFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "StoreGeneratedTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<IntToString>().Property(e => e.Id).HasConversion<string>();
                modelBuilder.Entity<GuidToString>().Property(e => e.Id).HasConversion<string>();
                modelBuilder.Entity<GuidToBytes>().Property(e => e.Id).HasConversion<byte[]>();
                modelBuilder.Entity<ShortToBytes>().Property(e => e.Id).HasConversion<byte[]>();

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
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.NeverIgnoreBeforeUseAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.NeverThrowBeforeUseAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.NeverUseBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.NeverIgnoreBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.NeverThrowBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.NeverUseBeforeThrowAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.NeverIgnoreBeforeThrowAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.NeverThrowBeforeThrowAfter).ValueGeneratedNever().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            b.Property(e => e.OnAdd).ValueGeneratedOnAdd();

                            property = b.Property(e => e.OnAddUseBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnAddIgnoreBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnAddThrowBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnAddUseBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnAddThrowBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnAddUseBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.OnAddIgnoreBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.OnAddThrowBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            b.Property(e => e.OnAddOrUpdate).ValueGeneratedOnAddOrUpdate();

                            property = b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            b.Property(e => e.OnUpdate).ValueGeneratedOnUpdate();

                            property = b.Property(e => e.OnUpdateUseBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnUpdateThrowBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Save;

                            property = b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Ignore;

                            property = b.Property(e => e.OnUpdateUseBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Save;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;

                            property = b.Property(e => e.OnUpdateThrowBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                            property.BeforeSaveBehavior = PropertySaveBehavior.Throw;
                            property.AfterSaveBehavior = PropertySaveBehavior.Throw;
                        });

                modelBuilder.Entity<Darwin>();

                modelBuilder.Entity<WithBackingFields>(b =>
                {
                    b.Property(e => e.Id).HasField("_id");
                    b.Property(e => e.NullableAsNonNullable).HasField("_nullableAsNonNullable").ValueGeneratedOnAddOrUpdate();
                    b.Property(e => e.NonNullableAsNullable).HasField("_nonNullableAsNullable").ValueGeneratedOnAddOrUpdate();
                });
            }
        }
    }
}
