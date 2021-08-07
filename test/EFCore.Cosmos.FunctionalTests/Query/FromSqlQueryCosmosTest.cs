// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlQueryCosmosTest : QueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
    {
        public FromSqlQueryCosmosTest(
            NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_simple(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>()
                .FromSqlRaw(@"SELECT *
                    FROM root c
                    WHERE c[""Discriminator""] = ""Customer"" AND c[""ContactName""] LIKE '%z%'");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(14, actual.Length);
            Assert.Equal(14, context.ChangeTracker.Entries().Count());

            AssertSql(@"SELECT c
FROM (
SELECT *
                    FROM root c
                    WHERE c[""Discriminator""] = ""Customer"" AND c[""ContactName""] LIKE '%z%') c
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_simple_columns_out_of_order(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT c[id], c[Region], c[PostalCode], c[Phone], c[Fax], c[CustomerID], c[Country], c[ContactTitle], c[ContactName], c[CompanyName], c[City], c[Address]
                        FROM root c
                        WHERE c[Discriminator] = ""Customer"""));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(91, actual.Length);
            Assert.Equal(91, context.ChangeTracker.Entries().Count());
            AssertSql(@"SELECT c
FROM (
SELECT c[""id""], c[""Region""], c[""PostalCode""], c[""Phone""], c[""Fax""], c[""CustomerID""], c[""Country""], c[""ContactTitle""], c[""ContactName""], c[""CompanyName""], c[""City""], c[""Address""]
                        FROM root c
                        WHERE c[""Discriminator""] = ""Customer"") c
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_simple_columns_out_of_order_and_extra_columns(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT c[id], c[Region], c[PostalCode], c[PostalCode] AS Foo, c[Phone], c[Fax], c[CustomerID], c[Country], c[ContactTitle], c[ContactName], c[CompanyName], c[City], c[Address]
                        FROM root c
                        WHERE c[Discriminator] = ""Customer"""));

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(91, actual.Length);
            Assert.Equal(91, context.ChangeTracker.Entries().Count());

            AssertSql(@"SELECT c
FROM (
SELECT c[""id""], c[""Region""], c[""PostalCode""], c[""PostalCode""] AS Foo, c[""Phone""], c[""Fax""], c[""CustomerID""], c[""Country""], c[""ContactTitle""], c[""ContactName""], c[""CompanyName""], c[""City""], c[""Address""]
                        FROM root c
                        WHERE c[""Discriminator""] = ""Customer"") c
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_composed(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"""))
                .Where(c => c.ContactName.Contains("z"));

            var sql = query.ToQueryString();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(14, actual.Length);
            Assert.Equal(14, context.ChangeTracker.Entries().Count());

            AssertSql(@"SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Customer"") c
WHERE CONTAINS(c[""ContactName""], ""z"")");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_composed_compiled(bool async)
        {
            if (async)
            {
                var query = EF.CompileAsyncQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"""))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = await query(context).ToListAsync();

                    Assert.Equal(14, actual.Count);
                }
            }
            else
            {
                var query = EF.CompileQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"""))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = query(context).ToArray();

                    Assert.Equal(14, actual.Length);
                }
            }

            AssertSql(@"SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Customer"") c
WHERE CONTAINS(c[""ContactName""], ""z"")");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_composed_compiled_with_DbParameter(bool async)
        {
            if (async)
            {
                var query = EF.CompileAsyncQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"" AND c[CustomerID] = @customer"),
                            CreateDbParameter("@customer", "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = await query(context).ToListAsync();

                    Assert.Single(actual);
                }
            }
            else
            {
                var query = EF.CompileQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"" AND c[CustomerID] = @customer"),
                            CreateDbParameter("@customer", "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = query(context).ToArray();

                    Assert.Single(actual);
                }
            }

            AssertSql(@"@customer='CONSH'

SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Customer"" AND c[""CustomerID""] = @customer) c
WHERE CONTAINS(c[""ContactName""], ""z"")");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_composed_compiled_with_nameless_DbParameter(bool async)
        {
            if (async)
            {
                var query = EF.CompileAsyncQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"" AND c[CustomerID] = {0}"),
                            CreateDbParameter(null, "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = await query(context).ToListAsync();

                    Assert.Single(actual);
                }
            }
            else
            {
                var query = EF.CompileQuery(
                    (NorthwindContext context) => context.Set<Customer>()
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"" AND c[CustomerID] = {0}"),
                            CreateDbParameter(null, "CONSH"))
                        .Where(c => c.ContactName.Contains("z")));

                using (var context = CreateContext())
                {
                    var actual = query(context).ToArray();

                    Assert.Single(actual);
                }
            }

            AssertSql(@"@p0='CONSH'

SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Customer"" AND c[""CustomerID""] = @p0) c
WHERE CONTAINS(c[""ContactName""], ""z"")");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_with_parameters(bool async)
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"" AND c[City] = {0} AND c[ContactTitle] = {1}"), city,
                    contactTitle);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

            AssertSql(@"@p0='London'
@p1='Sales Representative'

SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Customer"" AND c[""City""] = @p0 AND c[""ContactTitle""] = @p1) c
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_with_parameters_inline(bool async)
        {
            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"" AND c[City] = {0} AND c[ContactTitle] = {1}"), "London",
                    "Sales Representative");

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

            AssertSql(@"@p0='London'
@p1='Sales Representative'

SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Customer"" AND c[""City""] = @p0 AND c[""ContactTitle""] = @p1) c
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_with_null_parameter(bool async)
        {
            uint? reportsTo = null;

            using var context = CreateContext();
            var query = context.Set<Employee>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT * FROM root c WHERE c[Discriminator] = ""Employee"" AND c[ReportsTo] = {0} OR (IS_NULL(c[ReportsTo]) AND IS_NULL({0}))"), reportsTo);

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Single(actual);

            AssertSql(@"@p0=null

SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Employee"" AND c[""ReportsTo""] = @p0 OR (IS_NULL(c[""ReportsTo""]) AND IS_NULL(@p0))) c
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public async Task FromSqlRaw_queryable_with_parameters_and_closure(bool async)
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            using var context = CreateContext();
            var query = context.Set<Customer>().FromSqlRaw(
                    NormalizeDelimitersInRawString(@"SELECT * FROM root c WHERE c[Discriminator] = ""Customer"" AND c[City] = {0}"), city)
                .Where(c => c.ContactTitle == contactTitle);
            var queryString = query.ToQueryString();

            var actual = async
                ? await query.ToArrayAsync()
                : query.ToArray();

            Assert.Equal(3, actual.Length);
            Assert.True(actual.All(c => c.City == "London"));
            Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

            AssertSql(@"@p0='London'
@__contactTitle_1='Sales Representative'

SELECT c
FROM (
SELECT * FROM root c WHERE c[""Discriminator""] = ""Customer"" AND c[""City""] = @p0) c
WHERE (c[""ContactTitle""] = @__contactTitle_1)");
        }

        protected DbParameter CreateDbParameter(string name, object value)
            => new SqlParameter { ParameterName = name, Value = value };

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        public virtual string NormalizeDelimitersInRawString(string sql)
            => sql.Replace("[", OpenDelimiter).Replace("]", CloseDelimiter);

        protected virtual string OpenDelimiter
            => "[\"";

        protected virtual string CloseDelimiter
            => "\"]";
    }
}
