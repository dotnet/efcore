// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindQueryTaggingQuerySqliteTest : NorthwindQueryTaggingQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindQueryTaggingQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }
}
