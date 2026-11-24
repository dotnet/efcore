// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class EFCoreSqlServerApiBaselineTest
{
    [Fact]
    public void ApiSurfaceMatchesBaseline()
        => ApiBaselineTest.AssertBaselineMatch(
            "EFCore.SqlServer", "Microsoft.EntityFrameworkCore.SqlServer.dll");
}
