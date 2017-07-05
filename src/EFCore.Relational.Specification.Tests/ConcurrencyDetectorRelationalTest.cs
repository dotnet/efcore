// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ConcurrencyDetectorRelationalTest<TFixture> : ConcurrencyDetectorTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        protected ConcurrencyDetectorRelationalTest(TFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public virtual async Task FromSql_logs_concurrent_access_nonasync()
        {
            await ConcurrencyDetectorTest(
                c =>
                    {
                        c.Products.FromSql("select * from products").ToList();
                        return Task.FromResult(false);
                    },
                async: false);
        }

        [Fact]
        public virtual async Task FromSql_logs_concurrent_access_async()
        {
            await ConcurrencyDetectorTest(c => c.Products.FromSql("select * from products").ToListAsync(), async: true);
        }
    }
}
