// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

/// <summary>
///     Contains tests for apply LINQ operators to collection associations.
/// </summary>
public abstract class AssociationsCollectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalFact, MemberData(nameof(IsAsyncData))]
    public virtual Task Count()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.Count == 2));

    [ConditionalFact]
    public virtual Task Where()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.Where(r => r.Int != 8).Count() == 2));

    [ConditionalFact]
    public virtual Task OrderBy_ElementAt()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.OrderBy(r => r.Id).ElementAt(0).Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.Count > 0
                && e.AssociateCollection.OrderBy(r => r.Id).ElementAt(0).Int == 8));

    #region Distinct

    [ConditionalFact]
    public virtual Task Distinct()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.Distinct().Count() == 2));

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(e => e.AssociateCollection.Distinct().ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalFact]
    public virtual Task Distinct_over_projected_nested_collection()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.AssociateCollection.Select(r => r.NestedCollection).Distinct().Count() == 2));

    [ConditionalFact]
    public virtual Task Distinct_over_projected_filtered_nested_collection()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.AssociateCollection.Select(r => r.NestedCollection.Where(n => n.Int == 8)).Distinct().Count() == 2));

    #endregion Distinct

    #region Index

    [ConditionalFact]
    public virtual Task Index_constant()
        => AssertOrderedCollectionQuery(() => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection[0].Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.Count > 0 && e.AssociateCollection[0].Int == 8)));

    [ConditionalFact]
    public virtual Task Index_parameter()
        => AssertOrderedCollectionQuery(() =>
        {
            var i = 0;

            return AssertQuery(
                ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection[i].Int == 8),
                ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.Count > 0 && e.AssociateCollection[i].Int == 8));
        });

    [ConditionalFact]
    public virtual Task Index_column()
        => AssertOrderedCollectionQuery(() => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection[e.Id - 1].Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection.Count > e.Id - 1 && e.AssociateCollection[e.Id - 1].Int == 8)));

    [ConditionalFact]
    public virtual Task Index_on_nested_collection()
        => AssertOrderedCollectionQuery(() => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection[0].Int == 8),
            ss => ss.Set<RootEntity>().Where(
                e => e.RequiredAssociate.NestedCollection.Count > 0
                    && e.RequiredAssociate.NestedCollection[0].Int == 8)));

    [ConditionalFact]
    public virtual Task Index_out_of_bounds()
        => AssertOrderedCollectionQuery(() => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.AssociateCollection[9999].Int == 8),
            ss => ss.Set<RootEntity>().Where(e => false),
            assertEmpty: true));

    #endregion Index

    #region GroupBy

    [ConditionalFact]
    public virtual Task GroupBy()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.AssociateCollection.GroupBy(r => r.String).Select(g => g.Sum(int (AssociateType r) => r.Int)).Any(g => g == 16)));

    #endregion GroupBy

    [ConditionalFact]
    public virtual Task Select_within_Select_within_Select_with_aggregates()
        => AssertQuery(ss => ss.Set<RootEntity>().Select(e =>
            e.AssociateCollection.Select(r => r.NestedCollection.Select(n => n.Int).Max()).Sum()));

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
