// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Relational.Specification.Tests;
using Xunit;
using Xunit.Abstractions;

#if NETCOREAPP1_1
using System.Threading;
#endif
namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class EfFunctionsSqliteTest : RelationalEfFunctionsTestBase<NorthwindQuerySqliteFixture>
    {
        public EfFunctionsSqliteTest(NorthwindQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void String_Like_Literal()
        {
            using (var context = CreateContext())
            {
                var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "%M%"));
                Assert.Equal(34, count);
            }
        }
    }
}
