// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public abstract class SqlServerTypeFixture<T> : RelationalTypeFixtureBase<T>
{
    protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => TestEnvironment.SetCompatibilityLevelFromEnvironment(base.AddOptions(builder));

    public virtual bool UsingJsonType
        => TestEnvironment.SqlServerMajorVersion >= 17;
}
