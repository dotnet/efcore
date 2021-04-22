// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsQueryRelationalTestBase<TFixture> : ComplexNavigationsQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsQueryFixtureBase, new()
    {
        protected ComplexNavigationsQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Projecting_collection_with_FirstOrDefault_split_throws(bool async)
        {
            Assert.Equal(
                RelationalStrings.UnableToSplitCollectionProjectionInSplitQuery(
                    "QuerySplittingBehavior.SplitQuery", "AsSplitQuery", "AsSingleQuery"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertFirstOrDefault(
                        async,
                        ss => ss.Set<Level1>()
                            .AsSplitQuery()
                            .Select(e => new { e.Id, Level2s = e.OneToMany_Optional1.ToList() }),
                        predicate: l => l.Id == 1,
                        asserter: (e, a) =>
                        {
                            Assert.Equal(e.Id, a.Id);
                            AssertCollection(e.Level2s, a.Level2s);
                        }))).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Projecting_collection_with_FirstOrDefault_without_split_works(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Level1>()
                    .Select(e => new { e.Id, Level2s = e.OneToMany_Optional1.ToList() }),
                predicate: l => l.Id == 1,
                asserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    AssertCollection(e.Level2s, a.Level2s);
                });
        }

        public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool async)
        {
            return AssertTranslationFailed(() => base.Complex_query_with_optional_navigations_and_client_side_evaluation(async));
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
