// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class NorthwindRowNumberPagingQuerySqlServerFixture : NorthwindQuerySqlServerFixture
    {
        protected override DbContextOptions ConfigureOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder(base.ConfigureOptions());
            new SqlServerDbContextOptionsBuilder(optionsBuilder).UseRowNumberForPaging();
            return optionsBuilder.Options;
        }
    }
}
