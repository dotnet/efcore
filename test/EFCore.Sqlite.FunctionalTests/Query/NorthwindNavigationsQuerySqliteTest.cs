// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindNavigationsQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture) : NorthwindNavigationsQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>(fixture);
