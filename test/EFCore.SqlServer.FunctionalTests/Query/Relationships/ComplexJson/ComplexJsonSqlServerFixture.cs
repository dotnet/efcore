// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonSqlServerFixture : ComplexJsonRelationalFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    // When testing against SQL Server 2025 or later, set the compatibility level to 170 to use the json type instead of nvarchar(max).
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => TestEnvironment.SqlServerMajorVersion >= 17
            ? builder.UseSqlServer(o => o.UseCompatibilityLevel(170))
            : builder;

    public virtual bool UsingJsonType
        => TestEnvironment.SqlServerMajorVersion >= 17;
}
