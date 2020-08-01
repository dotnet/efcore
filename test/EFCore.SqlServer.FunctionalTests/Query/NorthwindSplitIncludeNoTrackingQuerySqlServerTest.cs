// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSplitIncludeNoTrackingQuerySqlServerTest : NorthwindSplitIncludeNoTrackingQueryTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindSplitIncludeNoTrackingQuerySqlServerTest(NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Include_collection_with_last_no_orderby(bool async)
        {
            var expectedMessage = CoreStrings.TranslationFailedWithDetails("DbSet<Customer>()    .Reverse()",
                RelationalStrings.MissingOrderingInSqlExpression);

            var exception = (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertLast(
                        async,
                        ss => ss.Set<Customer>().Include(c => c.Orders),
                        entryCount: 8)));

            Assert.Equal(
                expectedMessage,
                exception.Message.Replace("\r","").Replace("\n",""));
        }
    }
}
