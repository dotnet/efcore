// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindCompiledQuerySqliteTest : NorthwindCompiledQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindCompiledQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        public override void MakeBinary_does_not_throw_for_unsupported_operator()
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == (string)(__parameters[0]))"),
                Assert.Throws<InvalidOperationException>(
                    () => base.MakeBinary_does_not_throw_for_unsupported_operator()).Message.Replace("\r", "").Replace("\n", ""));
        }
    }
}
