// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class AsyncGearsOfWarQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : GearsOfWarQueryFixtureBase, new()
    {
        protected AsyncGearsOfWarQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected GearsOfWarContext CreateContext() => Fixture.CreateContext();

        [ConditionalFact]
        public virtual async Task Include_with_group_by_on_entity_qsre()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads.Include(s => s.Members).GroupBy(s => s);
                var results = await query.ToListAsync();

                foreach (var result in results)
                {
                    foreach (var grouping in result)
                    {
                        Assert.True(grouping.Members.Count > 0);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Include_with_group_by_on_entity_qsre_with_composite_key()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears.Include(g => g.Weapons).GroupBy(g => g);
                var results = await query.ToListAsync();

                foreach (var result in results)
                {
                    foreach (var grouping in result)
                    {
                        Assert.True(grouping.Weapons.Count > 0);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Include_with_group_by_on_entity_navigation()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Factions.OfType<LocustHorde>().Include(lh => lh.Leaders).GroupBy(lh => lh.Commander.DefeatedBy);
                var results = await query.ToListAsync();

                foreach (var result in results)
                {
                    foreach (var grouping in result)
                    {
                        Assert.True(grouping.Leaders.Count > 0);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual async Task Include_groupby_constant()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Squads.Include(s => s.Members).GroupBy(s => 1);
                var result = await query.ToListAsync();

                Assert.Equal(1, result.Count);
                var bucket = result[0].ToList();
                Assert.Equal(2, bucket.Count);
                Assert.NotNull(bucket[0].Members);
                Assert.NotNull(bucket[1].Members);
            }
        }

        [ConditionalFact]
        public virtual async Task Cast_to_derived_type_causes_client_eval()
        {
            using (var context = CreateContext())
            {
                await Assert.ThrowsAsync<InvalidCastException>(
                    () => context.Gears.Cast<Officer>().ToListAsync());
            }
        }

        [ConditionalFact]
        public virtual async Task Sum_with_no_data_nullable_double()
        {
            using (var ctx = CreateContext())
            {
                var result = await ctx.Missions.Where(m => m.CodeName == "Operation Foobar").Select(m => m.Rating).SumAsync();
                Assert.Equal(0, result);
            }
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Select_sum()
        {
            using (var ctx = CreateContext())
            {
                var result = await ctx.Missions.GroupBy(m => m.CodeName).Select(g => g.Sum(m => m.Rating)).ToListAsync();
                Assert.Equal(6.3.ToString(), result.Sum().ToString());
            }
        }
    }
}
