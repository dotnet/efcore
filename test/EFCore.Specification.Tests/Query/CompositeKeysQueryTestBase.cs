// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.CompositeKeysModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class CompositeKeysQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : CompositeKeysQueryFixtureBase, new()
{
    protected CompositeKeysContext CreateContext()
        => Fixture.CreateContext();

    protected CompositeKeysQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override Expression RewriteExpectedQueryExpression(Expression expectedQueryExpression)
        => new ExpectedQueryRewritingVisitor(Fixture.GetShadowPropertyMappings()).Visit(expectedQueryExpression);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_multiple_collections_same_level_top_level_ordering(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<CompositeOne>()
                  orderby e1.Id2
                  select new
                  {
                      e1.Name,
                      Optional = e1.OneToMany_Optional1.ToList(),
                      Required = e1.OneToMany_Required1.ToList()
                  },
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                AssertCollection(e.Optional, a.Optional);
                AssertCollection(e.Required, a.Required);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_multiple_collections_same_level_top_level_ordering_using_entire_composite_key(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<CompositeOne>()
                  orderby e1.Id2, e1.Id1 descending
                  select new
                  {
                      e1.Name,
                      Optional = e1.OneToMany_Optional1.ToList(),
                      Required = e1.OneToMany_Required1.ToList()
                  },
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                AssertCollection(e.Optional, a.Optional);
                AssertCollection(e.Required, a.Required);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_multiple_collections_with_ordering_same_level(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<CompositeOne>()
                  select new
                  {
                      e1.Name,
                      Optional = e1.OneToMany_Optional1.OrderBy(x => x.Id2).ToList(),
                      Required = e1.OneToMany_Required1.OrderByDescending(x => x.Name).ToList()
                  },
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                AssertCollection(e.Optional, a.Optional);
                AssertCollection(e.Required, a.Required);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_multiple_collections_with_ordering_same_level_top_level_ordering(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<CompositeOne>()
                  orderby e1.Id2
                  select new
                  {
                      e1.Name,
                      Optional = e1.OneToMany_Optional1.OrderBy(x => x.Id2).ToList(),
                      Required = e1.OneToMany_Required1.OrderByDescending(x => x.Name).ToList()
                  },
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                AssertCollection(e.Optional, a.Optional);
                AssertCollection(e.Required, a.Required);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_collections_multi_level(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<CompositeOne>()
                  orderby e1.Id2
                  select new
                  {
                      e1.Name,
                      Middle = e1.OneToMany_Optional1
                          .OrderBy(e2 => e2.Id2)
                          .Select(e2 => new { e2.Name, Inner = e2.OneToMany_Required2.OrderByDescending(x => x.Id2).ToList() })
                          .ToList(),
                  },
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                AssertCollection(
                    e.Middle,
                    a.Middle,
                    elementSorter: ee => ee.Name, elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Name, aa.Name);
                        AssertCollection(ee.Inner, aa.Inner);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_multiple_collections_on_multiple_levels_no_explicit_ordering(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<CompositeOne>()
                  select new
                  {
                      e1.Name,
                      Outer1 = e1.OneToMany_Optional1
                          .Select(
                              e2 => new
                              {
                                  e2.Name,
                                  Middle1 = e2.OneToMany_Required2
                                      .Select(
                                          e3 => new
                                          {
                                              e3.Name,
                                              Inner1 = e3.OneToMany_Optional3.ToList(),
                                              Inner2 = e3.OneToMany_Required3.ToList()
                                          }).ToList(),
                                  Middle2 = e2.OneToMany_Optional2
                                      .Select(
                                          e3 => new
                                          {
                                              e3.Name,
                                              Inner1 = e3.OneToMany_Required3.ToList(),
                                              Inner2 = e3.OneToMany_Optional3.ToList()
                                          }).ToList(),
                              }).ToList(),
                      Outer2 = e1.OneToMany_Required1
                          .Select(
                              e2 => new
                              {
                                  e2.Name,
                                  Middle1 = e2.OneToMany_Optional2
                                      .Select(
                                          e3 => new
                                          {
                                              e3.Name,
                                              Inner1 = e3.OneToMany_Required3.ToList(),
                                              Inner2 = e3.OneToMany_Optional3.ToList()
                                          }).ToList(),
                                  Middle2 = e2.OneToMany_Optional2
                                      .Select(
                                          e3 => new
                                          {
                                              e3.Name,
                                              Inner1 = e3.OneToMany_Optional3.ToList(),
                                              Inner2 = e3.OneToMany_Required3.ToList()
                                          }).ToList(),
                              }).ToList(),
                  },
            elementSorter: e => e.Name,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Name, a.Name);
                AssertCollection(
                    e.Outer1,
                    a.Outer1,
                    elementSorter: ee => ee.Name, elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Name, aa.Name);
                        AssertCollection(
                            ee.Middle1,
                            aa.Middle1,
                            elementSorter: eee => eee.Name,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Name, aaa.Name);
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });

                        AssertCollection(
                            ee.Middle2,
                            aa.Middle2,
                            elementSorter: eee => eee.Name,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Name, aaa.Name);
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });
                    });
                AssertCollection(
                    e.Outer2,
                    a.Outer2,
                    elementSorter: ee => ee.Name, elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Name, aa.Name);
                        AssertCollection(
                            ee.Middle1,
                            aa.Middle1,
                            elementSorter: eee => eee.Name,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Name, aaa.Name);
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });

                        AssertCollection(
                            ee.Middle2,
                            aa.Middle2,
                            elementSorter: eee => eee.Name,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Name, aaa.Name);
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_multiple_collections_on_multiple_levels_some_explicit_ordering(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<CompositeOne>()
                  orderby e1.Name
                  select new
                  {
                      Outer1 = e1.OneToMany_Optional1
                          .Select(
                              e2 => new
                              {
                                  e2.Name,
                                  Middle1 = e2.OneToMany_Required2
                                      .OrderByDescending(e3 => e3.Id2).ThenByDescending(e3 => e3.Id1)
                                      .Select(
                                          e3 => new { Inner1 = e3.OneToMany_Optional3.ToList(), Inner2 = e3.OneToMany_Required3.ToList() })
                                      .ToList(),
                                  Middle2 = e2.OneToMany_Optional2
                                      .Select(
                                          e3 => new
                                          {
                                              e3.Name,
                                              Inner1 = e3.OneToMany_Required3.ToList(),
                                              Inner2 = e3.OneToMany_Optional3.ToList()
                                          }).ToList(),
                              }).ToList(),
                      Outer2 = e1.OneToMany_Required1
                          .OrderBy(e2 => e2.Name.Length)
                          .Select(
                              e2 => new
                              {
                                  e2.Name,
                                  Middle1 = e2.OneToMany_Optional2
                                      .Select(
                                          e3 => new
                                          {
                                              e3.Name,
                                              Inner1 = e3.OneToMany_Required3.ToList(),
                                              Inner2 = e3.OneToMany_Optional3.ToList()
                                          }).ToList(),
                                  Middle2 = e2.OneToMany_Optional2
                                      .Select(
                                          e3 => new
                                          {
                                              e3.Name,
                                              Inner1 = e3.OneToMany_Optional3.ToList(),
                                              Inner2 = e3.OneToMany_Required3.OrderByDescending(x => x.Id1 + x.Id2).ToList()
                                          }).ToList(),
                              }).ToList(),
                  },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(
                    e.Outer1,
                    a.Outer1,
                    elementSorter: ee => ee.Name, elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Name, aa.Name);
                        AssertCollection(
                            ee.Middle1,
                            aa.Middle1,
                            ordered: true,
                            elementAsserter: (eee, aaa) =>
                            {
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });

                        AssertCollection(
                            ee.Middle2,
                            aa.Middle2,
                            elementSorter: eee => eee.Name,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Name, aaa.Name);
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });
                    });
                AssertCollection(
                    e.Outer2,
                    a.Outer2,
                    elementSorter: ee => ee.Name, elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Name, aa.Name);
                        AssertCollection(
                            ee.Middle1,
                            aa.Middle1,
                            elementSorter: eee => eee.Name,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Name, aaa.Name);
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });

                        AssertCollection(
                            ee.Middle2,
                            aa.Middle2,
                            elementSorter: eee => eee.Name,
                            elementAsserter: (eee, aaa) =>
                            {
                                Assert.Equal(eee.Name, aaa.Name);
                                AssertCollection(eee.Inner1, aaa.Inner1);
                                AssertCollection(eee.Inner2, aaa.Inner2);
                            });
                    });
            });
}
