// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQuerySqliteTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQuerySqliteFixture>
    {
        public ComplexNavigationsQuerySqliteTest(ComplexNavigationsQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public override async Task Let_let_contains_from_outer_let(bool async)
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Let_let_contains_from_outer_let(async))).Message);

        public override void Member_pushdown_chain_3_levels_deep_entity()
            => Assert.Equal(
                SqliteStrings.ApplyNotSupported,
                (Assert.Throws<InvalidOperationException>(
                    () => base.Member_pushdown_chain_3_levels_deep_entity())).Message);
    }
}
