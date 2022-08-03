// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public class TPCInheritanceQuerySqlServerTest : TPCInheritanceQuerySqlServerTestBase<TPCInheritanceQuerySqlServerFixture>
{
    public TPCInheritanceQuerySqlServerTest(TPCInheritanceQuerySqlServerFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }
}
