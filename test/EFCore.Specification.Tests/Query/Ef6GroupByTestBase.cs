// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
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
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => g.Key));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_group_count(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_expression_containing_group_key(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.Id).Select(g => g.Key * 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_aggregate_on_the_group(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => g.Max(p => p.Id)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_group_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => new { Key = g.Key, Aggregate = g.Max(p => p.Id) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_multiple_group_aggregates(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(
                    g => new { key1 = g.Key, key2 = g.Key, max = g.Max(p => p.Id), min = g.Min(s => s.Id + 2) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_conditional_expression_containing_group_key(bool async)
        {
            bool a = true;
            bool b = false;
            bool c = true;

            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(
                    g => new { keyIsNull = g.Key == null ? "is null" : "not null", logicExpression = (a && b || b && c) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_filerting_and_projecting_anonymous_type_with_group_key_and_function_aggregate(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().Where(o => o.Id > 5).GroupBy(o => o.FirstName).Select(g => new { FirstName = g.Key, AverageId = g.Average(p => p.Id) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_function_aggregate_with_expression(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(p => p.FirstName).Select(g => g.Max(p => p.Id * 2)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_projecting_expression_with_multiple_function_aggregates(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o.FirstName).Select(g => new { maxMinusMin = g.Max(p => p.Id) - g.Min(s => s.Id) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_is_optimized_when_grouping_by_row_and_projecting_column_of_the_key_row(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().Where(o => o.Id < 4).GroupBy(g => new { g.FirstName }).Select(g => g.Key.FirstName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_doesnt_produce_a_groupby_statement(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o).Select(g => g.Key));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_1(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => new { o.Id, o.FirstName, o.LastName, o.Alias }, c => new { c.LastName, c.FirstName }, (k, g) => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => new { c.LastName, c.FirstName }, (k, g) => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_3(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => c, (k, g) => g.Count()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_4(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => c, (k, g) => new { Count = g.Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_5(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => c, (k, g) => new { Id = k.Id, Count = g.Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_6(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<ArubaOwner>().GroupBy(o => o, c => c, (k, g) => new { Id = k.Id, Alias = k.Alias, Count = g.Count() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_7(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => from o in ss.Set<ArubaOwner>()
                      group o by o
                      into g
                      select g.Count());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_8(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<ArubaOwner>()
                      group o by o into g
                      select new { Id = g.Key.Id, Count = g.Count() });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_9(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<ArubaOwner>()
                      group o by o into g
                      select new { Id = g.Key.Id, Alias = g.Key.Alias, Count = g.Count() });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Grouping_by_all_columns_with_aggregate_function_works_10(bool async)
        {
            return AssertQuery(
                async,
                ss => from o in ss.Set<ArubaOwner>()
                      group o by o into g
                      select new { Id = g.Key.Id, Sum = g.Sum(x => x.Id), Count = g.Count() });
        }

        [ConditionalTheory (Skip = "Issue #19929")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Simple_1_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from n in ss.Set<NumberForLinq>()
                      group n by n.Value % 5
                      into g
                      select new
                      {
                          Remainder = g.Key,
                          Numbers = g
                      });
        }

        [ConditionalTheory (Skip = "Issue #19929")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Simple_2_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from w in ss.Set<NumberForLinq>()
                      group w by w.Name.Length
                      into g
                      select new
                      {
                          FirstLetter = g.Key,
                          Words = g
                      });
        }

        [ConditionalTheory (Skip = "Issue #19929")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Simple_3_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          Products = g
                      });
        }

        [ConditionalTheory (Skip = "Issue #19929")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_Nested_from_LINQ_101(bool async)
        {
            return AssertQuery(
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
                                                             new
                                                             {
                                                                 Month = mg.Key,
                                                                 Orders = mg
                                                             }
                                       }
                      });
        }

        [ConditionalTheory (Skip = "Issue #19929")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      where g.Any(p => p.UnitsInStock == 0)
                      select new
                      {
                          Category = g.Key,
                          Products = g
                      });
        }

        [ConditionalTheory (Skip = "Issue #19929")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task All_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      where g.All(p => p.UnitsInStock > 0)
                      select new
                      {
                          Category = g.Key,
                          Products = g
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          ProductCount = g.Count()
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LongCount_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          ProductLongCount = g.LongCount()
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          TotalUnitsInStock = g.Sum(p => p.UnitsInStock)
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          CheapestPrice = g.Min(p => p.UnitPrice)
                      });
        }

        [ConditionalTheory (Skip = "Issue #23206")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_Elements_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      let minPrice = g.Min(p => p.UnitPrice)
                      select new
                      {
                          Category = g.Key,
                          CheapestProducts = g.Where(p => p.UnitPrice == minPrice)
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          MostExpensivePrice = g.Max(p => p.UnitPrice)
                      });
        }

        [ConditionalTheory (Skip = "Issue #23206")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_Elements_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      let minPrice = g.Max(p => p.UnitPrice)
                      select new
                      {
                          Category = g.Key,
                          MostExpensiveProducts = g.Where(p => p.UnitPrice == minPrice)
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_Grouped_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          AveragePrice = g.Average(p => p.UnitPrice)
                      },
                ss => from p in ss.Set<ProductForLinq>()
                      group p by p.Category
                      into g
                      select new
                      {
                          Category = g.Key,
                          AveragePrice = Math.Round(g.Average(p => p.UnitPrice) - 0.0000005m, 6)
                      });
        }

        [ConditionalTheory (Skip = "Issue #19930")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_Join_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<CustomerForLinq>()
                      join o in ss.Set<OrderForLinq>() on c equals o.Customer into ps
                      select new
                      {
                          Customer = c,
                          Products = ps
                      });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cross_Join_with_Group_Join_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<CustomerForLinq>()
                      join o in ss.Set<OrderForLinq>() on c equals o.Customer into ps
                      from o in ps
                      select new
                      {
                          Customer = c,
                          o.Id
                      },
                ss => from c in ss.Set<CustomerForLinq>()
                      join o in ss.Set<OrderForLinq>() on c.Id equals o.Customer.Id into ps
                      from o in ps
                      select new
                      {
                          Customer = c,
                          o.Id
                      },
                r => (r.Id, r.Customer.Id),
                (l, r) =>
                {
                    Assert.Equal(l.Id, r.Id);
                    Assert.Equal(l.Customer.Id, r.Customer.Id);
                    Assert.Equal(l.Customer.Region, r.Customer.Region);
                    Assert.Equal(l.Customer.CompanyName, r.Customer.CompanyName);
                },
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_Outer_Join_with_Group_Join_from_LINQ_101(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<CustomerForLinq>().Include(e => e.Orders)
                      join o in ss.Set<OrderForLinq>() on c equals o.Customer into ps
                      from o in ps.DefaultIfEmpty()
                      select new
                      {
                          Customer = c,
                          OrderId = o == null ? -1 : o.Id
                      },
                ss => from c in ss.Set<CustomerForLinq>()
                      join o in ss.Set<OrderForLinq>() on c.Id equals o.Customer.Id into ps
                      from o in ps.DefaultIfEmpty()
                      select new
                      {
                          Customer = c,
                          OrderId = o == null ? -1 : o.Id
                      },
                r => (r.OrderId, r.Customer.Id),
                (l, r) =>
                {
                    Assert.Equal(l.OrderId, r.OrderId);
                    AssertEqual(l.Customer, r.Customer);
                },
                entryCount: 11);
        }

        protected ArubaContext CreateContext()
            => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        public abstract class Ef6GroupByFixtureBase : SharedStoreFixtureBase<ArubaContext>, IQueryFixtureBase
        {
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
            }

            protected override void Seed(ArubaContext context)
            {
                new ArubaData(context);
            }

            public virtual ISetSource GetExpectedData()
                => new ArubaData();

            public IReadOnlyDictionary<Type, object> GetEntitySorters()
                => new Dictionary<Type, Func<object, object>>
                {
                    { typeof(CustomerForLinq), e => ((CustomerForLinq)e)?.Id },
                    { typeof(OrderForLinq), e => ((OrderForLinq)e)?.Id },
                }.ToDictionary(e => e.Key, e => (object)e.Value);

            public IReadOnlyDictionary<Type, object> GetEntityAsserters()
                => new Dictionary<Type, Action<object, object>>
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
                    }
                }.ToDictionary(e => e.Key, e => (object)e.Value);
        }

        public class ArubaContext : PoolableDbContext
        {
            public ArubaContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        public class ArubaOwner
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Alias { get; set; }
        }

        public class NumberForLinq
        {
            public NumberForLinq(int value, string name)
            {
                Value = value;
                Name = name;
            }

            public int Id { get; set; }
            public int Value { get; set; }
            public string Name { get; set; }
        }

        public class ProductForLinq
        {
            public int Id { get; set; }
            public string ProductName { get; set; }
            public string Category { get; set; }
            public decimal UnitPrice { get; set; }
            public int UnitsInStock { get; set; }
        }

        public class FeaturedProductForLinq : ProductForLinq
        {
        }

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

        public class ArubaData : ISetSource
        {
            public IReadOnlyList<ArubaOwner> ArubaOwners { get; }
            public IReadOnlyList<NumberForLinq> NumbersForLinq { get; }
            public IReadOnlyList<ProductForLinq> ProductsForLinq { get; }
            public IReadOnlyList<CustomerForLinq> CustomersForLinq { get; }
            public IReadOnlyList<OrderForLinq> OrdersForLinq { get; }

            public ArubaData(ArubaContext context = null)
            {
                ArubaOwners = CreateArubaOwners();
                NumbersForLinq = CreateNumbersForLinq();
                ProductsForLinq = CreateProductsForLinq();
                CustomersForLinq = CreateCustomersForLinq();
                OrdersForLinq = CreateOrdersForLinq(CustomersForLinq);

                if (context != null)
                {
                    context.AddRange(ArubaOwners);
                    context.AddRange(NumbersForLinq);
                    context.AddRange(ProductsForLinq);
                    context.AddRange(CustomersForLinq);
                    context.AddRange(OrdersForLinq);
                    context.SaveChanges();
                }
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
        }
   }
}

