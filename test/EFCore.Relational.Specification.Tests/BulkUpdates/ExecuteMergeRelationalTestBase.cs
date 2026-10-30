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
    public virtual Task Match_on_computed_column_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Gadgets,
                [new Gadget { Id = 1, Price = 200, Tax = 10 }],
                merge => merge.Match(t => t.Tax, s => s.Tax)),
            RelationalStrings.ExecuteMergeComputedMatchColumn(nameof(Gadget.Tax)));

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
    public virtual Task When_matched_with_operator_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Visits,
                [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }],
                merge => merge
                    .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                    .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => t.Visits + s.Visits))),
            RelationalStrings.ExecuteMergeUnsupportedExpression("t.Visits + s.Visits"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_constant_of_incompatible_type_is_not_inlined(bool async)
        => AssertMerge(
            async context =>
            {
                // The int mapping would emit the string unquoted, so inlining it would add another assignment to the UPDATE.
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Merge(
                        async, context.Visits,
                        [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }],
                        merge => merge
                            .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                            .WhenMatched(u => u.SetProperty<object>(t => t.Visits, (t, s) => "1, \"Day\" = 'injected'"))));

                Assert.Equal("mon", (await context.Visits.SingleAsync()).Day);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_with_narrowing_conversion_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Visits,
                [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }],
                merge => merge
                    .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                    .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => unchecked((byte)t.Visits)))),
            RelationalStrings.ExecuteMergeUnsupportedExpression("(int)(byte)t.Visits"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_with_checked_conversion_throws(bool async)
        => AssertMerge(
            async context =>
            {
                // The printed ConvertChecked node isn't stable enough to compare, so only check that translation rejects it.
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Merge(
                        async, context.Visits,
                        [new DailyVisit { UserId = 1, Day = "mon", Visits = 3 }],
                        merge => merge
                            .Match(t => new { t.UserId, t.Day }, s => new { s.UserId, s.Day })
                            .WhenMatched(u => u.SetProperty(t => t.Visits, (t, s) => checked((byte)s.Visits)))));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_source_member_inserted_with_other_converter_throws(bool async)
        => AssertMergeThrows(
            // s.Value is only inserted through ConvertedValue's value converter, so it can't be read back as-is for Value.
            context => Merge(
                async, context.Measurements,
                [new Measurement { Id = 1, Value = 5 }],
                merge => merge
                    .WhenMatched(u => u.SetProperty(t => t.Value, (t, s) => s.Value))
                    .WhenNotMatched(insert => insert
                        .SetProperty(t => t.Id, s => s.Id)
                        .SetProperty(t => t.ConvertedValue, s => s.Value))),
            RelationalStrings.ExecuteMergeSourceMemberConverterMismatch("s.Value", nameof(Measurement.Value)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_target_property_with_other_converter_throws(bool async)
        => AssertMergeThrows(
            // Value is stored as-is, while ConvertedValue goes through a value converter, so copying the raw column would corrupt it.
            context => Merge(
                async, context.Measurements,
                [new Measurement { Id = 1, Value = 5 }],
                merge => merge.WhenMatched(u => u.SetProperty(t => t.ConvertedValue, (t, s) => t.Value))),
            RelationalStrings.ExecuteMergeTargetPropertyConverterMismatch(nameof(Measurement.Value), nameof(Measurement.ConvertedValue)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_with_narrowing_conversion_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Measurements,
                [new Measurement { Id = 2, Value = 300 }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.Id)
                    .SetProperty(t => t.Value, s => unchecked((byte)s.Value)))),
            RelationalStrings.ExecuteMergeUnsupportedExpression("(int)(byte)s.Value"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Match_with_narrowing_source_conversion_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 1, ExternalId = 300, Balance = 10 }],
                merge => merge.Match(t => t.Id, s => unchecked((byte)s.ExternalId))),
            RelationalStrings.ExecuteMergeUnsupportedExpression("(int)(byte)s.ExternalId"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Match_with_narrowing_target_conversion_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 1, ExternalId = 300, Balance = 10 }],
                merge => merge.Match(t => unchecked((byte)t.ExternalId), s => unchecked((byte)s.ExternalId))),
            RelationalStrings.ExecuteMergeUnsupportedExpression("(byte)t.ExternalId"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_without_setters_throws(bool async)
        => AssertMergeThrows(
            context => Merge(async, context.Blogs, [new Blog { Id = 3, Name = "Blog3" }], merge => merge.WhenNotMatched(_ => { })),
            RelationalStrings.ExecuteMergeEmptyWhenNotMatched);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_duplicate_property_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Blogs,
                [new Blog { Id = 3, Name = "Blog3" }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.Id)
                    .SetProperty(t => t.Name, s => s.Name)
                    .SetProperty(t => t.Name, "Other"))),
            RelationalStrings.ExecuteMergeDuplicateInsertProperty(nameof(Blog.Name)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_matched_duplicate_property_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Blogs,
                [new Blog { Id = 1, Name = "x" }],
                merge => merge.WhenMatched(u => u
                    .SetProperty(t => t.Name, (t, s) => s.Name)
                    .SetProperty(t => t.Name, "Other"))),
            RelationalStrings.ExecuteMergeDuplicateUpdateProperty(nameof(Blog.Name)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_computed_property_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Gadgets,
                [new Gadget { Id = 1, Price = 100, Tax = 10 }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.Id)
                    .SetProperty(t => t.Price, s => s.Price)
                    .SetProperty(t => t.Tax, s => s.Tax))),
            RelationalStrings.ExecuteMergeGeneratedInsertProperty(nameof(Gadget.Tax)));

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
    public virtual Task When_not_matched_match_column_from_other_source_member_throws(bool async)
        => AssertMergeThrows(
            // ON CONFLICT compares the inserted Id, so inserting s.Id would silently match on s.Id rather than s.ExternalId.
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 999, ExternalId = 1, Balance = 50 }],
                merge => merge
                    .Match(t => t.Id, s => s.ExternalId)
                    .WhenNotMatched(insert => insert
                        .SetProperty(t => t.Id, s => s.Id)
                        .SetProperty(t => t.Balance, s => s.Balance))),
            RelationalStrings.ExecuteMergeMatchColumnInsertMismatch(nameof(Account.Id), nameof(Account.ExternalId)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_primary_key_from_other_source_member_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 999, ExternalId = 1, Balance = 50 }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.ExternalId)
                    .SetProperty(t => t.Balance, s => s.Balance))),
            RelationalStrings.ExecuteMergeMatchColumnInsertMismatch(nameof(Account.Id), nameof(Account.Id)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task When_not_matched_match_column_from_constant_throws(bool async)
        => AssertMergeThrows(
            context => Merge(
                async, context.Accounts,
                [new Account { Id = 999, ExternalId = 1, Balance = 50 }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, 5)
                    .SetProperty(t => t.Balance, s => s.Balance))),
            RelationalStrings.ExecuteMergeMatchColumnInsertMismatch(nameof(Account.Id), nameof(Account.Id)));

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
    public virtual Task Returning_selector_with_captured_variable_throws(bool async)
    {
        var prefix = "Blog";

        return AssertMergeThrows(
            context => MergeReturning(
                async, context.Blogs, [new Blog { Id = 3, Name = "Blog3" }], merge => { }, t => new { t.Id, Prefix = prefix }),
            RelationalStrings.ExecuteMergeReturningUnsupportedExpression("@prefix"));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Null_source_throws(bool async)
        => AssertMerge(
            async context =>
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Merge(async, context.Blogs, (IEnumerable<Blog>)null!, merge => { }));

                Assert.Equal(RelationalStrings.ExecuteMergeNullSource, exception.Message);
            });

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
    public virtual Task Default_insert_source_member_of_incompatible_type_is_not_inlined(bool async)
        => AssertMerge(
            async context =>
            {
                // The string Id is mapped as the int Id column, so inlining it as a literal would inject the DELETE.
                await Assert.ThrowsAnyAsync<Exception>(
                    () => Merge(
                        async, context.Blogs,
                        new BlogRowWithStringId[] { new() { Id = "3, 'x', NULL); DELETE FROM Blogs; --", Name = "x" } },
                        merge => { }));

                Assert.Equal(2, await context.Blogs.CountAsync());
            });

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
    public virtual async Task Default_insert_skips_properties_generated_on_add_or_update(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<StampedContext>();
        await using var context = contextFactory.CreateDbContext();

        // Like SaveChanges, the default insert must leave Version to the database.
        var affected = await Merge(async, context.Set<Stamped>(), [new Stamped { Id = 1, Name = "a", Version = 7 }], merge => { });

        Assert.Equal(1, affected);
        Assert.Equal(42, (await context.Set<Stamped>().SingleAsync()).Version);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task When_not_matched_property_generated_on_add_or_update_throws(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<StampedContext>();
        await using var context = contextFactory.CreateDbContext();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Merge(
                async, context.Set<Stamped>(),
                [new Stamped { Id = 1, Name = "a", Version = 7 }],
                merge => merge.WhenNotMatched(insert => insert
                    .SetProperty(t => t.Id, s => s.Id)
                    .SetProperty(t => t.Version, s => s.Version))));

        Assert.Equal(RelationalStrings.ExecuteMergeGeneratedInsertProperty(nameof(Stamped.Version)), exception.InnerException!.Message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Commands_report_ExecuteMerge_command_source(bool async)
    {
        var interceptor = new CommandSourceInterceptor();
        var contextFactory = await InitializeNonSharedTest<StampedContext>(onConfiguring: o => o.AddInterceptors(interceptor));
        await using var context = contextFactory.CreateDbContext();
        interceptor.CommandSources.Clear();

        await Merge(async, context.Set<Stamped>(), [new Stamped { Id = 1, Name = "a" }], merge => { });
        await MergeReturning(async, context.Set<Stamped>(), [new Stamped { Id = 2, Name = "b" }], merge => { }, t => t.Id);

        Assert.Equal([CommandSource.ExecuteMerge, CommandSource.ExecuteMerge], interceptor.CommandSources);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Returning_with_nullable_provider_type_converter(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<RatedContext>();
        await using var context = contextFactory.CreateDbContext();

        var unbuffered = await MergeReturning(
            async, context.Set<Rated>(), [new Rated { Id = 1, Score = new Score(5) }], merge => { }, t => t.Score);
        var buffered = await new BufferingExecutionStrategy(context).ExecuteAsync(
            () => MergeReturning(async, context.Set<Rated>(), [new Rated { Id = 2, Score = new Score(7) }], merge => { }, t => t.Score));

        Assert.Equal(5, Assert.Single(unbuffered).Points);
        Assert.Equal(7, Assert.Single(buffered).Points);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Returning_with_converter_that_converts_nulls(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<LabeledContext>();
        await using var context = contextFactory.CreateDbContext();

        // "<null>" is stored as SQL NULL, and the converter must turn it back into "<null>".
        var unbuffered = await MergeReturning(
            async, context.Set<Labeled>(), [new Labeled { Id = 1, Label = "<null>" }], merge => { }, t => t.Label);
        var buffered = await new BufferingExecutionStrategy(context).ExecuteAsync(
            () => MergeReturning(async, context.Set<Labeled>(), [new Labeled { Id = 2, Label = "<null>" }], merge => { }, t => t.Label));

        Assert.Equal("<null>", Assert.Single(unbuffered));
        Assert.Equal("<null>", Assert.Single(buffered));
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

    protected class BlogRowWithStringId
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Note { get; set; }
    }

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

    protected class CommandSourceInterceptor : DbCommandInterceptor
    {
        public List<CommandSource> CommandSources { get; } = [];

        public override DbCommand CommandInitialized(CommandEndEventData eventData, DbCommand result)
        {
            CommandSources.Add(eventData.CommandSource);
            return result;
        }
    }

    protected class StampedContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Stamped>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Version).HasDefaultValue(42).ValueGeneratedOnAddOrUpdate();
                });
    }

    protected class Stamped
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Version { get; set; }
    }

    protected class RatedContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Rated>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Score).HasConversion(new ValueConverter<Score, int?>(s => s.Points, v => new Score(v!.Value)));
                });
    }

    protected class Rated
    {
        public int Id { get; set; }
        public Score Score { get; set; } = null!;
    }

    protected class Score(int points)
    {
        public int Points { get; } = points;
    }

    protected class LabeledContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Labeled>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.Label).HasConversion(
                        new ValueConverter<string?, string?>(
                            v => v == "<null>" ? null : v,
                            v => v ?? "<null>",
                            convertsNulls: true));
                });
    }

    protected class Labeled
    {
        public int Id { get; set; }
        public string? Label { get; set; }
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
