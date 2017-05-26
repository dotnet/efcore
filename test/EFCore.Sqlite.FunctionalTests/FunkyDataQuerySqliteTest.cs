// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microsoft.EntityFrameworkCore
{
    public class FunkyDataQuerySqliteTest : FunkyDataQueryTestBase<SqliteTestStore, FunkyDataQuerySqliteFixture>
    {
        public FunkyDataQuerySqliteTest(FunkyDataQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        [Xunit.Fact]
        public override void String_starts_with_on_argument_with_wildcard_column()
        {
            base.String_starts_with_on_argument_with_wildcard_column();

    }
    }
}
