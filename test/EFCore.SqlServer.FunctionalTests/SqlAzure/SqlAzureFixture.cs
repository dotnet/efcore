// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.SqlAzure;

public class SqlAzureFixture : SharedStoreFixtureBase<AdventureWorksContext>
{
    protected override string StoreName { get; } = "adventureworks";

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerAdventureWorksTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
