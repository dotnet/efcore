// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

[Collection("Type tests")]
public abstract class TypeTestBase<T, TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : TypeTestBase<T, TFixture>.TypeTestFixture
    where T : notnull
{
    [ConditionalFact]
    public async virtual Task Equality_in_query()
    {
        await using var context = Fixture.CreateContext();

        var result = await context.Set<TypeEntity>().Where(e => e.Value.Equals(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }

    protected class TypeEntity
    {
        public int Id { get; set; }

        public required T Value { get; set; }
        public required T OtherValue { get; set; }
    }

    protected TFixture Fixture { get; } = fixture;

    public abstract class TypeTestFixture : SharedStoreFixtureBase<DbContext>
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
            modelBuilder.Entity<TypeEntity>().Property(e => e.Id).ValueGeneratedNever();
        }

        protected override async Task SeedAsync(DbContext context)
        {
            context.Set<TypeEntity>().AddRange(
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
}
