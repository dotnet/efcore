// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class ConcurrencyDetectorEnabledRelationalTestBase<TFixture> : ConcurrencyDetectorEnabledTestBase<TFixture>
        where TFixture : ConcurrencyDetectorTestBase<TFixture>.ConcurrencyDetectorFixtureBase, new()
    {
        protected ConcurrencyDetectorEnabledRelationalTestBase(TFixture fixture)
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
