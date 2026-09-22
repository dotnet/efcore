// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsCollectionsQueryInMemoryTest(ComplexNavigationsQueryInMemoryFixture fixture)
    : ComplexNavigationsCollectionsQueryTestBase<ComplexNavigationsQueryInMemoryFixture>(fixture)
{
    public override Task Final_GroupBy_property_entity_Include_collection(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_collection(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_Include_collection_nested(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_collection_nested(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_Include_collection_reference(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_collection_reference(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_Include_collection_multiple(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_collection_multiple(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_Include_collection_reference_same_level(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_collection_reference_same_level(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_Include_reference(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_reference(async),
            InMemoryStrings.NonComposedGroupByNotSupported);

    public override Task Final_GroupBy_property_entity_Include_reference_multiple(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Final_GroupBy_property_entity_Include_reference_multiple(async),
            InMemoryStrings.NonComposedGroupByNotSupported);
}
