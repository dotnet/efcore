// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

[Collection("Type tests")]
public abstract class TypeTestBase<T, TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : TypeFixtureBase<T>
    where T : notnull
{
    [ConditionalFact]
    public async virtual Task Equality_in_query()
    {
        await using var context = Fixture.CreateContext();

        var result = await context.Set<TypeEntity<T>>().Where(e => e.Value.Equals(Fixture.Value)).SingleAsync();

        Assert.Equal(Fixture.Value, result.Value, Fixture.Comparer);
    }


    protected TFixture Fixture { get; } = fixture;
}
