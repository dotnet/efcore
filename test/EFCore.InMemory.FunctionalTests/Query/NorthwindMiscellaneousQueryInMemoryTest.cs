// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Abstractions;

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
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_one_element_Single(async));

    public override Task Where_query_composition_entity_equality_one_element_First(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_one_element_First(async));

    public override Task Where_query_composition_entity_equality_no_elements_Single(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_no_elements_Single(async));

    public override Task Where_query_composition_entity_equality_no_elements_First(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_no_elements_First(async));

    public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async));

    public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Where_query_composition_entity_equality_multiple_elements_Single(async));

    // Sending client code to server
    [ConditionalFact(Skip = "Issue#17050")]
    public override void Client_code_using_instance_in_anonymous_type()
        => base.Client_code_using_instance_in_anonymous_type();

    [ConditionalTheory(Skip = "Issue#17050")]
    public override Task Client_code_unknown_method(bool async)
        => base.Client_code_unknown_method(async);

    [ConditionalFact(Skip = "Issue#17050")]
    public override void Client_code_using_instance_in_static_method()
        => base.Client_code_using_instance_in_static_method();

    [ConditionalFact(Skip = "Issue#17050")]
    public override void Client_code_using_instance_method_throws()
        => base.Client_code_using_instance_method_throws();

    public override async Task Max_on_empty_sequence_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Set<Customer>().Select(e => new { Max = e.Orders.Max(o => o.OrderID) });

        var message = async
            ? (await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToListAsync())).Message
            : Assert.Throws<InvalidOperationException>(() => query.ToList()).Message;

        Assert.Equal("Sequence contains no elements", message);
    }
}
