// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.FSharp.FunctionalTests

open System.Linq
open Microsoft.EntityFrameworkCore.Query
open Microsoft.EntityFrameworkCore.TestModels.Northwind
open Microsoft.EntityFrameworkCore.TestUtilities
open global.Xunit

type NorthwindQueryFSharpTest(fixture) as self =
    inherit QueryTestBase<NorthwindFSharpQuerySqlServerFixture<NoopModelCustomizer>>(fixture)

    do fixture.TestSqlLoggerFactory.Clear()

    let assertSql (sql: string) =
        fixture.TestSqlLoggerFactory.AssertBaseline([|sql|])

    [<ConditionalTheory>]
    [<MemberData(nameof NorthwindQueryFSharpTest.IsAsyncData)>]
    let ListLiteral_Contains (isAsync: bool) =
        task {
            do! self.AssertQuery(isAsync, (fun ss -> ss.Set<Customer>().Where(fun c -> ["ALFKI"; "ALFKI2"].Contains(c.CustomerID))))
            assertSql(
                "SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ALFKI', N'ALFKI2')")
        }
        
    member private self.RewriteExpectedQueryExpressionRedirect expression = base.RewriteExpectedQueryExpression expression
    member private self.RewriteServerQueryExpressionRedirect expression = base.RewriteServerQueryExpression expression

    override self.CreateQueryAsserter fixture =
        new RelationalQueryAsserter(
            fixture,
            (fun e -> self.RewriteExpectedQueryExpressionRedirect(e)),
            (fun e -> self.RewriteServerQueryExpressionRedirect(e)))
