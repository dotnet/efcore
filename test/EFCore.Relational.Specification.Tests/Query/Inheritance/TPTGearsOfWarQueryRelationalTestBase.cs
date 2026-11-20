// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public abstract class TPTGearsOfWarQueryRelationalTestBase<TFixture>(TFixture fixture)
    : GearsOfWarQueryRelationalTestBase<TFixture>(fixture)
    where TFixture : TPTGearsOfWarQueryRelationalFixture, new()
{
    public override Task Project_discriminator_columns(bool async)
        => AssertUnableToTranslateEFProperty(() => base.Project_discriminator_columns(async));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_reading_base_and_derived_columns_lifts_to_single_join(bool async)
        => AssertQuery(
            async,
            ss => from h in ss.Set<LocustHorde>()
                  let leader = h.Leaders.OrderBy(l => l.Name).FirstOrDefault()
                  select new
                  {
                      h.Id,
                      leader.Name,
                      leader.ThreatLevel,
                      ((LocustCommander)leader).DefeatedByNickname
                  },
            ss => from h in ss.Set<LocustHorde>()
                  let leader = h.Leaders.OrderBy(l => l.Name).FirstOrDefault()
                  select new
                  {
                      h.Id,
                      leader.Name,
                      leader.ThreatLevel,
                      DefeatedByNickname = leader is LocustCommander ? ((LocustCommander)leader).DefeatedByNickname : null
                  },
            elementSorter: e => e.Id);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_reading_base_and_derived_columns_in_set_operation(bool async)
        => AssertQuery(
            async,
            ss => (from h in ss.Set<LocustHorde>()
                   let leader = h.Leaders.OrderBy(l => l.Name).FirstOrDefault()
                   select new
                   {
                       h.Id,
                       leader.Name,
                       leader.ThreatLevel,
                       ((LocustCommander)leader).DefeatedByNickname
                   })
                .Concat(
                    from h in ss.Set<LocustHorde>()
                    select new
                    {
                        h.Id,
                        Name = (string?)null,
                        ThreatLevel = (short)0,
                        DefeatedByNickname = (string?)null
                    })
                .Select(x => new { x.Id, x.Name, x.DefeatedByNickname }),
            ss => (from h in ss.Set<LocustHorde>()
                   let leader = h.Leaders.OrderBy(l => l.Name).FirstOrDefault()
                   select new
                   {
                       h.Id,
                       Name = (string?)leader.Name,
                       DefeatedByNickname = leader is LocustCommander ? ((LocustCommander)leader).DefeatedByNickname : null
                   })
                .Concat(
                    from h in ss.Set<LocustHorde>()
                    select new
                    {
                        h.Id,
                        Name = (string?)null,
                        DefeatedByNickname = (string?)null
                    }),
            elementSorter: e => (e.Id, e.Name));
}
