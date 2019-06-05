// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class ConvertToProviderTypesSqliteTest : ConvertToProviderTypesTestBase<
        ConvertToProviderTypesSqliteTest.ConvertToProviderTypesSqliteFixture>
    {
        public ConvertToProviderTypesSqliteTest(ConvertToProviderTypesSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
            //fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class ConvertToProviderTypesSqliteFixture : ConvertToProviderTypesFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            public override bool SupportsDecimalComparisons => false;

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();
        }
    }
}
