// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindSplitIncludeNoTrackingQuerySqlServerTest : NorthwindSplitIncludeNoTrackingQueryTestBase<NorthwindSplitIncludeNoTrackingQuerySqlServerTest.NorthwindQuerySqlServerMARSFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public NorthwindSplitIncludeNoTrackingQuerySqlServerTest(NorthwindQuerySqlServerMARSFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class NorthwindQuerySqlServerMARSFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerNorthwindMARSTestStoreFactory.Instance;
        }

        private class SqlServerNorthwindMARSTestStoreFactory : SqlServerNorthwindTestStoreFactory
        {
            public static new SqlServerNorthwindMARSTestStoreFactory Instance { get; } = new SqlServerNorthwindMARSTestStoreFactory();

            protected SqlServerNorthwindMARSTestStoreFactory()
            {
            }

            public override TestStore GetOrCreate(string storeName)
                => SqlServerTestStore.GetOrCreate(Name, "Northwind.sql", multipleActiveResultSets: true);
        }
    }
}
