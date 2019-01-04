// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryFilterFuncletizationRelationalFixture : QueryFilterFuncletizationFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            base.AddOptions(builder);
            return builder.ConfigureWarnings(w => w.Ignore(RelationalEventId.QueryClientEvaluationWarning));
        }
    }
}
