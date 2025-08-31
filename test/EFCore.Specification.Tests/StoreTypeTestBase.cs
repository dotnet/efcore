// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#pragma warning disable IDE0001 // Simplify name

public abstract class StoreTypeTestBase() : NonSharedModelTestBase(fixture: null)
{
    #region Numeric

    [ConditionalFact]
    public virtual Task Byte() => TestType<byte>(byte.MinValue, byte.MinValue + 1);

    [ConditionalFact]
    public virtual Task Short() => TestType<short>(short.MinValue, short.MinValue + 1);

    [ConditionalFact]
    public virtual Task Int() => TestType<int>(int.MinValue, int.MinValue + 1);

    [ConditionalFact]
    public virtual Task Long() => TestType<long>(long.MinValue, long.MinValue + 1);

    [ConditionalFact]
    public virtual Task SByte() => TestType<sbyte>(sbyte.MinValue, sbyte.MinValue + 1);

    [ConditionalFact]
    public virtual Task UShort() => TestType<ushort>(ushort.MaxValue, ushort.MaxValue - 1);

    [ConditionalFact]
    public virtual Task UInt() => TestType<uint>(uint.MaxValue, uint.MaxValue - 1);

    [ConditionalFact]
    public virtual Task ULong() => TestType<ulong>(ulong.MaxValue, ulong.MaxValue - 1);

    [ConditionalFact]
    public virtual Task Decimal() => TestType<decimal>(30.5m, 30m);

    [ConditionalFact]
    public virtual Task Double() => TestType<double>(30.5d, 30d);

    [ConditionalFact]
    public virtual Task Float() => TestType<float>(30.5f, 30f);

    #endregion Numeric

    #region Date/time

    // Note that DateTime equality does not take the Kind into account.
    // Since databases don't support persisting Kind directly (it's a .NET thing), we leave
    // it that way and consider it enough if the same timestamp instant is read etc.

    [ConditionalFact]
    public virtual Task DateTime_Unspecified()
        => TestType(
            new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Unspecified),
            new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Unspecified));

    [ConditionalFact]
    public virtual Task DateTime_Utc()
        => TestType(
            new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Utc),
            new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Utc));

    [ConditionalFact]
    public virtual Task DateTime_Local()
        => TestType(
            new DateTime(2020, 1, 5, 12, 30, 45, DateTimeKind.Local),
            new DateTime(2022, 5, 3, 0, 0, 0, DateTimeKind.Local));

    [ConditionalFact]
    public virtual Task DateTimeOffset()
        => TestType(
            new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(2)),
            new DateTimeOffset(2020, 1, 5, 12, 30, 45, TimeSpan.FromHours(3)));

    [ConditionalFact]
    public virtual Task DateOnly()
        => TestType(new DateOnly(2020, 1, 5), new DateOnly(2022, 5, 3));

    [ConditionalFact]
    public virtual Task TimeOnly()
        => TestType(new TimeOnly(12, 30, 45), new TimeOnly(13, 45, 00));

    #endregion Date/time

    #region Miscellaneous

    [ConditionalFact]
    public virtual Task String() => TestType("foo", "bar");

    [ConditionalFact]
    public virtual Task Bool() => TestType<bool>(true, false);

    [ConditionalFact]
    public virtual Task Guid() => TestType(
        "8f7331d6-cde9-44fb-8611-81fff686f280",
        "ae192c36-9004-49b2-b785-8be10d169627");

    [ConditionalFact]
    public virtual Task ByteArray()
        => TestType<byte[]>(
            [1, 2, 3],
            [4, 5, 6],
            comparer: (a, b) => a.SequenceEqual(b));

    #endregion Miscellaneous

    protected virtual async Task TestType<T>(
        T value,
        T otherValue,
        Action<DbContextOptionsBuilder>? onConfiguring = null,
        Action<ModelBuilder>? onModelCreating = null,
        Func<T, T, bool>? comparer = null)
        where T : notnull
    {
        var contextFactory = await InitializeAsync<DbContext>(
            onModelCreating: modelBuilder =>
            {
                // Don't rely on database generated values, which aren't supported everywhere (e.g. Cosmos)
                modelBuilder.Entity<StoreTypeEntity<T>>().Property(e => e.Id).ValueGeneratedNever();

                OnModelCreating<T>(modelBuilder);

                if (onModelCreating is not null)
                {
                    onModelCreating(modelBuilder);
                }
            },
            onConfiguring: optionsBuilder =>
            {
                if (onConfiguring is not null)
                {
                    onConfiguring(optionsBuilder);
                }
            },
            seed: async context =>
            {
                context.Set<StoreTypeEntity<T>>().AddRange(
                    new()
                    {
                        Id = 1,
                        Value = value,
                        OtherValue = otherValue,
                        Container = new()
                        {
                            Value = value,
                            OtherValue = otherValue
                        }
                    },
                    new()
                    {
                        Id = 2,
                        Value = otherValue,
                        OtherValue = value,
                        Container = new()
                        {
                            Value = otherValue,
                            OtherValue = value
                        }
                    });
                await context.SaveChangesAsync();
            });

        comparer ??= (x, y) => x.Equals(y);

        await TestType(value, otherValue, contextFactory, comparer);
        await using var context = contextFactory.CreateContext();

    }

    protected virtual async Task TestType<T>(
        T value,
        T otherValue,
        ContextFactory<DbContext> contextFactory,
        Func<T, T, bool> comparer)
        where T : notnull
    {
        await TestEqualityInQuery(contextFactory, value, comparer);
    }

    protected virtual async Task TestEqualityInQuery<T>(ContextFactory<DbContext> contextFactory, T value, Func<T, T, bool> comparer)
        where T : notnull
    {
        await using var context = contextFactory.CreateContext();

        var result = await context.Set<StoreTypeEntity<T>>().Where(e => e.Value.Equals(value)).SingleAsync();

        Assert.Equal(value, result.Container.Value, comparer);
    }

    protected class StoreTypeEntity<T>
    {
        public int Id { get; set; }
        public required T Value { get; set; }
        public required T OtherValue { get; set; }
        public required StoreTypeContainer<T> Container { get; set; }
    }

    protected class StoreTypeContainer<T>
    {
        public required T Value { get; set; }
        public required T OtherValue { get; set; }
    }

    public virtual void OnModelCreating<T>(ModelBuilder modelBuilder)
    {
    }

    public virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => throw new NotSupportedException();

    protected override string StoreName => "StoreTypeTest";
}
