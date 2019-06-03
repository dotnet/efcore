// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeSqlServerFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder);

        protected override bool ShouldLogCategory(string logCategory)
            => base.ShouldLogCategory(logCategory) || logCategory == DbLoggerCategory.Query.Name;
    }
}
