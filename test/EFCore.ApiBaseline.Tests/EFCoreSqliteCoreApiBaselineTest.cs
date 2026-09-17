// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class EFCoreSqliteCoreApiBaselineTest
{
    [Fact]
    public void ApiSurfaceMatchesBaseline()
        => ApiBaselineTest.AssertBaselineMatch(
            "EFCore.Sqlite.Core", "Microsoft.EntityFrameworkCore.Sqlite.dll");
}
