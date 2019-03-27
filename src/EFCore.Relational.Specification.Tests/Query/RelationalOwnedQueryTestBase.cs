// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class RelationalOwnedQueryTestBase<TFixture> : OwnedQueryTestBase<TFixture>
        where TFixture : RelationalOwnedQueryTestBase<TFixture>.RelationalOwnedQueryFixture, new()
    {
        protected RelationalOwnedQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public abstract class RelationalOwnedQueryFixture : OwnedQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(c => c.Log(RelationalEventId.QueryClientEvaluationWarning));
        }
    }
}
