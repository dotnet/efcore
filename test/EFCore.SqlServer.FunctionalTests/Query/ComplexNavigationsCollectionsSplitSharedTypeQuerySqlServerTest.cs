﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsCollectionsSplitSharedTypeQuerySqlServerTest : ComplexNavigationsCollectionsSplitSharedQueryTypeRelationalTestBase<
        ComplexNavigationsSharedTypeQuerySqlServerFixture>
    {
        public ComplexNavigationsCollectionsSplitSharedTypeQuerySqlServerTest(
            ComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
