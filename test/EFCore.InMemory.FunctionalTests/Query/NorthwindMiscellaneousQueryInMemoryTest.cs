// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindMiscellaneousQueryInMemoryTest : NorthwindMiscellaneousQueryTestBase<
    NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindMiscellaneousQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
        ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    public override Task Where_query_composition_entity_equality_one_element_Single(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_one_element_Single(async));

    public override Task Where_query_composition_entity_equality_one_element_First(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_one_element_First(async));

    public override Task Where_query_composition_entity_equality_no_elements_Single(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_no_elements_Single(async));

    public override Task Where_query_composition_entity_equality_no_elements_First(bool async)
        // Sequence contains no elements.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_no_elements_First(async));

    public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
        // Sequence contains more than one element.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async));

    public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
        // Sequence contains more than one element.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_multiple_elements_Single(async));

    public override Task Max_on_empty_sequence_throws(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Max_on_empty_sequence_throws(async));
}
