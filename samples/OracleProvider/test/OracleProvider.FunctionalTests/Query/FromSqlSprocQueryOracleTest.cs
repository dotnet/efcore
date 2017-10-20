// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlSprocQueryOracleTest : FromSqlSprocQueryTestBase<NorthwindQueryOracleFixture<NoopModelCustomizer>>
    {
        public FromSqlSprocQueryOracleTest(
            NorthwindQueryOracleFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override object[] GetCustomerOrderHistorySprocParameters()
        {
            return new[]
            {
                new OracleParameter(":p0","ALFKI"),

                new OracleParameter(
                    "cur",
                    OracleDbType.RefCursor,
                    DBNull.Value,
                    ParameterDirection.Output)
            };
        }

        protected override object[] GetTenMostExpensiveProductsParameters()
        {
            return new[]
            {
                new OracleParameter(
                        "cur",
                        OracleDbType.RefCursor,
                        DBNull.Value,
                        ParameterDirection.Output)
            };
        }

        protected override string TenMostExpensiveProductsSproc
            => "BEGIN \"Ten Most Expensive Products\"(:cur); END;";

        protected override string CustomerOrderHistorySproc
            => "BEGIN \"CustOrderHist\"(:p0, :cur); END;";
    }
}
