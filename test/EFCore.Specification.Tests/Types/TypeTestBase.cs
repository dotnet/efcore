// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

using static System.Linq.Expressions.Expression;

[Collection("Type tests")]
public abstract class TypeTestBase<T, TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : TypeFixtureBase<T>
    where T : notnull
{
    [ConditionalFact]
    public async virtual Task Equality_in_query_with_parameter()
    {
        await using var context = Fixture.CreateContext();

        var result = await context.Set<TypeEntity<T>>().Where(e => e.Value.Equals(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }

    [ConditionalFact]
    public async virtual Task Equality_in_query_with_constant()
    {
        await using var context = Fixture.CreateContext();

        var entityParameter = Parameter(typeof(TypeEntity<T>), "e");
        var predicate =
            Lambda<Func<TypeEntity<T>, bool>>(
                Equal(
                    Property(entityParameter, nameof(TypeEntity<>.Value)),
                    Constant(Fixture.Value)),
                entityParameter);

        var result = await context.Set<TypeEntity<T>>().Where(predicate).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges()
    {
        await using var context = Fixture.CreateContext();

        var entity = await context.Set<TypeEntity<T>>().SingleAsync(e => e.Id == 1);
        entity.Value = Fixture.OtherValue;
        await context.SaveChangesAsync();

        var result = await context.Set<TypeEntity<T>>().Where(e => e.Id == 1).SingleAsync();
        Assert.Equal(Fixture.OtherValue, result.Value, Fixture.Comparer);

        // Revert back to the original value to avoid affecting other tests (note that test parallelization is disabled)
        entity.Value = Fixture.Value;
        await context.SaveChangesAsync();
    }

    protected TFixture Fixture { get; } = fixture;
}
