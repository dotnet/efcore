// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NonSharedPrimitiveCollectionsQueryRelationalTestBase : NonSharedPrimitiveCollectionsQueryTestBase
{
    // On relational databases, byte[] gets mapped to a special binary data type, which isn't queryable as a regular primitive collection.
    [ConditionalFact]
    public override Task Array_of_byte()
        => AssertTranslationFailed(() => TestArray((byte)1, (byte)2));

    [ConditionalFact(Skip = "#28688")]
    public virtual async Task Column_collection_inside_json_owned_entity()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestOwner>().OwnsOne(t => t.Owned, b => b.ToJson()),
            seed: context =>
            {
                context.AddRange(
                    new TestOwner { Owned = new TestOwned { Strings = ["foo", "bar"] } },
                    new TestOwner { Owned = new TestOwned { Strings = ["baz"] } });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var result = await context.Set<TestOwner>().SingleAsync(o => o.Owned.Strings.Count() == 2);
        Assert.Equivalent(new[] { "foo", "bar" }, result.Owned.Strings);

        result = await context.Set<TestOwner>().SingleAsync(o => o.Owned.Strings[1] == "bar");
        Assert.Equivalent(new[] { "foo", "bar" }, result.Owned.Strings);
    }

    protected class TestOwner
    {
        public int Id { get; set; }
        public TestOwned Owned { get; set; }
    }

    [Owned]
    protected class TestOwned
    {
        public string[] Strings { get; set; }
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
