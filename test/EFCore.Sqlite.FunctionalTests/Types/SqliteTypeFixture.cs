// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public abstract class SqliteTypeFixture<T> : RelationalTypeFixtureBase<T>
{
    protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
}
