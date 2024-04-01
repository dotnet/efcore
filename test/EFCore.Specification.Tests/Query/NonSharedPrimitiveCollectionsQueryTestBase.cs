// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

using static Expression;

public abstract class NonSharedPrimitiveCollectionsQueryTestBase : NonSharedModelTestBase
{
    #region Support for specific element types

    [ConditionalFact]
    public virtual Task Array_of_string()
        => TestArray("a", "b");

    [ConditionalFact]
    public virtual Task Array_of_int()
        => TestArray(1, 2);

    [ConditionalFact]
    public virtual Task Array_of_long()
        => TestArray(1L, 2L);

    [ConditionalFact]
    public virtual Task Array_of_short()
        => TestArray((short)1, (short)2);

    [ConditionalFact]
    public virtual Task Array_of_byte()
        => TestArray((byte)1, (byte)2);

    [ConditionalFact]
    public virtual Task Array_of_double()
        => TestArray(1d, 2d);

    [ConditionalFact]
    public virtual Task Array_of_float()
        => TestArray(1f, 2f);

    [ConditionalFact]
    public virtual Task Array_of_decimal()
        => TestArray(1m, 2m);

    [ConditionalFact]
    public virtual Task Array_of_DateTime()
        => TestArray(new DateTime(2023, 1, 1, 12, 30, 0), new DateTime(2023, 1, 2, 12, 30, 0));

    [ConditionalFact]
    public virtual Task Array_of_DateTime_with_milliseconds()
        => TestArray(new DateTime(2023, 1, 1, 12, 30, 0, 123), new DateTime(2023, 1, 1, 12, 30, 0, 124));

    [ConditionalFact]
    public virtual Task Array_of_DateTime_with_microseconds()
        => TestArray(new DateTime(2023, 1, 1, 12, 30, 0, 123, 456), new DateTime(2023, 1, 1, 12, 30, 0, 123, 457));

    [ConditionalFact]
    public virtual Task Array_of_DateOnly()
        => TestArray(new DateOnly(2023, 1, 1), new DateOnly(2023, 1, 2));

    [ConditionalFact]
    public virtual Task Array_of_TimeOnly()
        => TestArray(new TimeOnly(12, 30, 0), new TimeOnly(12, 30, 1));

    [ConditionalFact]
    public virtual Task Array_of_TimeOnly_with_milliseconds()
        => TestArray(new TimeOnly(12, 30, 0, 123), new TimeOnly(12, 30, 0, 124));

    [ConditionalFact]
    public virtual Task Array_of_TimeOnly_with_microseconds()
        => TestArray(new TimeOnly(12, 30, 0, 123, 456), new TimeOnly(12, 30, 0, 124, 457));

    [ConditionalFact]
    public virtual Task Array_of_DateTimeOffset()
        => TestArray(
            new DateTimeOffset(2023, 1, 1, 12, 30, 0, TimeSpan.FromHours(2)),
            new DateTimeOffset(2023, 1, 2, 12, 30, 0, TimeSpan.FromHours(2)));

    [ConditionalFact]
    public virtual Task Array_of_bool()
        => TestArray(true, false);

    [ConditionalFact]
    public virtual Task Array_of_Guid()
        => TestArray(
            new Guid("dc8c903d-d655-4144-a0fd-358099d40ae1"),
            new Guid("008719a5-1999-4798-9cf3-92a78ffa94a2"));

    [ConditionalFact]
    public virtual Task Array_of_byte_array()
        => TestArray([1, 2], new byte[] { 3, 4 });

    [ConditionalFact]
    public virtual Task Array_of_enum()
        => TestArray(MyEnum.Label1, MyEnum.Label2);

    private enum MyEnum { Label1, Label2 }

    [ConditionalFact]
    public virtual async Task Array_of_array_is_not_supported()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => TestArray([1, 2, 3], new[] { 4, 5, 6 }));
        Assert.Equal(CoreStrings.PropertyNotMapped("int[][]", "TestEntity", "SomeArray"), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Multidimensional_array_is_not_supported()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<TestContext>(
                onModelCreating: mb => mb.Entity<TestEntity>().Property(typeof(int[,]), "MultidimensionalArray")));
        Assert.Equal(CoreStrings.PropertyNotMapped("int[,]", "TestEntity", "MultidimensionalArray"), exception.Message);
    }

    #endregion Support for specific element types

    [ConditionalFact]
    public virtual async Task Column_with_custom_converter()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>()
                .Property(m => m.Ints)
                .HasConversion(
                    i => string.Join(",", i!),
                    s => s.Split(",", StringSplitOptions.None).Select(int.Parse).ToArray(),
                    new ValueComparer<int[]>(favorStructuralComparisons: true)),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1, Ints = [1, 2, 3] },
                    new TestEntity { Id = 2, Ints = [1, 2, 4] });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ints = new[] { 1, 2, 3 };
        var result = await context.Set<TestEntity>().SingleAsync(m => m.Ints == ints);
        Assert.Equal(1, result.Id);

        // Custom converters allow reading/writing, but not querying, as we have no idea about the internal representation
        await AssertTranslationFailed(() => context.Set<TestEntity>().SingleAsync(m => m.Ints!.Length == 2));
    }

    [ConditionalFact(
        Skip =
            "Currently fails because we don't use the element mapping when serializing to JSON, but just do JsonSerializer.Serialize, #30677")]
    public virtual async Task Parameter_with_inferred_value_converter()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb
                .Entity<TestEntity>()
                .Property<IntWrapper>("PropertyWithValueConverter")
                .HasConversion(w => w.Value, i => new IntWrapper(i)),
            seed: context =>
            {
                var entry1 = context.Add(new TestEntity { Id = 1 });
                entry1.Property("PropertyWithValueConverter").CurrentValue = new IntWrapper(8);
                var entry2 = context.Add(new TestEntity { Id = 2 });
                entry2.Property("PropertyWithValueConverter").CurrentValue = new IntWrapper(9);
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var ints = new IntWrapper[] { new(1), new(8) };
        var result = await context.Set<TestEntity>()
            .SingleAsync(m => ints.Count(i => i == EF.Property<IntWrapper>(m, "PropertyWithValueConverter")) == 1);
        Assert.Equal(1, result.Id);
    }

    [ConditionalFact]
    public virtual async Task Constant_with_inferred_value_converter()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb
                .Entity<TestEntity>()
                .Property<IntWrapper>("PropertyWithValueConverter")
                .HasConversion(w => w.Value, i => new IntWrapper(i)),
            seed: context =>
            {
                var entry1 = context.Add(new TestEntity { Id = 1 });
                entry1.Property("PropertyWithValueConverter").CurrentValue = new IntWrapper(8);
                var entry2 = context.Add(new TestEntity { Id = 2 });
                entry2.Property("PropertyWithValueConverter").CurrentValue = new IntWrapper(9);
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var result = await context.Set<TestEntity>()
            .SingleAsync(
                m => new IntWrapper[] { new(1), new(8) }.Count(i => i == EF.Property<IntWrapper>(m, "PropertyWithValueConverter")) == 1);
        Assert.Equal(1, result.Id);
    }

    private class IntWrapper(int value)
    {
        public int Value { get; } = value;
    }

    [ConditionalFact]
    public virtual async Task Inline_collection_in_query_filter()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>().HasQueryFilter(t => new[] { 1, 2, 3 }.Count(i => i > t.Id) == 1),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1 },
                    new TestEntity { Id = 2 });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var result = await context.Set<TestEntity>().SingleAsync();
        Assert.Equal(2, result.Id);
    }

    /// <summary>
    ///     A utility that allows easy testing of querying out arbitrary element types from a primitive collection, provided two distinct
    ///     element values.
    /// </summary>
    protected async Task TestArray<TElement>(
        TElement value1,
        TElement value2,
        Action<ModelBuilder>? onModelCreating = null)
    {
        var arrayClrType = typeof(TElement).MakeArrayType();

        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: onModelCreating ?? (mb => mb.Entity<TestEntity>().Property(arrayClrType, "SomeArray")),
            seed: context =>
            {
                var instance1 = new TestEntity { Id = 1 };
                context.Add(instance1);
                var array1 = new TElement[2];
                array1.SetValue(value1, 0);
                array1.SetValue(value1, 1);
                context.Entry(instance1).Property("SomeArray").CurrentValue = array1;

                var instance2 = new TestEntity { Id = 2 };
                context.Add(instance2);
                var array2 = new TElement[2];
                array2.SetValue(value1, 0);
                array2.SetValue(value2, 1);
                context.Entry(instance2).Property("SomeArray").CurrentValue = array2;

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var entityParam = Parameter(typeof(TestEntity), "m");
        var efPropertyCall = Call(
            typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)!.MakeGenericMethod(arrayClrType),
            entityParam,
            Constant("SomeArray"));

        var elementParam = Parameter(typeof(TElement), "a");
        var predicate = Lambda<Func<TestEntity, bool>>(
            Equal(
                Call(
                    EnumerableMethods.CountWithPredicate.MakeGenericMethod(typeof(TElement)),
                    efPropertyCall,
                    Lambda(
                        Equal(elementParam, Constant(value1)),
                        elementParam)),
                Constant(2)),
            entityParam);

        // context.Set<TestEntity>().SingleAsync(m => EF.Property<int[]>(m, "SomeArray").Count(a => a == <value1>) == 2)
        var result = await context.Set<TestEntity>().SingleAsync(predicate);
        Assert.Equal(1, result.Id);
    }

    protected class TestContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TestEntity>().Property(e => e.Id).ValueGeneratedNever();
    }

    protected class TestEntity
    {
        public int Id { get; set; }
        public int[]? Ints { get; set; }
    }

    protected override string StoreName
        => "NonSharedPrimitiveCollectionsTest";

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
