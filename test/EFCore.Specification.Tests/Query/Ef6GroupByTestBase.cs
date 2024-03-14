// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class Ef6GroupByTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : Ef6GroupByTestBase<TFixture>.Ef6GroupByFixtureBase, new()
{
    protected Ef6GroupByTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_group_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => g.Key));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_group_count(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_expression_containing_group_key(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.Id).Select(g => g.Key * 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_aggregate_on_the_group(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => g.Max(p => p.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_group_aggregate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => new { g.Key, Aggregate = g.Max(p => p.Id) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_multiple_group_aggregates(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(
                g => new
                {
                    key1 = g.Key,
                    key2 = g.Key,
                    max = g.Max(p => p.Id),
                    min = g.Min(s => s.Id + 2)
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_conditional_expression_containing_group_key(bool async)
    {
        var a = true;
        var b = false;
        var c = true;

        return AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(
                g => new { keyIsNull = g.Key == null ? "is null" : "not null", logicExpression = (a && b || b && c) }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_filtering_and_projecting_anonymous_type_with_group_key_and_function_aggregate(
        bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().Where(o => o.Id > 5).GroupBy(o => o.FirstName)
                .Select(g => new { FirstName = g.Key, AverageId = g.Average(p => p.Id) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_function_aggregate_with_expression(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(p => p.FirstName).Select(g => g.Max(p => p.Id * 2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_projecting_expression_with_multiple_function_aggregates(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName)
                .Select(g => new { maxMinusMin = g.Max(p => p.Id) - g.Min(s => s.Id) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_is_optimized_when_grouping_by_row_and_projecting_column_of_the_key_row(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().Where(o => o.Id < 4).GroupBy(g => new { g.FirstName }).Select(g => g.Key.FirstName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_doesnt_produce_a_groupby_statement(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o).Select(g => g.Key),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Alias, a.Alias);
                Assert.Equal(e.FirstName, a.FirstName);
                Assert.Equal(e.LastName, a.LastName);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_1(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(
                o => new
                {
                    o.Id,
                    o.FirstName,
                    o.LastName,
                    o.Alias
                }, c => new { c.LastName, c.FirstName }, (k, g) => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => new { c.LastName, c.FirstName }, (k, g) => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_3(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => c, (k, g) => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => c, (k, g) => new { Count = g.Count() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_5(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => c, (k, g) => new { k.Id, Count = g.Count() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_6(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ArubaOwner>().GroupBy(
                o => o, c => c, (k, g) => new
                {
                    k.Id,
                    k.Alias,
                    Count = g.Count()
                }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_7(bool async)
        => AssertQueryScalar(
            async,
            ss => from o in ss.Set<ArubaOwner>()
                  group o by o
                  into g
                  select g.Count());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_8(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<ArubaOwner>()
                  group o by o
                  into g
                  select new { g.Key.Id, Count = g.Count() });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_9(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<ArubaOwner>()
                  group o by o
                  into g
                  select new
                  {
                      g.Key.Id,
                      g.Key.Alias,
                      Count = g.Count()
                  });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Grouping_by_all_columns_with_aggregate_function_works_10(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<ArubaOwner>()
                  group o by o
                  into g
                  select new
                  {
                      g.Key.Id,
                      Sum = g.Sum(x => x.Id),
                      Count = g.Count()
                  });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Simple_1_from_LINQ_101(bool async)
        // GroupBy final operator. Issue #19929.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from n in ss.Set<NumberForLinq>()
                      group n by n.Value % 5
                      into g
                      select new { Remainder = g.Key, Numbers = g }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Simple_2_from_LINQ_101(bool async)
        // GroupBy final operator. Issue #19929.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from w in ss.Set<NumberForLinq>()
                      group w by w.Name.Length
                      into g
                      select new { FirstLetter = g.Key, Words = g }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Simple_3_from_LINQ_101(bool async)
        // GroupBy final operator. Issue #19929.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new { Category = g.Key, Products = g }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Nested_from_LINQ_101(bool async)
        // GroupBy final operator. Issue #19929.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from c in ss.Set<CustomerForLinq>()
                      select new
                      {
                          c.CompanyName,
                          YearGroups = from o in c.Orders
                                       group o by o.OrderDate.Year
                                       into yg
                                       select new
                                       {
                                           Year = yg.Key,
                                           MonthGroups = from o in yg
                                                         group o by o.OrderDate.Month
                                                         into mg
                                                         select
                                                             new { Month = mg.Key, Orders = mg }
                                       }
                      }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Any_Grouped_from_LINQ_101(bool async)
        // GroupBy final operator. Issue #19929.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      where g.Any(p => p.UnitsInStock == 0)
                      select new { Category = g.Key, Products = g }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_Grouped_from_LINQ_101(bool async)
        // GroupBy final operator. Issue #19929.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      where g.All(p => p.UnitsInStock > 0)
                      select new { Category = g.Key, Products = g }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_Grouped_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<ProductForLinq>()
                  group p by p.Category
                  into g
                  select new { Category = g.Key, ProductCount = g.Count() });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LongCount_Grouped_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<ProductForLinq>()
                  group p by p.Category
                  into g
                  select new { Category = g.Key, ProductLongCount = g.LongCount() });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_Grouped_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<ProductForLinq>()
                  group p by p.Category
                  into g
                  select new { Category = g.Key, TotalUnitsInStock = g.Sum(p => p.UnitsInStock) });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_Grouped_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<ProductForLinq>()
                  group p by p.Category
                  into g
                  select new { Category = g.Key, CheapestPrice = g.Min(p => p.UnitPrice) });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_Elements_from_LINQ_101(bool async)
        // Navigation expansion phase 2. Issue #23206.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      let minPrice = g.Min(p => p.UnitPrice)
                      select new { Category = g.Key, CheapestProducts = g.Where(p => p.UnitPrice == minPrice) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_Grouped_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<ProductForLinq>()
                  group p by p.Category
                  into g
                  select new { Category = g.Key, MostExpensivePrice = g.Max(p => p.UnitPrice) });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_Elements_from_LINQ_101(bool async)
        // Navigation expansion phase 2. Issue #23206.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      let minPrice = g.Max(p => p.UnitPrice)
                      select new { Category = g.Key, MostExpensiveProducts = g.Where(p => p.UnitPrice == minPrice) }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_Grouped_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<ProductForLinq>()
                  group p by p.Category
                  into g
                  select new { Category = g.Key, AveragePrice = g.Average(p => p.UnitPrice) },
            elementSorter: e => (e.Category, e.AveragePrice),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Category, a.Category);
                Assert.Equal(e.AveragePrice, a.AveragePrice, 5);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Group_Join_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<CustomerForLinq>()
                  join o in ss.Set<OrderForLinq>() on c equals o.Customer into ps
                  select new { Customer = c, Products = ps },
            elementSorter: e => e.Customer.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Customer, a.Customer);
                AssertCollection(e.Products, a.Products);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cross_Join_with_Group_Join_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<CustomerForLinq>()
                  join o in ss.Set<OrderForLinq>() on c equals o.Customer into ps
                  from o in ps
                  select new { Customer = c, o.Id },
            ss => from c in ss.Set<CustomerForLinq>()
                  join o in ss.Set<OrderForLinq>() on c.Id equals o.Customer.Id into ps
                  from o in ps
                  select new { Customer = c, o.Id },
            r => (r.Id, r.Customer.Id),
            (l, r) =>
            {
                Assert.Equal(l.Id, r.Id);
                Assert.Equal(l.Customer.Id, r.Customer.Id);
                Assert.Equal(l.Customer.Region, r.Customer.Region);
                Assert.Equal(l.Customer.CompanyName, r.Customer.CompanyName);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_Outer_Join_with_Group_Join_from_LINQ_101(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<CustomerForLinq>().Include(e => e.Orders)
                  join o in ss.Set<OrderForLinq>() on c equals o.Customer into ps
                  from o in ps.DefaultIfEmpty()
                  select new { Customer = c, OrderId = o == null ? -1 : o.Id },
            ss => from c in ss.Set<CustomerForLinq>()
                  join o in ss.Set<OrderForLinq>() on c.Id equals o.Customer.Id into ps
                  from o in ps.DefaultIfEmpty()
                  select new { Customer = c, OrderId = o == null ? -1 : o.Id },
            r => (r.OrderId, r.Customer.Id),
            (l, r) =>
            {
                Assert.Equal(l.OrderId, r.OrderId);
                AssertEqual(l.Customer, r.Customer);
            });

    [ConditionalTheory] // From #12088
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .Include(e => e.Shoes)
                .GroupBy(e => e.FirstName)
                .Select(
                    g => g.OrderBy(e => e.FirstName)
                        .ThenBy(e => e.LastName)
                        .FirstOrDefault()));

    [ConditionalTheory] // From #16648
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_2(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Person>()
                .Select(
                    p => new { p.FirstName, FullName = p.FirstName + " " + p.MiddleInitial + " " + p.LastName })
                .GroupBy(p => p.FirstName)
                .OrderBy(e => e.Key)
                .Select(g => g.First()));

    [ConditionalTheory] // From #12640
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .Where(e => e.MiddleInitial == "Q" && e.Age == 20)
                .GroupBy(e => e.LastName)
                .Select(g => g.First().LastName)
                .OrderBy(e => e.Length),
            assertOrder: true);

    [ConditionalTheory] // From #18037
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_4(bool async)
        => AssertQuery(
            async,
            ss => from person in ss.Set<Person>()
                  join shoes in ss.Set<Shoes>() on person.Age equals shoes.Age
                  group shoes by shoes.Style
                  into people
                  select new
                  {
                      people.Key,
                      Style = people.Select(p => p.Style).FirstOrDefault(),
                      Count = people.Count()
                  });

    [ConditionalTheory] // From #12601
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_5(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .GroupBy(e => e.FirstName)
                .Select(g => g.First().LastName)
                .OrderBy(e => e),
            assertOrder: true);

    [ConditionalTheory] // From #12600
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_6(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .Where(e => e.Age == 20)
                .GroupBy(e => e.Id)
                .Select(g => g.First().MiddleInitial)
                .OrderBy(e => e),
            assertOrder: true);

    [ConditionalTheory] // From #25460
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_7(bool async)
    {
        var size = 11;

        return AssertQuery(
            async,
            ss => ss.Set<Person>()
                .Where(
                    p => p.Feet.Size == size
                        && p.MiddleInitial != null
                        && p.Feet.Id != 1)
                .GroupBy(
                    p => new { p.Feet.Size, p.Feet.Person.LastName })
                .Select(
                    g => new
                    {
                        g.Key.LastName,
                        g.Key.Size,
                        Min = g.Min(p => p.Feet.Size),
                    }));
    }

    [ConditionalTheory] // From #24869
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_8(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Person>()
                .Include(x => x.Shoes)
                .Include(x => x.Feet)
                .GroupBy(
                    x => new { x.Feet.Id, x.Feet.Size })
                .Select(
                    x => new
                    {
                        Key = x.Key.Id + x.Key.Size,
                        Count = x.Count(),
                        Sum = x.Sum(el => el.Id),
                        SumOver60 = x.Sum(el => el.Id) / (decimal)60,
                        TotalCallOutCharges = x.Sum(el => el.Feet.Size == 11 ? 1 : 0)
                    }));

    [ConditionalTheory] // From #24591
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_9(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .GroupBy(n => n.FirstName)
                .Select(g => new { Feet = g.Key, Total = g.Sum(n => n.Feet.Size) }));

    [ConditionalTheory] // From #24695
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_10(bool async)
        => AssertQuery(
            async,
            ss => from Person person1
                      in from Person person2
                             in ss.Set<Person>()
                         select person2
                  join Shoes shoes
                      in ss.Set<Shoes>()
                      on person1.Age equals shoes.Age
                  group shoes by
                      new
                      {
                          person1.Id,
                          shoes.Style,
                          shoes.Age
                      }
                  into temp
                  orderby temp.Key.Id, temp.Key.Style, temp.Key.Age
                  select
                      new
                      {
                          temp.Key.Id,
                          temp.Key.Age,
                          temp.Key.Style,
                          Values = from t
                                       in temp
                                   select
                                       new
                                       {
                                           t.Id,
                                           t.Style,
                                           t.Age
                                       }
                      },
            r => r.Id,
            (l, r) =>
            {
                Assert.Equal(l.Id, r.Id);
                Assert.Equal(l.Age, r.Age);
                Assert.Equal(l.Style, r.Style);
                AssertCollection(l.Values, r.Values, elementSorter: e => (e.Id, e.Style, e.Age));
            });

    [ConditionalTheory] // From #19506
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_11(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .GroupBy(i => i.LastName)
                .Select(
                    g => new
                    {
                        LastName = g.Key,
                        Count = g.Count(),
                        First = g.OrderBy(e => e.Id).FirstOrDefault(),
                        Take = g.OrderBy(e => e.Id).Take(2)
                    })
                .OrderByDescending(e => e.LastName)
                .Select(e => e),
            r => (r.First.FirstName, r.First.MiddleInitial, r.First.LastName),
            (l, r) =>
            {
                Assert.Equal(l.LastName, r.LastName);
                Assert.Equal(l.Count, r.Count);
                AssertEqual(l.First, r.First);

                var lTake = l.Take.ToList();
                var rTake = r.Take.ToList();

                Assert.Equal(lTake.Count, rTake.Count);
                for (var i = 0; i < lTake.Count; i++)
                {
                    AssertEqual(lTake[i], rTake[i]);
                }
            },
            assertOrder: false);

    [ConditionalTheory] // From #13805
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_12(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .Include(e => e.Shoes)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .GroupBy(e => e.FirstName)
                .Select(g => new { Name = g.Key, People = g.OrderBy(e => e.Id).ToList() }),
            r => (r.Name, r.People.Count),
            (l, r) =>
            {
                Assert.Equal(l.Name, r.Name);
                Assert.Equal(l.People.Count, r.People.Count);
                for (var i = 0; i < l.People.Count; i++)
                {
                    AssertEqual(l.People[i], r.People[i]);
                }
            });

    [ConditionalTheory] // From #12088
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_13(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .GroupBy(m => new { m.FirstName, m.MiddleInitial })
                .Select(
                    am => new { am.Key, Items = am.OrderBy(e => e.Id).ToList() }),
            r => (r.Key.FirstName, r.Key.MiddleInitial),
            (l, r) =>
            {
                Assert.Equal(l.Key, r.Key);
                Assert.Equal(l.Items.Count, r.Items.Count);
                for (var i = 0; i < l.Items.Count; i++)
                {
                    AssertEqual(l.Items[i], r.Items[i]);
                }
            });

    [ConditionalTheory] // From #12088
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_14(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Person>()
                    .GroupBy(bp => bp.Feet)
                    .SelectMany(g => g.OrderByDescending(bp => bp.Id).Take(1).DefaultIfEmpty())));

    [ConditionalTheory] // From #12088
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_15(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Person>()
                .GroupBy(bp => bp.Feet)
                .Select(g => g.OrderByDescending(bp => bp.Id).FirstOrDefault()));

    [ConditionalTheory] // From #12573
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Whats_new_2021_sample_16(bool async)
        // GroupBy final operator. Issue #19929.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<Person>()
                    .GroupBy(c => c.LastName)
                    .Select(g => g.OrderBy(c => c.FirstName).First())
                    .GroupBy(c => c.MiddleInitial)
                    .Select(g => g)));

    protected ArubaContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }

    public abstract class Ef6GroupByFixtureBase : SharedStoreFixtureBase<ArubaContext>, IQueryFixtureBase
    {
        private ArubaData _expectedData;

        protected override string StoreName
            => "Ef6GroupByTest";

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<ArubaOwner>(
                b =>
                {
                    b.Property(p => p.Id).ValueGeneratedNever();
                    b.Property(o => o.FirstName).HasMaxLength(30);
                });

            modelBuilder.Entity<NumberForLinq>();
            modelBuilder.Entity<ProductForLinq>().Property(e => e.UnitPrice).HasPrecision(18, 6);
            modelBuilder.Entity<FeaturedProductForLinq>();
            modelBuilder.Entity<CustomerForLinq>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<OrderForLinq>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Total).HasPrecision(18, 6);
                });

            modelBuilder.Entity<Person>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Shoes>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Feet>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.HasOne(e => e.Person).WithOne(e => e.Feet).HasForeignKey<Feet>();
                });
        }

        protected override Task SeedAsync(ArubaContext context)
        {
            var data = new ArubaData();
            context.AddRange(data.ArubaOwners);
            context.AddRange(data.NumbersForLinq);
            context.AddRange(data.ProductsForLinq);
            context.AddRange(data.CustomersForLinq);
            context.AddRange(data.OrdersForLinq);
            context.AddRange(data.People);
            context.AddRange(data.Feet);
            context.AddRange(data.Shoes);

            return context.SaveChangesAsync();
        }

        public virtual ISetSource GetExpectedData()
        {
            if (_expectedData == null)
            {
                _expectedData = new ArubaData();
            }

            return _expectedData;
        }

        public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
        {
            { typeof(CustomerForLinq), e => ((CustomerForLinq)e)?.Id },
            { typeof(OrderForLinq), e => ((OrderForLinq)e)?.Id },
            { typeof(Person), e => ((Person)e)?.Id },
            { typeof(Shoes), e => ((Shoes)e)?.Id },
            { typeof(Feet), e => ((Feet)e)?.Id }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(CustomerForLinq), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (CustomerForLinq)e;
                        var aa = (CustomerForLinq)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Region, aa.Region);
                        Assert.Equal(ee.CompanyName, aa.CompanyName);
                    }
                }
            },
            {
                typeof(OrderForLinq), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (OrderForLinq)e;
                        var aa = (OrderForLinq)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Total, aa.Total);
                        Assert.Equal(ee.OrderDate, aa.OrderDate);
                    }
                }
            },
            {
                typeof(Person), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Person)e;
                        var aa = (Person)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Age, aa.Age);
                        Assert.Equal(ee.FirstName, aa.FirstName);
                        Assert.Equal(ee.MiddleInitial, aa.MiddleInitial);
                        Assert.Equal(ee.LastName, aa.LastName);
                    }
                }
            },
            {
                typeof(Shoes), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Shoes)e;
                        var aa = (Shoes)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Age, aa.Age);
                        Assert.Equal(ee.Style, aa.Style);
                    }
                }
            },
            {
                typeof(Feet), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (Feet)e;
                        var aa = (Feet)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Size, aa.Size);
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    public class ArubaContext(DbContextOptions options) : PoolableDbContext(options);

    public class ArubaOwner
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Alias { get; set; }
    }

    public class NumberForLinq(int value, string name)
    {
        public int Id { get; set; }
        public int Value { get; set; } = value;
        public string Name { get; set; } = name;
    }

    public class ProductForLinq
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
    }

    public class FeaturedProductForLinq : ProductForLinq;

    public class CustomerForLinq
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string CompanyName { get; set; }
        public ICollection<OrderForLinq> Orders { get; } = new List<OrderForLinq>();
    }

    public class OrderForLinq
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public CustomerForLinq Customer { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }
        public Feet Feet { get; set; }
        public ICollection<Shoes> Shoes { get; } = new List<Shoes>();
    }

    public class Shoes
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string Style { get; set; }
        public Person Person { get; set; }
    }

    public class Feet
    {
        public int Id { get; set; }
        public int Size { get; set; }
        public Person Person { get; set; }
    }

    public class ArubaData : ISetSource
    {
        public IReadOnlyList<ArubaOwner> ArubaOwners { get; }
        public IReadOnlyList<NumberForLinq> NumbersForLinq { get; }
        public IReadOnlyList<ProductForLinq> ProductsForLinq { get; }
        public IReadOnlyList<CustomerForLinq> CustomersForLinq { get; }
        public IReadOnlyList<OrderForLinq> OrdersForLinq { get; }
        public IReadOnlyList<Person> People { get; }
        public IReadOnlyList<Feet> Feet { get; }
        public IReadOnlyList<Shoes> Shoes { get; }

        public ArubaData()
        {
            ArubaOwners = CreateArubaOwners();
            NumbersForLinq = CreateNumbersForLinq();
            ProductsForLinq = CreateProductsForLinq();
            CustomersForLinq = CreateCustomersForLinq();
            OrdersForLinq = CreateOrdersForLinq(CustomersForLinq);
            People = CreatePeople();
            Feet = CreateFeet(People);
            Shoes = CreateShoes(People);
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(ArubaOwner))
            {
                return (IQueryable<TEntity>)ArubaOwners.AsQueryable();
            }

            if (typeof(TEntity) == typeof(NumberForLinq))
            {
                return (IQueryable<TEntity>)NumbersForLinq.AsQueryable();
            }

            if (typeof(TEntity) == typeof(ProductForLinq))
            {
                return (IQueryable<TEntity>)ProductsForLinq.AsQueryable();
            }

            if (typeof(TEntity) == typeof(CustomerForLinq))
            {
                return (IQueryable<TEntity>)CustomersForLinq.AsQueryable();
            }

            if (typeof(TEntity) == typeof(OrderForLinq))
            {
                return (IQueryable<TEntity>)OrdersForLinq.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Person))
            {
                return (IQueryable<TEntity>)People.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Shoes))
            {
                return (IQueryable<TEntity>)Shoes.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Feet))
            {
                return (IQueryable<TEntity>)Feet.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        private static IReadOnlyList<NumberForLinq> CreateNumbersForLinq()
            => new List<NumberForLinq>
            {
                new(5, "Five"),
                new(4, "Four"),
                new(1, "One"),
                new(3, "Three"),
                new(9, "Nine"),
                new(8, "Eight"),
                new(6, "Six"),
                new(7, "Seven"),
                new(2, "Two"),
                new(0, "Zero"),
            };

        private static IReadOnlyList<ProductForLinq> CreateProductsForLinq()
            => new List<ProductForLinq>
            {
                new()
                {
                    ProductName = "Chai",
                    Category = "Beverages",
                    UnitPrice = 18.0000M,
                    UnitsInStock = 39
                },
                new()
                {
                    ProductName = "Chang",
                    Category = "Beverages",
                    UnitPrice = 19.0000M,
                    UnitsInStock = 17
                },
                new()
                {
                    ProductName = "Aniseed Syrup",
                    Category = "Condiments",
                    UnitPrice = 10.0000M,
                    UnitsInStock = 13
                },
                new()
                {
                    ProductName = "Chef Anton's Cajun Seasoning",
                    Category = "Condiments",
                    UnitPrice = 22.0000M,
                    UnitsInStock = 53
                },
                new()
                {
                    ProductName = "Chef Anton's Gumbo Mix",
                    Category = "Condiments",
                    UnitPrice = 21.3500M,
                    UnitsInStock = 0
                },
                new()
                {
                    ProductName = "Grandma's Boysenberry Spread",
                    Category = "Condiments",
                    UnitPrice = 25.0000M,
                    UnitsInStock = 120
                },
                new()
                {
                    ProductName = "Uncle Bob's Organic Dried Pears",
                    Category = "Produce",
                    UnitPrice = 30.0000M,
                    UnitsInStock = 15
                },
                new FeaturedProductForLinq
                {
                    ProductName = "Northwoods Cranberry Sauce",
                    Category = "Condiments",
                    UnitPrice = 40.0000M,
                    UnitsInStock = 6
                },
                new()
                {
                    ProductName = "Mishi Kobe Niku",
                    Category = "Meat/Poultry",
                    UnitPrice = 97.0000M,
                    UnitsInStock = 29
                },
                new()
                {
                    ProductName = "Ikura",
                    Category = "Seafood",
                    UnitPrice = 31.0000M,
                    UnitsInStock = 31
                },
                new()
                {
                    ProductName = "Queso Cabrales",
                    Category = "Dairy Products",
                    UnitPrice = 21.0000M,
                    UnitsInStock = 22
                },
                new FeaturedProductForLinq
                {
                    ProductName = "Queso Manchego La Pastora",
                    Category = "Dairy Products",
                    UnitPrice = 38.0000M,
                    UnitsInStock = 86
                },
                new()
                {
                    ProductName = "Konbu",
                    Category = "Seafood",
                    UnitPrice = 6.0000M,
                    UnitsInStock = 24
                },
                new()
                {
                    ProductName = "Tofu",
                    Category = "Produce",
                    UnitPrice = 23.2500M,
                    UnitsInStock = 35
                },
                new()
                {
                    ProductName = "Genen Shouyu",
                    Category = "Condiments",
                    UnitPrice = 15.5000M,
                    UnitsInStock = 39
                },
                new()
                {
                    ProductName = "Pavlova",
                    Category = "Confections",
                    UnitPrice = 17.4500M,
                    UnitsInStock = 29
                },
                new FeaturedProductForLinq
                {
                    ProductName = "Alice Mutton",
                    Category = "Meat/Poultry",
                    UnitPrice = 39.0000M,
                    UnitsInStock = 0
                },
                new FeaturedProductForLinq
                {
                    ProductName = "Carnarvon Tigers",
                    Category = "Seafood",
                    UnitPrice = 62.5000M,
                    UnitsInStock = 42
                },
                new()
                {
                    ProductName = "Teatime Chocolate Biscuits",
                    Category = "Confections",
                    UnitPrice = 9.2000M,
                    UnitsInStock = 25
                },
                new()
                {
                    ProductName = "Sir Rodney's Marmalade",
                    Category = "Confections",
                    UnitPrice = 81.0000M,
                    UnitsInStock = 40
                },
                new()
                {
                    ProductName = "Sir Rodney's Scones",
                    Category = "Confections",
                    UnitPrice = 10.0000M,
                    UnitsInStock = 3
                }
            };

        private static IReadOnlyList<CustomerForLinq> CreateCustomersForLinq()
            => new List<CustomerForLinq>
            {
                new()
                {
                    Id = 1,
                    Region = "WA",
                    CompanyName = "Microsoft"
                },
                new()
                {
                    Id = 2,
                    Region = "WA",
                    CompanyName = "NewMonics"
                },
                new()
                {
                    Id = 3,
                    Region = "OR",
                    CompanyName = "NewMonics"
                },
                new()
                {
                    Id = 4,
                    Region = "CA",
                    CompanyName = "Microsoft"
                }
            };

        private static IReadOnlyList<OrderForLinq> CreateOrdersForLinq(IReadOnlyList<CustomerForLinq> customers)
        {
            var orders = new List<OrderForLinq>
            {
                new()
                {
                    Id = 1,
                    Total = 111M,
                    OrderDate = new DateTime(1997, 9, 3),
                    Customer = customers[0]
                },
                new()
                {
                    Id = 2,
                    Total = 222M,
                    OrderDate = new DateTime(2006, 9, 3),
                    Customer = customers[1]
                },
                new()
                {
                    Id = 3,
                    Total = 333M,
                    OrderDate = new DateTime(1999, 9, 3),
                    Customer = customers[0]
                },
                new()
                {
                    Id = 4,
                    Total = 444M,
                    OrderDate = new DateTime(2010, 9, 3),
                    Customer = customers[1]
                },
                new()
                {
                    Id = 5,
                    Total = 2555M,
                    OrderDate = new DateTime(2009, 9, 3),
                    Customer = customers[2]
                },
                new()
                {
                    Id = 6,
                    Total = 6555M,
                    OrderDate = new DateTime(1976, 9, 3),
                    Customer = customers[3]
                },
                new()
                {
                    Id = 7,
                    Total = 555M,
                    OrderDate = new DateTime(1985, 9, 3),
                    Customer = customers[2]
                },
            };

            foreach (var order in orders)
            {
                order.Customer.Orders.Add(order);
            }

            return orders;
        }

        private static ArubaOwner[] CreateArubaOwners()
        {
            var owners = new ArubaOwner[10];
            for (var i = 0; i < 10; i++)
            {
                var owner = new ArubaOwner
                {
                    Id = i,
                    Alias = "Owner Alias " + i,
                    FirstName = "First Name " + i % 3,
                    LastName = "Last Name " + i,
                };
                owners[i] = owner;
            }

            return owners;
        }

        private static IReadOnlyList<Person> CreatePeople()
        {
            var people = new List<Person>
            {
                new()
                {
                    Id = 1,
                    FirstName = "Jim",
                    MiddleInitial = "A",
                    LastName = "Bob",
                    Age = 20,
                    Feet = new Feet { Id = 1, Size = 11 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 1,
                            Style = "Sneakers",
                            Age = 19
                        },
                        new Shoes
                        {
                            Id = 2,
                            Style = "Dress",
                            Age = 20
                        }
                    }
                },
                new()
                {
                    Id = 2,
                    FirstName = "Tom",
                    MiddleInitial = "A",
                    LastName = "Bob",
                    Age = 20,
                    Feet = new Feet { Id = 2, Size = 12 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 3,
                            Style = "Sneakers",
                            Age = 21
                        },
                        new Shoes
                        {
                            Id = 4,
                            Style = "Dress",
                            Age = 19
                        }
                    }
                },
                new()
                {
                    Id = 3,
                    FirstName = "Ben",
                    MiddleInitial = "Q",
                    LastName = "Bob",
                    Age = 20,
                    Feet = new Feet { Id = 3, Size = 12 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 5,
                            Style = "Sneakers",
                            Age = 20
                        },
                        new Shoes
                        {
                            Id = 6,
                            Style = "Dress",
                            Age = 21
                        }
                    }
                },
                new()
                {
                    Id = 4,
                    FirstName = "Jim",
                    MiddleInitial = "Q",
                    LastName = "Jon",
                    Age = 20,
                    Feet = new Feet { Id = 4, Size = 11 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 7,
                            Style = "Sneakers",
                            Age = 19
                        },
                        new Shoes
                        {
                            Id = 8,
                            Style = "Dress",
                            Age = 20
                        }
                    }
                },
                new()
                {
                    Id = 5,
                    FirstName = "Tom",
                    MiddleInitial = "A",
                    LastName = "Jon",
                    Age = 21,
                    Feet = new Feet { Id = 5, Size = 11 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 9,
                            Style = "Sneakers",
                            Age = 21
                        },
                        new Shoes
                        {
                            Id = 10,
                            Style = "Dress",
                            Age = 19
                        }
                    }
                },
                new()
                {
                    Id = 6,
                    FirstName = "Ben",
                    MiddleInitial = "A",
                    LastName = "Jon",
                    Age = 21,
                    Feet = new Feet { Id = 6, Size = 12 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 11,
                            Style = "Sneakers",
                            Age = 20
                        },
                        new Shoes
                        {
                            Id = 12,
                            Style = "Dress",
                            Age = 21
                        }
                    }
                },
                new()
                {
                    Id = 7,
                    FirstName = "Jim",
                    MiddleInitial = "Q",
                    LastName = "Don",
                    Age = 21,
                    Feet = new Feet { Id = 7, Size = 12 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 13,
                            Style = "Sneakers",
                            Age = 19
                        },
                        new Shoes
                        {
                            Id = 14,
                            Style = "Dress",
                            Age = 20
                        }
                    }
                },
                new()
                {
                    Id = 8,
                    FirstName = "Tom",
                    MiddleInitial = "Q",
                    LastName = "Don",
                    Age = 21,
                    Feet = new Feet { Id = 8, Size = 11 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 15,
                            Style = "Sneakers",
                            Age = 21
                        },
                        new Shoes
                        {
                            Id = 16,
                            Style = "Dress",
                            Age = 19
                        }
                    }
                },
                new()
                {
                    Id = 9,
                    FirstName = "Ben",
                    MiddleInitial = "A",
                    LastName = "Don",
                    Age = 21,
                    Feet = new Feet { Id = 9, Size = 11 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 17,
                            Style = "Sneakers",
                            Age = 20
                        },
                        new Shoes
                        {
                            Id = 18,
                            Style = "Dress",
                            Age = 21
                        }
                    }
                },
                new()
                {
                    Id = 10,
                    FirstName = "Jim",
                    MiddleInitial = "A",
                    LastName = "Zee",
                    Age = 21,
                    Feet = new Feet { Id = 10, Size = 12 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 19,
                            Style = "Sneakers",
                            Age = 19
                        },
                        new Shoes
                        {
                            Id = 20,
                            Style = "Dress",
                            Age = 20
                        }
                    }
                },
                new()
                {
                    Id = 11,
                    FirstName = "Tom",
                    MiddleInitial = "Q",
                    LastName = "Zee",
                    Age = 21,
                    Feet = new Feet { Id = 11, Size = 12 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 21,
                            Style = "Sneakers",
                            Age = 21
                        },
                        new Shoes
                        {
                            Id = 22,
                            Style = "Dress",
                            Age = 19
                        }
                    }
                },
                new()
                {
                    Id = 12,
                    FirstName = "Ben",
                    MiddleInitial = "Q",
                    LastName = "Zee",
                    Age = 21,
                    Feet = new Feet { Id = 12, Size = 11 },
                    Shoes =
                    {
                        new Shoes
                        {
                            Id = 23,
                            Style = "Sneakers",
                            Age = 20
                        },
                        new Shoes
                        {
                            Id = 24,
                            Style = "Dress",
                            Age = 21
                        }
                    }
                }
            };

            foreach (var person in people)
            {
                person.Feet.Person = person;

                foreach (var shoes in person.Shoes)
                {
                    shoes.Person = person;
                }
            }

            return people;
        }

        private static IReadOnlyList<Feet> CreateFeet(IReadOnlyList<Person> people)
            => people.Select(e => e.Feet).ToList();

        private static IReadOnlyList<Shoes> CreateShoes(IReadOnlyList<Person> people)
            => people.SelectMany(e => e.Shoes).ToList();
    }
}
