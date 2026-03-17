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
            RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteDelete", "RootEntity.RequiredAssociate#AssociateType"),
            () => AssertDelete(
                ss => ss.Set<RootEntity>().Select(c => c.RequiredAssociate),
                rowsAffectedCount: 0));

    [ConditionalFact]
    public virtual Task Update_property_inside_association()
        => AssertTranslationFailedWithDetails(
            RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteUpdate", "RootEntity.RequiredAssociate#AssociateType"),
            () => AssertUpdate(
                ss => ss.Set<RootEntity>(),
                e => e,
                s => s.SetProperty(c => c.RequiredAssociate.String, "foo_updated"),
                rowsAffectedCount: 0));

    [ConditionalFact]
    public virtual async Task Update_association()
    {
        var newNested = new NestedAssociateType
        {
            Id = 1000,
            Name = "Updated nested name",
            Int = 80,
            String = "Updated nested string",
            Ints = [1, 2, 4]
        };

        await AssertTranslationFailedWithDetails(
            RelationalStrings.InvalidPropertyInSetProperty(
                """r => EF.Property<NestedAssociateType>(EF.Property<AssociateType>(r, "RequiredAssociate"), "RequiredNestedAssociate")"""),
            () => AssertUpdate(
                ss => ss.Set<RootEntity>(),
                c => c,
                s => s.SetProperty(x => x.RequiredAssociate.RequiredNestedAssociate, newNested),
                rowsAffectedCount: 0));
    }

    protected static async Task AssertTranslationFailedWithDetails(string details, Func<Task> query)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(query);
        Assert.StartsWith(CoreStrings.NonQueryTranslationFailed("")[0..^1], exception.Message);
        var innerException = Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal(details, innerException.Message);
    }
}

