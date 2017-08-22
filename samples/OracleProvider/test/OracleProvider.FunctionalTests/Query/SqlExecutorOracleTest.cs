// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Oracle.ManagedDataAccess.Client;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SqlExecutorOracleTest : SqlExecutorTestBase<NorthwindQueryOracleFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public SqlExecutorOracleTest(NorthwindQueryOracleFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Query_with_dbParameter_with_name()
        {
            var city = CreateDbParameter(":city", "London");

            using (var context = CreateContext())
            {
                var actual = context.Database
                    .ExecuteSqlCommand(
                        @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = :city", city);

                Assert.Equal(-1, actual);
            }
        }

        public override void Query_with_dbParameters_mixed()
        {
            var city = "London";
            var contactTitle = "Sales Representative";

            var cityParameter = CreateDbParameter(":city", city);
            var contactTitleParameter = CreateDbParameter(":contactTitle", contactTitle);

            using (var context = CreateContext())
            {
                var actual = context.Database
                    .ExecuteSqlCommand(
                        @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = :contactTitle", city, contactTitleParameter);

                Assert.Equal(-1, actual);

                actual = context.Database
                    .ExecuteSqlCommand(
                        @"SELECT COUNT(*) FROM ""Customers"" WHERE ""City"" = :city AND ""ContactTitle"" = {1}", cityParameter, contactTitle);

                Assert.Equal(-1, actual);
            }
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new OracleParameter
            {
                ParameterName = name?.Replace("@", ":"),
                Value = value
            };

        protected override string TenMostExpensiveProductsSproc => "BEGIN \"SimpleProcedure\"; END;";
        protected override string CustomerOrderHistorySproc => "BEGIN \"SimpleProcedure2\"(:CustomerID); END;";
        protected override string CustomerOrderHistoryWithGeneratedParameterSproc => "BEGIN \"SimpleProcedure2\"(:p0); END;";
    }
}
