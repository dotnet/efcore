// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class FindTestBase<TFixture> : IClassFixture<TFixture>, IDisposable
        where TFixture : FindTestBase<TFixture>.FindFixtureBase, new()
    {
        protected FindTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.Initialize();
        }

        [Fact]
        public virtual void Find_int_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new IntKey { Id = 88 }).Entity;

                Assert.Same(entity, context.IntKeys.Find(88));
            }
        }

        [Fact]
        public virtual void Find_int_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Smokey", context.IntKeys.Find(77).Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_int_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.IntKeys.Find(99));
            }
        }

        [Fact]
        public virtual void Find_string_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new StringKey { Id = "Rabbit" }).Entity;

                Assert.Same(entity, context.StringKeys.Find("Rabbit"));
            }
        }

        [Fact]
        public virtual void Find_string_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Alice", context.StringKeys.Find("Cat").Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_string_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.StringKeys.Find("Fox"));
            }
        }

        [Fact]
        public virtual void Find_composite_key_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new CompositeKey { Id1 = 88, Id2 = "Rabbit" }).Entity;

                Assert.Same(entity, context.CompositeKeys.Find(88, "Rabbit"));
            }
        }

        [Fact]
        public virtual void Find_composite_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Olive", context.CompositeKeys.Find(77, "Dog").Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_composite_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.CompositeKeys.Find(77, "Fox"));
            }
        }

        [Fact]
        public virtual void Find_base_type_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new BaseType { Id = 88 }).Entity;

                Assert.Same(entity, context.BaseTypes.Find(88));
            }
        }

        [Fact]
        public virtual void Find_base_type_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Baxter", context.BaseTypes.Find(77).Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_base_type_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.BaseTypes.Find(99));
            }
        }

        [Fact]
        public virtual void Find_derived_type_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

                Assert.Same(entity, context.DerivedTypes.Find(88));
            }
        }

        [Fact]
        public virtual void Find_derived_type_from_store()
        {
            using (var context = CreateContext())
            {
                var derivedType = context.DerivedTypes.Find(78);
                Assert.Equal("Strawberry", derivedType.Foo);
                Assert.Equal("Cheesecake", derivedType.Boo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_derived_type_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.DerivedTypes.Find(99));
            }
        }

        [Fact]
        public virtual void Find_base_type_using_derived_set_tracked()
        {
            using (var context = CreateContext())
            {
                context.Attach(new BaseType { Id = 88 });

                Assert.Null(context.DerivedTypes.Find(88));
            }
        }

        [Fact]
        public virtual void Find_base_type_using_derived_set_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.DerivedTypes.Find(77));
            }
        }

        [Fact]
        public virtual void Find_derived_type_using_base_set_tracked()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

                Assert.Same(entity, context.BaseTypes.Find(88));
            }
        }

        [Fact]
        public virtual void Find_derived_using_base_set_type_from_store()
        {
            using (var context = CreateContext())
            {
                var derivedType = context.BaseTypes.Find(78);
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

                Assert.Same(entry.Entity, context.ShadowKeys.Find(88));
            }
        }

        [Fact]
        public virtual void Find_shadow_key_from_store()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Clippy", context.ShadowKeys.Find(77).Foo);
            }
        }

        [Fact]
        public virtual void Returns_null_for_shadow_key_not_in_store()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.ShadowKeys.Find(99));
            }
        }

        [Fact]
        public virtual void Throws_for_null_key_values_array()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("keyValues",
                    Assert.Throws<ArgumentNullException>(() => context.CompositeKeys.Find(null)).ParamName);
            }
        }

        [Fact]
        public virtual void Throws_for_null_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("keyValues",
                    Assert.Throws<ArgumentNullException>(() => context.IntKeys.Find(new object[] { null })).ParamName);
            }
        }

        [Fact]
        public virtual void Throws_for_null_in_composite_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("keyValues",
                    Assert.Throws<ArgumentNullException>(() => context.CompositeKeys.Find(77, null)).ParamName);
            }
        }

        [Fact]
        public virtual void Throws_for_multiple_values_passed_for_simple_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindNotCompositeKey("IntKey", 2),
                    Assert.Throws<ArgumentException>(() => context.IntKeys.Find(77, 88)).Message);
            }
        }

        [Fact]
        public virtual void Throws_for_wrong_number_of_values_for_composite_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindValueCountMismatch("CompositeKey", 2, 1),
                    Assert.Throws<ArgumentException>(() => context.CompositeKeys.Find(77)).Message);
            }
        }

        [Fact]
        public virtual void Throws_for_bad_type_for_simple_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                    Assert.Throws<ArgumentException>(() => context.IntKeys.Find("77")).Message);
            }
        }

        [Fact]
        public virtual void Throws_for_bad_type_for_composite_key()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                    Assert.Throws<ArgumentException>(() => context.CompositeKeys.Find(77, 88)).Message);
            }
        }

        [Fact]
        public virtual async Task Find_int_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new IntKey { Id = 88 }).Entity;

                Assert.Same(entity, await context.IntKeys.FindAsync(88));
            }
        }

        [Fact]
        public virtual async Task Find_int_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Smokey", (await context.IntKeys.FindAsync(77)).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_int_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await context.IntKeys.FindAsync(99));
            }
        }

        [Fact]
        public virtual async Task Find_string_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new StringKey { Id = "Rabbit" }).Entity;

                Assert.Same(entity, await context.StringKeys.FindAsync("Rabbit"));
            }
        }

        [Fact]
        public virtual async Task Find_string_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Alice", (await context.StringKeys.FindAsync("Cat")).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_string_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await context.StringKeys.FindAsync("Fox"));
            }
        }

        [Fact]
        public virtual async Task Find_composite_key_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new CompositeKey { Id1 = 88, Id2 = "Rabbit" }).Entity;

                Assert.Same(entity, await context.CompositeKeys.FindAsync(88, "Rabbit"));
            }
        }

        [Fact]
        public virtual async Task Find_composite_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Olive", (await context.CompositeKeys.FindAsync(77, "Dog")).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_composite_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await context.CompositeKeys.FindAsync(77, "Fox"));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new BaseType { Id = 88 }).Entity;

                Assert.Same(entity, await context.BaseTypes.FindAsync(88));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Baxter", (await context.BaseTypes.FindAsync(77)).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_base_type_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await context.BaseTypes.FindAsync(99));
            }
        }

        [Fact]
        public virtual async Task Find_derived_type_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

                Assert.Same(entity, await context.DerivedTypes.FindAsync(88));
            }
        }

        [Fact]
        public virtual async Task Find_derived_type_from_store_async()
        {
            using (var context = CreateContext())
            {
                var derivedType = await context.DerivedTypes.FindAsync(78);
                Assert.Equal("Strawberry", derivedType.Foo);
                Assert.Equal("Cheesecake", derivedType.Boo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_derived_type_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await context.DerivedTypes.FindAsync(99));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_using_derived_set_tracked_async()
        {
            using (var context = CreateContext())
            {
                context.Attach(new BaseType { Id = 88 });

                Assert.Null(await context.DerivedTypes.FindAsync(88));
            }
        }

        [Fact]
        public virtual async Task Find_base_type_using_derived_set_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await context.DerivedTypes.FindAsync(77));
            }
        }

        [Fact]
        public virtual async Task Find_derived_type_using_base_set_tracked_async()
        {
            using (var context = CreateContext())
            {
                var entity = context.Attach(new DerivedType { Id = 88 }).Entity;

                Assert.Same(entity, await context.BaseTypes.FindAsync(88));
            }
        }

        [Fact]
        public virtual async Task Find_derived_using_base_set_type_from_store_async()
        {
            using (var context = CreateContext())
            {
                var derivedType = await context.BaseTypes.FindAsync(78);
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

                Assert.Same(entry.Entity, await context.ShadowKeys.FindAsync(88));
            }
        }

        [Fact]
        public virtual async Task Find_shadow_key_from_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("Clippy", (await context.ShadowKeys.FindAsync(77)).Foo);
            }
        }

        [Fact]
        public virtual async Task Returns_null_for_shadow_key_not_in_store_async()
        {
            using (var context = CreateContext())
            {
                Assert.Null(await context.ShadowKeys.FindAsync(99));
            }
        }

        [Fact]
        public virtual async Task Throws_for_null_key_values_array_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("keyValues",
                    (await Assert.ThrowsAsync<ArgumentNullException>(() => context.CompositeKeys.FindAsync(null))).ParamName);
            }
        }

        [Fact]
        public virtual async Task Throws_for_null_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("keyValues",
                    (await Assert.ThrowsAsync<ArgumentNullException>(() => context.IntKeys.FindAsync(new object[] { null }))).ParamName);
            }
        }

        [Fact]
        public virtual async Task Throws_for_null_in_composite_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal("keyValues",
                    (await Assert.ThrowsAsync<ArgumentNullException>(() => context.CompositeKeys.FindAsync(77, null))).ParamName);
            }
        }

        [Fact]
        public virtual async Task Throws_for_multiple_values_passed_for_simple_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindNotCompositeKey("IntKey", 2),
                    (await Assert.ThrowsAsync<ArgumentException>(() => context.IntKeys.FindAsync(77, 88))).Message);
            }
        }

        [Fact]
        public virtual async Task Throws_for_wrong_number_of_values_for_composite_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindValueCountMismatch("CompositeKey", 2, 1),
                    (await Assert.ThrowsAsync<ArgumentException>(() => context.CompositeKeys.FindAsync(77))).Message);
            }
        }

        [Fact]
        public virtual async Task Throws_for_bad_type_for_simple_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                    (await Assert.ThrowsAsync<ArgumentException>(() => context.IntKeys.FindAsync("77"))).Message);
            }
        }

        [Fact]
        public virtual async Task Throws_for_bad_type_for_composite_key_async()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                    (await Assert.ThrowsAsync<ArgumentException>(() => context.CompositeKeys.FindAsync(77, 88))).Message);
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

        protected class FindContext : DbContext
        {
            public FindContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<IntKey> IntKeys { get; set; }
            public DbSet<StringKey> StringKeys { get; set; }
            public DbSet<CompositeKey> CompositeKeys { get; set; }
            public DbSet<BaseType> BaseTypes { get; set; }
            public DbSet<DerivedType> DerivedTypes { get; set; }
            public DbSet<ShadowKey> ShadowKeys { get; set; }
        }

        protected FindContext CreateContext() => (FindContext)Fixture.CreateContext();

        protected TFixture Fixture { get; }

        public virtual void Dispose()
        {
        }

        public abstract class FindFixtureBase
        {
            private static readonly object _lock = new object();
            private static bool _initialized;

            public virtual void Initialize()
            {
                lock (_lock)
                {
                    if (!_initialized)
                    {
                        CreateTestStore();
                        _initialized = true;
                    }
                }
            }

            public abstract void CreateTestStore();

            public abstract DbContext CreateContext();

            protected virtual void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CompositeKey>().HasKey(e => new { e.Id1, e.Id2 });
                modelBuilder.Entity<ShadowKey>().Property(typeof(int), "Id").ValueGeneratedNever();
            }

            protected virtual void Seed(DbContext context)
            {
                context.AddRange(
                    new IntKey { Id = 77, Foo = "Smokey" },
                    new StringKey { Id = "Cat", Foo = "Alice" },
                    new CompositeKey { Id1 = 77, Id2 = "Dog", Foo = "Olive" },
                    new BaseType { Id = 77, Foo = "Baxter" },
                    new DerivedType { Id = 78, Foo = "Strawberry", Boo = "Cheesecake" });

                var entry = context.Entry(new ShadowKey { Foo = "Clippy" });
                entry.Property("Id").CurrentValue = 77;
                entry.State = EntityState.Added;

                context.SaveChanges();
            }
        }
    }
}
