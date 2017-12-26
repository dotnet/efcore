// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class CustomConvertersOracleTest : CustomConvertersTestBase<CustomConvertersOracleTest.CustomConvertersOracleFixture>
    {
        public CustomConvertersOracleTest(CustomConvertersOracleFixture fixture)
            : base(fixture)
        {
        }

        public override void Can_perform_query_with_max_length()
        {
            // Disabled--sample Oracle cannot query against large data types
        }

        public class CustomConvertersOracleFixture : CustomConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

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
