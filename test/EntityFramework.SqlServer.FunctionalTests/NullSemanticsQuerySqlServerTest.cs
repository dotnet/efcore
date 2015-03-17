// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class NullSemanticsQuerySqlServerTest : NullSemanticsQueryTestBase<SqlServerTestStore, NullSemanticsQuerySqlServerFixture>
    {
        public NullSemanticsQuerySqlServerTest(NullSemanticsQuerySqlServerFixture fixture)
            : base(fixture)
        {
        }

        public override void Compare_bool_with_bool_equal()
        {
            base.Compare_bool_with_bool_equal();

            Assert.Equal(
                @"TBD",
                Sql);
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}

