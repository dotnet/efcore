// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public class OwnedJsonSqlServerFixture : OwnedJsonRelationalFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    // When testing against SQL Server 2025 or later, set the compatibility level to 170 to use the json type instead of nvarchar(max).
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var options = base.AddOptions(builder);
        return TestEnvironment.SqlServerMajorVersion < 17
            ? options
            : options.UseSqlServerCompatibilityLevel(170);
    }

    public virtual bool UsingJsonType
        => TestEnvironment.SqlServerMajorVersion >= 17;
}
