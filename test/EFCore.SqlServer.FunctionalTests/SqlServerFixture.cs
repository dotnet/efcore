// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerFixture : ServiceProviderFixtureBase
{
    public static IServiceProvider DefaultServiceProvider { get; }
        = new ServiceCollection().AddEntityFrameworkSqlServer().BuildServiceProvider(validateScopes: true);

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w =>
            {
                w.Log(SqlServerEventId.ByteIdentityColumnWarning);
                w.Log(SqlServerEventId.DecimalTypeKeyWarning);
            });
}
