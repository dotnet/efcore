// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class FindTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : FindTestBase<TFixture>.FindFixtureBase
    {
        protected FindTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected abstract TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            where TEntity : class;

        protected abstract Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
            where TEntity : class;

        [Fact]
        public virtual void Find_int_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new IntKey
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, Find<IntKey>(context, 88));
            }
        }

        [Fact]
        public virtual void Find_int_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Smokey", Find<IntKey>(context, 77).Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_int_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<IntKey>(context, 99));
            }
        }

        [Fact]
        public virtual void Find_nullable_int_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new NullableIntKey
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, Find<NullableIntKey>(context, 88));
            }
        }

        [Fact]
        public virtual void Find_nullable_int_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Smokey", Find<NullableIntKey>(context, 77).Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_nullable_int_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<NullableIntKey>(context, 99));
            }
        }

        [Fact]
        public virtual void Find_string_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new StringKey
                    {
                        Id = "Rabbit"
                    }).Entity;

                Assert.Same(entity, Find<StringKey>(context, "Rabbit"));
            }
        }

        [Fact]
        public virtual void Find_string_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Alice", Find<StringKey>(context, "Cat").Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_string_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<StringKey>(context, "Fox"));
            }
        }

        [Fact]
        public virtual void Find_composite_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new CompositeKey
                    {
                        Id1 = 88,
                        Id2 = "Rabbit"
                    }).Entity;

                Assert.Same(entity, Find<CompositeKey>(context, 88, "Rabbit"));
            }
        }

        [Fact]
        public virtual void Find_composite_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Olive", Find<CompositeKey>(context, 77, "Dog").Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_composite_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<CompositeKey>(context, 77, "Fox"));
            }
        }

        [Fact]
        public virtual void Find_base_type_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new BaseType
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, Find<BaseType>(context, 88));
            }
        }

        [Fact]
        public virtual void Find_base_type_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Baxter", Find<BaseType>(context, 77).Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_base_type_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<BaseType>(context, 99));
            }
        }

        [Fact]
        public virtual void Find_derived_type_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new DerivedType
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, Find<DerivedType>(context, 88));
            }
        }

        [Fact]
        public virtual void Find_derived_type_from_store()
        {
            using (var context = CreateContext())
            {
                var derivedType = Find<DerivedType>(context, 78);
                Assert.Equal("Strawberry", derivedType.Foo);
                Assert.Equal("Cheesecake", derivedType.Boo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_derived_type_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<DerivedType>(context, 99));
            }
        }

        [Fact]
        public virtual void Find_base_type_using_derived_set_tracked()
        {
            using (var context = CreateContext())
            {
                context.Attach(
                    new BaseType
                    {
                        Id = 88
                    });

                Assert.Null(Find<DerivedType>(context, 88));
            }
        }

        [Fact]
        public virtual void Find_base_type_using_derived_set_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<DerivedType>(context, 77));
            }
        }

        [Fact]
        public virtual void Find_derived_type_using_base_set_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new DerivedType
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, Find<BaseType>(context, 88));
            }
        }

        [Fact]
        public virtual void Find_derived_using_base_set_type_from_store()
        {
            using (var context = CreateContext())
            {
                var derivedType = Find<BaseType>(context, 78);
                Assert.Equal("Strawberry", derivedType.Foo);
                Assert.Equal("Cheesecake", ((DerivedType)derivedType).Boo);
            }
        }

        [Fact]
        public virtual void Find_shadow_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entry = context.Entry(new ShadowKey());
                entry.Property("Id").CurrentValue = 88;
                entry.State = EntityState.Unchanged;

                Assert.Same(entry.Entity, Find<ShadowKey>(context, 88));
            }
        }

        [Fact]
        public virtual void Find_shadow_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Clippy", Find<ShadowKey>(context, 77).Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_shadow_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<ShadowKey>(context, 99));
            }
        }

        [Fact]
        public virtual void Returns_null_for_null_key_values_array()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<CompositeKey>(context, null));
            }
        }

        [Fact]
        public virtual void Returns_null_for_null_key()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<IntKey>(context, new object[] { null }));
            }
        }

        [Fact]
        public virtual void Returns_null_for_null_nullable_key()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<NullableIntKey>(context, new object[] { null }));
            }
        }

        [Fact]
        public virtual void Returns_null_for_null_in_composite_key()
        {
            using (var context = CreateContext())
            {
                Assert.Null(Find<CompositeKey>(context, 77, null));
            }
        }

        [Fact]
        public virtual void Throws_for_multiple_values_passed_for_simple_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindNotCompositeKey("IntKey", 2),
                    Assert.Throws<ArgumentException>(() => Find<IntKey>(context, 77, 88)).Message);
            }
        }

        [Fact]
        public virtual void Throws_for_wrong_number_of_values_for_composite_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindValueCountMismatch("CompositeKey", 2, 1),
                    Assert.Throws<ArgumentException>(() => Find<CompositeKey>(context, 77)).Message);
            }
        }

        [Fact]
        public virtual void Throws_for_bad_type_for_simple_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                    Assert.Throws<ArgumentException>(() => Find<IntKey>(context, "77")).Message);
            }
        }

        [Fact]
        public virtual void Throws_for_bad_type_for_composite_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                    Assert.Throws<ArgumentException>(() => Find<CompositeKey>(context, 77, 88)).Message);
            }
        }

        [Fact]
        public virtual void Throws_for_bad_entity_type()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.InvalidSetType(nameof(Random)),
                    Assert.Throws<InvalidOperationException>(() => Find<Random>(context, 77)).Message);
            }
        }

        [Fact]
        public virtual async Task Find_int_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new IntKey
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, await FindAsync<IntKey>(context, 88));
            }
        }

        [Fact]
        public virtual async Task Find_int_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Smokey", (await FindAsync<IntKey>(context, 77)).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_int_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<IntKey>(context, 99));
            }
        }

        [Fact]
        public virtual async Task Find_nullable_int_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new NullableIntKey
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, await FindAsync<NullableIntKey>(context, 88));
            }
        }

        [Fact]
        public virtual async Task Find_nullable_int_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Smokey", (await FindAsync<NullableIntKey>(context, 77)).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_nullable_int_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<NullableIntKey>(context, 99));
            }
        }

        [Fact]
        public virtual async Task Find_string_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new StringKey
                    {
                        Id = "Rabbit"
                    }).Entity;

                Assert.Same(entity, await FindAsync<StringKey>(context, "Rabbit"));
            }
        }

        [Fact]
        public virtual async Task Find_string_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Alice", (await FindAsync<StringKey>(context, "Cat")).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_string_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<StringKey>(context, "Fox"));
            }
        }

        [Fact]
        public virtual async Task Find_composite_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new CompositeKey
                    {
                        Id1 = 88,
                        Id2 = "Rabbit"
                    }).Entity;

                Assert.Same(entity, await FindAsync<CompositeKey>(context, 88, "Rabbit"));
            }
        }

        [Fact]
        public virtual async Task Find_composite_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Olive", (await FindAsync<CompositeKey>(context, 77, "Dog")).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_composite_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<CompositeKey>(context, 77, "Fox"));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new BaseType
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, await FindAsync<BaseType>(context, 88));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Baxter", (await FindAsync<BaseType>(context, 77)).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_base_type_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<BaseType>(context, 99));
            }
        }

        [Fact]
        public virtual async Task Find_derived_type_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new DerivedType
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, await FindAsync<DerivedType>(context, 88));
            }
        }

        [Fact]
        public virtual async Task Find_derived_type_from_store_async()
        {
            using (var context = CreateContext())
            {
                var derivedType = await FindAsync<DerivedType>(context, 78);
                Assert.Equal("Strawberry", derivedType.Foo);
                Assert.Equal("Cheesecake", derivedType.Boo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_derived_type_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<DerivedType>(context, 99));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_using_derived_set_tracked_async()
        {
            using (var context = CreateContext())
            {
                context.Attach(
                    new BaseType
                    {
                        Id = 88
                    });

                Assert.Null(await FindAsync<DerivedType>(context, 88));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_using_derived_set_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<DerivedType>(context, 77));
            }
        }

        [Fact]
        public virtual async Task Find_derived_type_using_base_set_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(
                    new DerivedType
                    {
                        Id = 88
                    }).Entity;

                Assert.Same(entity, await FindAsync<BaseType>(context, 88));
            }
        }

        [Fact]
        public virtual async Task Find_derived_using_base_set_type_from_store_async()
        {
            using (var context = CreateContext())
            {
                var derivedType = await FindAsync<BaseType>(context, 78);
                Assert.Equal("Strawberry", derivedType.Foo);
                Assert.Equal("Cheesecake", ((DerivedType)derivedType).Boo);
            }
        }

        [Fact]
        public virtual async Task Find_shadow_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entry = context.Entry(new ShadowKey());
                entry.Property("Id").CurrentValue = 88;
                entry.State = EntityState.Unchanged;

                Assert.Same(entry.Entity, await FindAsync<ShadowKey>(context, 88));
            }
        }

        [Fact]
        public virtual async Task Find_shadow_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Clippy", (await FindAsync<ShadowKey>(context, 77)).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_shadow_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<ShadowKey>(context, 99));
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_null_key_values_array_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<CompositeKey>(context, null));
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_null_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<IntKey>(context, new object[] { null }));
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_null_in_composite_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await FindAsync<CompositeKey>(context, 77, null));
            }
        }

        [Fact]
        public virtual async Task Throws_for_multiple_values_passed_for_simple_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindNotCompositeKey("IntKey", 2),
                    (await Assert.ThrowsAsync<ArgumentException>(() => FindAsync<IntKey>(context, 77, 88))).Message);
            }
        }

        [Fact]
        public virtual async Task Throws_for_wrong_number_of_values_for_composite_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindValueCountMismatch("CompositeKey", 2, 1),
                    (await Assert.ThrowsAsync<ArgumentException>(() => FindAsync<CompositeKey>(context, 77))).Message);
            }
        }

        [Fact]
        public virtual async Task Throws_for_bad_type_for_simple_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                    (await Assert.ThrowsAsync<ArgumentException>(() => FindAsync<IntKey>(context, "77"))).Message);
            }
        }

        [Fact]
        public virtual async Task Throws_for_bad_type_for_composite_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                    (await Assert.ThrowsAsync<ArgumentException>(() => FindAsync<CompositeKey>(context, 77, 88))).Message);
            }
        }

        [Fact]
        public virtual async Task Throws_for_bad_entity_type_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.InvalidSetType(nameof(Random)),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => FindAsync<Random>(context, 77))).Message);
            }
        }

        protected class BaseType
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Foo { get; set; }
        }

        protected class DerivedType : BaseType
        {
            public string Boo { get; set; }
        }

        protected class IntKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Foo { get; set; }
        }

        protected class NullableIntKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int? Id { get; set; }

            public string Foo { get; set; }
        }

        protected class StringKey
        {
            public string Id { get; set; }

            public string Foo { get; set; }
        }

        protected class CompositeKey
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }
            public string Foo { get; set; }
        }

        protected class ShadowKey
        {
            public string Foo { get; set; }
        }

        protected DbContext CreateContext() => Fixture.CreateContext();

        public abstract class FindFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "FindTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<IntKey>();
                modelBuilder.Entity<NullableIntKey>();
                modelBuilder.Entity<StringKey>();
                modelBuilder.Entity<CompositeKey>().HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2
                    });
                modelBuilder.Entity<BaseType>();
                modelBuilder.Entity<DerivedType>();
                modelBuilder.Entity<ShadowKey>().Property(typeof(int), "Id").ValueGeneratedNever();
            }

            protected override void Seed(PoolableDbContext context)
            {
                context.AddRange(
                    new IntKey
                    {
                        Id = 77,
                        Foo = "Smokey"
                    },
                    new NullableIntKey
                    {
                        Id = 77,
                        Foo = "Smokey"
                    },
                    new StringKey
                    {
                        Id = "Cat",
                        Foo = "Alice"
                    },
                    new CompositeKey
                    {
                        Id1 = 77,
                        Id2 = "Dog",
                        Foo = "Olive"
                    },
                    new BaseType
                    {
                        Id = 77,
                        Foo = "Baxter"
                    },
                    new DerivedType
                    {
                        Id = 78,
                        Foo = "Strawberry",
                        Boo = "Cheesecake"
                    });

                var entry = context.Entry(
                    new ShadowKey
                    {
                        Foo = "Clippy"
                    });
                entry.Property("Id").CurrentValue = 77;
                entry.State = EntityState.Added;

                context.SaveChanges();
            }
        }
    }
}
