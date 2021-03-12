// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class ConcurrencyDetectorDisabledRelationalTestBase<TFixture> : ConcurrencyDetectorDisabledTestBase<TFixture>
        where TFixture : ConcurrencyDetectorTestBase<TFixture>.ConcurrencyDetectorFixtureBase, new()
    {
        protected ConcurrencyDetectorDisabledRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FromSql(bool async)
            => ConcurrencyDetectorTest(async c => async
                ? await c.Products.FromSqlRaw("select * from products").ToListAsync()
                : c.Products.FromSqlRaw("select * from products").ToList());
    }
}
