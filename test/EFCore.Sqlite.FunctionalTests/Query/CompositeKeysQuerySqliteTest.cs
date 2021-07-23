﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompositeKeysQuerySqliteTest : CompositeKeysQueryRelationalTestBase<CompositeKeysQuerySqliteFixture>
    {
        public CompositeKeysQuerySqliteTest(CompositeKeysQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }
    }
}
