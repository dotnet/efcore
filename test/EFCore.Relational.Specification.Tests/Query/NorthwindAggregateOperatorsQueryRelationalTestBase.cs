// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindAggregateOperatorsQueryRelationalTestBase<TFixture> : NorthwindAggregateOperatorsQueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindAggregateOperatorsQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override async Task Last_when_no_order_by(bool async)
        {
            Assert.Equal(
                RelationalStrings.LastUsedWithoutOrderBy(nameof(Enumerable.Last)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Last_when_no_order_by(async))).Message);
        }

        public override async Task LastOrDefault_when_no_order_by(bool async)
        {
            Assert.Equal(
                RelationalStrings.LastUsedWithoutOrderBy(nameof(Enumerable.LastOrDefault)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.LastOrDefault_when_no_order_by(async))).Message);
        }

        public override Task Contains_with_local_tuple_array_closure(bool async)
        {
            return AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture,
                RewriteExpectedQueryExpression,
                RewriteServerQueryExpression,
                canExecuteQueryString: CanExecuteQueryString);
    }
}
