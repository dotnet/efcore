// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class SimpleQueryRelationalTestBase : SimpleQueryTestBase
    {
        protected TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected void ClearLog() => TestSqlLoggerFactory.Clear();

        protected void AssertSql(params string[] expected) => TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
