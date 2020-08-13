// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FromSqlQuerySqliteTest : FromSqlQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public FromSqlQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Bad_data_error_handling_invalid_cast_key()
        {
            // Not supported on SQLite
        }

        public override void Bad_data_error_handling_invalid_cast()
        {
            // Not supported on SQLite
        }

        public override void Bad_data_error_handling_invalid_cast_projection()
        {
            // Not supported on SQLite
        }

        public override void Bad_data_error_handling_invalid_cast_no_tracking()
        {
            // Not supported on SQLite
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new SqliteParameter { ParameterName = name, Value = value };
    }
}
