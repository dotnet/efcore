// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSplitIncludeNoTrackingQuerySqlServerTest : NorthwindSplitIncludeNoTrackingQueryTestBase<NorthwindSplitIncludeNoTrackingQuerySqlServerTest.NorthwindQuerySqlServerMarsEnabledFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindSplitIncludeNoTrackingQuerySqlServerTest(NorthwindQuerySqlServerMarsEnabledFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class NorthwindQuerySqlServerMarsEnabledFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerNorthwindMarsEnabledTestStoreFactory.Instance;
        }

        private class SqlServerNorthwindMarsEnabledTestStoreFactory : SqlServerNorthwindTestStoreFactory
        {
            public static new SqlServerNorthwindMarsEnabledTestStoreFactory Instance { get; } = new SqlServerNorthwindMarsEnabledTestStoreFactory();

            protected SqlServerNorthwindMarsEnabledTestStoreFactory()
            {
            }

            public override TestStore GetOrCreate(string storeName)
                => SqlServerTestStore.GetOrCreate(Name, "Northwind.sql", multipleActiveResultSets: true);
        }
    }
}
