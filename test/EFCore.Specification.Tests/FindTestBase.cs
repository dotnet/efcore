// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class FindTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : FindTestBase<TFixture>.FindFixtureBase
    {
        protected FindTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected abstract TestFinder Finder { get; }

        [ConditionalFact]
        public virtual void Find_int_key_tracked()
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new IntKey { Id = 88 }).Entity;

            Assert.Same(entity, Finder.Find<IntKey>(context, 88));
        }

        [ConditionalFact]
        public virtual void Find_int_key_from_store()
        {
            using var context = CreateContext();
            Assert.Equal("Smokey", Finder.Find<IntKey>(context, 77).Foo);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_int_key_not_in_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<IntKey>(context, 99));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_key_tracked()
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new NullableIntKey { Id = 88 }).Entity;

            Assert.Same(entity, Finder.Find<NullableIntKey>(context, 88));
        }

        [ConditionalFact]
        public virtual void Find_nullable_int_key_from_store()
        {
            using var context = CreateContext();
            Assert.Equal("Smokey", Finder.Find<NullableIntKey>(context, 77).Foo);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_nullable_int_key_not_in_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<NullableIntKey>(context, 99));
        }

        [ConditionalFact]
        public virtual void Find_string_key_tracked()
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new StringKey { Id = "Rabbit" }).Entity;

            Assert.Same(entity, Finder.Find<StringKey>(context, "Rabbit"));
        }

        [ConditionalFact]
        public virtual void Find_string_key_from_store()
        {
            using var context = CreateContext();
            Assert.Equal("Alice", Finder.Find<StringKey>(context, "Cat").Foo);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_string_key_not_in_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<StringKey>(context, "Fox"));
        }

        [ConditionalFact]
        public virtual void Find_composite_key_tracked()
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new CompositeKey { Id1 = 88, Id2 = "Rabbit" }).Entity;

            Assert.Same(entity, Finder.Find<CompositeKey>(context, 88, "Rabbit"));
        }

        [ConditionalFact]
        public virtual void Find_composite_key_from_store()
        {
            using var context = CreateContext();
            Assert.Equal("Olive", Finder.Find<CompositeKey>(context, 77, "Dog").Foo);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_composite_key_not_in_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<CompositeKey>(context, 77, "Fox"));
        }

        [ConditionalFact]
        public virtual void Find_base_type_tracked()
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new BaseType { Id = 88 }).Entity;

            Assert.Same(entity, Finder.Find<BaseType>(context, 88));
        }

        [ConditionalFact]
        public virtual void Find_base_type_from_store()
        {
            using var context = CreateContext();
            Assert.Equal("Baxter", Finder.Find<BaseType>(context, 77).Foo);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_base_type_not_in_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<BaseType>(context, 99));
        }

        [ConditionalFact]
        public virtual void Find_derived_type_tracked()
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, Finder.Find<DerivedType>(context, 88));
        }

        [ConditionalFact]
        public virtual void Find_derived_type_from_store()
        {
            using var context = CreateContext();
            var derivedType = Finder.Find<DerivedType>(context, 78);
            Assert.Equal("Strawberry", derivedType.Foo);
            Assert.Equal("Cheesecake", derivedType.Boo);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_derived_type_not_in_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<DerivedType>(context, 99));
        }

        [ConditionalFact]
        public virtual void Find_base_type_using_derived_set_tracked()
        {
            using var context = CreateContext();
            context.Attach(
                new BaseType { Id = 88 });

            Assert.Null(Finder.Find<DerivedType>(context, 88));
        }

        [ConditionalFact]
        public virtual void Find_base_type_using_derived_set_from_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<DerivedType>(context, 77));
        }

        [ConditionalFact]
        public virtual void Find_derived_type_using_base_set_tracked()
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, Finder.Find<BaseType>(context, 88));
        }

        [ConditionalFact]
        public virtual void Find_derived_using_base_set_type_from_store()
        {
            using var context = CreateContext();
            var derivedType = Finder.Find<BaseType>(context, 78);
            Assert.Equal("Strawberry", derivedType.Foo);
            Assert.Equal("Cheesecake", ((DerivedType)derivedType).Boo);
        }

        [ConditionalFact]
        public virtual void Find_shadow_key_tracked()
        {
            using var context = CreateContext();
            var entry = context.Entry(new ShadowKey());
            entry.Property("Id").CurrentValue = 88;
            entry.State = EntityState.Unchanged;

            Assert.Same(entry.Entity, Finder.Find<ShadowKey>(context, 88));
        }

        [ConditionalFact]
        public virtual void Find_shadow_key_from_store()
        {
            using var context = CreateContext();
            Assert.Equal("Clippy", Finder.Find<ShadowKey>(context, 77).Foo);
        }

        [ConditionalFact]
        public virtual void Returns_null_for_shadow_key_not_in_store()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<ShadowKey>(context, 99));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_key_values_array()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<CompositeKey>(context, null));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_key()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<IntKey>(context, [null]));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_nullable_key()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<NullableIntKey>(context, [null]));
        }

        [ConditionalFact]
        public virtual void Returns_null_for_null_in_composite_key()
        {
            using var context = CreateContext();
            Assert.Null(Finder.Find<CompositeKey>(context, 77, null));
        }

        [ConditionalFact]
        public virtual void Throws_for_multiple_values_passed_for_simple_key()
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.FindNotCompositeKey("IntKey", 2),
                Assert.Throws<ArgumentException>(() => Finder.Find<IntKey>(context, 77, 88)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_wrong_number_of_values_for_composite_key()
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.FindValueCountMismatch("CompositeKey", 2, 1),
                Assert.Throws<ArgumentException>(() => Finder.Find<CompositeKey>(context, 77)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_type_for_simple_key()
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                Assert.Throws<ArgumentException>(() => Finder.Find<IntKey>(context, "77")).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_type_for_composite_key()
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                Assert.Throws<ArgumentException>(() => Finder.Find<CompositeKey>(context, 77, 88)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_entity_type()
        {
            using var context = CreateContext();

            Assert.Equal(
                CoreStrings.InvalidSetType(nameof(Random)),
                Assert.Throws<InvalidOperationException>(() => Finder.Find<Random>(context, 77)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_entity_type_with_different_namespace()
        {
            using var context = CreateContext();

            Assert.Equal(
                CoreStrings.InvalidSetSameTypeWithDifferentNamespace(
                    typeof(DifferentNamespace.ShadowKey).DisplayName(), typeof(ShadowKey).DisplayName()),
                Assert.Throws<InvalidOperationException>(() => Finder.Find<DifferentNamespace.ShadowKey>(context, 77)).Message);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_int_key_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new IntKey { Id = 88 }).Entity;

            var valueTask = Finder.FindAsync<IntKey>(cancellationType, context, [88]);

            Assert.True(valueTask.IsCompleted);
            Assert.Same(entity, await valueTask);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_int_key_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal("Smokey", (await Finder.FindAsync<IntKey>(cancellationType, context, [77])).Foo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_int_key_not_in_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<IntKey>(cancellationType, context, [99]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_nullable_int_key_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new NullableIntKey { Id = 88 }).Entity;

            Assert.Same(entity, await Finder.FindAsync<NullableIntKey>(cancellationType, context, [88]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_nullable_int_key_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal("Smokey", (await Finder.FindAsync<NullableIntKey>(cancellationType, context, [77])).Foo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_nullable_int_key_not_in_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<NullableIntKey>(cancellationType, context, [99]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_string_key_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new StringKey { Id = "Rabbit" }).Entity;

            Assert.Same(entity, await Finder.FindAsync<StringKey>(cancellationType, context, ["Rabbit"]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_string_key_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal("Alice", (await Finder.FindAsync<StringKey>(cancellationType, context, ["Cat"])).Foo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_string_key_not_in_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<StringKey>(cancellationType, context, ["Fox"]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_composite_key_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new CompositeKey { Id1 = 88, Id2 = "Rabbit" }).Entity;

            Assert.Same(entity, await Finder.FindAsync<CompositeKey>(cancellationType, context, [88, "Rabbit"]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_composite_key_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal("Olive", (await Finder.FindAsync<CompositeKey>(cancellationType, context, [77, "Dog"])).Foo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_composite_key_not_in_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<CompositeKey>(cancellationType, context, [77, "Fox"]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_base_type_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new BaseType { Id = 88 }).Entity;

            Assert.Same(entity, await Finder.FindAsync<BaseType>(cancellationType, context, [88]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_base_type_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal("Baxter", (await Finder.FindAsync<BaseType>(cancellationType, context, [77])).Foo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_base_type_not_in_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<BaseType>(cancellationType, context, [99]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_derived_type_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, await Finder.FindAsync<DerivedType>(cancellationType, context, [88]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_derived_type_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var derivedType = await Finder.FindAsync<DerivedType>(cancellationType, context, [78]);
            Assert.Equal("Strawberry", derivedType.Foo);
            Assert.Equal("Cheesecake", derivedType.Boo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_derived_type_not_in_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<DerivedType>(cancellationType, context, [99]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_base_type_using_derived_set_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            context.Attach(
                new BaseType { Id = 88 });

            Assert.Null(await Finder.FindAsync<DerivedType>(cancellationType, context, [88]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_base_type_using_derived_set_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<DerivedType>(cancellationType, context, [77]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_derived_type_using_base_set_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entity = context.Attach(
                new DerivedType { Id = 88 }).Entity;

            Assert.Same(entity, await Finder.FindAsync<BaseType>(cancellationType, context, [88]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_derived_using_base_set_type_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var derivedType = await Finder.FindAsync<BaseType>(cancellationType, context, [78]);
            Assert.Equal("Strawberry", derivedType.Foo);
            Assert.Equal("Cheesecake", ((DerivedType)derivedType).Boo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_shadow_key_tracked_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            var entry = context.Entry(new ShadowKey());
            entry.Property("Id").CurrentValue = 88;
            entry.State = EntityState.Unchanged;

            Assert.Same(entry.Entity, await Finder.FindAsync<ShadowKey>(cancellationType, context, [88]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Find_shadow_key_from_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal("Clippy", (await Finder.FindAsync<ShadowKey>(cancellationType, context, [77])).Foo);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_shadow_key_not_in_store_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<ShadowKey>(cancellationType, context, [99]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_null_key_values_array_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<CompositeKey>(cancellationType, context, null));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_null_key_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<IntKey>(cancellationType, context, [null]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Returns_null_for_null_in_composite_key_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Null(await Finder.FindAsync<CompositeKey>(cancellationType, context, [77, null]));
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Throws_for_multiple_values_passed_for_simple_key_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.FindNotCompositeKey("IntKey", cancellationType == CancellationType.Wrong ? 3 : 2),
                (await Assert.ThrowsAsync<ArgumentException>(
                    () => Finder.FindAsync<IntKey>(cancellationType, context, [77, 88]).AsTask())).Message);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Throws_for_wrong_number_of_values_for_composite_key_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal(
                cancellationType == CancellationType.Wrong
                    ? CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "CancellationToken", "string")
                    : CoreStrings.FindValueCountMismatch("CompositeKey", 2, 1),
                (await Assert.ThrowsAsync<ArgumentException>(
                    () => Finder.FindAsync<CompositeKey>(cancellationType, context, [77]).AsTask())).Message);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Throws_for_bad_type_for_simple_key_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(0, "IntKey", "string", "int"),
                (await Assert.ThrowsAsync<ArgumentException>(
                    () => Finder.FindAsync<IntKey>(cancellationType, context, ["77"]).AsTask())).Message);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Throws_for_bad_type_for_composite_key_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.FindValueTypeMismatch(1, "CompositeKey", "int", "string"),
                (await Assert.ThrowsAsync<ArgumentException>(
                    () => Finder.FindAsync<CompositeKey>(cancellationType, context, [77, 78]).AsTask())).Message);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Throws_for_bad_entity_type_async(CancellationType cancellationType)
        {
            using var context = CreateContext();
            Assert.Equal(
                CoreStrings.InvalidSetType(nameof(Random)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Finder.FindAsync<Random>(cancellationType, context, [77]).AsTask())).Message);
        }

        [ConditionalTheory]
        [InlineData((int)CancellationType.Right)]
        [InlineData((int)CancellationType.Wrong)]
        [InlineData((int)CancellationType.None)]
        public virtual async Task Throws_for_bad_entity_type_with_different_namespace_async(CancellationType cancellationType)
        {
            using var context = CreateContext();

            Assert.Equal(
                CoreStrings.InvalidSetSameTypeWithDifferentNamespace(
                    typeof(DifferentNamespace.ShadowKey).DisplayName(), typeof(ShadowKey).DisplayName()),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Finder.FindAsync<DifferentNamespace.ShadowKey>(cancellationType, context, [77]).AsTask()))
                .Message);
        }

        public enum CancellationType
        {
            Right,
            Wrong,
            None
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

        protected DbContext CreateContext()
            => Fixture.CreateContext();

        public abstract class FindFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName
                => "FindTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<IntKey>();
                modelBuilder.Entity<NullableIntKey>();
                modelBuilder.Entity<StringKey>();
                modelBuilder.Entity<CompositeKey>().HasKey(
                    e => new { e.Id1, e.Id2 });
                modelBuilder.Entity<BaseType>();
                modelBuilder.Entity<DerivedType>();
                modelBuilder.Entity<ShadowKey>().Property(typeof(int), "Id").ValueGeneratedNever();
            }

            protected override Task SeedAsync(PoolableDbContext context)
            {
                context.AddRange(
                    new IntKey { Id = 77, Foo = "Smokey" },
                    new NullableIntKey { Id = 77, Foo = "Smokey" },
                    new StringKey { Id = "Cat", Foo = "Alice" },
                    new CompositeKey
                    {
                        Id1 = 77,
                        Id2 = "Dog",
                        Foo = "Olive"
                    },
                    new BaseType { Id = 77, Foo = "Baxter" },
                    new DerivedType
                    {
                        Id = 78,
                        Foo = "Strawberry",
                        Boo = "Cheesecake"
                    });

                var entry = context.Entry(
                    new ShadowKey { Foo = "Clippy" });
                entry.Property("Id").CurrentValue = 77;
                entry.State = EntityState.Added;

                return context.SaveChangesAsync();
            }
        }

        public abstract class TestFinder
        {
            public abstract TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                where TEntity : class;

            public abstract ValueTask<TEntity> FindAsync<TEntity>(
                CancellationType cancellationType,
                DbContext context,
                object[] keyValues,
                CancellationToken cancellationToken = default)
                where TEntity : class;
        }

        public class FindViaSetFinder : TestFinder
        {
            public override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => context.Set<TEntity>().Find(keyValues);

            public override ValueTask<TEntity> FindAsync<TEntity>(
                CancellationType cancellationType,
                DbContext context,
                object[] keyValues,
                CancellationToken cancellationToken = default)
                => cancellationType switch
                {
                    CancellationType.Right => context.Set<TEntity>().FindAsync(keyValues, cancellationToken: cancellationToken),
                    CancellationType.Wrong => context.Set<TEntity>()
                        .FindAsync(keyValues?.Concat(new object[] { cancellationToken }).ToArray()),
                    CancellationType.None => context.Set<TEntity>().FindAsync(keyValues),
                    _ => throw new ArgumentOutOfRangeException(nameof(cancellationType), cancellationType, null)
                };
        }

        public class FindViaContextFinder : TestFinder
        {
            public override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => (TEntity)context.Find(typeof(TEntity), keyValues);

            public override async ValueTask<TEntity> FindAsync<TEntity>(
                CancellationType cancellationType,
                DbContext context,
                object[] keyValues,
                CancellationToken cancellationToken = default)
                => cancellationType switch
                {
                    CancellationType.Right => (TEntity)await context.FindAsync(
                        typeof(TEntity), keyValues, cancellationToken: cancellationToken),
                    CancellationType.Wrong => (TEntity)await context.FindAsync(
                        typeof(TEntity), keyValues?.Concat(new object[] { cancellationToken }).ToArray()),
                    CancellationType.None => (TEntity)await context.FindAsync(typeof(TEntity), keyValues),
                    _ => throw new ArgumentOutOfRangeException(nameof(cancellationType), cancellationType, null)
                };
        }

        public class FindViaNonGenericContextFinder : TestFinder
        {
            public override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
                => context.Find<TEntity>(keyValues);

            public override ValueTask<TEntity> FindAsync<TEntity>(
                CancellationType cancellationType,
                DbContext context,
                object[] keyValues,
                CancellationToken cancellationToken = default)
                => cancellationType switch
                {
                    CancellationType.Right => context.FindAsync<TEntity>(keyValues, cancellationToken: cancellationToken),
                    CancellationType.Wrong => context.FindAsync<TEntity>(
                        keyValues?.Concat(new object[] { cancellationToken }).ToArray()),
                    CancellationType.None => context.FindAsync<TEntity>(keyValues),
                    _ => throw new ArgumentOutOfRangeException(nameof(cancellationType), cancellationType, null)
                };
        }
    }
}

namespace Microsoft.EntityFrameworkCore.DifferentNamespace
{
    internal class ShadowKey
    {
        public string Foo { get; set; }
    }
}
