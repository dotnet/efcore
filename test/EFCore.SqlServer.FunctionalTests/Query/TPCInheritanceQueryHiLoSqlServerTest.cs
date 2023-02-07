// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TPCInheritanceQueryHiLoSqlServerTest : TPCInheritanceQuerySqlServerTestBase<TPCInheritanceQueryHiLoSqlServerFixture>
{
    public TPCInheritanceQueryHiLoSqlServerTest(TPCInheritanceQueryHiLoSqlServerFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }
}
