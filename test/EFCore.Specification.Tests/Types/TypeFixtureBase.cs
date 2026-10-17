// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public abstract class TypeFixtureBase<T> : SharedStoreFixtureBase<DbContext>
{
    /// <summary>
    ///     The main value used in the tests.
    /// </summary>
    public abstract T Value { get; }

    /// <summary>
    ///     An additional value that is different from <see cref="Value" />.
    /// </summary>
    public abstract T OtherValue { get; }

    protected override string StoreName => "TypeTest";

    public virtual Func<T, T, bool> Comparer { get; } = EqualityComparer<T>.Default.Equals;

    protected override bool RecreateStore => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        // Don't rely on database generated values, which aren't supported everywhere (e.g. Cosmos)
        modelBuilder.Entity<TypeEntity<T>>().Property(e => e.Id).ValueGeneratedNever();
    }

    protected override async Task SeedAsync(DbContext context)
    {
        context.Set<TypeEntity<T>>().AddRange(
            new()
            {
                Id = 1,
                Value = Value,
                OtherValue = OtherValue
            },
            new()
            {
                Id = 2,
                Value = OtherValue,
                OtherValue = Value
            });

        await context.SaveChangesAsync();
    }
}
