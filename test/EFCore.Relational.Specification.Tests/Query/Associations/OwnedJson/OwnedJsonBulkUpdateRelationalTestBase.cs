// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public abstract class OwnedJsonBulkUpdateRelationalTestBase<TFixture> : BulkUpdatesTestBase<TFixture>
    where TFixture : OwnedJsonRelationalFixtureBase, new()
{
    public OwnedJsonBulkUpdateRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Bulk update is not supported with owned JSON.
    // We just have a couple of tests here to verify that the correct exceptions are thrown, and don't extend
    // the actual AssociationsBulkUpdateTestBase with all the different tests.

    [ConditionalFact]
    public virtual Task Delete_association()
        => AssertTranslationFailedWithDetails(
            RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteDelete", "RootEntity.RequiredRelated#RelatedType"),
            () => AssertDelete(
                ss => ss.Set<RootEntity>().Select(c => c.RequiredRelated),
                rowsAffectedCount: 0));

    [ConditionalFact]
    public virtual Task Update_property_inside_association()
        => AssertTranslationFailedWithDetails(
            RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteUpdate", "RootEntity.RequiredRelated#RelatedType"),
            () => AssertUpdate(
                ss => ss.Set<RootEntity>(),
                e => e,
                s => s.SetProperty(c => c.RequiredRelated.String, "foo_updated"),
                rowsAffectedCount: 0));

    [ConditionalFact]
    public virtual async Task Update_association()
    {
        var newNested = new NestedType
        {
            Name = "Updated nested name",
            Int = 80,
            String = "Updated nested string"
        };

        await AssertTranslationFailedWithDetails(
            RelationalStrings.InvalidPropertyInSetProperty("x => x.RequiredRelated.RequiredNested"),
            // RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteUpdate", "RootEntity.RequiredRelated#RelatedType"),
            () => AssertUpdate(
                ss => ss.Set<RootEntity>(),
                c => c,
                s => s.SetProperty(x => x.RequiredRelated.RequiredNested, newNested),
                rowsAffectedCount: 0));
    }

    protected static async Task AssertTranslationFailedWithDetails(string details, Func<Task> query)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(query);
        Assert.Contains(CoreStrings.NonQueryTranslationFailedWithDetails("", details)[21..], exception.Message);
    }
}

