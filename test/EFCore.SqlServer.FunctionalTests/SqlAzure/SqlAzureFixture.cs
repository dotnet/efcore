// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.SqlAzure.Model;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.SqlAzure
{
    public class SqlAzureFixture : SharedStoreFixtureBase<AdventureWorksContext>
    {
        protected override string StoreName { get; } = "adventureworks";
        protected override ITestStoreFactory TestStoreFactory => SqlServerAdventureWorksTestStoreFactory.Instance;
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
