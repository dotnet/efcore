// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class PrimitiveCollectionsQueryRelationalTestBase<TFixture>(TFixture fixture) : PrimitiveCollectionsQueryTestBase<TFixture>(fixture)
    where TFixture : PrimitiveCollectionsQueryTestBase<TFixture>.PrimitiveCollectionsQueryFixtureBase, new()
{
    [ConditionalFact]
    public override async Task Inline_collection_Count_with_zero_values()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Inline_collection_Count_with_zero_values());

        Assert.Equal(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot, exception.Message);
    }

    public override Task Column_collection_Concat_parameter_collection_equality_inline_collection()
        => AssertTranslationFailed(base.Column_collection_Concat_parameter_collection_equality_inline_collection);

    public override Task Column_collection_equality_inline_collection_with_parameters()
        => AssertTranslationFailed(base.Column_collection_equality_inline_collection_with_parameters);

    [ConditionalFact]
    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        // The array indexing is translated as a subquery over e.g. OPENJSON with LIMIT/OFFSET.
        // Since there's a CAST over that, the type mapping inference from the other side (p.String) doesn't propagate inside to the
        // subquery. In this case, the CAST operand gets the default CLR type mapping, but that's object in this case.
        // We should apply the default type mapping to the parameter, but need to figure out the exact rules when to do this.
        var exception =
            Assert.Throws<InvalidOperationException>(() => base.Parameter_collection_in_subquery_and_Convert_as_compiled_query());

        Assert.Contains("in the SQL tree does not have a type mapping assigned", exception.Message);
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query)).Message;

        Assert.Equal(RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping("Union"), message);
    }

    public override async Task Project_inline_collection_with_Concat()
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(base.Project_inline_collection_with_Concat)).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    // TODO: Requires converting the results of a subquery (relational rowset) to a primitive collection for comparison,
    // not yet supported (#33792)
    public override async Task Column_collection_Where_equality_inline_collection()
        => await AssertTranslationFailed(base.Column_collection_Where_equality_inline_collection);
}
