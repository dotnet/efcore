// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class WarningsAsErrorsSqlServerFixture : NorthwindQuerySqlServerFixture
    {
        protected override void ConfigureOptions(SqlServerDbContextOptionsBuilder sqlServerDbContextOptionsBuilder)
            => sqlServerDbContextOptionsBuilder
                .QueryClientEvaluationBehavior(QueryClientEvaluationBehavior.Warn)
                .SetWarningsAsErrors(RelationalLoggingEventId.QueryClientEvaluationWarning);
    }
}
