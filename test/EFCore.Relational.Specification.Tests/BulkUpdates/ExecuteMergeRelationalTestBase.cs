// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ExecuteMergeModel;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#pragma warning disable EF9107 // ExecuteMerge is experimental

public abstract class ExecuteMergeRelationalTestBase<TFixture>(TFixture fixture) : ExecuteMergeTestBase<TFixture>(fixture)
    where TFixture : ExecuteMergeRelationalFixtureBase, new()
{
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Default_insert_skips_computed_columns(bool async)
        => AssertMerge(
            async context =>
            {
                context.Gadgets.Add(new Gadget { Id = 1, Price = 100 });
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();

                // The computed Tax column must be left out of the default insert, since it can't be written to.
                var affected = await Merge(
                    async, context.Gadgets,
                    [new Gadget { Id = 1, Price = 200 }, new Gadget { Id = 2, Price = 500 }],
                    merge => merge.WhenMatched(u => u.SetProperty(t => t.Price, (t, s) => s.Price)));

                Assert.Equal(2, affected);
                var gadgets = await context.Gadgets.OrderBy(g => g.Id).ToListAsync();
                Assert.Equal([200, 500], gadgets.Select(g => g.Price));
                Assert.Equal([20, 50], gadgets.Select(g => g.Tax));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_with_computed_value_throws(bool async)
    {
        var prefix = "INS-";

        return AssertMergeThrows(
            context => Merge(
                async, context.Blogs,
                [new Blog { Id = 3, Name = "Blog3" }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.Id)
                    .SetProperty(t => t.Name, s => prefix + s.Name))),
            RelationalStrings.ExecuteMergeUnsupportedExpression("s => @prefix + s.Name"));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Match_with_computed_source_key_throws(bool async)
    {
        var offset = 100;

        return AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 2, ExternalId = 2, Balance = 10 }],
                merge => merge.Match(t => t.Id, s => s.ExternalId + offset)),
            RelationalStrings.ExecuteMergeUnsupportedExpression("s => s.ExternalId + @offset"));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_unsupported_expression_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Blogs,
                [new Blog { Id = 1, Name = "x" }],
                merge => merge.WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => t.Name.ToUpper()))),
            RelationalStrings.ExecuteMergeUnsupportedExpression("t.Name.ToUpper()"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_setters_omit_match_column_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 999, ExternalId = 777, Balance = 50 }],
                merge => merge
                    .Match(t => t.Id, s => s.ExternalId)
                    .WhenNotMatched(insert => insert
                        .SetProperty(t => t.ExternalId, s => s.ExternalId)
                        .SetProperty(t => t.Balance, s => s.Balance))),
            RelationalStrings.ExecuteMergeMatchColumnNotInserted(nameof(Account.Id)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_source_member_not_inserted_unchanged_throws(bool async)
        => AssertMergeThrows(
            // WhenMatched can only read source values through the inserted row, and s.Name isn't inserted.
            context => Merge(
                async, context.Blogs,
                [new Blog { Id = 1, Name = "x" }],
                merge => merge
                    .WhenMatched(u => u.SetProperty(t => t.Name, (t, s) => s.Name))
                    .WhenNotMatched(insert => insert
                        .SetProperty(t => t.Id, s => s.Id)
                        .SetProperty(t => t.Name, "INS"))),
            RelationalStrings.ExecuteMergeSourceMemberNotInserted("s.Name"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_nested_member_with_same_name_as_entity_property_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 1, ExternalId = 10, Balance = 50 }],
                merge => merge.WhenMatched(u => u.SetProperty(t => t.Details.Balance, (t, s) => s.Balance))),
            RelationalStrings.ExecuteMergeInvalidPropertySelector("t.Details.Balance"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_nested_member_with_same_name_as_entity_property_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 2, ExternalId = 20, Balance = 50 }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.Id)
                    .SetProperty(t => t.Details.Balance, s => s.Balance))),
            RelationalStrings.ExecuteMergeInvalidPropertySelector("t.Details.Balance"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Duplicate_match_column_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 1, ExternalId = 1, Balance = 10 }],
                merge => merge
                    .Match(t => t.Id, s => s.Id)
                    .Match(t => t.Id, s => s.ExternalId)),
            RelationalStrings.ExecuteMergeDuplicateMatchColumn(nameof(Account.Id)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Returning_selector_without_target_columns_throws(bool async)
        => AssertMergeThrows(
            context => MergeReturning(async, context.Blogs, [new Blog { Id = 1, Name = "x" }], merge => { }, t => 1),
            RelationalStrings.ExecuteMergeReturningUnsupportedSelector("t => 1"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task On_filtered_target_throws(bool async)
        => AssertMergeThrows(
            context => Merge(async, context.Blogs.Where(b => b.Id > 0), [new Blog { Id = 1, Name = "x" }], merge => { }),
            RelationalStrings.ExecuteMergeOnComplexQuery);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task On_target_with_take_throws(bool async)
        => AssertMergeThrows(
            // The generated merge always targets the whole table, so Take() must be rejected rather than silently ignored.
            context => Merge(async, context.Blogs.OrderBy(b => b.Id).Take(1), [new Blog { Id = 1, Name = "x" }], merge => { }),
            RelationalStrings.ExecuteMergeOnComplexQuery);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Without_primary_key_and_no_match_throws(bool async)
        => AssertMergeThrows(
            context => Merge(async, context.Keyless, [new Keyless { Value = 1 }], merge => { }),
            RelationalStrings.ExecuteMergeNoPrimaryKey(nameof(Keyless)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Property_not_found_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Blogs,
                [new Blog { Id = 1, Name = "x" }],
                merge => merge.WhenMatched(u => u.SetProperty(t => EF.Property<int>(t, "Nonexistent"), (t, s) => 0))),
            RelationalStrings.ExecuteMergePropertyNotFound("Nonexistent", nameof(Blog)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Default_insert_missing_source_member_throws(bool async)
        => AssertMergeThrows(
            // The DTO has no Name member, so the default insert can't populate the Name column.
            context => Merge(async, context.Blogs, new BlogWithoutName[] { new() { Id = 3, Note = "x" } }, merge => { }),
            RelationalStrings.ExecuteMergePropertyNotFound(nameof(Blog.Name), nameof(BlogWithoutName)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task On_entity_mapped_to_table_and_view_targets_table(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<WidgetContext>(
            seed: async context =>
            {
                var helper = context.GetService<ISqlGenerationHelper>();
                await context.Database.ExecuteSqlRawAsync(
                    $"CREATE VIEW {helper.DelimitIdentifier("WidgetsView")} AS SELECT * FROM {helper.DelimitIdentifier("Widgets")}");
            });
        await using var context = contextFactory.CreateDbContext();

        // Queries on Widget go through the view, but the merge must write to the table.
        var affected = await Merge(async, context.Set<Widget>(), [new Widget { Id = 1, Name = "a" }], merge => { });

        Assert.Equal(1, affected);
        Assert.Equal("a", (await context.Set<Widget>().SingleAsync()).Name);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task On_entity_with_owned_type_throws(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<OrderContext>();
        await using var context = contextFactory.CreateDbContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Merge(async, context.Set<Order>(), [new Order { Id = 1, ShipTo = new OrderAddress { City = "x" } }], merge => { }));

        Assert.Equal(RelationalStrings.ExecuteMergeOnNonEntityType, exception.InnerException!.Message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task On_entity_with_complex_property_throws(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<ParcelContext>();
        await using var context = contextFactory.CreateDbContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Merge(async, context.Set<Parcel>(), [new Parcel { Id = 1, Size = new ParcelSize { Weight = 5 } }], merge => { }));

        Assert.Equal(RelationalStrings.ExecuteMergeOnComplexProperties(nameof(Parcel)), exception.InnerException!.Message);
    }

    protected Task AssertMergeThrows(Func<ExecuteMergeContext, Task> merge, string expectedMessage)
        => AssertMerge(
            async context =>
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => merge(context));
                Assert.Equal(expectedMessage, exception.InnerException!.Message);
            });

    protected class WidgetContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Widget>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ToTable("Widgets").ToView("WidgetsView");
                });
    }

    protected class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    protected class OrderContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.OwnsOne(e => e.ShipTo);
                });
    }

    protected class Order
    {
        public int Id { get; set; }
        public OrderAddress ShipTo { get; set; } = null!;
    }

    protected class OrderAddress
    {
        public string City { get; set; } = null!;
    }

    protected class ParcelContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Parcel>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.ComplexProperty(e => e.Size);
                });
    }

    protected class Parcel
    {
        public int Id { get; set; }
        public ParcelSize Size { get; set; } = null!;
    }

    protected class ParcelSize
    {
        public int Weight { get; set; }
    }
}
