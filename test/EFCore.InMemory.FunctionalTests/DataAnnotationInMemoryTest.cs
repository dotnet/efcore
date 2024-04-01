// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class DataAnnotationInMemoryTest(DataAnnotationInMemoryTest.DataAnnotationInMemoryFixture fixture)
    : DataAnnotationTestBase<DataAnnotationInMemoryTest.DataAnnotationInMemoryFixture>(fixture)
{
    protected override TestHelpers TestHelpers
        => InMemoryTestHelpers.Instance;

    public override Task ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
    {
        using var context = CreateContext();
        Assert.True(context.Model.FindEntityType(typeof(One)).FindProperty("RowVersion").IsConcurrencyToken);
        return Task.CompletedTask;
    }

    public override Task MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
    {
        using var context = CreateContext();
        Assert.Equal(10, context.Model.FindEntityType(typeof(One)).FindProperty("MaxLengthProperty").GetMaxLength());
        return Task.CompletedTask;
    }

    public override Task RequiredAttribute_for_navigation_throws_while_inserting_null_value()
    {
        using var context = CreateContext();
        Assert.True(
            context.Model.FindEntityType(typeof(BookDetails)).FindNavigation(nameof(BookDetails.AnotherBook)).ForeignKey.IsRequired);
        return Task.CompletedTask;
    }

    public override Task RequiredAttribute_for_property_throws_while_inserting_null_value()
    {
        using var context = CreateContext();
        Assert.False(context.Model.FindEntityType(typeof(One)).FindProperty("RequiredColumn").IsNullable);
        return Task.CompletedTask;
    }

    public override Task StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
    {
        using var context = CreateContext();
        Assert.Equal(16, context.Model.FindEntityType(typeof(Two)).FindProperty("Data").GetMaxLength());
        return Task.CompletedTask;
    }

    public override Task TimestampAttribute_throws_if_value_in_database_changed()
    {
        using var context = CreateContext();
        Assert.True(context.Model.FindEntityType(typeof(Two)).FindProperty("Timestamp").IsConcurrencyToken);
        return Task.CompletedTask;
    }

    public class DataAnnotationInMemoryFixture : DataAnnotationFixtureBase
    {
        public static readonly string DatabaseName = "DataAnnotations";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}
