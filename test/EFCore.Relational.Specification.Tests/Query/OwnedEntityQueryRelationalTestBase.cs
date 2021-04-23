// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedEntityQueryRelationalTestBase : OwnedEntityQueryTestBase
    {
        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected void ClearLog() => TestSqlLoggerFactory.Clear();

        protected void AssertSql(params string[] expected) => TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
