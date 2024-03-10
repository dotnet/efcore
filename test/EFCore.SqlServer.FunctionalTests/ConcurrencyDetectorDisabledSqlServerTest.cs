// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConcurrencyDetectorDisabledSqlServerTest : ConcurrencyDetectorDisabledRelationalTestBase<
    ConcurrencyDetectorDisabledSqlServerTest.ConcurrencyDetectorSqlServerFixture>
{
    public ConcurrencyDetectorDisabledSqlServerTest(ConcurrencyDetectorSqlServerFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    protected override async Task ConcurrencyDetectorTest(Func<ConcurrencyDetectorDbContext, Task<object>> test)
    {
        await base.ConcurrencyDetectorTest(test);

        Assert.NotEmpty(Fixture.TestSqlLoggerFactory.SqlStatements);
    }

    public class ConcurrencyDetectorSqlServerFixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.EnableThreadSafetyChecks(enableChecks: false);
    }
}
