// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class ComplexNavigationsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : ComplexNavigationsQueryFixtureBase, new()
{
    protected ComplexNavigationsContext CreateContext()
        => Fixture.CreateContext();

    protected ComplexNavigationsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override Expression RewriteExpectedQueryExpression(Expression expectedQueryExpression)
        => new ExpectedQueryRewritingVisitor(Fixture.GetShadowPropertyMappings()).Visit(expectedQueryExpression);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_equality_empty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l => l.OneToOne_Optional_FK1 == new Level2()),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_when_sentinel_ef_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l => EF.Property<int>(l.OneToOne_Optional_FK1, "Id") == 0),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_using_property_method_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l => EF.Property<int>(l.OneToOne_Required_FK1, "Id") > 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_using_property_method_required2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse2, "Id") > 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_using_property_method_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l => EF.Property<int>(EF.Property<Level2>(l, "OneToOne_Required_FK1"), "Id") == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_using_property_method_nested2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Where(l => EF.Property<int>(EF.Property<Level1>(l, "OneToOne_Required_FK_Inverse2"), "Id") == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_using_property_method_and_member_expression1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l => EF.Property<Level2>(l, "OneToOne_Required_FK1").Id == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_using_property_method_and_member_expression2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l => EF.Property<int>(l.OneToOne_Required_FK1, "Id") == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_using_property_method_and_member_expression3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Where(l => EF.Property<int>(l.OneToOne_Required_FK_Inverse2, "Id") == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_navigation_converted_to_FK(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Where(l => l.OneToOne_Required_FK_Inverse2 == new Level1 { Id = 1 }),
            ss => ss.Set<Level2>().Where(l => l.OneToOne_Required_FK_Inverse2.Id == 1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_two_conditions_on_same_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(
                l => l.OneToOne_Required_FK1 == new Level2 { Id = 1 }
                    || l.OneToOne_Required_FK1 == new Level2 { Id = 2 }),
            ss => ss.Set<Level1>().Where(
                l => l.OneToOne_Required_FK1.Id == 1
                    || l.OneToOne_Required_FK1.Id == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Key_equality_two_conditions_on_same_navigation2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Where(
                l => l.OneToOne_Required_FK_Inverse2 == new Level1 { Id = 1 }
                    || l.OneToOne_Required_FK_Inverse2 == new Level1 { Id = 2 }),
            ss => ss.Set<Level2>().Where(
                l => l.OneToOne_Required_FK_Inverse2.Id == 1
                    || l.OneToOne_Required_FK_Inverse2.Id == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multi_level_include_with_short_circuiting(bool async)
    {
        using var context = CreateContext();

        var query = context.Fields
            .Include(x => x.Label.Globalizations)
            .ThenInclude(x => x.Language)
            .Include(x => x.Placeholder.Globalizations)
            .ThenInclude(x => x.Language);

        var result = (async ? await query.ToListAsync() : query.ToList()).OrderBy(e => e.Name).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Field1", result[0].Name);
        Assert.Equal("Field2", result[1].Name);

        Assert.Equal("MLS1", result[0].Label.DefaultText);
        Assert.Equal("MLS3", result[1].Label.DefaultText);
        Assert.Null(result[0].Placeholder);
        Assert.Equal("MLS4", result[1].Placeholder.DefaultText);

        var globalizations_0_label = result[0].Label.Globalizations.OrderBy(g => g.Text).ToList();
        Assert.Equal(3, globalizations_0_label.Count);
        Assert.Equal("Globalization0", globalizations_0_label[0].Text);
        Assert.Equal("Language0", globalizations_0_label[0].Language.Name);
        Assert.Equal("Globalization1", globalizations_0_label[1].Text);
        Assert.Equal("Language1", globalizations_0_label[1].Language.Name);
        Assert.Equal("Globalization2", globalizations_0_label[2].Text);
        Assert.Equal("Language2", globalizations_0_label[2].Language.Name);

        var globalizations_1_label = result[1].Label.Globalizations.OrderBy(g => g.Text).ToList();
        Assert.Equal(3, globalizations_1_label.Count);
        Assert.Equal("Globalization6", globalizations_1_label[0].Text);
        Assert.Equal("Language6", globalizations_1_label[0].Language.Name);
        Assert.Equal("Globalization7", globalizations_1_label[1].Text);
        Assert.Equal("Language7", globalizations_1_label[1].Language.Name);
        Assert.Equal("Globalization8", globalizations_1_label[2].Text);
        Assert.Equal("Language8", globalizations_1_label[2].Language.Name);

        var globalizations_1_placeholder = result[1].Placeholder.Globalizations.OrderBy(g => g.Text).ToList();
        Assert.Single(globalizations_1_placeholder);
        Assert.Equal("Globalization9", globalizations_1_placeholder[0].Text);
        Assert.Equal("Language9", globalizations_1_placeholder[0].Language.Name);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_key_access_optional(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  join e2 in ss.Set<Level2>() on e1.Id equals e2.OneToOne_Optional_FK_Inverse2.Id
                  select new { Id1 = e1.Id, Id2 = e2.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_key_access_required(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  join e2 in ss.Set<Level2>() on e1.Id equals e2.OneToOne_Required_FK_Inverse2.Id
                  select new { Id1 = e1.Id, Id2 = e2.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_key_access_optional_comparison(bool async)
        => AssertQueryScalar(
            async,
            ss => from e2 in ss.Set<Level2>()
                  where e2.OneToOne_Optional_PK_Inverse2.Id > 5
                  select e2.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Simple_level1_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Required_PK1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Simple_level1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Simple_level1_level2_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Simple_level1_level2_GroupBy_Count(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().GroupBy(
                    l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name)
                .Select(g => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Simple_level1_level2_GroupBy_Having_Count(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().GroupBy(
                    l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name,
                    l1 => new { Id = (int?)l1.OneToOne_Required_PK1.Id ?? 0 })
                .Where(g => g.Min(l1 => l1.Id) > 0)
                .Select(g => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Simple_level1_level2_level3_include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.OneToOne_Required_PK3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_key_access_required_comparison(bool async)
        => AssertQueryScalar(
            async,
            ss => from e2 in ss.Set<Level2>()
                  where e2.OneToOne_Required_PK_Inverse2.Id > 5
                  select e2.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_inside_method_call_translated_to_join(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Required_FK1.Name.StartsWith("L")
                  select e1,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Required_FK1.Name.MaybeScalar(x => x.StartsWith("L")) == true
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_inside_method_call_translated_to_join2(bool async)
        => AssertQuery(
            async,
            ss => from e3 in ss.Set<Level3>()
                  where e3.OneToOne_Required_FK_Inverse3.Name.StartsWith("L")
                  select e3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_inside_method_call_translated_to_join(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Optional_FK1.Name.StartsWith("L")
                  select e1,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Optional_FK1.Name.MaybeScalar(x => x.StartsWith("L")) == true
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_inside_property_method_translated_to_join(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  where EF.Property<string>(EF.Property<Level2>(e1, "OneToOne_Optional_FK1"), "Name") == "L2 01"
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Optional_FK1.Name.ToUpper().StartsWith("L")
                  select e1,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Optional_FK1.Name.MaybeScalar(x => x.ToUpper().StartsWith("L"))
                      == true
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Optional_FK1.Name.StartsWith(e1.OneToOne_Optional_FK1.Name)
                  select e1,
            ss => from e1 in ss.Set<Level1>()
                  where e1.OneToOne_Optional_FK1.Name.MaybeScalar(x => x.StartsWith(e1.OneToOne_Optional_FK1.Name)) == true
                  select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(bool async)
        => AssertQuery(
            async,
            ss =>
                from e1 in ss.Set<Level1>()
                where e1.OneToOne_Optional_FK1.Date.AddDays(10) > new DateTime(2000, 2, 1)
                select e1,
            ss =>
                from e1 in ss.Set<Level1>()
                where e1.OneToOne_Optional_FK1.MaybeScalar(x => x.Date.AddDays(10)) > new DateTime(2000, 2, 1)
                select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(bool async)
        => AssertQuery(
            async,
            ss =>
                from e1 in ss.Set<Level1>()
                where e1.OneToOne_Optional_FK1.Date.AddDays(10).AddDays(15).AddMonths(2) > new DateTime(2002, 2, 1)
                select e1,
            ss =>
                from e1 in ss.Set<Level1>()
                where e1.OneToOne_Optional_FK1.MaybeScalar(x => x.Date.AddDays(10).AddDays(15).AddMonths(2)) > new DateTime(2000, 2, 1)
                select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(
        bool async)
        => AssertQuery(
            async,
            ss =>
                from e1 in ss.Set<Level1>()
                where e1.OneToOne_Optional_FK1.Date.AddDays(15).AddDays(e1.OneToOne_Optional_FK1.Id) > new DateTime(2002, 2, 1)
                select e1,
            ss =>
                from e1 in ss.Set<Level1>()
                where e1.OneToOne_Optional_FK1.MaybeScalar(x => x.Date.AddDays(15).AddDays(x.Id)) > new DateTime(2000, 2, 1)
                select e1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  join e2 in ss.Set<Level2>() on e1.OneToOne_Optional_FK1.Id equals e2.Id
                  select new { Id1 = e1.Id, Id2 = e2.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join_nested(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  join e3 in ss.Set<Level3>() on e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id equals e3.Id
                  select new { Id1 = e1.Id, Id3 = e3.Id },
            e => (e.Id1, e.Id3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_in_outer_selector_translated_to_extra_join_nested2(bool async)
        => AssertQuery(
            async,
            ss => from e3 in ss.Set<Level3>()
                  join e1 in ss.Set<Level1>() on e3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id equals e1.Id
                  select new { Id3 = e3.Id, Id1 = e1.Id },
            e => (e.Id1, e.Id3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_in_inner_selector(bool async)
        => AssertQuery(
            async,
            ss => from e2 in ss.Set<Level2>()
                  join e1 in ss.Set<Level1>() on e2.Id equals e1.OneToOne_Optional_FK1.Id
                  select new { Id2 = e2.Id, Id1 = e1.Id },
            e => (e.Id2, e.Id1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigations_in_inner_selector_translated_without_collision(bool async)
        => AssertQuery(
            async,
            ss => from e2 in ss.Set<Level2>()
                  join e1 in ss.Set<Level1>() on e2.Id equals e1.OneToOne_Optional_FK1.Id
                  join e3 in ss.Set<Level3>() on e2.Id equals e3.OneToOne_Optional_FK_Inverse3.Id
                  select new
                  {
                      Id2 = e2.Id,
                      Id1 = e1.Id,
                      Id3 = e3.Id
                  },
            e => (e.Id2, e.Id1, e.Id3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_non_key_join(bool async)
        => AssertQuery(
            async,
            ss =>
                from e2 in ss.Set<Level2>()
                join e1 in ss.Set<Level1>() on e2.Name equals e1.OneToOne_Optional_FK1.Name
                select new
                {
                    Id2 = e2.Id,
                    Name2 = e2.Name,
                    Id1 = e1.Id,
                    Name1 = e1.Name
                },
            e => (e.Id2, e.Name2, e.Id1, e.Name1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_orderby_on_inner_sequence_navigation_non_key_join(bool async)
        => AssertQuery(
            async,
            ss =>
                from e2 in ss.Set<Level2>()
                join e1 in ss.Set<Level1>().OrderBy(l1 => l1.Id) on e2.Name equals e1.OneToOne_Optional_FK1.Name
                select new
                {
                    Id2 = e2.Id,
                    Name2 = e2.Name,
                    Id1 = e1.Id,
                    Name1 = e1.Name
                },
            e => (e.Id2, e.Name2, e.Id1, e.Name1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_self_ref(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  join e2 in ss.Set<Level1>() on e1.Id equals e2.OneToMany_Optional_Self_Inverse1.Id
                  select new { Id1 = e1.Id, Id2 = e2.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_nested(bool async)
        => AssertQuery(
            async,
            ss => from e3 in ss.Set<Level3>()
                  join e1 in ss.Set<Level1>() on e3.Id equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id
                  select new { Id3 = e3.Id, Id1 = e1.Id },
            e => (e.Id3, e.Id1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_nested2(bool async)
        => AssertQuery(
            async,
            ss => from e3 in ss.Set<Level3>()
                  join e1 in ss.Set<Level1>().OrderBy(ll => ll.Id) on e3.Id equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id
                  select new { Id3 = e3.Id, Id1 = e1.Id },
            e => (e.Id3, e.Id1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_deeply_nested_non_key_join(bool async)
        => AssertQuery(
            async,
            ss =>
                from e4 in ss.Set<Level4>()
                join e1 in ss.Set<Level1>() on e4.Name equals e1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToOne_Required_PK3.Name
                select new
                {
                    Id4 = e4.Id,
                    Name4 = e4.Name,
                    Id1 = e1.Id,
                    Name1 = e1.Name
                },
            e => (e.Id4, e.Name4, e.Id1, e.Name1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_navigation_deeply_nested_required(bool async)
        => AssertQuery(
            async,
            ss =>
                from e1 in ss.Set<Level1>()
                join e4 in ss.Set<Level4>() on e1.Name equals e4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3
                    .OneToOne_Required_PK_Inverse2.Name
                select new
                {
                    Id4 = e4.Id,
                    Name4 = e4.Name,
                    Id1 = e1.Id,
                    Name1 = e1.Name
                },
            e => (e.Id4, e.Name4, e.Id1, e.Name1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_and_project_into_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(e => e.OneToOne_Optional_FK1).Select(e => new { e.Id, entity = e }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertInclude(e.entity, a.entity, new ExpectedInclude<Level1>(ee => ee.OneToOne_Optional_FK1));
                AssertEqual(e.Id, a.Id);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nav_prop_reference_optional1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(e => e.OneToOne_Optional_FK1.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                from l2 in groupJoin.DefaultIfEmpty()
                select l2 == null ? null : l2.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nav_prop_reference_optional2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                  from l2 in groupJoin.DefaultIfEmpty()
                  select l2 == null ? null : (int?)l2.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_nav_prop_reference_optional3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Select(e => e.OneToOne_Optional_FK_Inverse2.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nav_prop_reference_optional1(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>()
                .Where(e => e.OneToOne_Optional_FK1.Name == "L2 05" || e.OneToOne_Optional_FK1.Name == "L2 07")
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2Left in ss.Set<Level2>() on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                  from l2Left in groupJoinLeft.DefaultIfEmpty()
                  join l2Right in ss.Set<Level2>() on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                  from l2Right in groupJoinRight.DefaultIfEmpty()
                  where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) == "L2 07"
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nav_prop_reference_optional2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>()
                .Where(e => e.OneToOne_Optional_FK1.Name == "L2 05" || e.OneToOne_Optional_FK1.Name != "L2 42")
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_nav_prop_reference_optional2_via_DefaultIfEmpty(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2Left in ss.Set<Level2>() on l1.Id equals l2Left.Level1_Optional_Id into groupJoinLeft
                  from l2Left in groupJoinLeft.DefaultIfEmpty()
                  join l2Right in ss.Set<Level2>() on l1.Id equals l2Right.Level1_Optional_Id into groupJoinRight
                  from l2Right in groupJoinRight.DefaultIfEmpty()
                  where (l2Left == null ? null : l2Left.Name) == "L2 05" || (l2Right == null ? null : l2Right.Name) != "L2 42"
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_multiple_nav_prop_reference_optional(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_reference_optional_member_compared_to_value(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name != "L3 05"
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_reference_optional_member_compared_to_null(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name != null
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null1(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2 == null
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null2(bool async)
        => AssertQuery(
            async,
            ss =>
                from l3 in ss.Set<Level3>()
                where l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2 == null
                select l3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null3(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where null != l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null4(bool async)
        => AssertQuery(
            async,
            ss =>
                from l3 in ss.Set<Level3>()
                where null != l3.OneToOne_Optional_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                select l3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_reference_optional_compared_to_null5(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(e => e.OneToOne_Optional_FK1.OneToOne_Required_FK2.OneToOne_Required_FK3 == null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_multiple_nav_prop_reference_required(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Required_FK1.OneToOne_Required_FK2.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_multiple_nav_prop_reference_required2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level3>().Select(e => e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_multiple_nav_prop_optional_required(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select (int?)l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_multiple_nav_prop_optional_required(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name != "L3 05"
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_comparison1(bool async)
        => AssertQuery(
            async,
            ss =>
                from l11 in ss.Set<Level1>()
                from l12 in ss.Set<Level1>()
                where l11 == l12
                select new { Id1 = l11.Id, Id2 = l12.Id },
            ss =>
                from l11 in ss.Set<Level1>()
                from l12 in ss.Set<Level1>()
                where l11.Id == l12.Id
                select new { Id1 = l11.Id, Id2 = l12.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_comparison2(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                from l2 in ss.Set<Level2>()
                where l1 == l2.OneToOne_Optional_FK_Inverse2
                select new { Id1 = l1.Id, Id2 = l2.Id },
            ss =>
                from l1 in ss.Set<Level1>()
                from l2 in ss.Set<Level2>()
                where l1.Id == l2.OneToOne_Optional_FK_Inverse2.Id
                select new { Id1 = l1.Id, Id2 = l2.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_comparison3(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                from l2 in ss.Set<Level2>()
                where l1.OneToOne_Optional_FK1 == l2
                select new { Id1 = l1.Id, Id2 = l2.Id },
            ss =>
                from l1 in ss.Set<Level1>()
                from l2 in ss.Set<Level2>()
                where l1.OneToOne_Optional_FK1.Id == l2.Id
                select new { Id1 = l1.Id, Id2 = l2.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse1(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                from l2 in ss.Set<Level2>()
                where l1.OneToOne_Optional_FK1.Name == "L2 01" || l2.OneToOne_Required_FK_Inverse2.Name != "Bar"
                select new { Id1 = (int?)l1.Id, Id2 = (int?)l2.Id },
            e => (e.Id1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse2(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  where l1.OneToOne_Optional_FK1.OneToOne_Required_FK2.Name == "L3 05" || l1.OneToOne_Optional_FK1.Name != "L2 05"
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse3(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  where l1.OneToOne_Optional_FK1.Name != "L2 05" || l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.Name == "L3 05"
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_complex_predicate_with_with_nav_prop_and_OrElse4(bool async)
        => AssertQueryScalar(
            async,
            ss => from l3 in ss.Set<Level3>()
                  where l3.OneToOne_Optional_FK_Inverse3.Name != "L2 05"
                      || l3.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Name == "L1 05"
                  select l3.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_navigations_with_predicate_projected_into_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Where(
                    e => e.OneToOne_Required_FK1.OneToOne_Required_FK2 == e.OneToOne_Required_FK1.OneToOne_Optional_FK2
                        && e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id != 7)
                .Select(e => new { e.Name, Id = (int?)e.OneToOne_Required_FK1.OneToOne_Optional_FK2.Id }),
            elementSorter: e => (e.Name, e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_navigations_with_predicate_projected_into_anonymous_type2(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Level3>()
                  where e.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2
                      != e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2
                      && e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id != 7
                  select new { e.Name, Id = (int?)e.OneToOne_Required_FK_Inverse3.OneToOne_Optional_FK_Inverse2.Id },
            e => (e.Name, e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_projected_into_DTO(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(
                e => new MyOuterDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Inner = e.OneToOne_Optional_FK1 != null
                        ? new MyInnerDto { Id = e.OneToOne_Optional_FK1.Id, Name = e.OneToOne_Optional_FK1.Name }
                        : null
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Name, a.Name);
                Assert.Equal(e.Inner?.Id, a.Inner?.Id);
                Assert.Equal(e.Inner?.Name, a.Inner?.Name);
            });

    public class MyOuterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public MyInnerDto Inner { get; set; }
    }

    public class MyInnerDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_nav_prop_reference_optional(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().OrderBy(e => e.OneToOne_Optional_FK1.Name).ThenBy(e => e.Id).Select(e => e.Id),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                  from l2 in groupJoin.DefaultIfEmpty()
                  orderby l2 == null ? null : l2.Name, l1.Id
                  select l1.Id,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Result_operator_nav_prop_reference_optional_Sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Level1>(),
            ss => ss.Set<Level1>(),
            actualSelector: e => e.OneToOne_Optional_FK1.Level1_Required_Id,
            expectedSelector: e => e.OneToOne_Optional_FK1.MaybeScalar(x => x.Level1_Required_Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Result_operator_nav_prop_reference_optional_Min(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Level1>(),
            ss => ss.Set<Level1>(),
            actualSelector: e => e.OneToOne_Optional_FK1.Level1_Required_Id,
            expectedSelector: e => e.OneToOne_Optional_FK1.MaybeScalar(x => x.Level1_Required_Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Result_operator_nav_prop_reference_optional_Max(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Level1>(),
            ss => ss.Set<Level1>(),
            actualSelector: e => e.OneToOne_Optional_FK1.Level1_Required_Id,
            expectedSelector: e => e.OneToOne_Optional_FK1.MaybeScalar(x => x.Level1_Required_Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Result_operator_nav_prop_reference_optional_Average(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Level1>(),
            ss => ss.Set<Level1>(),
            actualSelector: e => e.OneToOne_Optional_FK1.Level1_Required_Id,
            expectedSelector: e => e.OneToOne_Optional_FK1.MaybeScalar(x => x.Level1_Required_Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Result_operator_nav_prop_reference_optional_Average_with_identity_selector(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id),
            selector: e => e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Result_operator_nav_prop_reference_optional_Average_without_selector(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Level1>().Select(e => (int?)e.OneToOne_Optional_FK1.Level1_Required_Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        => AssertSum(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                  from l2 in groupJoin.DefaultIfEmpty()
                  select l2,
            selector: e => e == null ? 0 : e.Level1_Required_Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>().Include(e => e.OneToOne_Optional_FK1)
                  where l1.OneToOne_Optional_FK1.Name != "L2 05"
                  select l1,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_flattening_bug_4539(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                join l1_Optional in ss.Set<Level2>() on l1.Id equals l1_Optional.Level1_Optional_Id into grouping
                from l1_Optional in grouping.DefaultIfEmpty()
                from l2 in ss.Set<Level2>()
                join l2_Required_Reverse in ss.Set<Level1>() on l2.Level1_Required_Id equals l2_Required_Reverse.Id
                select new { l1_Optional, l2_Required_Reverse },
            elementSorter: e => (e.l1_Optional?.Id, e.l2_Required_Reverse.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_source_materialization_bug_4547(bool async)
        => AssertQueryScalar(
            async,
            ss => from e3 in ss.Set<Level3>()
                  join e1 in ss.Set<Level1>()
                      on
                      e3.Id
                      equals
                      (
                          from subQuery2 in ss.Set<Level2>()
                          join subQuery3 in ss.Set<Level3>()
                              on
                              subQuery2 != null ? subQuery2.Id : null
                              equals
                              subQuery3.Level2_Optional_Id
                              into
                              grouping
                          from subQuery3 in grouping.DefaultIfEmpty()
                          orderby subQuery3 != null ? (int?)subQuery3.Id : null
                          select subQuery3 != null ? (int?)subQuery3.Id : null
                      ).Where(x => x != null).FirstOrDefault()
                  select e1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_property_and_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).Select(e => e.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_property_and_filter_before(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(e => e.Id == 1).SelectMany(l1 => l1.OneToMany_Optional1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_property_and_filter_after(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1).Where(e => e.Id != 6));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_nested_navigation_property_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2),
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2 ?? new List<Level3>()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_nested_navigation_property_optional_and_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2).Select(e => e.Name),
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2 ?? new List<Level3>())
                .Select(e => e.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_SelectMany_calls(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(e => e.OneToMany_Optional1).SelectMany(e => e.OneToMany_Optional2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_navigation_property_with_another_navigation_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_navigation_property_to_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2.Count > 0),
            ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Required_FK1.OneToMany_Optional2.MaybeScalar(x => x.Count) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_navigation_property_to_collection2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>().Where(l3 => l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2.Count > 0),
            ss => ss.Set<Level3>().Where(
                l3 => l3.OneToOne_Required_FK_Inverse3.OneToMany_Optional2.MaybeScalar(x => x.Count) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_navigation_property_to_collection_of_original_entity_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Where(l2 => l2.OneToMany_Required_Inverse2.OneToMany_Optional1.Count() > 0),
            ss => ss.Set<Level2>().Where(
                l2 => l2.OneToMany_Required_Inverse2.OneToMany_Optional1.MaybeScalar(x => x.Count()) > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        => AssertQuery(
            async,
            ss =>
                (from l1 in ss.Set<Level1>()
                 where ss.Set<Level2>().Any(l2 => l2.Level1_Required_Id == l1.Id)
                 select l1.Name).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_subquery_doesnt_project_unnecessary_columns_in_top_level_join(bool async)
        => AssertQuery(
            async,
            ss => from e1 in ss.Set<Level1>()
                  join e2 in ss.Set<Level2>() on e1.Id equals e2.OneToOne_Optional_FK_Inverse2.Id
                  where ss.Set<Level2>().Any(l2 => l2.Level1_Required_Id == e1.Id)
                  select new { Name1 = e1.Name, Id2 = e2.Id },
            e => (e.Name1, e.Id2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_nested_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        => AssertQuery(
            async,
            ss => (from l1 in ss.Set<Level1>()
                   where ss.Set<Level2>().Any(l2 => ss.Set<Level3>().Select(l3 => l2.Id).Any())
                   select l1.Name).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_nested_two_levels_up_subquery_doesnt_project_unnecessary_columns_in_top_level(bool async)
        => AssertQuery(
            async,
            ss => (from l1 in ss.Set<Level1>()
                   where ss.Set<Level2>().Any(l2 => ss.Set<Level3>().Select(l3 => l1.Id).Any())
                   select l1.Name).Distinct()
        );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_where_with_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(l1 => l1.OneToMany_Required1).Where(l2 => l2.OneToMany_Required2.Any()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>().OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id).Select(l3 => l3.OneToOne_Required_FK_Inverse3),
            elementAsserter: (e, a) => AssertEqual(e, a),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>().OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id)
                .Select(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse3")),
            elementAsserter: (e, a) => AssertEqual(e, a),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>().OrderBy(l3 => EF.Property<Level2>(l3, "OneToOne_Required_FK_Inverse3").Id)
                .Select(l3 => l3.OneToOne_Required_FK_Inverse3),
            elementAsserter: (e, a) => AssertEqual(e, a),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_key_of_navigation_similar_to_projected_gets_optimized_into_FK_access(bool async)
        => AssertQuery(
            async,
            ss => from l3 in ss.Set<Level3>()
                  orderby l3.OneToOne_Required_FK_Inverse3.Id
                  select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2,
            elementAsserter: (e, a) => AssertEqual(e, a),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_key_of_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>()
                .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                .OrderBy(l2 => l2.Id)
                .Take(10)
                .Select(l2 => l2.OneToOne_Required_FK_Inverse2.Name),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_key_of_anonymous_type_projected_navigation_doesnt_get_optimized_into_FK_access_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>()
                .Select(
                    l3 => new { l3.OneToOne_Required_FK_Inverse3, name = l3.Name })
                .OrderBy(l3 => l3.OneToOne_Required_FK_Inverse3.Id)
                .Take(10)
                .Select(l2 => l2.OneToOne_Required_FK_Inverse3.Name),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_take_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Select(l1 => l1.OneToOne_Optional_FK1)
                .OrderBy(l2 => (int?)l2.Id)
                .Take(10)
                .Select(l2 => l2.OneToOne_Optional_FK2.Name),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_select_correct_table_from_subquery_when_materialization_is_not_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Where(l2 => l2.OneToOne_Required_FK_Inverse2.Name == "L1 03")
                .OrderBy(l => l.Id)
                .Take(3)
                .Select(l2 => l2.Name));

    // see issue #31887
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_select_correct_table_with_anonymous_projection_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => (from l2 in ss.Set<Level2>()
                   join l1 in ss.Set<Level1>() on l2.Level1_Required_Id equals l1.Id
                   join l3 in ss.Set<Level3>() on l1.Id equals l3.Level2_Required_Id
                   //where l1.Name == "L1 01"
                   //where l3.Name == "L3 010"
                   select new { l2, l1 })
                .OrderBy(l => l.l1.Id)
                .Take(3)
                .Select(l => l.l2.Name));

    // see issue #31887
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_select_correct_table_in_subquery_when_materialization_is_not_required_in_multiple_joins(bool async)
        => AssertQuery(
            async,
            ss => (from l2 in ss.Set<Level2>()
                   join l1 in ss.Set<Level1>() on l2.Level1_Required_Id equals l1.Id
                   join l3 in ss.Set<Level3>() on l1.Id equals l3.Level2_Required_Id
                   //where l1.Name == "L1 03"
                   //where l3.Name == "L3 08"
                   select l1).OrderBy(l1 => l1.Id).Take(3).Select(l1 => l1.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_predicate_on_optional_reference_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Where(l1 => l1.OneToOne_Required_FK1.Name == "L2 03")
                .OrderBy(l1 => l1.Id)
                .Take(3)
                .Select(l1 => l1.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_string_based_Include1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include("OneToOne_Required_FK2"),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_EF_Property_Include1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include(l2 => EF.Property<Level2>(l2, "OneToOne_Required_FK2")),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_string_based_Include2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .Include("OneToOne_Required_FK2.OneToOne_Required_FK3"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Required_FK2),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3, "OneToOne_Required_FK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_SelectMany_with_string_based_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .SelectMany(l1 => l1.OneToMany_Optional2)
                .Include("OneToOne_Required_FK3"),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_SelectMany_with_EF_Property_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Optional1)
                .SelectMany(l1 => l1.OneToMany_Optional2)
                .Include(l3 => EF.Property<Level3>(l3, "OneToOne_Required_FK3")),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level3>(l3 => l3.OneToOne_Required_FK3)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_required_navigations_with_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_required_navigation_using_multiple_selects_with_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_required_navigation_with_string_based_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include("OneToOne_Optional_FK2"),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_required_navigation_with_EF_Property_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => EF.Property<Level2>(l2, "OneToOne_Optional_FK2")),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include("OneToOne_Optional_FK2"),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_required_navigation_using_multiple_selects_with_EF_Property_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level4>()
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4)
                    .Select(l3 => l3.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => EF.Property<Level2>(l2, "OneToOne_Optional_FK2")),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_with_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>()
                    .Select(l1 => l1.OneToOne_Optional_FK1)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                from l2 in l1.OneToMany_Optional1.DefaultIfEmpty()
                where l2 != null
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l2 in l1.OneToMany_Optional1.Where(l => l.Id > 5).DefaultIfEmpty()
                  where l2 != null
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l3 in l1.OneToOne_Required_FK1.OneToMany_Optional2.DefaultIfEmpty()
                  where l3 != null
                  select l1,
            ss => from l1 in ss.Set<Level1>()
                  from l3 in l1.OneToOne_Required_FK1.OneToMany_Optional2.DefaultIfEmpty() ?? new List<Level3>()
                  where l3 != null
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l3 in l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty()
                  where l3 != null
                  select l1,
            ss => from l1 in ss.Set<Level1>().Where(l => l.OneToOne_Optional_FK1 != null)
                  from l3 in l1.OneToOne_Optional_FK1.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty()
                  where l3 != null
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l3 in l1.OneToOne_Required_FK1.OneToMany_Required2.Where(l => l.Id > 5).DefaultIfEmpty()
                  where l3 != null
                  select l1,
            ss => from l1 in ss.Set<Level1>().Where(l => l.OneToOne_Required_FK1 != null)
                  from l3 in l1.OneToOne_Required_FK1.OneToMany_Required2.Where(l => l.Id > 5).DefaultIfEmpty()
                  where l3 != null
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2) on l1.Id
                      equals l2.Level1_Optional_Id
                  select new { l1, l2 },
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                              ?? new List<Level2>()) on l1.Id
                      equals l2.Level1_Optional_Id
                  select new { l1, l2 },
            elementSorter: e => (e.l1.Id, e.l2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l1, a.l1);
                AssertEqual(e.l2, a.l2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(
        bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                              .DefaultIfEmpty())
                      on l1.Id equals l2.Level1_Optional_Id
                  select new { l1, l2 },
            elementSorter: e => (e.l1?.Id, e.l2?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l1, a.l1);
                AssertEqual(e.l2, a.l2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(
        bool async)
        => AssertQuery(
            async,
            ss => from l2 in ss.Set<Level4>().SelectMany(
                      l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2.DefaultIfEmpty())
                  join l1 in ss.Set<Level1>() on l2.Level1_Optional_Id equals l1.Id
                  select new { l2, l1 },
            elementSorter: e => (e.l2?.Id, e.l1?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l2, a.l2);
                AssertEqual(e.l1, a.l1);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(
        bool async)
        => AssertQuery(
            async,
            ss => from l4 in ss.Set<Level1>().SelectMany(
                      l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                  join l2 in ss.Set<Level2>() on l4.Id equals l2.Id
                  select new { l4, l2 },
            elementSorter: e => (e.l4?.Id, e.l2?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l4, a.l4);
                AssertEqual(e.l2, a.l2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(
        bool async)
        => AssertQuery(
            async,
            ss => from l4 in ss.Set<Level1>().SelectMany(
                      l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                  join l2 in ss.Set<Level2>() on l4.Id equals l2.Id into grouping
                  from l2 in grouping.DefaultIfEmpty()
                  select new { l4, l2 },
            elementSorter: e => (e.l4?.Id, e.l2?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l4, a.l4);
                AssertEqual(e.l2, a.l2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool async)
        => AssertQuery(
            async,
            ss => from l4 in ss.Set<Level1>().SelectMany(
                      l1 => l1.OneToOne_Required_FK1.OneToOne_Optional_FK2.OneToMany_Required3.DefaultIfEmpty())
                  join l2 in ss.Set<Level4>().SelectMany(
                          l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Optional_FK_Inverse3.OneToMany_Required_Self2
                              .DefaultIfEmpty())
                      on l4.Id equals l2.Id
                  select new { l4, l2 },
            elementSorter: e => (e.l4?.Id, e.l2?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l4, a.l4);
                AssertEqual(e.l2, a.l2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(
            bool async)
        => AssertQuery(
            async,
            ss => from l3 in ss.Set<Level4>().SelectMany(
                      l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3.OneToMany_Required2.DefaultIfEmpty())
                  select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(
            bool async)
        => AssertQuery(
            async,
            ss => from l3 in ss.Set<Level1>().SelectMany(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.DefaultIfEmpty())
                  select l3.OneToOne_Required_FK_Inverse3.OneToOne_Required_PK_Inverse2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l2 in l1.OneToMany_Optional1
                  from l3 in l2.OneToMany_Optional2.Where(l => l.Id > 5).DefaultIfEmpty()
                  where l3 != null
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l2 in l1.OneToMany_Required1.Where(l => l.Id > 5).OrderBy(l => l.Id).Take(3).DefaultIfEmpty()
                  where l2 != null
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_join_subquery_containing_filter_and_distinct(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                join l2 in ss.Set<Level2>().Where(l => l.Id > 2).Distinct() on l1.Id equals l2.Level1_Optional_Id
                select new { l1, l2 },
            elementSorter: e => (e.l1.Id, e.l2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l1, a.l1);
                AssertEqual(e.l2, a.l2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_join_with_key_selector_being_a_subquery(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals ss.Set<Level2>().Select(l => l.Id).OrderBy(l => l).FirstOrDefault()
                  select new { l1, l2 },
            elementSorter: e => (e.l1.Id, e.l2.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l1, a.l1);
                AssertEqual(e.l2, a.l2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_subquery_optional_navigation_and_constant_item(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Distinct().Select(l3 => l3.Id).Contains(6)),
            ss => ss.Set<Level1>().Where(
                l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.MaybeScalar(x => x.Distinct().Select(l3 => l3.Id).Contains(6))
                    == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_subquery_optional_navigation_scalar_distinct_and_constant_item(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(
                l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.Select(l3 => l3.Name.Length).Distinct().Contains(5)),
            ss => ss.Set<Level1>().Where(
                l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2.MaybeScalar(
                        x => x.Select(l3 => l3.Name.Length).Distinct().Contains(5))
                    == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(
                l1 => l1.Id < 3
                    && !l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3.Id)
                        .All(l4 => ClientMethod(l4))),
            ss => ss.Set<Level1>().Where(
                l1 => l1.Id < 3
                    && !l1.OneToMany_Optional1.Select(l2 => l2.OneToOne_Optional_FK2.OneToOne_Optional_FK3.MaybeScalar(x => x.Id))
                        .All(a => true)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Required_navigation_on_a_subquery_with_First_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Where(l2o => l2o.Id == 7)
                .Select(l2o => ss.Set<Level2>().OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool async)
        => AssertQuery(
            async,
            ss =>
                from l2o in ss.Set<Level2>()
                where l2o.Id == 7
                select
                    (from l2i in ss.Set<Level2>()
                     join l1i in ss.Set<Level1>()
                         on l2i.Level1_Required_Id equals l1i.Id
                     orderby l2i.Id
                     select new { Navigation = l2i.OneToOne_Required_FK_Inverse2, Constant = 7 }).First().Navigation.Name);

    // see issue #31887
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Required_navigation_on_a_subquery_with_First_in_predicate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Where(l2o => l2o.Id == 7)
                .Where(
                    l1 => EF.Property<string>(
                            ss.Set<Level2>().OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2, "Name")
                        == "L1 10" ||
                          EF.Property<string>(
                            ss.Set<Level2>().OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2, "Name")
                        == "L1 01"));
        //=> AssertQuery(
        //    async,
        //    ss => ss.Set<Level2>()
        //        .Where(l2o => l2o.Id == 7)
        //        .Where(
        //            l1 => EF.Property<string>(
        //                    ss.Set<Level2>().OrderBy(l2i => l2i.Id).First().OneToOne_Required_FK_Inverse2, "Name")
        //                == "L1 10"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Manually_created_left_join_propagates_nullability_to_navigations(bool async)
        => AssertQuery(
            async,
            ss => from l1_manual in ss.Set<Level1>()
                  join l2_manual in ss.Set<Level2>() on l1_manual.Id equals l2_manual.Level1_Optional_Id into grouping
                  from l2_manual in grouping.DefaultIfEmpty()
                  where l2_manual.OneToOne_Required_FK_Inverse2.Name != "L3 02"
                  select l2_manual.OneToOne_Required_FK_Inverse2.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_propagates_nullability_to_manually_created_left_join1(bool async)
        => AssertQuery(
            async,
            ss => from l2_nav in ss.Set<Level1>().Select(ll => ll.OneToOne_Optional_FK1)
                  join l2 in ss.Set<Level2>() on l2_nav.Level1_Required_Id equals l2.Id into grouping
                  from l2 in grouping.DefaultIfEmpty()
                  select new { Id1 = (int?)l2_nav.Id, Id2 = (int?)l2.Id },
            elementSorter: e => e.Id1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool async)
        => AssertQuery(
            async,
            ss => from l3 in ss.Set<Level3>()
                  join l2_nav in ss.Set<Level1>().Select(ll => ll.OneToOne_Optional_FK1) on l3.Level2_Required_Id equals l2_nav.Id into
                      grouping
                  from l2_nav in grouping.DefaultIfEmpty()
                  select new { Name1 = l3.Name, Name2 = l2_nav.Name },
            elementSorter: e => (e.Name1, e.Name2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_reference_protection_complex(bool async)
        => AssertQuery(
            async,
            ss => from l3 in ss.Set<Level3>()
                  join l2_outer in
                      from l1_inner in ss.Set<Level1>()
                      join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                      from l2_inner in grouping_inner.DefaultIfEmpty()
                      select l2_inner
                      on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                  from l2_outer in grouping_outer.DefaultIfEmpty()
                  select l2_outer.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_reference_protection_complex_materialization(bool async)
        => AssertQuery(
            async,
            ss => from l3 in ss.Set<Level3>()
                  join l2_outer in
                      from l1_inner in ss.Set<Level1>()
                      join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                      from l2_inner in grouping_inner.DefaultIfEmpty()
                      select l2_inner
                      on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                  from l2_outer in grouping_outer.DefaultIfEmpty()
                  select new { entity = l2_outer, property = l2_outer.Name },
            elementSorter: e => e.property,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.entity, a.entity);
                Assert.Equal(e.property, a.property);
            });

    private static TResult ClientMethodReturnSelf<TResult>(TResult element)
        => element;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_reference_protection_complex_client_eval(bool async)
        => AssertQuery(
            async,
            ss => from l3 in ss.Set<Level3>()
                  join l2_outer in
                      from l1_inner in ss.Set<Level1>()
                      join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                      from l2_inner in grouping_inner.DefaultIfEmpty()
                      select l2_inner
                      on l3.Level2_Required_Id equals l2_outer.Id into grouping_outer
                  from l2_outer in grouping_outer.DefaultIfEmpty()
                  select ClientMethodReturnSelf(l2_outer.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1_outer in ss.Set<Level1>()
                  join subquery in
                      from l2_inner in ss.Set<Level2>()
                      join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id
                      select l2_inner
                      on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                  from subquery in grouping.DefaultIfEmpty()
                  select (int?)subquery.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1_outer in ss.Set<Level1>()
                  join subquery in
                      from l2_inner in ss.Set<Level2>()
                      join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id
                      select l2_inner
                      on l1_outer.Id equals subquery.Level1_Optional_Id into grouping
                  from subquery in grouping.DefaultIfEmpty()
                  select subquery != null ? (int?)subquery.Id : null);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1_outer in ss.Set<Level1>()
                  join subquery in
                      from l2_inner in ss.Set<Level2>()
                      join l1_inner in ss.Set<Level1>() on l2_inner.Level1_Required_Id equals l1_inner.Id into grouping_inner
                      from l1_inner in grouping_inner.DefaultIfEmpty()
                      select l2_inner
                      on l1_outer.Id equals subquery.Level1_Required_Id into grouping
                  from subquery in grouping.DefaultIfEmpty()
                  select (int?)subquery.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer(bool async)
        => AssertQuery(
            async,
            ss =>
                from x in
                    (from l1 in ss.Set<Level1>()
                     join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                     from l2 in grouping.DefaultIfEmpty()
                     orderby l1.Id
                     select l1).Take(2)
                join l2_outer in ss.Set<Level2>() on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                from l2_outer in grouping_outer.DefaultIfEmpty()
                select l2_outer.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method(bool async)
        // Translation failed message. Issue #17328.
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select ClientLevel1(l1)).Take(2)
                    join l2_outer in ss.Set<Level2>() on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name,
                ss =>
                    from x in
                        (from l1 in ss.Set<Level1>()
                         join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                         from l2 in grouping.DefaultIfEmpty()
                         orderby l1.Id
                         select ClientLevel1(l1)).Take(2)
                    join l2_outer in ss.Set<Level2>() on x.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                    from l2_outer in grouping_outer.DefaultIfEmpty()
                    select l2_outer.Name));

    private static Level1 ClientLevel1(Level1 arg)
        => arg;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner(bool async)
        => AssertQuery(
            async,
            ss =>
                from x in
                    (from l1 in ss.Set<Level1>()
                     join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                     from l2 in grouping.DefaultIfEmpty()
                     orderby l1.Id
                     select l2).Take(2)
                join l1_outer in ss.Set<Level1>() on x.Level1_Optional_Id equals l1_outer.Id into grouping_outer
                from l1_outer in grouping_outer.DefaultIfEmpty()
                select l1_outer.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_on_a_subquery_containing_another_GroupJoin_with_orderby_on_inner_sequence_projecting_inner(
        bool async)
        => AssertQuery(
            async,
            ss =>
                from x in
                    (from l1 in ss.Set<Level1>()
                     join l2 in ss.Set<Level2>().OrderBy(ee => ee.Date) on l1.Id equals l2.Level1_Optional_Id into grouping
                     from l2 in grouping.DefaultIfEmpty()
                     orderby l1.Id
                     select l2).Take(2)
                join l1_outer in ss.Set<Level1>() on x.Level1_Optional_Id equals l1_outer.Id into grouping_outer
                from l1_outer in grouping_outer.DefaultIfEmpty()
                select l1_outer.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_on_left_side_being_a_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.OneToOne_Optional_FK1.Name)
                .ThenBy(l1 => l1.Id)
                .Take(2)
                .Select(x => new { x.Id, Brand = x.OneToOne_Optional_FK1.Name }),
            e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_on_right_side_being_a_subquery(bool async)
        => AssertQuery(
            async,
            ss =>
                from l2 in ss.Set<Level2>()
                join l1 in ss.Set<Level1>().OrderBy(x => x.OneToOne_Optional_FK1.Name).Take(2) on l2.Level1_Optional_Id equals l1.Id
                    into grouping
                from l1 in grouping.DefaultIfEmpty()
                select new { l2.Id, Name = l1 != null ? l1.Name : null },
            e => e.Id);

    private static bool ClientMethod(int? id)
        => true;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_in_subquery_with_client_result_operator(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where (from l1_inner in ss.Set<Level1>()
                       join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                       from l2_inner in grouping.DefaultIfEmpty()
                       select l1_inner).Distinct().Count()
                    > 7
                where l1.Id < 3
                select l1.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_in_subquery_with_client_projection(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where (from l1_inner in ss.Set<Level1>()
                       join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping
                       from l2_inner in grouping.DefaultIfEmpty()
                       select ClientStringMethod(l1_inner.Name)).Count()
                    > 7
                where l1.Id < 3
                select l1.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_in_subquery_with_client_projection_nested1(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1_outer in ss.Set<Level1>()
                where (from l1_middle in ss.Set<Level1>()
                       join l2_middle in ss.Set<Level2>() on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                       from l2_middle in grouping_middle.DefaultIfEmpty()
                       where (from l1_inner in ss.Set<Level1>()
                              join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                              from l2_inner in grouping_inner.DefaultIfEmpty()
                              select ClientStringMethod(l1_inner.Name)).Count()
                           > 7
                       select l1_middle).OrderBy(l1 => l1.Id).Take(10).Count()
                    > 4
                where l1_outer.Id < 2
                select l1_outer.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_in_subquery_with_client_projection_nested2(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1_outer in ss.Set<Level1>()
                where (from l1_middle in ss.Set<Level1>()
                       join l2_middle in ss.Set<Level2>() on l1_middle.Id equals l2_middle.Level1_Optional_Id into grouping_middle
                       from l2_middle in grouping_middle.DefaultIfEmpty()
                       where (from l1_inner in ss.Set<Level1>()
                              join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                              from l2_inner in grouping_inner.DefaultIfEmpty()
                              select l1_inner.Name).Count()
                           > 7
                       select ClientStringMethod(l1_middle.Name)).Count()
                    > 4
                where l1_outer.Id < 2
                select l1_outer.Name);

    private static string ClientStringMethod(string argument)
        => argument;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_client_method_on_outer(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                from l2 in groupJoin.DefaultIfEmpty()
                select new { l1.Id, client = ClientMethodNullableInt(l1.Id) },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_client_method_in_OrderBy(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                  from l2 in groupJoin.DefaultIfEmpty()
                  orderby ClientMethodNullableInt(l1.Id), ClientMethodNullableInt(l2 != null ? l2.Id : null)
                  select l1.Id,
            assertOrder: true);

    private static int ClientMethodNullableInt(int? id)
        => id ?? 0;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_without_DefaultIfEmpty(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                  from l2 in groupJoin.Select(gg => gg)
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_with_subquery_on_inner(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                  from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10).DefaultIfEmpty()
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_with_subquery_on_inner_and_no_DefaultIfEmpty(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into groupJoin
                  from l2 in groupJoin.Where(gg => gg.Id > 0).OrderBy(gg => gg.Id).Take(10)
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Optional_navigation_in_subquery_with_unrelated_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1.Name != "Foo")
                .OrderBy(l1 => l1.Id)
                .Take(15)
                .Select(l1 => l1.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in (from l1 in ss.Set<Level1>()
                              join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                              from l2 in grouping.DefaultIfEmpty()
#pragma warning disable IDE0031 // Use null propagation
                              where (l2 != null ? l2.Name : null) != "Foo"
#pragma warning restore IDE0031 // Use null propagation
                              select l1).OrderBy(l1 => l1.Id).Take(15)
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in (from l1 in ss.Set<Level1>()
                              join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                              from l2 in grouping.DefaultIfEmpty()
                              where (l2 != null ? l2.Name : null) != "Foo"
                              select l1).Distinct()
                  select l1.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection3(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in (from l1 in ss.Set<Level1>()
                              join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                              from l2 in grouping.DefaultIfEmpty()
                              where (l2 != null ? l2.Name : null) != "Foo"
                              select l1.Id).Distinct()
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Explicit_GroupJoin_in_subquery_with_unrelated_projection4(bool async)
        => AssertQueryScalar(
            async,
            ss => from l1 in (from l1 in ss.Set<Level1>()
                              join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                              from l2 in grouping.DefaultIfEmpty()
                              where (l2 != null ? l2.Name : null) != "Foo"
                              select l1.Id).Distinct().OrderBy(id => id).Take(20)
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Explicit_GroupJoin_in_subquery_with_scalar_result_operator(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where (from l1_inner in ss.Set<Level1>()
                       join l2 in ss.Set<Level2>() on l1_inner.Id equals l2.Level1_Optional_Id into grouping
                       from l2 in grouping.DefaultIfEmpty()
                       select l1_inner).Count()
                    > 4
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Explicit_GroupJoin_in_subquery_with_multiple_result_operator_distinct_count_materializes_main_clause(
        bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where (from l1_inner in ss.Set<Level1>()
                       join l2 in ss.Set<Level2>() on l1_inner.Id equals l2.Level1_Optional_Id into grouping
                       from l2 in grouping.DefaultIfEmpty()
                       select l1_inner).Distinct().Count()
                    > 4
                select l1);

    // issue #31887
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_on_multilevel_reference_in_subquery_with_outer_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>()
                .Where(l3 => l3.OneToMany_Required_Inverse3.OneToOne_Required_FK_Inverse2.Name == "L1 10"
                    || l3.OneToMany_Required_Inverse3.OneToOne_Required_FK_Inverse2.Name == "L1 01")
                .OrderBy(l3 => l3.Level2_Required_Id)
                .Skip(0)
                .Take(10)
                .Select(l3 => l3.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>()
                      on new { A = EF.Property<int?>(l1, "OneToMany_Optional_Self_Inverse1Id") }
                      equals new { A = EF.Property<int?>(l2, "Level1_Optional_Id") }
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                join l2 in ss.Set<Level2>()
                    on new
                    {
                        A = EF.Property<int?>(l1, "OneToMany_Optional_Self_Inverse1Id"),
                        B = EF.Property<int?>(l1, "OneToOne_Optional_Self1Id")
                    }
                    equals new
                    {
                        A = EF.Property<int?>(l2, "Level1_Optional_Id"), B = EF.Property<int?>(l2, "OneToMany_Optional_Self_Inverse2Id")
                    }
                select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_group_join_with_take(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1_outer in
                    (from l1_inner in ss.Set<Level1>()
                     orderby l1_inner.Id
                     join l2_inner in ss.Set<Level2>() on l1_inner.Id equals l2_inner.Level1_Optional_Id into grouping_inner
                     from l2_inner in grouping_inner.DefaultIfEmpty()
                     select l2_inner).Take(2)
                join l2_outer in ss.Set<Level2>() on l1_outer.Id equals l2_outer.Level1_Optional_Id into grouping_outer
                from l2_outer in grouping_outer.DefaultIfEmpty()
                select l2_outer.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_with_same_navigation_compared_to_null(bool async)
        => AssertQueryScalar(
            async,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToMany_Required_Inverse2.Name != "L1 07" && l2.OneToMany_Required_Inverse2 != null
                  select l2.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multi_level_navigation_compared_to_null(bool async)
        => AssertQueryScalar(
            async,
            ss => from l3 in ss.Set<Level3>()
                  where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2 != null
                  select l3.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multi_level_navigation_with_same_navigation_compared_to_null(bool async)
        => AssertQueryScalar(
            async,
            ss => from l3 in ss.Set<Level3>()
                  where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2.Name != "L1 07"
                  where l3.OneToMany_Optional_Inverse3.OneToOne_Required_FK_Inverse2 != null
                  select l3.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigations_compared_to_each_other1(bool async)
        => AssertQuery(
            async,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToMany_Required_Inverse2 == l2.OneToMany_Required_Inverse2
                  select l2.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigations_compared_to_each_other2(bool async)
        => AssertQuery(
            async,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToMany_Required_Inverse2 == l2.OneToOne_Optional_PK_Inverse2
                  select l2.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigations_compared_to_each_other3(bool async)
        => AssertQuery(
            async,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToMany_Optional2.Select(i => i.OneToOne_Optional_PK_Inverse3 == l2).Any()
                  select l2.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigations_compared_to_each_other4(bool async)
        => AssertQuery(
            async,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToOne_Required_FK2.OneToMany_Optional3
                      .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Required_FK2).Any()
                  select l2.Name,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToOne_Required_FK2.OneToMany_Optional3.MaybeScalar(
                          x => x.Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Required_FK2).Any())
                      == true
                  select l2.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigations_compared_to_each_other5(bool async)
        => AssertQuery(
            async,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToOne_Required_FK2.OneToMany_Optional3
                      .Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Optional_PK2).Any()
                  select l2.Name,
            ss => from l2 in ss.Set<Level2>()
                  where l2.OneToOne_Required_FK2.OneToMany_Optional3.MaybeScalar(
                          x => x.Select(i => i.OneToOne_Optional_PK_Inverse4 == l2.OneToOne_Optional_PK2).Any())
                      == true
                  select l2.Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Level4_Include(bool async)
        // Include after select. Issue #16752.
        => AssertIncludeOnNonEntity(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(l1 => l1.OneToOne_Required_PK1)
                    .Where(t => t != null)
                    .Select(l2 => l2.OneToOne_Required_PK2)
                    .Where(t => t != null)
                    .Select(l3 => l3.OneToOne_Required_PK3)
                    .Where(t => t != null)
                    .Select(l4 => l4.OneToOne_Required_FK_Inverse4.OneToOne_Required_FK_Inverse3)
                    .Include(l2 => l2.OneToOne_Optional_FK2),
                elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2)),
                elementSorter: e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Comparing_collection_navigation_on_optional_reference_to_null(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2 == null).Select(l1 => l1.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_with_client_eval_and_navigation1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>().Select(l2 => ss.Set<Level2>().OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse2.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_with_client_eval_and_navigation2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level2>().Select(
                l2 => ss.Set<Level2>().OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse2.Name == "L1 02"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_with_client_eval_and_multi_level_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level3>().Select(
                l3 => ss.Set<Level3>().OrderBy(l => l.Id).First().OneToOne_Required_FK_Inverse3.OneToOne_Required_FK_Inverse2.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_doesnt_get_pushed_down_into_subquery_with_result_operator(bool async)
        => AssertQuery(
            async,
            ss =>
                from l1 in ss.Set<Level1>()
                where l1.Id < 3
                select (from l3 in ss.Set<Level3>()
                        select l3).Distinct().OrderBy(l => l.Id).Skip(1).FirstOrDefault().Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  where l1.Id < 3
                  select new
                  {
                      Key = l1.Id,
                      Subquery = (from l3 in ss.Set<Level3>()
                                  orderby l3.Id
                                  select l3).Distinct().Skip(1).FirstOrDefault().Name
                  },
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => Assert.Equal(e.Key, a.Key));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_collection_navigation_count(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  select new { l1.Id, l1.OneToOne_Optional_FK1.OneToMany_Optional2.Count },
            ss => from l1 in ss.Set<Level1>()
                  select new { l1.Id, Count = l1.OneToOne_Optional_FK1.OneToMany_Optional2.MaybeScalar(x => x.Count) ?? 0 },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_optional_navigation_property_string_concat(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  from l2 in l1.OneToMany_Optional1.Where(l => l.Id > 5).OrderByDescending(l => l.Name).DefaultIfEmpty()
                  select l1.Name + " " + (l2 != null ? l2.Name : "NULL"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Entries_for_detached_entities_are_removed(bool async)
    {
        using var context = CreateContext();

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        var queryable = QueryAsserter.SetSourceCreator(context).Set<Level2>().OrderBy(l2 => l2.Id);
        var entity = async ? await queryable.FirstAsync() : queryable.First();
        var entry = context.ChangeTracker.Entries().Single();
        Assert.Same(entity, entry.Entity);

        entry.State = EntityState.Detached;

        Assert.Empty(context.ChangeTracker.Entries());

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_with_groupby_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1)
                .GroupBy(g => g.Name)
                .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(e => e.OneToOne_Optional_FK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multi_include_with_groupby_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToMany_Optional2)
                .GroupBy(g => g.Name)
                .Select(g => g.OrderBy(e => e.Id).FirstOrDefault()),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(e1 => e1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(e2 => e2.OneToMany_Optional2, "OneToOne_Optional_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_include_multiple_derived_navigation_with_same_name_and_same_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<InheritanceBase1>().Include("ReferenceSameType"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<InheritanceDerived1>(e1 => e1.ReferenceSameType),
                new ExpectedInclude<InheritanceDerived2>(e2 => e2.ReferenceSameType)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_include_multiple_derived_navigation_with_same_name_and_different_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<InheritanceBase1>().Include("ReferenceDifferentType"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<InheritanceDerived1>(e1 => e1.ReferenceDifferentType),
                new ExpectedInclude<InheritanceDerived2>(e2 => e2.ReferenceDifferentType)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        String_include_multiple_derived_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
            bool async)
        => AssertQuery(
            async,
            ss => ss.Set<InheritanceBase1>().Include("ReferenceDifferentType.BaseCollection"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<InheritanceDerived1>(e1 => e1.ReferenceDifferentType),
                new ExpectedInclude<InheritanceDerived2>(e2 => e2.ReferenceDifferentType),
                new ExpectedInclude<InheritanceLeaf2>(e3 => e3.BaseCollection, "ReferenceDifferentType")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_include_multiple_derived_collection_navigation_with_same_name_and_same_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<InheritanceBase1>().Include("CollectionSameType"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<InheritanceDerived1>(e1 => e1.CollectionSameType),
                new ExpectedInclude<InheritanceDerived2>(e2 => e2.CollectionSameType)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_include_multiple_derived_collection_navigation_with_same_name_and_different_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<InheritanceBase1>().Include("CollectionDifferentType"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<InheritanceDerived1>(e1 => e1.CollectionDifferentType),
                new ExpectedInclude<InheritanceDerived2>(e2 => e2.CollectionDifferentType)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task
        String_include_multiple_derived_collection_navigation_with_same_name_and_different_type_nested_also_includes_partially_matching_navigation_chains(
            bool async)
        => AssertQuery(
            async,
            ss => ss.Set<InheritanceBase1>().Include("CollectionDifferentType.BaseCollection"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<InheritanceDerived1>(e1 => e1.CollectionDifferentType),
                new ExpectedInclude<InheritanceDerived2>(e2 => e2.CollectionDifferentType),
                new ExpectedInclude<InheritanceLeaf2>(e3 => e3.BaseCollection, "CollectionDifferentType")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_include_multiple_derived_navigations_complex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<InheritanceBase2>().Include("Reference.CollectionDifferentType").Include("Collection.ReferenceSameType"),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<InheritanceBase2>(e1 => e1.Reference),
                new ExpectedInclude<InheritanceDerived1>(e2 => e2.CollectionDifferentType, "Reference"),
                new ExpectedInclude<InheritanceDerived2>(e3 => e3.CollectionDifferentType, "Reference"),
                new ExpectedInclude<InheritanceBase2>(e4 => e4.Collection),
                new ExpectedInclude<InheritanceDerived1>(e5 => e5.ReferenceSameType, "Collection"),
                new ExpectedInclude<InheritanceDerived2>(e6 => e6.ReferenceSameType, "Collection")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nav_rewrite_doesnt_apply_null_protection_for_function_arguments(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_PK1 != null)
                .Select(l1 => Math.Max(l1.OneToOne_Optional_PK1.Level1_Required_Id, 7)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Accessing_optional_property_inside_result_operator_subquery(bool async)
    {
        var names = new[] { "Name1", "Name2" };

        return AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l1 => names.All(n => l1.OneToOne_Optional_FK1.Name != n)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_subquery_with_custom_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).SelectMany(
                l1 => l1.OneToMany_Optional1.Select(
                    l2 => new { l2.Name })).Take(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToOne_Optional_FK1),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToOne_Optional_PK1),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include4(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l1 => l1.OneToOne_Optional_PK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include5(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_FK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include6(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2).Select(l1 => l1.OneToOne_Optional_FK1),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include7(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                .Select(l1 => l1.OneToOne_Optional_PK1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include8(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Where(l2 => l2.OneToOne_Optional_FK_Inverse2.Name != "Fubar")
                .Include(l2 => l2.OneToOne_Optional_FK_Inverse2),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK_Inverse2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include9(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Include(l2 => l2.OneToOne_Optional_FK_Inverse2)
                .Where(l2 => l2.OneToOne_Optional_FK_Inverse2.Name != "Fubar"),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK_Inverse2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include10(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_PK3, "OneToOne_Optional_FK1.OneToOne_Optional_FK2")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include11(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2)
                .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_FK3)
                .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3)
                .Include(l1 => l1.OneToOne_Optional_PK1.OneToOne_Optional_PK2),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_FK1"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_FK3, "OneToOne_Optional_PK1.OneToOne_Optional_FK2"),
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_PK1),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_PK1"),
                new ExpectedInclude<Level3>(l3 => l3.OneToOne_Optional_PK3, "OneToOne_Optional_PK1.OneToOne_Optional_FK2"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_PK2, "OneToOne_Optional_PK1")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include12(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                .Select(l1 => l1.OneToOne_Optional_FK1),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include13(bool async)
    {
        var expectedIncludes = new IExpectedInclude[] { new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1) };

        return AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1)
                .Select(l1 => new { one = l1, two = l1 }),
            elementAsserter: (e, a) =>
            {
                AssertInclude(e.one, a.one, expectedIncludes);
                AssertInclude(e.two, a.two, expectedIncludes);
            },
            elementSorter: e => e.one.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include14(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(l1 => l1.OneToOne_Optional_FK1).ThenInclude(l2 => l2.OneToOne_Optional_FK2)
                .Select(
                    l1 => new
                    {
                        one = l1,
                        two = l1.OneToOne_Optional_FK1,
                        three = l1.OneToOne_Optional_PK1
                    }),
            elementAsserter: (e, a) =>
            {
                AssertInclude(
                    e.one, a.one, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1),
                    new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2, "OneToOne_Optional_FK1"));
                AssertInclude(e.two, a.two, new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK2));
                AssertEqual(e.three, a.three);
            },
            elementSorter: e => e.one.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include17(bool async)
    {
        using var ctx = CreateContext();
        var query = ctx.LevelOne.Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 })
            .Include(x => x.foo.OneToOne_Optional_FK2).Distinct();

        // Include after select. Issue #16752.
        return AssertIncludeOnNonEntity(
            () =>
            {
                if (async)
                {
                    return query.ToListAsync();
                }

                query.ToList();
                return Task.CompletedTask;
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include18_1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(x => x.OneToOne_Optional_FK1).Distinct(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include18_1_1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(x => x.OneToOne_Required_FK1.Name).Include(x => x.OneToOne_Optional_FK1).Take(10),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include18_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(x => x.OneToOne_Required_FK1.Name != "Foo").Include(x => x.OneToOne_Optional_FK1).Distinct(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include18_3(bool async)
    {
        using var ctx = CreateContext();

        var query = ctx.LevelOne
            .OrderBy(x => x.OneToOne_Required_FK1.Name)
            .Include(x => x.OneToOne_Optional_FK1)
            .Select(l1 => new { foo = l1, bar = l1 }).Take(10);

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include18_3_1(bool async)
    {
        using var ctx = CreateContext();

        var query = ctx.LevelOne
            .OrderBy(x => x.OneToOne_Required_FK1.Name)
            .Include(x => x.OneToOne_Optional_FK1)
            .Select(l1 => new { foo = l1, bar = l1 })
            .Take(10)
            .Select(x => new { x.foo, x.bar });

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include18_3_2(bool async)
    {
        using var ctx = CreateContext();

        var query = ctx.LevelOne
            .OrderBy(x => x.OneToOne_Required_FK1.Name)
            .Include(x => x.OneToOne_Optional_FK1)
            .Select(l1 => new { outer_foo = new { inner_foo = l1, inner_bar = l1.Name }, outer_bar = l1 })
            .Take(10);

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include18_3_3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(x => x.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                .Select(l1 => l1.OneToOne_Optional_FK1)
                .Distinct(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level2>(l1 => l1.OneToOne_Optional_FK2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include18_4(bool async)
    {
        using var ctx = CreateContext();

        var query = ctx.LevelOne.Include(x => x.OneToOne_Optional_FK1).Select(l1 => new { foo = l1, bar = l1 }).Distinct();

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include18(bool async)
    {
        using var ctx = CreateContext();

        var query = ctx.LevelOne
            .Include(x => x.OneToOne_Optional_FK1)
            .Select(l1 => new { foo = l1, bar = l1.OneToOne_Optional_PK1 })
            .OrderBy(x => x.foo.Id)
            .Take(10);

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Include19(bool async)
    {
        using var ctx = CreateContext();

        var query = ctx.LevelOne
            .Include(x => x.OneToOne_Optional_FK1)
            .Select(l1 => new { foo = l1.OneToOne_Optional_FK1, bar = l1.OneToOne_Optional_PK1 })
            .Distinct();

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_all_method_include_gets_ignored(bool async)
        => AssertAll(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToOne_Optional_FK1).Include(l1 => l1.OneToMany_Optional1),
            predicate: l1 => l1.Name != "Foo");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_navigations_in_the_result_selector1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Join(
                ss.Set<Level2>(), l1 => l1.Id, l2 => l2.Level1_Required_Id, (o, i) => new { o.OneToOne_Optional_FK1, i }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_with_navigations_in_the_result_selector2(bool async)
    {
        using var ctx = CreateContext();

        var query = ctx.LevelOne.Join(
            ctx.LevelTwo, l1 => l1.Id, l2 => l2.Level1_Required_Id,
            (o, i) => new { o.OneToOne_Optional_FK1, i.OneToMany_Optional2 });

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Member_pushdown_chain_3_levels_deep(bool async)
    {
        using var ctx = CreateContext();

        var query = from l1 in ctx.LevelOne
                    orderby l1.Id
                    where (from l2 in ctx.LevelTwo
                           orderby l2.Id
                           where l2.Level1_Optional_Id == l1.Id
                           select (from l3 in ctx.LevelThree
                                   orderby l3.Id
                                   where l3.Level2_Required_Id == l2.Id
                                   select (from l4 in ctx.LevelFour
                                           where l4.Level3_Required_Id == l3.Id
                                           orderby l4.Id
                                           select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault().Name
                        != "Foo"
                    select l1;

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Member_pushdown_chain_3_levels_deep_entity(bool async)
    {
        using var ctx = CreateContext();

        var query = from l1 in ctx.LevelOne
                    orderby l1.Id
                    select (from l2 in ctx.LevelTwo
                            orderby l2.Id
                            where l2.Level1_Optional_Id == l1.Id
                            select (from l3 in ctx.LevelThree
                                    orderby l3.Id
                                    where l3.Level2_Required_Id == l2.Id
                                    select (from l4 in ctx.LevelFour
                                            where l4.Level3_Required_Id == l3.Id
                                            orderby l4.Id
                                            select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault();

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Member_pushdown_with_collection_navigation_in_the_middle(bool async)
    {
        using var ctx = CreateContext();

        var query = from l1 in ctx.LevelOne
                    orderby l1.Id
                    select (from l2 in ctx.LevelTwo
                            orderby l2.Id
                            where l2.Level1_Required_Id == l1.Id
                            select l2.OneToMany_Optional2.Select(
                                l3 => (from l4 in ctx.LevelFour
                                       where l4.Level3_Required_Id == l3.Id
                                       orderby l4.Id
                                       select l4).FirstOrDefault()).FirstOrDefault()).FirstOrDefault().Name;

        _ = async ? await query.ToListAsync() : query.ToList();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_pushdown_with_multiple_collections(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(
                l1 => l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).FirstOrDefault().OneToMany_Optional2.OrderBy(l3 => l3.Id)
                    .FirstOrDefault().Name),
            ss => ss.Set<Level1>().Select(
                l1 => l1.OneToMany_Optional1.OrderBy(l2 => l2.Id).FirstOrDefault().Maybe(
                    x => x.OneToMany_Optional2.OrderBy(l3 => l3.Id)
                        .FirstOrDefault().Maybe(xx => xx.Name))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_multiple_collections_on_same_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Include(l1 => l1.OneToMany_Optional1).Include(l1 => l1.OneToMany_Required1),
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional1),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Required1)),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_check_removal_applied_recursively(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(
                l1 =>
                    (((l1.OneToOne_Optional_FK1 == null
                                ? null
                                : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2)
                            == null
                                ? null
                                : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3)
                        == null
                            ? null
                            : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3.Name)
                    == "L4 01"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_check_different_structure_does_not_remove_null_checks(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(
                l1 =>
                    (l1.OneToOne_Optional_FK1 == null
                        ? null
                        : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2 == null
                            ? null
                            : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3 == null
                                ? null
                                : l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.OneToOne_Optional_PK3.Name)
                    == "L4 01"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_over_entities_with_different_nullability(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>()
                .GroupJoin(ss.Set<Level2>(), l1 => l1.Id, l2 => l2.Level1_Optional_Id, (l1, l2s) => new { l1, l2s })
                .SelectMany(g => g.l2s.DefaultIfEmpty(), (g, l2) => new { g.l1, l2 })
                .Concat(
                    ss.Set<Level2>().GroupJoin(ss.Set<Level1>(), l2 => l2.Level1_Optional_Id, l1 => l1.Id, (l2, l1s) => new { l2, l1s })
                        .SelectMany(g => g.l1s.DefaultIfEmpty(), (g, l1) => new { l1, g.l2 })
                        .Where(e => e.l1.Equals(null)))
                .Select(e => (int?)e.l1.Id),
            ss => ss.Set<Level1>()
                .GroupJoin(ss.Set<Level2>(), l1 => l1.Id, l2 => l2.Level1_Optional_Id, (l1, l2s) => new { l1, l2s })
                .SelectMany(g => g.l2s.DefaultIfEmpty(), (g, l2) => new { g.l1, l2 })
                .Concat(
                    ss.Set<Level2>().GroupJoin(ss.Set<Level1>(), l2 => l2.Level1_Optional_Id, l1 => l1.Id, (l2, l1s) => new { l2, l1s })
                        .SelectMany(g => g.l1s.DefaultIfEmpty(), (g, l1) => new { l1, g.l2 })
                        .Where(e => e.l1 == null))
                .Select(e => e.l1.MaybeScalar(x => x.Id)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Including_reference_navigation_and_projecting_collection_navigation_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(e => e.OneToOne_Required_FK1)
                .Include(e => e.OneToMany_Required1)
                .Select(e => new { e, First = e.OneToMany_Required1.OrderByDescending(e => e.Id).FirstOrDefault() }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_collection_count_ThenBy_reference_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.OneToOne_Required_FK1.OneToMany_Required2.Count())
                .ThenBy(l1 => l1.OneToOne_Required_FK1.OneToOne_Required_FK2.Name),
            ss => ss.Set<Level1>()
                .OrderBy(l1 => l1.OneToOne_Required_FK1.OneToMany_Required2.MaybeScalar(x => x.Count()) ?? 0)
                .ThenBy(l1 => l1.OneToOne_Required_FK1.OneToOne_Required_FK2.Name),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_conditional_is_not_applied_explicitly_for_optional_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l1 => l1.OneToOne_Optional_FK1 != null && l1.OneToOne_Optional_FK1.Name == "L2 01"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_selector_cast_using_as(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Level1>().Select(s => s.Id as int?));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_filter_with_include_selector_cast_using_as(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(l1 => l1.Id > l1.OneToMany_Optional1.Select(l2 => l2.Id as int?).Sum()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_with_joined_where_clause_cast_using_as(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Where(w => (w.Id + 7) == w.OneToOne_Optional_FK1.Id as int?));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Required_Id
                  join l3 in ss.Set<Level3>() on l2.Id equals l3.Level2_Required_Id
                  join l4 in ss.Set<Level4>() on l3.Id equals l4.Level3_Required_Id
                  from other in ss.Set<Level1>().Where(x => x.Id <= l2.Id && x.Name == l4.Name).DefaultIfEmpty()
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(
                l1 => l1.OneToMany_Optional1.DefaultIfEmpty().SelectMany(
                    l2 => l2.OneToOne_Required_PK2.OneToMany_Optional3.DefaultIfEmpty()
                        .Select(
                            l4 => new
                            {
                                l1Name = l1.Name,
                                l2Name = l2.OneToOne_Required_PK2.Name,
                                l3Name = l4.OneToOne_Optional_PK_Inverse4.Name
                            }))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_over_optional_navigation_with_null_constant(bool async)
    {
        using var ctx = CreateContext();

        var queryable = ctx.Set<Level1>().Select(l1 => l1.OneToOne_Optional_FK1);
        var result = async ? await queryable.ContainsAsync(null) : queryable.Contains(null);
        var expected = Fixture.GetExpectedData().Set<Level1>().Select(l1 => l1.OneToOne_Optional_FK1).Contains(null);

        Assert.Equal(expected, result);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_optional_navigation_with_null_parameter(bool async)
        => AssertSingleResult(
            async,
            ss => ss.Set<Level1>().Select(l1 => l1.OneToOne_Optional_FK1).Contains(null),
            ss => ss.Set<Level1>().Select(l1 => l1.OneToOne_Optional_FK1).ContainsAsync(null, default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_optional_navigation_with_null_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(
                l1 => new
                {
                    l1.Name,
                    OptionalName = l1.OneToOne_Optional_FK1.Name,
                    Contains = ss.Set<Level1>().Select(x => x.OneToOne_Optional_FK1.Name).Contains(l1.OneToOne_Optional_FK1.Name)
                }),
            elementSorter: e => (e.Name, e.OptionalName, e.Contains));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_optional_navigation_with_null_entity_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(
                l1 => new
                {
                    l1.Name,
                    OptionalName = l1.OneToOne_Optional_FK1.Name,
                    Contains = ss.Set<Level1>().Select(x => x.OneToOne_Optional_FK1).Contains(l1.OneToOne_Optional_PK1)
                }),
            elementSorter: e => (e.Name, e.OptionalName, e.Contains));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Element_selector_with_coalesce_repeated_in_aggregate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>().GroupBy(
                    l1 => l1.OneToOne_Required_PK1.OneToOne_Required_PK2.Name,
                    l1 => new { Id = ((int?)l1.OneToOne_Required_PK1.Id ?? 0) })
                .Where(g => g.Min(l1 => l1.Id + l1.Id) > 0)
                .Select(g => g.Count()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nested_object_constructed_from_group_key_properties(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Where(l1 => l1.OneToOne_Optional_FK1 != null)
                .GroupBy(
                    l1 => new
                    {
                        l1.Id,
                        l1.Date,
                        l1.Name,
                        InnerId = l1.OneToOne_Optional_FK1.Id,
                        InnerDate = l1.OneToOne_Optional_FK1.Date,
                        InnerOptionalId = l1.OneToOne_Optional_FK1.Level1_Optional_Id,
                        InnerRequiredId = l1.OneToOne_Optional_FK1.Level1_Required_Id,
                        InnerName = l1.OneToOne_Required_FK1.Name
                    })
                .Select(
                    g => new
                    {
                        NestedEntity = new Level1
                        {
                            Id = g.Key.Id,
                            Name = g.Key.Name,
                            Date = g.Key.Date,
                            OneToOne_Optional_FK1 = new Level2
                            {
                                Id = g.Key.InnerId,
                                Name = g.Key.InnerName,
                                Date = g.Key.InnerDate,
                                Level1_Optional_Id = g.Key.InnerOptionalId,
                                Level1_Required_Id = g.Key.InnerRequiredId
                            }
                        },
                        Aggregate = g.Sum(x => x.Name.Length)
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_where_required_relationship(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .GroupBy(l2 => l2.OneToMany_Required_Inverse2.Id)
                .Select(g => new { g.Key, Max = g.Max(e => e.Id) })
                .Where(x => x.Max != 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_where_required_relationship_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .GroupBy(l2 => l2.OneToMany_Required_Inverse2.Id)
                .Select(g => new { g.Key, Max = g.Max(e => e.Id) })
                .Where(x => x.Max < 2 || x.Max > 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_over_null_check_ternary_and_nested_dto_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Select(
                    l1 => new Level1Dto
                    {
                        Id = l1.Id,
                        Name = l1.Name,
                        Level2 = l1.OneToOne_Optional_FK1 == null
                            ? null
                            : new Level2Dto
                            {
                                Id = l1.OneToOne_Optional_FK1.Id, Name = l1.OneToOne_Optional_FK1.Name,
                            }
                    })
                .OrderBy(e => e.Level2.Name)
                .ThenBy(e => e.Id),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Name, a.Name);
                if (e.Level2 == null)
                {
                    Assert.Null(a.Level2);
                }
                else
                {
                    Assert.NotNull(a.Level2);
                    Assert.Equal(e.Level2.Id, a.Level2.Id);
                    Assert.Equal(e.Level2.Name, a.Level2.Name);
                }
            });

    private class Level1Dto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Level2Dto Level2 { get; set; }
    }

    private class Level2Dto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Member_over_null_check_ternary_and_nested_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        l1.Name,
                        Level2 = l1.OneToOne_Optional_FK1 == null
                            ? null
                            : new
                            {
                                l1.OneToOne_Optional_FK1.Id,
                                l1.OneToOne_Optional_FK1.Name,
                                Level3 = l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2 == null
                                    ? null
                                    : new
                                    {
                                        l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Id,
                                        l1.OneToOne_Optional_FK1.OneToOne_Optional_FK2.Name
                                    }
                            }
                    })
                .Where(e => e.Level2.Level3.Name != "L"),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Name, a.Name);
                if (e.Level2 == null)
                {
                    Assert.Null(a.Level2);
                }
                else
                {
                    Assert.NotNull(a.Level2);
                    Assert.Equal(e.Level2.Id, a.Level2.Id);
                    Assert.Equal(e.Level2.Name, a.Level2.Name);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_skip_without_orderby(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  where l1.Id < 3
                  select (from l3 in ss.Set<Level3>()
                          select l3).Distinct().Skip(1).OrderBy(e => e.Id).FirstOrDefault().Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_take_without_orderby(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  where l1.Id < 3
                  select (from l3 in ss.Set<Level3>()
                          select l3).Distinct().Take(1).OrderBy(e => e.Id).FirstOrDefault().Name);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Let_let_contains_from_outer_let(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>().Include(l => l.OneToMany_Required1)
                  let level2Ids = from level2 in l1.OneToMany_Required1 select level2.Id
                  let level3s = (from level3 in ss.Set<Level3>()
                                 where level2Ids.Contains(level3.Level2_Required_Id)
                                 select level3).AsEnumerable()
                  from level3 in level3s.DefaultIfEmpty()
                  select new { l1, level3 },
            elementSorter: e => (e.l1.Id, e.level3?.Id),
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.l1, a.l1);
                AssertEqual(e.level3, a.level3);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_conditionals_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level2>()
                .Select(
                    l2 => new Level1Dto
                    {
                        Id = l2.Id,
                        Name = l2.OneToOne_Optional_FK2 == null ? null : l2.OneToOne_Optional_FK2.Name,
                        Level2 = l2.OneToOne_Optional_FK_Inverse2 == null ? null : new Level2Dto()
                    }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.Name, a.Name);
                if (e.Level2 == null)
                {
                    Assert.Null(a.Level2);
                }
                else
                {
                    AssertEqual(e.Level2.Id, a.Level2.Id);
                    AssertEqual(e.Level2.Name, a.Level2.Name);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Level1>()
                .Join(
                    ss.Set<Level2>().GroupBy(g => g.Id % 3).Select(g => new { g.Key, Sum = g.Sum(x => x.Id) }),
                    o => new { o.Id, Condition = true },
                    i => new
                    {
                        Id = i.Key, Condition = i.Sum > 10,
                    },
                    (o, i) => i.Key));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_joins_groupby_predicate(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping1
                  from l2 in grouping1.DefaultIfEmpty()
                  join x in (from l3 in ss.Set<Level3>()
                             group l3 by l3.Name
                             into g
                             select new { g.Key, Count = g.Count() }) on l1.Name equals x.Key into grouping2
                  from x in grouping2.DefaultIfEmpty()
                  where l2.Name != null || x.Count > 0
                  select new
                  {
                      l1.Id,
                      l1.Name,
                      Foo = l2 == null ? "Foo" : "Bar"
                  },
            elementSorter: e => (e.Id, e.Name, e.Foo));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_FirstOrDefault_property_accesses_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(x => x.OneToMany_Optional1).ThenInclude(x => x.OneToMany_Optional2)
                .Where(l1 => l1.Id < 3)
                .Select(l1 => new { l1.Id, Pushdown = l1.OneToMany_Optional1.Where(x => x.Name == "L2 02").FirstOrDefault().Name }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_FirstOrDefault_entity_reference_accesses_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Include(x => x.OneToMany_Optional1).ThenInclude(x => x.OneToMany_Optional2)
                .Where(l1 => l1.Id < 3)
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        Pushdown = l1.OneToMany_Optional1
                            .Where(x => x.Name == "L2 02")
                            .FirstOrDefault().OneToOne_Optional_FK2
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_FirstOrDefault_entity_collection_accesses_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Where(l1 => l1.Id < 2)
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        Pushdown = l1.OneToMany_Optional1
                            .Where(x => x.Name == "L2 02")
                            .FirstOrDefault().OneToMany_Optional2.ToList()
                    }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertCollection(e.Pushdown, a.Pushdown);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_collection_FirstOrDefault_followed_by_member_access_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .Where(l1 => l1.Id < 2)
                .Select(
                    l1 => new
                    {
                        l1.Id,
                        Pushdown = l1.OneToMany_Optional1
                            .Where(x => x.Name == "L2 02")
                            .FirstOrDefault().OneToMany_Optional2
                            .OrderBy(x => x.Id)
                            .FirstOrDefault().Name
                    }));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_columns_with_same_name_from_different_entities_making_sure_aliasing_works_after_Distinct(bool async)
        => AssertQuery(
            async,
            ss => (from l1 in ss.Set<Level1>()
                   join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id
                   join l3 in ss.Set<Level3>() on l2.Id equals l3.Level2_Optional_Id
                   select new
                   {
                       Id1 = l1.Id,
                       Id2 = l2.Id,
                       Id3 = l3.Id,
                       Name1 = l1.Name,
                       Name2 = l2.Name
                   }).Distinct().Select(
                x => new
                {
                    Foo = x.Id1,
                    Bar = x.Id2,
                    Baz = x.Id3
                }).Take(10),
            elementSorter: e => (e.Foo, e.Bar, e.Baz));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complex_query_with_let_collection_SelectMany(bool async)
        // Materialization type. Issue #23302.
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => from l1 in ss.Set<Level1>()
                      where l1.Name.StartsWith("L1 0")
                      let inner = from i in ss.Set<Level1>()
                                  where i.Id == l1.Id && i.Id > 5
                                  select i
                      from die in inner.DefaultIfEmpty()
                      select die ?? l1),
            "IQueryable<Level1>");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_without_collection_selector_returning_queryable(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().SelectMany(x => ss.Set<Level2>().Where(l2 => l2.Id < 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_projecting_queryable_followed_by_SelectMany(bool async)
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(x => ss.Set<Level2>().Where(l2 => l2.Id < 10)).SelectMany(x => x)),
            "IQueryable<Level2>");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        => AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id
                  select ss.Set<Level3>().Where(x => x.Id < 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_projecting_queryable_followed_by_Join(bool async)
        // Materialization type. Issue #23302.
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(
                        x => ss.Set<Level2>().Where(l2 => l2.Id < 10))
                    .Join(ss.Set<Level3>(), o => 7, i => i.Id, (o, i) => i)),
            "IQueryable<Level1>");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_projecting_queryable_in_anonymous_projection_followed_by_Join(bool async)
        // Materialization type. Issue #23302.
        => AssertInvalidMaterializationType(
            () => AssertQuery(
                async,
                ss => ss.Set<Level1>().Select(
                        x => new
                        {
                            Subquery = ss.Set<Level2>()
                                .Where(l2 => l2.Id < 10)
                        })
                    .Join(ss.Set<Level3>(), o => 7, i => i.Id, (o, i) => i)),
            "IQueryable<Level1>");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties1(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<Level1>()
                  select new
                  {
                      x.Id,
                      OneToOne_Optional_Self1Id = EF.Property<int?>(x, "OneToOne_Optional_Self1Id"),
                      OneToMany_Required_Self_Inverse1Id = EF.Property<int?>(x, "OneToMany_Required_Self_Inverse1Id"),
                      OneToMany_Optional_Self_Inverse1Id = EF.Property<int?>(x, "OneToMany_Optional_Self_Inverse1Id"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties2(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<Level2>()
                  select new
                  {
                      x.Id,
                      OneToOne_Optional_PK_Inverse2Id = EF.Property<int?>(x, "OneToOne_Optional_PK_Inverse2Id"),
                      OneToMany_Required_Inverse2Id = EF.Property<int?>(x, "OneToMany_Required_Inverse2Id"),
                      OneToMany_Optional_Inverse2Id = EF.Property<int?>(x, "OneToMany_Optional_Inverse2Id"),
                      OneToOne_Optional_Self2Id = EF.Property<int?>(x, "OneToOne_Optional_Self2Id"),
                      OneToMany_Required_Self_Inverse2Id = EF.Property<int?>(x, "OneToMany_Required_Self_Inverse2Id"),
                      OneToMany_Optional_Self_Inverse2Id = EF.Property<int?>(x, "OneToMany_Optional_Self_Inverse2Id"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties3(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<Level3>()
                  select new
                  {
                      x.Id,
                      OneToOne_Optional_PK_Inverse3Id = EF.Property<int?>(x, "OneToOne_Optional_PK_Inverse3Id"),
                      OneToMany_Required_Inverse3Id = EF.Property<int?>(x, "OneToMany_Required_Inverse3Id"),
                      OneToMany_Optional_Inverse3Id = EF.Property<int?>(x, "OneToMany_Optional_Inverse3Id"),
                      OneToOne_Optional_Self3Id = EF.Property<int?>(x, "OneToOne_Optional_Self3Id"),
                      OneToMany_Required_Self_Inverse3Id = EF.Property<int?>(x, "OneToMany_Required_Self_Inverse3Id"),
                      OneToMany_Optional_Self_Inverse3Id = EF.Property<int?>(x, "OneToMany_Optional_Self_Inverse3Id"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties4(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<Level4>()
                  select new
                  {
                      x.Id,
                      OneToOne_Optional_PK_Inverse4Id = EF.Property<int?>(x, "OneToOne_Optional_PK_Inverse4Id"),
                      OneToMany_Required_Inverse4Id = EF.Property<int?>(x, "OneToMany_Required_Inverse4Id"),
                      OneToMany_Optional_Inverse4Id = EF.Property<int?>(x, "OneToMany_Optional_Inverse4Id"),
                      OneToOne_Optional_Self4Id = EF.Property<int?>(x, "OneToOne_Optional_Self4Id"),
                      OneToMany_Required_Self_Inverse4Id = EF.Property<int?>(x, "OneToMany_Required_Self_Inverse4Id"),
                      OneToMany_Optional_Self_Inverse4Id = EF.Property<int?>(x, "OneToMany_Optional_Self_Inverse4Id"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties5(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<InheritanceBase1>()
                  select new
                  {
                      x.Id,
                      InheritanceBase2Id = EF.Property<int?>(x, "InheritanceBase2Id"),
                      InheritanceBase2Id1 = EF.Property<int?>(x, "InheritanceBase2Id1"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties6(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<InheritanceBase1>().OfType<InheritanceDerived1>()
                  select new
                  {
                      x.Id,
                      InheritanceBase2Id = EF.Property<int?>(x, "InheritanceBase2Id"),
                      InheritanceBase2Id1 = EF.Property<int?>(x, "InheritanceBase2Id1"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties7(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<InheritanceBase1>().OfType<InheritanceDerived2>()
                  select new
                  {
                      x.Id,
                      InheritanceBase2Id = EF.Property<int?>(x, "InheritanceBase2Id"),
                      InheritanceBase2Id1 = EF.Property<int?>(x, "InheritanceBase2Id1"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties8(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<InheritanceBase2>()
                  select new
                  {
                      x.Id, InheritanceLeaf2Id = EF.Property<int?>(x, "InheritanceLeaf2Id"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties9(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<InheritanceLeaf1>()
                  select new
                  {
                      x.Id,
                      DifferentTypeReference_InheritanceDerived1Id =
                          EF.Property<int?>(x, "DifferentTypeReference_InheritanceDerived1Id"),
                      InheritanceDerived1Id = EF.Property<int?>(x, "InheritanceDerived1Id"),
                      InheritanceDerived1Id1 = EF.Property<int?>(x, "InheritanceDerived1Id1"),
                      InheritanceDerived2Id = EF.Property<int?>(x, "InheritanceDerived2Id"),
                      SameTypeReference_InheritanceDerived1Id = EF.Property<int?>(x, "SameTypeReference_InheritanceDerived1Id"),
                      SameTypeReference_InheritanceDerived2Id = EF.Property<int?>(x, "SameTypeReference_InheritanceDerived2Id"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_shadow_properties10(bool async)
        => AssertQuery(
            async,
            ss => from x in ss.Set<InheritanceLeaf2>()
                  select new
                  {
                      x.Id,
                      DifferentTypeReference_InheritanceDerived2Id =
                          EF.Property<int?>(x, "DifferentTypeReference_InheritanceDerived2Id"),
                      InheritanceDerived2Id = EF.Property<int?>(x, "InheritanceDerived2Id"),
                  },
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Prune_does_not_throw_null_ref(bool async)
        => AssertQuery(
            async,
            ss => from ids in (from l2 in ss.Set<Level2>().Where(i => i.Id < 5)
                               select l2.Level1_Required_Id).DefaultIfEmpty()
                  from l1 in ss.Set<Level1>().Where(x => x.Id != ids)
                  select l1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure(bool async)
    {
        var prm = 10;

        return AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                  from l2 in grouping.Where(x => x.Id != prm).DefaultIfEmpty()
                  select new { Id1 = l1.Id, Id2 = (int?)l2.Id },
            elementSorter: e => (e.Id1, e.Id2),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_with_predicate_using_closure(bool async)
    {
        var prm = 10;

        return AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping
                  from l2 in grouping.Where(x => x.Id != prm)
                  select new { Id1 = l1.Id, Id2 = l2.Id },
            elementSorter: e => (e.Id1, e.Id2),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure_nested(bool async)
    {
        var prm1 = 10;
        var prm2 = 20;

        return AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping1
                  from l2 in grouping1.Where(x => x.Id != prm1).DefaultIfEmpty()
                  join l3 in ss.Set<Level3>() on l2.Id equals l3.Level2_Optional_Id into grouping2
                  from l3 in grouping2.Where(x => x.Id != prm2).DefaultIfEmpty()
                  select new
                  {
                      Id1 = l1.Id,
                      Id2 = (int?)l2.Id,
                      Id3 = (int?)l3.Id
                  },
            elementSorter: e => (e.Id1, e.Id2, e.Id3),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                Assert.Equal(e.Id3, a.Id3);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_with_predicate_using_closure_nested(bool async)
    {
        var prm1 = 10;
        var prm2 = 20;

        return AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping1
                  from l2 in grouping1.Where(x => x.Id != prm1)
                  join l3 in ss.Set<Level3>() on l2.Id equals l3.Level2_Optional_Id into grouping2
                  from l3 in grouping2.Where(x => x.Id != prm2)
                  select new
                  {
                      Id1 = l1.Id,
                      Id2 = l2.Id,
                      Id3 = l3.Id
                  },
            elementSorter: e => (e.Id1, e.Id2, e.Id3),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                Assert.Equal(e.Id3, a.Id3);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_DefaultIfEmpty_with_predicate_using_closure_nested_same_param(bool async)
    {
        var prm = 10;

        return AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping1
                  from l2 in grouping1.Where(x => x.Id != prm).DefaultIfEmpty()
                  join l3 in ss.Set<Level3>() on l2.Id equals l3.Level2_Optional_Id into grouping2
                  from l3 in grouping2.Where(x => x.Id != prm).DefaultIfEmpty()
                  select new
                  {
                      Id1 = l1.Id,
                      Id2 = (int?)l2.Id,
                      Id3 = (int?)l3.Id
                  },
            elementSorter: e => (e.Id1, e.Id2, e.Id3),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                Assert.Equal(e.Id3, a.Id3);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_with_predicate_using_closure_nested_same_param(bool async)
    {
        var prm = 10;

        return AssertQuery(
            async,
            ss => from l1 in ss.Set<Level1>()
                  join l2 in ss.Set<Level2>() on l1.Id equals l2.Level1_Optional_Id into grouping1
                  from l2 in grouping1.Where(x => x.Id != prm)
                  join l3 in ss.Set<Level3>() on l2.Id equals l3.Level2_Optional_Id into grouping2
                  from l3 in grouping2.Where(x => x.Id != prm)
                  select new
                  {
                      Id1 = l1.Id,
                      Id2 = l2.Id,
                      Id3 = l3.Id
                  },
            elementSorter: e => (e.Id1, e.Id2, e.Id3),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id1, a.Id1);
                Assert.Equal(e.Id2, a.Id2);
                Assert.Equal(e.Id3, a.Id3);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_optional_navs_should_not_deadlock(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Level2>().Where(x => x.OneToMany_Optional_Inverse2 != null
                && x.OneToMany_Optional_Inverse2.Name.Contains("L1 01")
                || x.OneToOne_Optional_FK_Inverse2 != null
                    && x.OneToOne_Optional_FK_Inverse2.Name.Contains("L1 01")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Null_check_removal_applied_recursively_complex(bool async)
    {
        var userParam = Expression.Parameter(typeof(Level3), "l3");
        var builderProperty = Expression.MakeMemberAccess(
            userParam, typeof(Level3).GetProperty(nameof(Level3.OneToMany_Required_Inverse3)));
        var cityProperty = Expression.MakeMemberAccess(
            builderProperty, typeof(Level2).GetProperty(nameof(Level2.OneToMany_Required_Inverse2)));
        var nameProperty = Expression.MakeMemberAccess(cityProperty, typeof(Level1).GetProperty(nameof(Level1.Name)));

        //{s => (IIF((IIF((l3.Inverse3 == null), null, s.Inverse3.Inverse2) == null), null, s.Inverse3.Inverse2.Name) == "L1 01")}
        var selection = Expression.Lambda<Func<Level3, bool>>(
            Expression.Equal(
                Expression.Condition(
                    Expression.Equal(
                        Expression.Condition(
                            Expression.Equal(
                                builderProperty,
                                Expression.Constant(null, typeof(Level2))),
                            Expression.Constant(null, typeof(Level1)),
                            cityProperty),
                        Expression.Constant(null, typeof(Level1))),
                    Expression.Constant(null, typeof(string)),
                    nameProperty),
                Expression.Constant("L1 01", typeof(string))),
            userParam);

        return AssertQuery(
            async,
            ss => ss.Set<Level3>()
                .Where(selection)
                .Include(x => x.OneToMany_Required_Inverse3).ThenInclude(x => x.OneToMany_Required_Inverse2)
                .Include(x => x.OneToMany_Optional3),
            elementAsserter: (e, a) => AssertInclude(
                e, a, new ExpectedInclude<Level3>(x => x.OneToMany_Required_Inverse3),
                new ExpectedInclude<Level2>(x => x.OneToMany_Required_Inverse2, "OneToMany_Required_Inverse3"),
                new ExpectedInclude<Level3>(x => x.OneToMany_Optional3)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_projection_with_first(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(x => new
            {
                x.Id,
                Results = x.OneToMany_Optional1.OrderBy(xx => xx.Id).First().OneToMany_Optional2.Select(xx => xx.OneToOne_Required_FK3.Id)
            }),
            ss => ss.Set<Level1>().Select(x => new
            {
                x.Id,
                Results = x.OneToMany_Optional1.OrderBy(xx => xx.Id).Any()
                    ? x.OneToMany_Optional1.OrderBy(xx => xx.Id).First().OneToMany_Optional2.Select(xx => xx.OneToOne_Required_FK3.Id)
                    : new List<int>()
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(e.Results, a.Results);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_in_multi_level_nested_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().OrderBy(l1 => l1.Id).Take(2).Select(x => new
            {
                x.Id,
                LevelTwos = x.OneToMany_Optional1.AsQueryable().Select(xx => new
                {
                    xx.Id,
                    LevelThree = new
                    {
                        xx.OneToOne_Required_FK2.Id,
                        LevelFour = new
                        {
                            xx.OneToOne_Required_FK2.OneToOne_Required_FK3.Id,
                            Result = (xx.OneToOne_Required_FK2.OneToMany_Optional3.Max(xxx => (int?)xxx.Id) ?? 0) > 1
                        }
                    }
                }).ToList()
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(
                    e.LevelTwos,
                    a.LevelTwos,
                    elementSorter: ee => ee.Id,
                    elementAsserter: (ee, aa) =>
                    {
                        AssertEqual(ee.Id, aa.Id);
                        AssertEqual(ee.LevelThree.Id, aa.LevelThree.Id);
                        AssertEqual(ee.LevelThree.LevelFour.Id, aa.LevelThree.LevelFour.Id);
                        AssertEqual(ee.LevelThree.LevelFour.Result, aa.LevelThree.LevelFour.Result);
                    });
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_select_many_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(x => new
            {
                x.Id,
                Collection = x.OneToMany_Optional1
                    .SelectMany(xx => xx.OneToMany_Optional2)
                    .OrderBy(xx => xx.Id).Take(12)
                    .Select(xx => new
                    {
                        xx.Id,
                        RefId = xx.OneToOne_Optional_FK3.Id
                    }).ToList(),
                Count = x.OneToMany_Optional1
                    .SelectMany(xx => xx.OneToMany_Optional2).Count(xx => xx.Name != "")
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(
                    e.Collection,
                    a.Collection,
                    ordered: true,
                    elementAsserter: (ee, aa) =>
                    {
                        AssertEqual(ee.Id, aa.Id);
                        AssertEqual(ee.RefId, aa.RefId);
                    });
                AssertEqual(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Single_select_many_in_projection_with_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Level1>().Select(x => new
            {
                x.Id,
                Collection = x.OneToMany_Optional1
                    .SelectMany(xx => xx.OneToMany_Optional2)
                    .OrderBy(xx => xx.Id).Take(12)
                    .Select(xx => new
                    {
                        xx.Id,
                        RefId = xx.OneToOne_Optional_FK3.Id
                    }).ToList(),
            }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertCollection(
                    e.Collection,
                    a.Collection,
                    ordered: true,
                    elementAsserter: (ee, aa) =>
                    {
                        AssertEqual(ee.Id, aa.Id);
                        AssertEqual(ee.RefId, aa.RefId);
                    });
            });
}
