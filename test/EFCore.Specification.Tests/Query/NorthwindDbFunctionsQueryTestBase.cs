// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindDbFunctionsQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindDbFunctionsQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Like_literal(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Like(c.ContactName, "%M%"),
                c => c.ContactName.Contains("M") || c.ContactName.Contains("m"));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Like_identity(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Like(c.ContactName, c.ContactName),
                c => true);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Like_literal_with_escape(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Like(c.ContactName, "!%", "!"),
                c => c.ContactName.Contains("%"));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Like_all_literals(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Like("FOO", "%O%"),
                c => "FOO".Contains("O") || "FOO".Contains("m"));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Like_all_literals_with_escape(bool async)
            => AssertCount(
                async,
                ss => ss.Set<Customer>(),
                ss => ss.Set<Customer>(),
                c => EF.Functions.Like("%", "!%", "!"),
                c => "%".Contains("%"));

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();
    }
}
