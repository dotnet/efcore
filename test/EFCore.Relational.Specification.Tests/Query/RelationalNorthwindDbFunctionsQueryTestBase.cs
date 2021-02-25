// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindDbFunctionsQueryRelationalTestBase<TFixture> : NorthwindDbFunctionsQueryTestBase<TFixture>
        where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
    {
        public NorthwindDbFunctionsQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collate_case_insensitive(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Collate(c.ContactName, CaseInsensitiveCollation) == "maria anders",
                c => c.ContactName.Equals("maria anders", StringComparison.OrdinalIgnoreCase));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collate_case_sensitive(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Collate(c.ContactName, CaseSensitiveCollation) == "maria anders",
                c => c.ContactName.Equals("maria anders", StringComparison.Ordinal));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collate_case_sensitive_constant(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => c.ContactName == EF.Functions.Collate("maria anders", CaseSensitiveCollation),
                c => c.ContactName.Equals("maria anders", StringComparison.Ordinal));

        protected abstract string CaseInsensitiveCollation { get; }
        protected abstract string CaseSensitiveCollation { get; }
    }
}
