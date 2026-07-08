// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindGroupByQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindGroupByQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    public override Task Final_GroupBy_property_entity(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_entity(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_entity(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_non_nullable(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_non_nullable(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_anonymous_type(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_anonymous_type(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_multiple_properties_entity(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_multiple_properties_entity(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_complex_key_entity(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_complex_key_entity(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_nominal_type_entity(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_nominal_type_entity(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_anonymous_type_element_selector(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_anonymous_type_element_selector(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_Include_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_collection(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_projecting_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_projecting_collection(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_projecting_collection_composed(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_projecting_collection_composed(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_projecting_collection_and_single_result(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_projecting_collection_and_single_result(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_TagWith(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_TagWith(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    // The in-memory provider doesn't implement joining on a client-evaluated GroupBy result
    // (InMemoryQueryExpression.AddJoin throws NotImplementedException); unrelated to the fold under test.
    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_ValueTuple_projection_joined_on_tuple_member(bool async)
        => base.GroupBy_ValueTuple_projection_joined_on_tuple_member(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Anonymous_Type_With_Entire_Entity(bool async)
        => base.GroupBy_Select_Anonymous_Type_With_Entire_Entity(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Entire_Entity_Order(bool async)
        => base.GroupBy_Select_Entire_Entity_Order(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Entire_Entity_Where(bool async)
        => base.GroupBy_Select_Entire_Entity_Where(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Entire_Entity_Select(bool async)
        => base.GroupBy_Select_Entire_Entity_Select(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Entire_Entity_Where_Select(bool async)
        => base.GroupBy_Select_Entire_Entity_Where_Select(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Entire_Entity_FirstOrDefault_Where(bool async)
        => base.GroupBy_Select_Entire_Entity_FirstOrDefault_Where(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_ResultSelector_Entire_Entity_Where(bool async)
        => base.GroupBy_ResultSelector_Entire_Entity_Where(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Entire_Entity_GroupBy(bool async)
        => base.GroupBy_Select_Entire_Entity_GroupBy(async);

    [Theory(Skip = "Issue#31209")]
    public override Task GroupBy_Select_Entire_Entity_composite_key_Select(bool async)
        => base.GroupBy_Select_Entire_Entity_composite_key_Select(async);
}
