// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class GearsOfWarQueryRelationalTestBase<TFixture> : GearsOfWarQueryTestBase<TFixture>
        where TFixture : GearsOfWarQueryFixtureBase, new()
    {
        protected GearsOfWarQueryRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Correlated_collection_with_Distinct_missing_indentifying_columns_in_projection(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                async,
                ss => ss.Set<Gear>()
                    .OrderBy(g => g.Nickname)
                    .Select(g => g.Weapons.SelectMany(x => x.Owner.AssignedCity.BornGears)
                    .Select(x => (bool?)x.HasSoulPatch).Distinct().ToList())))).Message;

            Assert.Equal(RelationalStrings.MissingIdentifyingProjectionInDistinctGroupBySubquery("w.Id"), message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Correlated_collection_with_GroupBy_missing_indentifying_columns_in_projection(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                async,
                ss => ss.Set<Mission>()
                    .Select(m => new
                    {
                        m.Id,
                        grouping = m.ParticipatingSquads
                            .Select(ps => ps.SquadId)
                            .GroupBy(s => s)
                            .Select(g => new { g.Key, Count = g.Count() })
                    })))).Message;

            Assert.Equal(RelationalStrings.MissingIdentifyingProjectionInDistinctGroupBySubquery("s.MissionId"), message);
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
