// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ComplexNavigationsCollectionsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : ComplexNavigationsQueryFixtureBase, new()
{
    protected ComplexNavigationsContext CreateContext()
        => Fixture.CreateContext();

    protected ComplexNavigationsCollectionsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override Expression RewriteExpectedQueryExpression(Expression expectedQueryExpression)
        => new ExpectedQueryRewritingVisitor(Fixture.GetShadowPropertyMappings()).Visit(expectedQueryExpression);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nav_prop_collection_one_to_many_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(e => e.Id).Select(e => e.OneToMany_Required1.Select(i => i.Id)),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
        => AssertQuery(
            async,
            ss => from l4 in ss.Set<Level1>().SelectMany(
                      l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                              .DefaultIfEmpty())
                      on l4.Id equals l2.Id
                  join l3 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty())
                      on l2.Id equals l3.Id into grouping
                  from l3 in grouping.DefaultIfEmpty()
                  where l4.OneToMany_Optional_Inverse4.Name != "Foo"
                  orderby l2.OneToOne_Optional_FK2.Id
                  select new
                  {
                      Entity = l4,
                      Collection = l2.OneToMany_Optional_Self2.Where(e => e.Id != 42).ToList(),
                      Property = l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2.Name
                  },
            ss => from l4 in ss.Set<Level1>().SelectMany(
                      l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                              .DefaultIfEmpty())
                      on l4.Id equals l2.Id
                  join l3 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty())
                      on l2.Id equals l3.Id into grouping
                  from l3 in grouping.DefaultIfEmpty()
                  where l4.OneToMany_Optional_Inverse4.Name != "Foo"
                  orderby l2.OneToOne_Optional_FK2.MaybeScalar(x => x.Id)
                  select new
                  {
                      Entity = l4,
                      Collection = l2.OneToMany_Optional_Self2.Where(e => e.Id != 42).ToList(),
                      Property = l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2.Name
                  },
            elementSorter: e => e.Entity.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Entity, a.Entity);
                AssertCollection(e.Collection, a.Collection);
                Assert.Equal(e.Property, a.Property);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select l1.OneToMany_Optional1,
            elementSorter: e => e != null ? e.Count : 0,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_nested(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select l1.OneToOne_Optional_FK1.OneToMany_Optional2,
            ss => from l1 in ss.Set<Level1>()
                  select l1.OneToOne_Optional_FK1.OneToMany_Optional2 ?? new List<Level3>(),
            elementSorter: e => e.Count,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_nested_with_take(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select l1.OneToOne_Optional_FK1.OneToMany_Optional2.Take(50),
            ss => from l1 in ss.Set<Level1>()
                  select (l1.OneToOne_Optional_FK1.OneToMany_Optional2 ?? new List<Level3>()).Take(50),
            elementSorter: e => e?.Count() ?? 0,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_using_ef_property(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select EF.Property<ICollection<Level3>>(
                      EF.Property<Level2>(
                          l1,
                          "OneToOne_Optional_FK1"),
                      "OneToMany_Optional2"),
            elementSorter: e => e?.Count ?? 0,
            elementAsserter: (e, a) => AssertCollection(e ?? new List<Level3>(), a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_nested_anonymous(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select new { l1.Id, l1.OneToOne_Optional_FK1.OneToMany_Optional2 },
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.OneToMany_Optional2 ?? new List<Level3>(), a.OneToMany_Optional2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_composed(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  where l1.Id < 3
                  select new { l1.Id, collection = l1.OneToMany_Optional1.Where(l2 => l2.Name != "Foo").ToList() },
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.collection, a.collection);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_and_root_entity(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select new { l1, l1.OneToMany_Optional1 },
            elementSorter: e => e.l1.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.l1.Id, a.l1.Id);
                AssertCollection(e.OneToMany_Optional1, a.OneToMany_Optional1);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_and_include(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>().Include(l => l.OneToMany_Optional1)
                  select new { l1, l1.OneToMany_Optional1 },
            elementSorter: e => e.l1.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.l1.Id, a.l1.Id);
                AssertCollection(e.OneToMany_Optional1, a.OneToMany_Optional1);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_navigation_and_collection(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select new { l1.OneToOne_Optional_FK1, l1.OneToOne_Optional_FK1.OneToMany_Optional2 },
            elementSorter: e => e.OneToOne_Optional_FK1?.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OneToOne_Optional_FK1?.Id, a.OneToOne_Optional_FK1?.Id);
                AssertCollection(e.OneToMany_Optional2 ?? new List<Level3>(), a.OneToMany_Optional2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_inside_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Where(l1 => l1.Id < 3)
                .OrderBy(l1 => l1.Id)
                .Select(l1 => new { subquery = ss.Set<Level2>().Include(l => l.OneToMany_Optional2).Where(l => l.Id > 0).ToList() }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.subquery, a.subquery));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(
                l1 => new
                {
                    Level2s = l1.OneToMany_Optional1.Select(
                        l2 => new
                        {
                            Level3 = l2.OneToOne_Required_FK2 == null
                                ? null
                                : new { l2.OneToOne_Required_FK2.Name }
                        }).ToList()
                }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e.Level2s,
                a.Level2s,
                elementSorter: ee => ee?.Level3.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_check_in_Dto_projection_should_not_be_removed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(
                l1 => new
                {
                    Level2s = l1.OneToMany_Optional1.Select(
                        l2 => new
                        {
                            Level3 = l2.OneToOne_Required_FK2 == null
                                ? null
                                : new ProjectedDto<string> { Value = l2.OneToOne_Required_FK2.Name }
                        }).ToList()
                }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e.Level2s,
                a.Level2s,
                elementSorter: ee => ee.Level3?.Value,
                elementAsserter: (ee, aa) => Assert.Equal(ee.Level3?.Value, aa.Level3?.Value)));

    private class ProjectedDto<T>
    {
        public T Value { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).Select(l2 => new { l2.Id, l2.OneToMany_Optional2 }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.OneToMany_Optional2, a.OneToMany_Optional2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).SelectMany(l2 => l2.OneToMany_Optional2)
                .Select(l2 => new { l2.Id, l2.OneToMany_Optional3 }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.OneToMany_Optional3, a.OneToMany_Optional3);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include(l2 => l2.OneToMany_Required2)
                .Select(l2 => new { l2, l2.OneToMany_Optional2 }),
            elementSorter: e => e.l2.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l2, a.l2);
                AssertCollection(e.l2.OneToMany_Required2, a.l2.OneToMany_Required2);
                AssertCollection(e.OneToMany_Optional2, a.OneToMany_Optional2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Take(25)
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        c1 = l1.OneToMany_Required1.Select(l2 => new { l2.Id }).FirstOrDefault(),
                        c2 = l1.OneToMany_Required1.Select(l2 => new { l2.Id })
                    }),
            elementSorter: t => t.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.c1?.Id, a.c1?.Id);
                AssertCollection(e.c2, a.c2, elementSorter: i => i.Id, elementAsserter: (ie, ia) => Assert.Equal(ie.Id, ia.Id));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_single_nested_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(
                l1 => new
                {
                    Level2 = l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).Select(
                            l2 => new { Level3s = l2.OneToMany_Optional2.OrderBy(l3 => l3.Id).Select(l3 => new { l3.Id }).ToList() })
                        .FirstOrDefault()
                }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                if (e.Level2 == null)
                {
                    Assert.Null(a.Level2);
                }
                else
                {
                    AssertCollection(e.Level2.Level3s, a.Level2.Level3s, ordered: true);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_single_nested_subquery2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Select(
                l1 => new
                {
                    Level2s = l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).Select(
                        l2 => new
                        {
                            Level3 = l2.OneToMany_Optional2.OrderBy(l3 => l3.Id).Select(
                                l3 => new
                                {
                                    Level4s = l3.OneToMany_Optional3.OrderBy(l4 => l4.Id).Select(l4 => new { l4.Id })
                                        .ToList()
                                }).FirstOrDefault()
                        })
                }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(
                    e.Level2s, a.Level2s, ordered: true, elementAsserter:
                    (e2, a2) =>
                    {
                        if (e2.Level3 == null)
                        {
                            Assert.Null(a2.Level3);
                        }
                        else
                        {
                            AssertCollection(e2.Level3.Level4s, a2.Level3.Level4s, ordered: true);
                        }
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Queryable_in_subquery_works_when_final_projection_is_List(bool async)
        => AssertInvalidMaterializationType(
            () =>
                AssertQuery(
                    async,
                    ss => from l1 in ss.Set<Level1>()
                          orderby l1.Id
                          let inner = (from l2 in l1.OneToMany_Optional1
                                       where l2.Name != "Foo"
                                       let innerL1s = from innerL1 in ss.Set<Level1>()
                                                      where innerL1.OneToMany_Optional1.Any(innerL2 => innerL2.Id == l2.Id)
                                                      select innerL1.Name
                                       select innerL1s).FirstOrDefault()
                          select inner.ToList(),
                    assertOrder: true,
                    elementAsserter: (e, a) => AssertCollection(e, a)),
            typeof(IQueryable<string>).ShortDisplayName());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  orderby l1.Id
                  let inner = (from l2 in ss.Set<Level2>()
                               where l2.Name != "Foo"
                               let innerL1s = (from innerL1 in ss.Set<Level1>()
                                               where innerL1.OneToMany_Optional1.Any(innerL2 => innerL2.Id == l2.Id)
                                               select innerL1.Name).AsEnumerable()
                               select innerL1s.ToList()).FirstOrDefault()
                  select inner.ToList(),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  orderby l1.Id
                  let inner = (from l2 in l1.OneToMany_Optional1
                               where l2.Name != "Foo"
                               let innerL1s = from innerL1 in ss.Set<Level1>()
                                              where innerL1.OneToMany_Optional1.Any(innerL2 => innerL2.Id == l2.Id)
                                              select innerL1.Name
                               select innerL1s.ToList()).FirstOrDefault()
                  select inner,
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
        => AssertQuery(
            async,
            ss => from l4 in ss.Set<Level1>().SelectMany(
                      l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                              .DefaultIfEmpty()) on
                      l4.Id equals l2.Id
                  join l3 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty()) on
                      l2.Id equals l3.Id into grouping
                  from l3 in grouping.DefaultIfEmpty()
                  where l4.OneToMany_Optional_Inverse4.Name != "Foo"
                  orderby l2.OneToOne_Optional_FK2.Id
                  select new
                  {
                      Entity = l4,
                      Collection = l2.OneToMany_Optional_Self2.Where(e => e.Id != 42).ToList(),
                      Property = l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2.Name
                  },
            ss => from l4 in ss.Set<Level1>().SelectMany(
                      l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                              .DefaultIfEmpty()) on
                      l4.Id equals l2.Id
                  join l3 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty()) on
                      l2.Id equals l3.Id into grouping
                  from l3 in grouping.DefaultIfEmpty()
                  where l4.OneToMany_Optional_Inverse4.Name != "Foo"
                  orderby l2.OneToOne_Optional_FK2.MaybeScalar(e => e.Id)
                  select new
                  {
                      Entity = l4,
                      Collection = l2.OneToMany_Optional_Self2.Where(e => e.Id != 42).ToList(),
                      Property = l3.OneToOne_Optional_FK_Inverse3.OneToOne_Required_FK2.Name
                  },
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Entity, a.Entity);
                AssertCollection(e.Collection, a.Collection);
                AssertEqual(e.Property, a.Property);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Select_collection_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Take(1)
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        l1.Name,
                        Level2s = l1.OneToMany_Required1.OrderBy(l2 => l2.Id).Take(3)
                            .Select(
                                l2 => new
                                {
                                    l2.Id,
                                    l2.Name,
                                    Level1Id = EF.Property<int>(l2, "OneToMany_Required_Inverse2Id"),
                                    Level2Id = l2.Level1_Required_Id,
                                    Level2 = l2.OneToOne_Required_FK_Inverse2
                                })
                    }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Name, a.Name);
                AssertCollection(
                    e.Level2s, a.Level2s, ordered: true,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.Level1Id, aa.Level1Id);
                        Assert.Equal(ee.Level2Id, aa.Level2Id);
                        AssertEqual(ee.Level2, aa.Level2);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_Select_collection_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Skip(1).Take(1)
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        l1.Name,
                        Level2s = l1.OneToMany_Required1.OrderBy(l2 => l2.Id).Skip(1).Take(3)
                            .Select(
                                l2 => new
                                {
                                    l2.Id,
                                    l2.Name,
                                    Level1Id = EF.Property<int>(l2, "OneToMany_Required_Inverse2Id"),
                                    Level2Id = l2.Level1_Required_Id,
                                    Level2 = l2.OneToOne_Required_FK_Inverse2
                                })
                    }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Name, a.Name);
                AssertCollection(
                    e.Level2s, a.Level2s, ordered: true,
                    elementAsserter: (ee, aa) =>
                    {
                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                        Assert.Equal(ee.Level1Id, aa.Level1Id);
                        Assert.Equal(ee.Level2Id, aa.Level2Id);
                        AssertEqual(ee.Level2, aa.Level2);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(e => e.OneToMany_Optional1).ThenInclude(e => e.OneToMany_Optional2),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(e => e.OneToMany_Optional1).ThenInclude(e => e.OneToMany_Optional2)
                .ThenInclude(e => e.OneToMany_Required_Inverse3.OneToMany_Optional2),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Required_Inverse3, "OneToMany_Optional1.OneToMany_Optional2"),
                new ExpectedInclude<Level2>(
                    l2 => l2.OneToMany_Optional2, "OneToMany_Optional1.OneToMany_Optional2.OneToMany_Required_Inverse3")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_complex_includes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Optional_FK1)
                .ThenInclude(e => e.OneToMany_Optional2)
                .Include(e => e.OneToMany_Optional1)
                .ThenInclude(e => e.OneToOne_Optional_FK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_complex_includes_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => EF.Property<Level2>(e, "OneToOne_Optional_FK1"))
                .ThenInclude(e => EF.Property<ICollection<Level3>>(e, "OneToMany_Optional2"))
                .Include(e => EF.Property<ICollection<Level2>>(e, "OneToMany_Optional1"))
                .ThenInclude(e => EF.Property<Level3>(e, "OneToOne_Optional_FK2")),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_complex_includes_self_ref(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Optional_Self1)
                .ThenInclude(e => e.OneToMany_Optional_Self1)
                .Include(e => e.OneToMany_Optional_Self1)
                .ThenInclude(e => e.OneToOne_Optional_Self1),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_Self1),
                new ExpectedInclude<Level1>(l2 => l2.OneToMany_Optional_Self1, "OneToOne_Optional_Self1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional_Self1),
                new ExpectedInclude<Level1>(l2 => l2.OneToOne_Optional_Self1, "OneToMany_Optional_Self1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_complex_includes_self_ref_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => EF.Property<Level1>(e, "OneToOne_Optional_Self1"))
                .ThenInclude(e => EF.Property<ICollection<Level1>>(e, "OneToMany_Optional_Self1"))
                .Include(e => EF.Property<ICollection<Level1>>(e, "OneToMany_Optional_Self1"))
                .ThenInclude(e => EF.Property<Level1>(e, "OneToOne_Optional_Self1")),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_Self1),
                new ExpectedInclude<Level1>(l2 => l2.OneToMany_Optional_Self1, "OneToOne_Optional_Self1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional_Self1),
                new ExpectedInclude<Level1>(l2 => l2.OneToOne_Optional_Self1, "OneToMany_Optional_Self1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_and_collection_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Optional_FK1.OneToMany_Optional2)
                .OrderBy(e => e.Name),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_ThenInclude_collection_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Optional_FK1)
                .ThenInclude(e => e.OneToMany_Optional2)
                .OrderBy(e => e.Name),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_then_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToMany_Optional1)
                .ThenInclude(e => e.OneToOne_Optional_FK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_conditional_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToMany_Optional1)
                .OrderBy(e => e.Name.EndsWith("03") ? 1 : 2)
                .Select(e => e),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(ee => ee.OneToMany_Optional1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_complex_include_select(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Optional_FK1)
                .ThenInclude(e => e.OneToMany_Optional2)
                .Include(e => e.OneToMany_Optional1)
                .ThenInclude(e => e.OneToOne_Optional_FK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_with_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                      .Include(e => e.OneToOne_Optional_FK1.OneToMany_Required2)
                      .ThenInclude(e => e.OneToOne_Required_FK3)
                  where l1.OneToOne_Optional_FK1.Name != "L2 09"
                  select l1,
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(l1 => l1.OneToOne_Required_FK3, "OneToOne_Optional_FK1.OneToMany_Required2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_multi_include_with_order_by_and_paging(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Optional2)
                .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Required2)
                .OrderBy(t => t.Name)
                .Skip(0).Take(10),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Optional2, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l1 => l1.OneToMany_Required2, "OneToOne_Required_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Optional_FK1).ThenInclude(e => e.OneToMany_Optional2)
                .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Required2)
                .OrderBy(t => t.Name)
                .Skip(0).Take(10),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required2, "OneToOne_Required_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Optional_FK1.OneToOne_Required_FK2).ThenInclude(e => e.OneToMany_Optional3)
                .OrderBy(t => t.Name)
                .Skip(0).Take(10),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToOne_Optional_FK1.OneToOne_Required_FK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_include_with_multiple_optional_navigations(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToMany_Optional2)
                .Include(e => e.OneToOne_Required_FK1).ThenInclude(e => e.OneToOne_Optional_FK2)
                .Include(e => e.OneToOne_Optional_FK1).ThenInclude(e => e.OneToOne_Optional_FK2)
                .Where(e => e.OneToOne_Required_FK1.OneToOne_Optional_PK2.Name != "Foo")
                .OrderBy(e => e.Id),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Required_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Required_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK1")),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_Include1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include(l2 => l2.OneToMany_Optional2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Orderby_SelectMany_with_Include1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id)
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include(l2 => l2.OneToMany_Optional2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_Include2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include(l2 => l2.OneToOne_Required_FK2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_Include_ThenInclude(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include(l2 => l2.OneToOne_Required_FK2)
                .ThenInclude(l3 => l3.OneToMany_Optional3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3, "OneToOne_Required_FK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_SelectMany_with_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .SelectMany(l2 => l2.OneToMany_Optional2)
                .Include(l3 => l3.OneToOne_Required_FK3)
                .Include(l3 => l3.OneToMany_Optional3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3),
                new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Required_navigation_with_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>()
                .Include(l3 => l3.OneToOne_Required_FK_Inverse3.OneToMany_Required_Inverse2)
                .Select(l3 => l3.OneToOne_Required_FK_Inverse3),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Required_Inverse2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Required_navigation_with_Include_ThenInclude(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level4>()
                .Include(l4 => l4.OneToOne_Required_FK_Inverse4.OneToMany_Required_Inverse3)
                .ThenInclude(l2 => l2.OneToMany_Optional_Inverse2)
                .Select(l4 => l4.OneToOne_Required_FK_Inverse4),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level3>(l3 => l3.OneToMany_Required_Inverse3),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional_Inverse2, "OneToMany_Required_Inverse3")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_with_Include_ThenInclude(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Select(l1 => l1.OneToOne_Optional_FK1),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK3, "OneToMany_Optional2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_optional_navigation_with_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3)
                .Select(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_optional_navigation_with_string_based_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include("OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3")
                .Select(l1 => l1.OneToOne_Optional_FK1)
                .Select(l2 => l2.OneToOne_Optional_PK2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level3>(l3 => l3.OneToMany_Optional3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_with_order_by_and_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                .Select(l1 => l1.OneToOne_Optional_FK1)
                .OrderBy(l2 => l2.Name),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_with_Include_and_order(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                .Select(l1 => l1.OneToOne_Optional_FK1)
                .OrderBy(l2 => l2.Name),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_order_by_and_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .OrderBy(l2 => l2.Name)
                .Include(l2 => l2.OneToMany_Optional2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_Include_and_order_by(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include(l2 => l2.OneToMany_Optional2)
                .OrderBy(l2 => l2.Name),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_navigation_and_Distinct(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>().Include(l => l.OneToMany_Optional1)
                  from l2 in l1.OneToMany_Optional1.Distinct()
                  where l2 != null
                  select l1,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>().Include(l => l.OneToMany_Optional1)
                  from l2 in l1.OneToMany_Optional1.Select(
                      x => new
                      {
                          x.Id,
                          x.Name,
                          FK = EF.Property<int>(x, "OneToMany_Optional_Inverse2Id")
                      }).Distinct()
                  select l1,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_multiple_orderbys_member(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Include(l2 => l2.OneToMany_Optional2)
                .OrderBy(l2 => l2.Name)
                .ThenBy(l2 => l2.Level1_Required_Id),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(e => e.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_multiple_orderbys_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Include(l2 => l2.OneToMany_Optional2)
                .OrderBy(l2 => EF.Property<int>(l2, "Level1_Required_Id"))
                .ThenBy(l2 => l2.Name),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(e => e.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_multiple_orderbys_methodcall(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Include(l2 => l2.OneToMany_Optional2)
                .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id))
                .ThenBy(l2 => l2.Name),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(e => e.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_multiple_orderbys_complex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Include(l2 => l2.OneToMany_Optional2)
                .OrderBy(l2 => Math.Abs(l2.Level1_Required_Id) + 7)
                .ThenBy(l2 => l2.Name),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(e => e.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Include(l2 => l2.OneToMany_Optional2)
                .OrderBy(l2 => -l2.Level1_Required_Id)
                .ThenBy(l2 => -l2.Level1_Required_Id).ThenBy(l2 => l2.Name),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(e => e.OneToMany_Optional2)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
    {
        checked
        {
            return AssertQuery(
                async,
                ss => ss.Set<Level2>()
                    .Include(l2 => l2.OneToMany_Optional2)
                    .OrderBy(l2 => -l2.Level1_Required_Id)
                    .ThenBy(l2 => -l2.Level1_Required_Id).ThenBy(l2 => l2.Name),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(e => e.OneToMany_Optional2)),
                assertOrder: true);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_groupby_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .GroupBy(g => g.Name)
                .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(e => e.OneToMany_Optional1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_groupby_in_subquery_and_filter_before_groupby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .Where(l1 => l1.Id > 3)
                .GroupBy(g => g.Name)
                .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(e => e.OneToMany_Optional1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_with_groupby_in_subquery_and_filter_after_groupby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .GroupBy(g => g.Name)
                .Where(g => g.Key != "Foo")
                .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(e => e.OneToMany_Optional1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_collection_order_by_reference_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                .OrderBy(l1 => (int?)l1.OneToOne_Optional_FK1.Id),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToOne_Optional_FK1")),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_after_Select(bool async)
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(l1 => l1.OneToOne_Optional_FK1).Include(l2 => l2.OneToMany_Optional2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_after_SelectMany_and_reference_navigation(bool async)
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Select(l2 => l2.OneToOne_Optional_FK2)
                    .Include(l3 => l3.OneToMany_Optional3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_after_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Include(l2 => l2.OneToMany_Optional2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(x => x.OneToMany_Optional2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Required1)
                .SelectMany(l2 => l2.OneToMany_Optional2)
                .Include(l3 => l3.OneToOne_Required_FK3.OneToMany_Required_Self4)
                .Select(l3 => l3.OneToOne_Required_FK3),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level4>(l4 => l4.OneToMany_Required_Self4)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Required1)
                .Include(l2 => l2.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4)
                .Select(l2 => l2.OneToOne_Optional_FK2)
                .Select(l3 => l3.OneToOne_Required_FK3),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level4>(l4 => l4.OneToMany_Optional_Self4)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l3 => l3.OneToMany_Optional1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_followed_by_include_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(e1 => e1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(e2 => e2.OneToOne_Optional_PK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_followed_by_include_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l2 => l2.OneToMany_Optional2),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(e1 => e1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(e2 => e2.OneToMany_Optional2, "OneToOne_Optional_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_followed_by_projecting_the_included_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Include(l1 => l1.OneToMany_Optional1).Select(l1 => l1.OneToMany_Optional1),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Id)
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .Select(l1 => l1.OneToMany_Optional1),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Level2>(x => x.OneToOne_Optional_PK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Id)
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Select(l1 => l1.OneToMany_Optional1),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e,
                a,
                elementAsserter: (ee, aa) => AssertInclude(
                    ee, aa, new ExpectedInclude<Level2>(e1 => e1.OneToOne_Optional_PK2),
                    new ExpectedInclude<Level3>(e2 => e2.OneToOne_Optional_FK3, "OneToOne_Optional_PK2"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_ThenInclude_two_references(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(e1 => e1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(e2 => e2.OneToOne_Optional_PK2, "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(e3 => e3.OneToOne_Optional_FK3, "OneToMany_Optional1.OneToOne_Optional_PK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Id)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .ThenInclude(l3 => l3.OneToMany_Optional3)
                .Select(l1 => l1.OneToMany_Optional1),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level2>(e1 => e1.OneToOne_Optional_PK2),
                new ExpectedInclude<Level3>(e2 => e2.OneToOne_Optional_FK3, "OneToOne_Optional_PK2"),
                new ExpectedInclude<Level2>(e3 => e3.OneToOne_Optional_FK2),
                new ExpectedInclude<Level3>(e4 => e4.OneToMany_Optional3, "OneToOne_Optional_FK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .ThenInclude(l3 => l3.OneToMany_Optional3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(e1 => e1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(e2 => e2.OneToOne_Optional_PK2, "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(e3 => e3.OneToOne_Optional_FK3, "OneToMany_Optional1.OneToOne_Optional_PK2"),
                new ExpectedInclude<Level2>(e4 => e4.OneToOne_Optional_FK2, "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(e5 => e5.OneToMany_Optional3, "OneToMany_Optional1.OneToOne_Optional_FK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Id)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Select(l1 => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_PK2)),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e, a, elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Level3>(x => x.OneToOne_Optional_FK3))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Id)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .Select(l1 => new { l1, l1.OneToMany_Optional1 }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertInclude(
                    e.l1, a.l1, new ExpectedInclude<Level1>(e1 => e1.OneToMany_Optional1),
                    new ExpectedInclude<Level2>(e2 => e2.OneToOne_Optional_PK2, "OneToMany_Optional1"));
                AssertCollection(
                    e.OneToMany_Optional1,
                    a.OneToMany_Optional1,
                    elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Level2>(e => e.OneToOne_Optional_PK2)));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_multiple_with_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToOne_Optional_PK2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3)
                .Where(l1 => l1.OneToMany_Optional1.Where(l2 => l2.OneToOne_Optional_PK2.Name != "Foo").Count() > 0),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(e1 => e1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(e2 => e2.OneToOne_Optional_PK2, "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(e3 => e3.OneToOne_Optional_FK3, "OneToMany_Optional1.OneToOne_Optional_PK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection_multiple_with_filter_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => EF.Property<ICollection<Level2>>(l1, "OneToMany_Optional1"))
                .ThenInclude(l2 => EF.Property<Level3>(l2, "OneToOne_Optional_PK2"))
                .ThenInclude(l3 => EF.Property<Level4>(l3, "OneToOne_Optional_FK3"))
                .Where(
                    l1 => EF.Property<ICollection<Level2>>(l1, "OneToMany_Optional1").Where(l2 => l2.OneToOne_Optional_PK2.Name != "Foo")
                            .Count()
                        > 0),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(e1 => e1.OneToMany_Optional1),
                new ExpectedInclude<Level2>(e2 => e2.OneToOne_Optional_PK2, "OneToMany_Optional1"),
                new ExpectedInclude<Level3>(e3 => e3.OneToOne_Optional_FK3, "OneToMany_Optional1.OneToOne_Optional_PK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Including_reference_navigation_and_projecting_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Required_FK1)
                .ThenInclude(e => e.OneToOne_Optional_FK2)
                .Select(
                    e => new Level1
                    {
                        Id = e.Id,
                        OneToOne_Required_FK1 = e.OneToOne_Required_FK1,
                        OneToMany_Required1 = e.OneToMany_Required1
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(bool async)
    {
        var validIds = new List<string> { "L1 01", "L1 02" };

        return AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>().Where(l1 => validIds.Any(e => e == l1.Name))
                  join l2 in ss.Set<Level2>()
                      on l1.Id equals l2.Level1_Required_Id into l2s
                  from l2 in l2s.DefaultIfEmpty()
                  select new Level2 { Id = l2 == null ? 0 : l2.Id, OneToMany_Required2 = l2 == null ? null : l2.OneToMany_Required2 });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_basic_Where(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.Where(l2 => l2.Id > 5)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(l2 => l2.Id > 5))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_basic_Where_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => EF.Property<ICollection<Level2>>(l1, "OneToMany_Optional1").Where(l2 => l2.Id > 5)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(l2 => l2.Id > 5))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Name),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_OrderBy_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => EF.Property<ICollection<Level2>>(l1, "OneToMany_Optional1").OrderBy(x => x.Name)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Name),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_ThenInclude_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToMany_Optional2.OrderBy(x => x.Name)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(e => e.OneToMany_Optional1),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Optional2,
                    "OneToMany_Optional1",
                    includeFilter: x => x.OrderBy(x => x.Name),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_ThenInclude_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name))
                .ThenInclude(l2 => l2.OneToMany_Optional2.OrderByDescending(x => x.Name)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Name),
                    assertOrder: true),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Optional2,
                    "OneToMany_Optional1",
                    includeFilter: x => x.OrderByDescending(x => x.Name),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_basic_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Name).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_basic_OrderBy_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name).Skip(1)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Name).Skip(1),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Name).Skip(1).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Name).Skip(1).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_basic_OrderBy_Skip_Take_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(
                l1 => EF.Property<ICollection<Level2>>(l1, "OneToMany_Optional1")
                    .OrderBy(x => x.Name).Skip(1).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Name).Skip(1).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_Skip_without_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.Skip(1)),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e, a);
                Assert.Equal(Math.Max(e.OneToMany_Optional1.Count - 1, 0), a.OneToMany_Optional1?.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_Take_without_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.Take(1)),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e, a);
                Assert.Equal(e.OneToMany_Optional1.Any() ? 1 : 0, a.OneToMany_Optional1?.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_on_ThenInclude(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1)
                .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Optional2,
                    "OneToOne_Optional_FK1",
                    x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_on_ThenInclude_EF_Property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => EF.Property<Level2>(l1, "OneToOne_Optional_FK1"))
                .ThenInclude(
                    l2 => EF.Property<ICollection<Level3>>(l2, "OneToMany_Optional2")
                        .Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Optional2,
                    "OneToOne_Optional_FK1",
                    x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_after_reference_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(
                    l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1)
                        .Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Optional2,
                    "OneToOne_Optional_FK1",
                    x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Skip(1).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_after_different_filtered_include_same_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3))
                .Include(l1 => l1.OneToMany_Required1.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3),
                    assertOrder: true),
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Required1,
                    includeFilter: x => x.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_after_different_filtered_include_different_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3))
                .ThenInclude(l2 => l2.OneToMany_Required2.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Name).Take(3),
                    assertOrder: true),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Required2,
                    "OneToMany_Optional1",
                    includeFilter: x => x.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Skip(1),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Filtered_include_different_filter_set_on_same_navigation_twice(bool async)
        => Assert.Equal(
            CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                    "navigation    .Where(x => x.Name != \"Bar\")    .OrderByDescending(x => x.Name)    .Take(3)",
                    "navigation    .Where(x => x.Name != \"Foo\")    .OrderBy(x => x.Id)    .Take(3)")
                .Replace("\r", "").Replace("\n", ""),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>()
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3))
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Bar").OrderByDescending(x => x.Name).Take(3)))))
            .Message
            .Replace("\r", "").Replace("\n", ""));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(bool async)
        => Assert.Equal(
            CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                    "navigation    .Where(x => x.Name != \"Bar\")",
                    "navigation    .Where(x => x.Name != \"Foo\")")
                .Replace("\r", "").Replace("\n", ""),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>()
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo")).ThenInclude(l2 => l2.OneToMany_Optional2)
                        .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Bar"))
                        .ThenInclude(l2 => l2.OneToOne_Required_FK2))))
            .Message
            .Replace("\r", "").Replace("\n", ""));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderByDescending(x => x.Id).Take(2))
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderByDescending(x => x.Id).Take(2)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderByDescending(x => x.Id).Take(2),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2))
                .ThenInclude(l2 => l2.OneToMany_Optional2)
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2))
                .ThenInclude(l2 => l2.OneToOne_Required_FK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2),
                    assertOrder: true),
                new ExpectedInclude<Level2>(e => e.OneToMany_Optional2),
                new ExpectedInclude<Level2>(e => e.OneToOne_Required_FK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2))
                .ThenInclude(l2 => l2.OneToMany_Optional2)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Required_FK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(2),
                    assertOrder: true),
                new ExpectedInclude<Level2>(e => e.OneToMany_Optional2, "OneToMany_Optional1"),
                new ExpectedInclude<Level2>(e => e.OneToOne_Required_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3))
                .Include(l1 => l1.OneToMany_Optional1),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(3),
                    assertOrder: true)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToOne_Optional_PK2.OneToMany_Optional3.Where(x => x.Id > 1)),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    e => e.OneToMany_Optional1,
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1),
                    assertOrder: true),
                new ExpectedInclude<Level2>(e => e.OneToOne_Optional_PK2, "OneToMany_Optional1"),
                new ExpectedFilteredInclude<Level3, Level4>(
                    e => e.OneToMany_Optional3,
                    "OneToMany_Optional1.OneToOne_Optional_PK2",
                    includeFilter: x => x.Where(x => x.Id > 1))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                .ThenInclude(l3 => l3.OneToMany_Optional3)
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                .ThenInclude(l3 => l3.OneToMany_Required3),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(e => e.OneToMany_Optional1),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Optional2,
                    "OneToMany_Optional1",
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1),
                    assertOrder: true),
                new ExpectedInclude<Level3>(e => e.OneToMany_Optional3, "OneToMany_Optional1.OneToMany_Optional2"),
                new ExpectedInclude<Level3>(e => e.OneToMany_Required3, "OneToMany_Optional1.OneToMany_Optional2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .ThenInclude(l2 => l2.OneToMany_Optional2.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1))
                .ThenInclude(l3 => l3.OneToMany_Optional3)
                .Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToMany_Optional2)
                .ThenInclude(l3 => l3.OneToMany_Required3),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(e => e.OneToMany_Optional1),
                new ExpectedFilteredInclude<Level2, Level3>(
                    e => e.OneToMany_Optional2,
                    "OneToMany_Optional1",
                    includeFilter: x => x.Where(x => x.Name != "Foo").OrderBy(x => x.Id).Take(1),
                    assertOrder: true),
                new ExpectedInclude<Level3>(e => e.OneToMany_Optional3, "OneToMany_Optional1.OneToMany_Optional2"),
                new ExpectedInclude<Level3>(e => e.OneToMany_Required3, "OneToMany_Optional1.OneToMany_Optional2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_variable_used_inside_filter(bool async)
    {
        var prm = "Foo";

        return AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => x.Name != prm).OrderBy(x => x.Id).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    x => x.OneToMany_Optional1,
                    includeFilter: x => x.Where(xx => xx.Name != "Foo").OrderBy(x => x.Id).Take(3))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_context_accessed_inside_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.Where(x => ss.Set<Level1>().Count() > 7).OrderBy(x => x.Id).Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    x => x.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Id).Take(3))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_context_accessed_inside_filter_correlated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(
                    l1 => l1.OneToMany_Optional1.Where(x => ss.Set<Level1>().Count(xx => xx.Id != x.Id) > 1).OrderBy(x => x.Id)
                        .Take(3)),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    x => x.OneToMany_Optional1,
                    includeFilter: x => x.OrderBy(x => x.Id).Take(3))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_include_parameter_used_inside_filter_throws(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => ss.Set<Level2>().Include(l2 => l2.OneToMany_Optional2.Where(x => x.Id != l2.Id)))),
            "IEnumerable<T>");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_outer_parameter_used_inside_filter(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(
                l1 => new
                {
                    l1.Id,
                    FullInclude = ss.Set<Level2>().Include(l2 => l2.OneToMany_Optional2).ToList(),
                    FilteredInclude = ss.Set<Level2>().Include(l2 => l2.OneToMany_Optional2.Where(x => x.Id != l1.Id)).ToList()
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertInclude(e.FullInclude, a.FullInclude, new ExpectedInclude<Level2>(x => x.OneToMany_Optional2));
                AssertInclude(
                    e.FilteredInclude,
                    a.FilteredInclude,
                    new ExpectedFilteredInclude<Level2, Level3>(
                        x => x.OneToMany_Optional2,
                        includeFilter: x => x.Where(x => x.Id != e.Id)));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Filtered_include_is_considered_loaded(bool async)
    {
        using var ctx = CreateContext();
        var query = ctx.LevelOne.AsTracking().Include(l1 => l1.OneToMany_Optional1.OrderBy(x => x.Id).Take(1));
        var result = async ? await query.ToListAsync() : query.ToList();
        foreach (var resultElement in result)
        {
            var entry = ctx.Entry(resultElement);
            Assert.True(entry.Navigation("OneToMany_Optional1").IsLoaded);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Filtered_include_with_Distinct_throws(bool async)
        => Assert.Equal(
            CoreStrings.InvalidIncludeExpression("l1.OneToMany_Optional1.AsQueryable().Distinct()"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1.Distinct())))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Filtered_include_calling_methods_directly_on_parameter_throws(bool async)
        => Assert.Equal(
            CoreStrings.InvalidIncludeExpression("l2.AsQueryable().Where(xx => (xx.Id != 42))"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => ss.Set<Level1>()
                        .Include(l1 => l1.OneToMany_Optional1)
                        .ThenInclude(l2 => l2.AsQueryable().Where(xx => xx.Id != 42))))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.OrderByDescending(x => x.Name).Take(4))
                .ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .OrderBy(l1 => l1.Id)
                .Take(5),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    x => x.OneToMany_Optional1,
                    includeFilter: x => x.OrderByDescending(xx => xx.Name).Take(4)),
                new ExpectedInclude<Level2>(x => x.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1.OrderByDescending(x => x.Name).Skip(1).Take(4))
                .ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .OrderByDescending(l1 => l1.Id)
                .Skip(1)
                .Take(5),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    x => x.OneToMany_Optional1,
                    includeFilter: x => x.OrderByDescending(xx => xx.Name).Skip(2).Take(4)),
                new ExpectedInclude<Level2>(x => x.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Id)
                .Include(l1 => l1.OneToMany_Optional1.Take(40))
                .ThenInclude(l2 => l2.OneToOne_Optional_FK2),
            asserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    x => x.OneToMany_Optional1,
                    includeFilter: x => x.Take(40)),
                new ExpectedInclude<Level2>(x => x.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Id)
                .Include(l1 => l1.OneToMany_Optional1.Take(40))
                .ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .Take(30),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedFilteredInclude<Level1, Level2>(
                    x => x.OneToMany_Optional1,
                    includeFilter: x => x.Take(40)),
                new ExpectedInclude<Level2>(x => x.OneToOne_Optional_FK2, "OneToMany_Optional1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_collection_with_FirstOrDefault(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Level1>()
                .Select(e => new { e.Id, Level2s = e.OneToMany_Optional1.ToList() }),
            predicate: l => l.Id == 1,
            asserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Level2s, a.Level2s);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_on_grouping_element(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1) }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(e.Level1s, a.Level1s);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_on_grouping_element(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderByDescending(e => e.Name).Take(10) }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(e.Level1s, a.Level1s);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_on_grouping_element(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1).Take(5) }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(e.Level1s, a.Level1s);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_Distinct_on_grouping_element(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1).Take(5).Distinct() }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(e.Level1s, a.Level1s);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_ToList_on_grouping_element(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1).Take(5).ToList() }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(e.Level1s, a.Level1s);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_on_grouping_element_into_non_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1).Take(5).Select(l1 => new { l1.Name }) }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(e.Level1s, a.Level1s, ordered: true);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_on_grouping_element_with_collection_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToMany_Optional1)
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1).Take(5) }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(
                    e.Level1s, a.Level1s,
                    elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Level1>(l => l.OneToMany_Optional1)));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_on_grouping_element_with_reference_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1)
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1).Take(5) }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(
                    e.Level1s, a.Level1s,
                    elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Level1>(l => l.OneToOne_Optional_FK1)));
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Take_on_grouping_element_inside_collection_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        group = ss.Set<Level1>()
                            .Where(l => l.Name == l1.Name)
                            .GroupBy(l => l.Date)
                            .Select(g => new { g.Key, Level1s = g.OrderBy(e => e.Name).Skip(1).Take(5) }).ToList()
                    }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(
                    e.group, a.group,
                    elementSorter: i => i.Key,
                    elementAsserter: (ee, aa) =>
                    {
                        AssertEqual(ee.Key, aa.Key);
                        AssertCollection(ee.Level1s, aa.Level1s);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_projection_over_GroupBy_over_parameter(bool async)
    {
        var validIds = new List<string> { "L1 01", "L1 02" };

        return AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Where(l1 => validIds.Contains(l1.Name))
                .GroupBy(l => l.Date)
                .Select(g => new { g.Key, Ids = g.Select(e => e.Id) }),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Key, a.Key);
                AssertCollection(e.Ids, a.Ids);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_over_conditional_null_source(bool async)
        => AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => ss.Set<Level2>()
                    .SelectMany(
                        l2 => l2.Id == 1
                            ? l2.OneToMany_Required_Inverse2.OneToMany_Optional1.Select(e => e.Id)
                            : null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_over_conditional_empty_source(bool async)
        => AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => ss.Set<Level2>()
                    .SelectMany(
                        l2 => l2.Id == 1
                            ? l2.OneToMany_Required_Inverse2.OneToMany_Optional1.Select(e => e.Id)
                            : Enumerable.Empty<int>())));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_partially_added_before_Where_and_then_build_upon(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1)
                .Where(l1 => l1.OneToOne_Optional_PK1.Id < 3 || l1.OneToOne_Optional_FK1.Id > 8)
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l2 => l2.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK3, "OneToOne_Optional_FK1.OneToMany_Optional2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.OrderBy(x => x.Id).Take(3))
                .Where(l1 => l1.OneToOne_Optional_PK1.Id < 3 || l1.OneToOne_Optional_FK1.Id > 8)
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Required2)
                .ThenInclude(l3 => l3.OneToOne_Optional_FK3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l2 => l2.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK3, "OneToOne_Optional_FK1.OneToMany_Optional2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_on_correlated_collection_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(l1 => new { l1.Id, Collection = l1.OneToMany_Optional1.Take(50) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.Collection, a.Collection);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_with_predicate_on_correlated_collection_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(l1 => new { l1.Id, Element = l1.OneToMany_Optional1.FirstOrDefault(l2 => l2.Id == l1.Id) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Element, a.Element);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(
        bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l2 in ss.Set<Level2>().Where(x => x.Level1_Required_Id == l1.Id * 2 || x.Name.Length == x.Id).DefaultIfEmpty()
                  select new
                  {
                      Root = l1,
                      Element = l2,
                      Collection = l1.OneToMany_Optional1
                  },
            elementSorter: e => (e.Root.Id, e.Element.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Root, a.Root);
                AssertEqual(e.Element, a.Element);
                AssertCollection(e.Collection, a.Collection);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_query_issue_21665(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.Name)
                .Select(
                    l1 => new
                    {
                        Level1 = l1,
                        Level2Name = l1.OneToOne_Optional_FK1.Name,
                        ChildCount = l1.OneToMany_Optional_Self1.Count,
                        Level2Count = l1.OneToMany_Optional1.Count(),
                        IsLevel2There = l1.OneToMany_Optional1.Any(l2 => l2.Id == 2),
                        Children = l1.OneToMany_Optional_Self1
                            .OrderBy(e => e.Name)
                            .Skip(1)
                            .Take(5)
                            .Select(
                                lc1 => new
                                {
                                    Level1 = lc1,
                                    ChildCount = lc1.OneToMany_Optional_Self1.Count,
                                    Level2Name = lc1.OneToOne_Optional_FK1.Name,
                                    Level2Count = lc1.OneToMany_Optional1.Count(),
                                    IsLevel2There = lc1.OneToMany_Optional1.Any(l2 => l2.Id == 2)
                                })
                    }),
            e => e.Level1.Id == 2,
            asserter: (e, a) =>
            {
                AssertEqual(e.Level1, a.Level1);
                AssertEqual(e.Level2Name, a.Level2Name);
                AssertEqual(e.ChildCount, a.ChildCount);
                AssertEqual(e.Level2Count, a.Level2Count);
                AssertEqual(e.IsLevel2There, a.IsLevel2There);
                AssertCollection(
                    e.Children, a.Children, ordered: true,
                    elementAsserter: (ee, aa) =>
                    {
                        AssertEqual(ee.Level1, aa.Level1);
                        AssertEqual(ee.Level2Name, aa.Level2Name);
                        AssertEqual(ee.ChildCount, aa.ChildCount);
                        AssertEqual(ee.Level2Count, aa.Level2Count);
                        AssertEqual(ee.IsLevel2There, aa.IsLevel2There);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_collection_after_optional_reference_correlated_with_parent(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(
                l1 => new
                {
                    l1.Id,
                    Collection = l1.OneToOne_Optional_FK1.OneToMany_Optional2.Select(
                        l3 => new { ChildId = l3.Id, ParentName = l1.OneToOne_Optional_FK1.Name })
                }),
            ss => ss.Set<Level1>().Select(
                l1 => new
                {
                    l1.Id,
                    Collection = l1.Maybe(
                        x => x.OneToOne_Optional_FK1.Maybe(
                            xx => xx.OneToMany_Optional2.Select(
                                l3 => new
                                {
                                    ChildId = (int)l3.MaybeScalar(xxx => xxx.Id),
                                    ParentName = l1.OneToOne_Optional_FK1.Maybe(xxx => xxx.Name)
                                })))
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (!(e.Collection == null && a.Collection != null && a.Collection.Count() == 0))
                {
                    AssertCollection(e.Collection, a.Collection, elementSorter: ee => ee.ChildId);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_collection_with_group_by_after_optional_reference_correlated_with_parent(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(
                l1 => new
                {
                    l1.Id,
                    Entity = l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                    Collection = l1.OneToOne_Optional_FK1.OneToMany_Optional2.GroupBy(x => x.Name)
                        .Select(g => new { g.Key, Count = g.Count() })
                }),
            ss => ss.Set<Level1>().Select(
                l1 => new
                {
                    l1.Id,
                    Entity = l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2,
                    Collection = l1.Maybe(
                        x => x.OneToOne_Optional_FK1.Maybe(
                            xx => xx.OneToMany_Optional2.GroupBy(x => x.Name).Select(g => new { g.Key, Count = g.Count() })))
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.Entity, a.Entity);
                if (!(e.Collection == null && a.Collection != null && a.Collection.Count() == 0))
                {
                    AssertCollection(e.Collection, a.Collection, elementSorter: ee => ee.Key);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_property_entity_Include_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).GroupBy(l1 => l1.Name),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Level1>(i => i.OneToMany_Optional1))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_property_entity_Include_collection_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToMany_Optional2).GroupBy(l1 => l1.Name),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementAsserter: (ee, aa) => AssertInclude(
                    ee, aa,
                    new ExpectedInclude<Level1>(i => i.OneToMany_Optional1),
                    new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional2, "OneToManyOptional1"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_property_entity_Include_collection_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).ThenInclude(l2 => l2.OneToOne_Optional_FK2).GroupBy(l1 => l1.Name),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementAsserter: (ee, aa) => AssertInclude(
                    ee, aa,
                    new ExpectedInclude<Level1>(i => i.OneToMany_Optional1),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToManyOptional1"))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_property_entity_Include_collection_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).Include(l1 => l1.OneToMany_Required1).GroupBy(l1 => l1.Name),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementAsserter: (ee, aa) => AssertInclude(
                    ee, aa,
                    new ExpectedInclude<Level1>(i => i.OneToMany_Optional1),
                    new ExpectedInclude<Level1>(l2 => l2.OneToMany_Required1))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_property_entity_Include_collection_reference_same_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).Include(l1 => l1.OneToOne_Optional_FK1).GroupBy(l1 => l1.Name),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementAsserter: (ee, aa) => AssertInclude(
                    ee, aa,
                    new ExpectedInclude<Level1>(i => i.OneToMany_Optional1),
                    new ExpectedInclude<Level1>(l2 => l2.OneToOne_Optional_FK1))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_property_entity_Include_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).GroupBy(l1 => l1.Name),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementAsserter: (ee, aa) => AssertInclude(
                    ee, aa,
                    new ExpectedInclude<Level1>(l2 => l2.OneToOne_Optional_FK1))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_property_entity_Include_reference_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToOne_Required_FK1).GroupBy(l1 => l1.Name),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementAsserter: (ee, aa) => AssertInclude(
                    ee, aa,
                    new ExpectedInclude<Level1>(l2 => l2.OneToOne_Optional_FK1),
                    new ExpectedInclude<Level1>(l2 => l2.OneToOne_Required_FK1))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_and_nested_conditional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(x => x.Id).Select(x => new
            {
                Collection = x.OneToMany_Optional1.OrderBy(xx => xx.Id).Select(xx => xx.Name).ToList(),
                Condition = x.Id == 1
                    ? "01"
                    : x.Id == 2
                        ? "02"
                        : x.Id == 3
                            ? "03"
                            : null
            }).Where(x => x.Condition == "02"),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Collection, a.Collection, ordered: true);
                AssertEqual(e.Condition, a.Condition);
            });
}
