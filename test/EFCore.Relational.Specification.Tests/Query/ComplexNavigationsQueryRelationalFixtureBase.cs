// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ComplexNavigationsQueryRelationalFixtureBase : ComplexNavigationsQueryFixtureBase, ITestSqlLoggerFactory
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
                c => c
                    .Log(CoreEventId.DistinctAfterOrderByWithoutRowLimitingOperatorWarning)
                    .Log(CoreEventId.FirstWithoutOrderByAndFilterWarning))
            .EnableDetailedErrors();
}
