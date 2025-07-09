// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

/// <summary>
///     Contains tests for apply LINQ operators to collection relationships.
/// </summary>
public abstract class RelationshipsCollectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count == 2));

    // Possibly split to a separate RelationshipsOrderedCollectionTestBase.

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Index_constant(bool async)
        => AssertOrderedCollectionQuery(
            () => AssertQuery(
                async,
                ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[0].Int == 21),
                ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > 0 && e.RelatedCollection[0].Int == 21)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(
                e => e.RelatedCollection.OrderBy(r => r.Id).ElementAt(0).Int == 21),
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > 0
                && e.RelatedCollection.OrderBy(r => r.Id).ElementAt(0).Int == 21));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Index_parameter(bool async)
        => AssertOrderedCollectionQuery(() =>
            {
                var i = 0;

                return AssertQuery(
                    async,
                    ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[i].Int == 21),
                    ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > 0 && e.RelatedCollection[i].Int == 21));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Index_column(bool async)
        => AssertOrderedCollectionQuery(
            () => AssertQuery(
                async,
                ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[e.Id - 1].Int == 21),
                ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > e.Id - 1 && e.RelatedCollection[e.Id - 1].Int == 21)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Index_out_of_bounds(bool async)
        => AssertOrderedCollectionQuery(
            () => AssertQuery(
                async,
                ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[e.Id].Int == 50),
                ss => ss.Set<RootEntity>().Where(e => false),
                assertEmpty: true));

    /// <summary>
    ///     Utility for tests that depend on the collection being naturally ordered
    ///     (e.g. JSON collection as opposed to a classical relational collection navigation, which is unordered).
    /// </summary>
    private async Task AssertOrderedCollectionQuery(Func<Task> action)
    {
        if (Fixture.AreCollectionsOrdered)
        {
            await action();
        }
        else
        {
            // An error was generated for warning 'Microsoft.EntityFrameworkCore.Query.RowLimitingOperationWithoutOrderByWarning':
            // The query uses a row limiting operator ('Skip'/'Take') without an 'OrderBy' operator.
            // This may lead to unpredictable results. If the 'Distinct' operator is used after 'OrderBy', then make sure to use the 'OrderBy' operator after 'Distinct' as the ordering would otherwise get erased.
            // This exception can be suppressed or logged by passing event ID 'CoreEventId.RowLimitingOperationWithoutOrderByWarning' to the 'ConfigureWarnings' method in 'DbContext.OnConfiguring' or 'AddDbContext'.
            await Assert.ThrowsAsync<InvalidOperationException>(action);
        }
    }
}
