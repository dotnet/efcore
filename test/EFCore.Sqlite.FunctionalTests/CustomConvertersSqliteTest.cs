// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class CustomConvertersSqliteTest : CustomConvertersTestBase<CustomConvertersSqliteTest.CustomConvertersSqliteFixture>
    {
        public CustomConvertersSqliteTest(CustomConvertersSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class CustomConvertersSqliteFixture : CustomConvertersFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => false;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base
                    .AddOptions(builder)
                    .ConfigureWarnings(
                        c => c.Log(RelationalEventId.QueryClientEvaluationWarning));
        }
    }
}
