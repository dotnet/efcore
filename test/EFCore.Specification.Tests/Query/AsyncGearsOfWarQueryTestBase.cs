// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
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

        protected GearsOfWarContext CreateContext()
            => Fixture.CreateContext();

        [ConditionalFact]
        public virtual async Task Cast_to_derived_type_causes_client_eval()
        {
            using var context = CreateContext();
            await Assert.ThrowsAsync<InvalidCastException>(
                () => context.Gears.Cast<Officer>().ToListAsync());
        }

        [ConditionalFact]
        public virtual async Task Sum_with_no_data_nullable_double()
        {
            using var ctx = CreateContext();
            var result = await ctx.Missions.Where(m => m.CodeName == "Operation Foobar").Select(m => m.Rating).SumAsync();
            Assert.Equal(0, result);
        }

        [ConditionalFact]
        public virtual async Task GroupBy_Select_sum()
        {
            using var ctx = CreateContext();
            var result = await ctx.Missions.GroupBy(m => m.CodeName).Select(g => g.Sum(m => m.Rating)).ToListAsync();
            Assert.Equal(6.3, result.Sum() ?? double.NaN, precision: 1);
        }
    }
}
