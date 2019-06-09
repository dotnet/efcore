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
    public abstract class ConcurrencyDetectorRelationalTestBase<TFixture> : ConcurrencyDetectorTestBase<TFixture>
        where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
    {
        protected ConcurrencyDetectorRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual Task FromSql_logs_concurrent_access_nonasync()
        {
            return ConcurrencyDetectorTest(
                c =>
                {
                    // ReSharper disable once UnusedVariable
                    var result = c.Products.FromSqlRaw("select * from products").ToList();
                    return Task.FromResult(false);
                });
        }

        [ConditionalFact]
        public virtual Task FromSql_logs_concurrent_access_async()
        {
            return ConcurrencyDetectorTest(c => c.Products.FromSqlRaw("select * from products").ToListAsync());
        }
    }
}
