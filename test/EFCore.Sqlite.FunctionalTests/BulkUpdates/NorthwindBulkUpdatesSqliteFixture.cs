// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class NorthwindBulkUpdatesSqliteFixture<TModelCustomizer> : NorthwindBulkUpdatesFixture<TModelCustomizer>
    where TModelCustomizer : ITestModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override Type ContextType
        => typeof(NorthwindSqliteContext);
}
