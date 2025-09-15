// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

/// <summary>
///     Contains tests for apply LINQ operators to collection relationships.
/// </summary>
public abstract class AssociationsCollectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalFact, MemberData(nameof(IsAsyncData))]
    public virtual Task Count()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count == 2));

    [ConditionalFact]
    public virtual Task Where()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Where(r => r.Int != 8).Count() == 2));

    [ConditionalFact]
    public virtual Task OrderBy_ElementAt()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.OrderBy(r => r.Id).ElementAt(0).Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > 0
                && e.RelatedCollection.OrderBy(r => r.Id).ElementAt(0).Int == 8));

    #region Distinct

    [ConditionalFact]
    public virtual Task Distinct()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Distinct().Count() == 2));

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(e => e.RelatedCollection.Distinct().ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalFact]
    public virtual Task Distinct_over_projected_nested_collection()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RelatedCollection.Select(r => r.NestedCollection).Distinct().Count() == 2));

    [ConditionalFact]
    public virtual Task Distinct_over_projected_filtered_nested_collection()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RelatedCollection.Select(r => r.NestedCollection.Where(n => n.Int == 8)).Distinct().Count() == 2));

    #endregion Distinct

    #region Index

    [ConditionalFact]
    public virtual Task Index_constant()
        => AssertOrderedCollectionQuery(() => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[0].Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > 0 && e.RelatedCollection[0].Int == 8)));

    [ConditionalFact]
    public virtual Task Index_parameter()
        => AssertOrderedCollectionQuery(() =>
        {
            var i = 0;

            return AssertQuery(
                ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[i].Int == 8),
                ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > 0 && e.RelatedCollection[i].Int == 8));
        });

    [ConditionalFact]
    public virtual Task Index_column()
        => AssertOrderedCollectionQuery(() => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[e.Id - 1].Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection.Count > e.Id - 1 && e.RelatedCollection[e.Id - 1].Int == 8)));

    [ConditionalFact]
    public virtual Task Index_out_of_bounds()
        => AssertOrderedCollectionQuery(() => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RelatedCollection[9999].Int == 8),
            ss => ss.Set<RootEntity>().Where(e => false),
            assertEmpty: true));

    #endregion Index

    #region GroupBy

    [ConditionalFact]
    public virtual Task GroupBy()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RelatedCollection.GroupBy(r => r.String).Select(g => g.Sum(int (RelatedType r) => r.Int)).Any(g => g == 16)));

    #endregion GroupBy

    [ConditionalFact]
    public virtual Task Select_within_Select_within_Select_with_aggregates()
        => AssertQuery(ss => ss.Set<RootEntity>().Select(e =>
            e.RelatedCollection.Select(r => r.NestedCollection.Select(n => n.Int).Max()).Sum()));

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
