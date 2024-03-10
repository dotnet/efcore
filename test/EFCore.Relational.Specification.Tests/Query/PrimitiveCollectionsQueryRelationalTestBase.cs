// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class PrimitiveCollectionsQueryRelationalTestBase<TFixture>(TFixture fixture) : PrimitiveCollectionsQueryTestBase<TFixture>(fixture)
    where TFixture : PrimitiveCollectionsQueryTestBase<TFixture>.PrimitiveCollectionsQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Inline_collection_Count_with_zero_values(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Inline_collection_Count_with_zero_values(async));

        Assert.Equal(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot, exception.Message);
    }

    public override Task Column_collection_Concat_parameter_collection_equality_inline_collection(bool async)
        => AssertTranslationFailed(() => base.Column_collection_Concat_parameter_collection_equality_inline_collection(async));

    public override Task Column_collection_equality_inline_collection_with_parameters(bool async)
        => AssertTranslationFailed(() => base.Column_collection_equality_inline_collection_with_parameters(async));

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

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(async))).Message;

        Assert.Equal(RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping("Union"), message);
    }
}
