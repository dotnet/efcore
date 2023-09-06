// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class ComplexTypesTrackingTestBase<TFixture>(TFixture fixture) : IClassFixture<TFixture>
    where TFixture : ComplexTypesTrackingTestBase<TFixture>.FixtureBase
{
    protected TFixture Fixture { get; } = fixture;

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_objects(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePub());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_type_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePub());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_types(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePub());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_types(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePub());

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_structs(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePubWithStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_structs(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_structs(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithStructs());

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_readonly_structs(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePubWithReadonlyStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_readonly_struct_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithReadonlyStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_readonly_structs(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithReadonlyStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_readonly_structs(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithReadonlyStructs());

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_record_objects(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePubWithRecords());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_type_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithRecords());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_record_complex_types(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithRecords());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_record_complex_types(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithRecords());

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_objects_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldPub());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_type_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPub());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPub());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPub());

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_structs_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldPubWithStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPubWithStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPubWithStructs());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPubWithStructs());

    [ConditionalTheory(Skip = "Constructor binding")]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_readonly_structs_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldPubWithReadonlyStructs());

    [ConditionalTheory(Skip = "Constructor binding")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPubWithReadonlyStructs());

    [ConditionalTheory(Skip = "Constructor binding")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPubWithReadonlyStructs());

    [ConditionalTheory(Skip = "Constructor binding")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPubWithReadonlyStructs());

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_record_objects_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldPubWithRecords());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_type_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPubWithRecords());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPubWithRecords());

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPubWithRecords());

    private async Task TrackAndSaveTest<TEntity>(EntityState state, bool async, TEntity pub)
        where TEntity : class
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entry = state switch
                {
                    EntityState.Unchanged => context.Attach(pub),
                    EntityState.Deleted => context.Remove(pub),
                    EntityState.Modified => context.Update(pub),
                    EntityState.Added => async ? await context.AddAsync(pub) : context.Add(pub),
                    _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
                };

                Assert.Equal(state, entry.State);
                AssertPropertyValues(entry);
                AssertPropertiesModified(entry, state == EntityState.Modified);

                if (state == EntityState.Added || state == EntityState.Unchanged)
                {
                    _ = async ? await context.SaveChangesAsync() : context.SaveChanges();

                    Assert.Equal(EntityState.Unchanged, entry.State);
                    AssertPropertyValues(entry);
                }
            });

    private void MarkModifiedTest<TEntity>(bool trackFromQuery, TEntity pub)
        where TEntity : class
    {
        using var context = CreateContext();

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        var membersEntry = entry.ComplexProperty("LunchtimeActivity").ComplexProperty("RunnersUp").Property("Members");
        membersEntry.IsModified = true;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);

        var dayEntry = entry.ComplexProperty("LunchtimeActivity").Property("Day");
        dayEntry.IsModified = true;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);

        var coverChargeEntry = entry.ComplexProperty("EveningActivity").Property("CoverCharge");
        coverChargeEntry.IsModified = true;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);

        var lunchtimeEntry = entry.ComplexProperty("LunchtimeActivity");
        var lunchtimeChampionsEntry = lunchtimeEntry.ComplexProperty("Champions");
        var lunchtimeRunnersUpEntry = lunchtimeEntry.ComplexProperty("RunnersUp");
        var eveningEntry = entry.ComplexProperty("EveningActivity");
        var eveningChampionsEntry = eveningEntry.ComplexProperty("Champions");
        var eveningRunnersUpEntry = eveningEntry.ComplexProperty("RunnersUp");
        var teamEntry = entry.ComplexProperty("FeaturedTeam");

        Assert.False(lunchtimeEntry.Property("Name").IsModified);
        Assert.False(lunchtimeEntry.Property("Description").IsModified);
        Assert.True(lunchtimeEntry.Property("Day").IsModified);
        Assert.False(lunchtimeEntry.Property("Notes").IsModified);
        Assert.False(lunchtimeEntry.Property("CoverCharge").IsModified);
        Assert.False(lunchtimeEntry.Property("IsTeamBased").IsModified);
        Assert.False(lunchtimeChampionsEntry.Property("Name").IsModified);
        Assert.False(lunchtimeChampionsEntry.Property("Members").IsModified);
        Assert.False(lunchtimeRunnersUpEntry.Property("Name").IsModified);
        Assert.True(lunchtimeRunnersUpEntry.Property("Members").IsModified);

        Assert.False(eveningEntry.Property("Name").IsModified);
        Assert.False(eveningEntry.Property("Description").IsModified);
        Assert.False(eveningEntry.Property("Day").IsModified);
        Assert.False(eveningEntry.Property("Notes").IsModified);
        Assert.True(eveningEntry.Property("CoverCharge").IsModified);
        Assert.False(eveningEntry.Property("IsTeamBased").IsModified);
        Assert.False(eveningChampionsEntry.Property("Name").IsModified);
        Assert.False(eveningChampionsEntry.Property("Members").IsModified);
        Assert.False(eveningRunnersUpEntry.Property("Name").IsModified);
        Assert.False(eveningRunnersUpEntry.Property("Members").IsModified);

        Assert.False(teamEntry.Property("Name").IsModified);
        Assert.False(teamEntry.Property("Members").IsModified);

        membersEntry.IsModified = false;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.False(membersEntry.IsModified);

        dayEntry.IsModified = false;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.False(dayEntry.IsModified);

        coverChargeEntry.IsModified = false;
        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(coverChargeEntry.IsModified);

        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);
    }

    private void ReadOriginalValuesTest<TEntity>(bool trackFromQuery, TEntity pub)
        where TEntity : class
    {
        using var context = CreateContext();

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        var membersEntry = entry.ComplexProperty("LunchtimeActivity").ComplexProperty("Champions").Property("Members");
        membersEntry.CurrentValue = new List<string> { "1", "2", "3" };
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);
        Assert.Equal(new[] { "Boris", "David", "Theresa" }, membersEntry.OriginalValue);

        var dayEntry = entry.ComplexProperty("LunchtimeActivity").Property("Day");
        dayEntry.CurrentValue = DayOfWeek.Wednesday;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);
        Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);

        var coverChargeEntry = entry.ComplexProperty("EveningActivity").Property("CoverCharge");
        coverChargeEntry.CurrentValue = 3.0m;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);
        Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
    }

    private void WriteOriginalValuesTest<TEntity>(bool trackFromQuery, TEntity pub)
        where TEntity : class
    {
        using var context = CreateContext();
        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        var membersEntry = entry.ComplexProperty("EveningActivity").ComplexProperty("Champions").Property("Members");
        membersEntry.OriginalValue = new List<string> { "1", "2", "3" };
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.CurrentValue);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.OriginalValue);

        var dayEntry = entry.ComplexProperty("LunchtimeActivity").Property("Day");
        dayEntry.OriginalValue = DayOfWeek.Wednesday;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Monday, dayEntry.CurrentValue);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.OriginalValue);

        var coverChargeEntry = entry.ComplexProperty("EveningActivity").Property("CoverCharge");
        coverChargeEntry.OriginalValue = 3.0m;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(5.0m, coverChargeEntry.CurrentValue);
        Assert.Equal(3.0m, coverChargeEntry.OriginalValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detect_changes_in_complex_type_properties(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePub();

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        pub.EveningActivity.Champions.Members = new List<string>
        {
            "1",
            "2",
            "3"
        };
        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);

        pub.LunchtimeActivity.Day = DayOfWeek.Wednesday;
        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);
        Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);

        pub.EveningActivity.CoverCharge = 3.0m;
        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);
        Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Throws_only_when_saving_with_null_top_level_complex_property(bool async)
    {
        using var context = CreateContext();

        var yogurt = CreateYogurt(nullMilk: true);
        var entry = async ? await context.AddAsync(yogurt) : context.Add(yogurt);
        entry.State = EntityState.Unchanged;
        context.ChangeTracker.DetectChanges();
        entry.State = EntityState.Modified;

        Assert.Equal(
            CoreStrings.NullRequiredComplexProperty("Yogurt", "Milk"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => async ? context.SaveChangesAsync() : Task.FromResult(context.SaveChanges()))).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Throws_only_when_saving_with_null_second_level_complex_property(bool async)
    {
        using var context = CreateContext();

        var yogurt = CreateYogurt(nullManufacturer: true);
        var entry = async ? await context.AddAsync(yogurt) : context.Add(yogurt);
        entry.State = EntityState.Unchanged;
        context.ChangeTracker.DetectChanges();
        entry.State = EntityState.Modified;

        Assert.Equal(
            CoreStrings.NullRequiredComplexProperty("Culture", "Manufacturer"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => async ? context.SaveChangesAsync() : Task.FromResult(context.SaveChanges()))).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Throws_only_when_saving_with_null_third_level_complex_property(bool async)
    {
        using var context = CreateContext();

        var yogurt = CreateYogurt(nullTag: true);
        var entry = async ? await context.AddAsync(yogurt) : context.Add(yogurt);
        entry.State = EntityState.Unchanged;
        context.ChangeTracker.DetectChanges();
        entry.State = EntityState.Modified;

        Assert.Equal(
            CoreStrings.NullRequiredComplexProperty("License", "Tag"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => async ? context.SaveChangesAsync() : Task.FromResult(context.SaveChanges()))).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detect_changes_in_complex_struct_type_properties(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithStructs();

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        var eveningActivity = pub.EveningActivity;
        var champions = eveningActivity.Champions;
        champions.Members = new()
        {
            "1",
            "2",
            "3"
        };
        eveningActivity.Champions = champions;
        pub.EveningActivity = eveningActivity;

        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);

        var lunchtimeActivity = pub.LunchtimeActivity;
        lunchtimeActivity.Day = DayOfWeek.Wednesday;
        pub.LunchtimeActivity = lunchtimeActivity;

        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);
        Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);

        eveningActivity = pub.EveningActivity;
        eveningActivity.CoverCharge = 3.0m;
        pub.EveningActivity = eveningActivity;

        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);
        Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detects_changes_in_complex_readonly_struct_type_properties(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithReadonlyStructs();

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        pub.EveningActivity = new()
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = Array.Empty<string>(),
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Champions = new()
            {
                Name = "Dazed and Confused",
                Members = new()
                {
                    "1",
                    "2",
                    "3"
                }
            },
            RunnersUp = new() { Name = "Banksy", Members = new() }
        };

        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);

        pub.LunchtimeActivity = new()
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Wednesday,
            Description = "A general knowledge pub quiz.",
            Notes = new[] { "One", "Two", "Three" },
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Champions = new()
            {
                Name = "Clueless",
                Members = new()
                {
                    "Boris",
                    "David",
                    "Theresa"
                }
            },
            RunnersUp = new()
            {
                Name = "ZZ",
                Members = new()
                {
                    "Has Beard",
                    "Has Beard",
                    "Is Called Beard"
                }
            },
        };

        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);
        Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);

        pub.EveningActivity = new()
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = Array.Empty<string>(),
            CoverCharge = 3.0m,
            IsTeamBased = true,
            Champions = new()
            {
                Name = "Dazed and Confused",
                Members = new()
                {
                    "1",
                    "2",
                    "3"
                }
            },
            RunnersUp = new() { Name = "Banksy", Members = new() }
        };

        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);
        Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detects_changes_in_complex_record_type_properties(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithRecords();

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        pub.EveningActivity = pub.EveningActivity with
        {
            Champions = pub.EveningActivity.Champions with
            {
                Members = new()
                {
                    "1",
                    "2",
                    "3"
                }
            }
        };

        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);

        pub.LunchtimeActivity = pub.LunchtimeActivity with { Day = DayOfWeek.Wednesday };
        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);
        Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);

        pub.EveningActivity = pub.EveningActivity with { CoverCharge = 3.0m };
        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);
        Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
    }

    protected void AssertPropertyValues(EntityEntry entry)
    {
        Assert.Equal("The FBI", entry.Property("Name").CurrentValue);

        var lunchtimeEntry = entry.ComplexProperty("LunchtimeActivity");
        Assert.Equal("Pub Quiz", lunchtimeEntry.Property("Name").CurrentValue);
        Assert.Equal(DayOfWeek.Monday, lunchtimeEntry.Property("Day").CurrentValue);
        Assert.Equal("A general knowledge pub quiz.", lunchtimeEntry.Property("Description").CurrentValue);
        Assert.Equal(new[] { "One", "Two", "Three" }, lunchtimeEntry.Property("Notes").CurrentValue);
        Assert.Equal(2.0m, lunchtimeEntry.Property("CoverCharge").CurrentValue);
        Assert.True((bool)lunchtimeEntry.Property("IsTeamBased").CurrentValue!);

        var lunchtimeChampionsEntry = lunchtimeEntry.ComplexProperty("Champions");
        Assert.Equal("Clueless", lunchtimeChampionsEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Boris", "David", "Theresa" }, lunchtimeChampionsEntry.Property("Members").CurrentValue);

        var lunchtimeRunnersUpEntry = lunchtimeEntry.ComplexProperty("RunnersUp");
        Assert.Equal("ZZ", lunchtimeRunnersUpEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Has Beard", "Has Beard", "Is Called Beard" }, lunchtimeRunnersUpEntry.Property("Members").CurrentValue);

        var eveningEntry = entry.ComplexProperty("EveningActivity");
        Assert.Equal("Music Quiz", eveningEntry.Property("Name").CurrentValue);
        Assert.Equal(DayOfWeek.Friday, eveningEntry.Property("Day").CurrentValue);
        Assert.Equal("A music pub quiz.", eveningEntry.Property("Description").CurrentValue);
        Assert.Empty((IEnumerable<string>)eveningEntry.Property("Notes").CurrentValue!);
        Assert.Equal(5.0m, eveningEntry.Property("CoverCharge").CurrentValue);
        Assert.True((bool)eveningEntry.Property("IsTeamBased").CurrentValue!);

        var eveningChampionsEntry = eveningEntry.ComplexProperty("Champions");
        Assert.Equal("Dazed and Confused", eveningChampionsEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, eveningChampionsEntry.Property("Members").CurrentValue);

        var eveningRunnersUpEntry = eveningEntry.ComplexProperty("RunnersUp");
        Assert.Equal("Banksy", eveningRunnersUpEntry.Property("Name").CurrentValue);
        Assert.Empty((IEnumerable<string>)eveningRunnersUpEntry.Property("Members").CurrentValue!);

        var teamEntry = entry.ComplexProperty("FeaturedTeam");
        Assert.Equal("Not In This Lifetime", teamEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Slash", "Axl" }, teamEntry.Property("Members").CurrentValue);
    }

    protected void AssertPropertiesModified(EntityEntry entry, bool expected)
    {
        Assert.Equal("The FBI", entry.Property("Name").CurrentValue);

        var lunchtimeEntry = entry.ComplexProperty("LunchtimeActivity");
        Assert.Equal(expected, lunchtimeEntry.Property("Name").IsModified);
        Assert.Equal(expected, lunchtimeEntry.Property("Day").IsModified);
        Assert.Equal(expected, lunchtimeEntry.Property("Description").IsModified);
        Assert.Equal(expected, lunchtimeEntry.Property("Notes").IsModified);
        Assert.Equal(expected, lunchtimeEntry.Property("CoverCharge").IsModified);
        Assert.Equal(expected, lunchtimeEntry.Property("IsTeamBased").IsModified);

        var lunchtimeChampionsEntry = lunchtimeEntry.ComplexProperty("Champions");
        Assert.Equal(expected, lunchtimeChampionsEntry.Property("Name").IsModified);
        Assert.Equal(expected, lunchtimeChampionsEntry.Property("Members").IsModified);

        var lunchtimeRunnersUpEntry = lunchtimeEntry.ComplexProperty("RunnersUp");
        Assert.Equal(expected, lunchtimeRunnersUpEntry.Property("Name").IsModified);
        Assert.Equal(expected, lunchtimeRunnersUpEntry.Property("Members").IsModified);

        var eveningEntry = entry.ComplexProperty("EveningActivity");
        Assert.Equal(expected, eveningEntry.Property("Name").IsModified);
        Assert.Equal(expected, eveningEntry.Property("Day").IsModified);
        Assert.Equal(expected, eveningEntry.Property("Description").IsModified);
        Assert.Equal(expected, eveningEntry.Property("Notes").IsModified);
        Assert.Equal(expected, eveningEntry.Property("CoverCharge").IsModified);
        Assert.Equal(expected, eveningEntry.Property("IsTeamBased").IsModified);

        var eveningChampionsEntry = eveningEntry.ComplexProperty("Champions");
        Assert.Equal(expected, eveningChampionsEntry.Property("Name").IsModified);
        Assert.Equal(expected, eveningChampionsEntry.Property("Members").IsModified);

        var eveningRunnersUpEntry = eveningEntry.ComplexProperty("RunnersUp");
        Assert.Equal(expected, eveningRunnersUpEntry.Property("Name").IsModified);
        Assert.Equal(expected, eveningRunnersUpEntry.Property("Members").IsModified!);

        var teamEntry = entry.ComplexProperty("FeaturedTeam");
        Assert.Equal(expected, teamEntry.Property("Name").IsModified);
        Assert.Equal(expected, teamEntry.Property("Members").IsModified);
    }

    protected static EntityEntry<TEntity> TrackFromQuery<TEntity>(DbContext context, TEntity pub)
        where TEntity : class
        => new(
            context.GetService<IStateManager>().StartTrackingFromQuery(
                context.Model.FindEntityType(typeof(TEntity))!, pub, new ValueBuffer()));

    protected virtual void ExecuteWithStrategyInTransaction(
        Action<DbContext> testOperation,
        Action<DbContext>? nestedTestOperation1 = null,
        Action<DbContext>? nestedTestOperation2 = null)
        => TestHelpers.ExecuteWithStrategyInTransaction(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2);

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task>? nestedTestOperation1 = null,
        Func<DbContext, Task>? nestedTestOperation2 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public abstract class FixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "ComplexTypesTrackingTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Pub>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e!.Champions);
                            b.ComplexProperty(e => e!.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<PubWithStructs>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e!.Champions);
                            b.ComplexProperty(e => e!.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<PubWithReadonlyStructs>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e!.Champions);
                            b.ComplexProperty(e => e!.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<PubWithRecords>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e!.Champions);
                            b.ComplexProperty(e => e!.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<FieldPub>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e!.Champions);
                            b.ComplexProperty(e => e!.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<FieldPubWithStructs>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e!.Champions);
                            b.ComplexProperty(e => e!.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            // TODO: Allow binding of complex properties to constructors
            // modelBuilder.Entity<FieldPubWithReadonlyStructs>(
            //     b =>
            //     {
            //         b.ComplexProperty(
            //             e => e.LunchtimeActivity, b =>
            //             {
            //                 b.ComplexProperty(e => e!.Champions);
            //                 b.ComplexProperty(e => e!.RunnersUp);
            //             });
            //         b.ComplexProperty(
            //             e => e.EveningActivity, b =>
            //             {
            //                 b.ComplexProperty(e => e.Champions);
            //                 b.ComplexProperty(e => e.RunnersUp);
            //             });
            //         b.ComplexProperty(e => e.FeaturedTeam);
            //         b.ComplexProperty(e => e.FeaturedTeam);
            //     });

            modelBuilder.Entity<FieldPubWithRecords>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e!.Champions);
                            b.ComplexProperty(e => e!.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<Yogurt>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.Culture, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.Milk, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });
                });
        }
    }

    protected static Pub CreatePub(bool nullActivity = false, bool nullChampions = false, bool nullRunnersUp = false)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = nullActivity ? null! : new()
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = new[] { "One", "Two", "Three" },
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = nullChampions ? null! : new()
                {
                    Name = "Clueless",
                    Members =
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = nullRunnersUp ? null! : new()
                {
                    Name = "ZZ",
                    Members =
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                },
            },
            EveningActivity = new()
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = Array.Empty<string>(),
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Dazed and Confused",
                    Members =
                    {
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    }
                },
                RunnersUp = new()
                {
                    Name = "Banksy", Members = new()
                },
            },
            FeaturedTeam = new()
            {
                Name = "Not In This Lifetime",
                Members =
                {
                    "Slash",
                    "Axl"
                }
            }
        };

    protected class Pub
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Activity LunchtimeActivity { get; set; } = null!;
        public Activity EveningActivity { get; set; } = null!;
        public Team FeaturedTeam { get; set; } = null!;
    }

    protected class Activity
    {
        public string Name { get; set; } = null!;
        public decimal? CoverCharge { get; set; }
        public bool IsTeamBased { get; set; }
        public string? Description { get; set; }
        public string[]? Notes { get; set; }
        public DayOfWeek Day { get; set; }
        public Team Champions { get; set; } = null!;
        public Team RunnersUp { get; set; } = null!;
    }

    protected class Team
    {
        public string Name { get; set; } = null!;
        public List<string> Members { get; set; } = new();
    }

    protected static PubWithStructs CreatePubWithStructs()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new()
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = new[] { "One", "Two", "Three" },
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Clueless",
                    Members = new()
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = new()
                {
                    Name = "ZZ",
                    Members = new()
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                },
            },
            EveningActivity = new()
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = Array.Empty<string>(),
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Dazed and Confused",
                    Members = new()
                    {
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    }
                },
                RunnersUp = new() { Name = "Banksy", Members = new() }
            },
            FeaturedTeam = new() { Name = "Not In This Lifetime", Members = new() { "Slash", "Axl" } }
        };

    protected class PubWithStructs
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ActivityStruct LunchtimeActivity { get; set; }
        public ActivityStruct EveningActivity { get; set; }
        public TeamStruct FeaturedTeam { get; set; }
    }

    protected struct ActivityStruct
    {
        public string Name { get; set; }
        public decimal? CoverCharge { get; set; }
        public bool IsTeamBased { get; set; }
        public string? Description { get; set; }
        public string[]? Notes { get; set; }
        public DayOfWeek Day { get; set; }
        public TeamStruct Champions { get; set; }
        public TeamStruct RunnersUp { get; set; }
    }

    protected struct TeamStruct
    {
        public string Name { get; set; }
        public List<string> Members { get; set; }
    }

    protected static PubWithReadonlyStructs CreatePubWithReadonlyStructs()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new()
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = new[] { "One", "Two", "Three" },
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Clueless",
                    Members = new()
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = new()
                {
                    Name = "ZZ",
                    Members = new()
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                },
            },
            EveningActivity = new()
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = Array.Empty<string>(),
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Dazed and Confused",
                    Members = new()
                    {
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    }
                },
                RunnersUp = new() { Name = "Banksy", Members = new() }
            },
            FeaturedTeam = new() { Name = "Not In This Lifetime", Members = new() { "Slash", "Axl" } }
        };

    protected class PubWithReadonlyStructs
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ActivityReadonlyStruct LunchtimeActivity { get; set; }
        public ActivityReadonlyStruct EveningActivity { get; set; }
        public TeamReadonlyStruct FeaturedTeam { get; set; }
    }

    protected readonly struct ActivityReadonlyStruct
    {
        public string Name { get; init; }
        public decimal? CoverCharge { get; init; }
        public bool IsTeamBased { get; init; }
        public string? Description { get; init; }
        public string[]? Notes { get; init; }
        public DayOfWeek Day { get; init; }
        public TeamReadonlyStruct Champions { get; init; }
        public TeamReadonlyStruct RunnersUp { get; init; }
    }

    protected readonly struct TeamReadonlyStruct
    {
        public string Name { get; init; }
        public List<string> Members { get; init; }
    }

    protected static PubWithRecords CreatePubWithRecords()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new()
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = new[] { "One", "Two", "Three" },
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Clueless",
                    Members = new()
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = new()
                {
                    Name = "ZZ",
                    Members = new()
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                },
            },
            EveningActivity = new()
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = Array.Empty<string>(),
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Dazed and Confused",
                    Members = new()
                    {
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    }
                },
                RunnersUp = new() { Name = "Banksy", Members = new() }
            },
            FeaturedTeam = new() { Name = "Not In This Lifetime", Members = new() { "Slash", "Axl" } }
        };

    protected class PubWithRecords
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ActivityRecord LunchtimeActivity { get; set; } = null!;
        public ActivityRecord EveningActivity { get; set; } = null!;
        public TeamRecord FeaturedTeam { get; set; } = null!;
    }

    protected record ActivityRecord
    {
        public string Name { get; init; } = null!;
        public decimal? CoverCharge { get; init; }
        public bool IsTeamBased { get; init; }
        public string? Description { get; init; }
        public string[]? Notes { get; init; }
        public DayOfWeek Day { get; init; }
        public TeamRecord Champions { get; init; } = null!;
        public TeamRecord RunnersUp { get; init; } = null!;
    }

    protected record TeamRecord
    {
        public string Name { get; init; } = null!;
        public List<string> Members { get; init; } = null!;
    }

    protected static FieldPub CreateFieldPub()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new()
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = new[] { "One", "Two", "Three" },
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Clueless",
                    Members =
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = new()
                {
                    Name = "ZZ",
                    Members =
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                },
            },
            EveningActivity = new()
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = Array.Empty<string>(),
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Dazed and Confused",
                    Members =
                    {
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    }
                },
                RunnersUp = new()
                {
                    Name = "Banksy", Members = new()
                },
            },
            FeaturedTeam = new()
            {
                Name = "Not In This Lifetime",
                Members =
                {
                    "Slash",
                    "Axl"
                }
            }
        };

    protected class FieldPub
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivity LunchtimeActivity = null!;
        public FieldActivity EveningActivity = null!;
        public FieldTeam FeaturedTeam = null!;
    }

    protected class FieldActivity
    {
        public string Name = null!;
        public decimal? CoverCharge;
        public bool IsTeamBased;
        public string? Description;
        public string[]? Notes;
        public DayOfWeek Day;
        public Team Champions = null!;
        public Team RunnersUp = null!;
    }

    protected class FieldTeam
    {
        public string Name = null!;
        public List<string> Members = new();
    }

    protected static FieldPubWithStructs CreateFieldPubWithStructs()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new()
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = new[] { "One", "Two", "Three" },
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Clueless",
                    Members = new()
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = new()
                {
                    Name = "ZZ",
                    Members = new()
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                },
            },
            EveningActivity = new()
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = Array.Empty<string>(),
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Dazed and Confused",
                    Members = new()
                    {
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    }
                },
                RunnersUp = new() { Name = "Banksy", Members = new() }
            },
            FeaturedTeam = new() { Name = "Not In This Lifetime", Members = new() { "Slash", "Axl" } }
        };

    protected class FieldPubWithStructs
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivityStruct LunchtimeActivity;
        public FieldActivityStruct EveningActivity;
        public FieldTeamStruct FeaturedTeam;
    }

    protected struct FieldActivityStruct
    {
        public string Name;
        public decimal? CoverCharge;
        public bool IsTeamBased;
        public string? Description;
        public string[]? Notes;
        public DayOfWeek Day;
        public FieldTeamStruct Champions;
        public FieldTeamStruct RunnersUp;
    }

    protected struct FieldTeamStruct
    {
        public string Name;
        public List<string> Members;
    }

    protected static FieldPubWithReadonlyStructs CreateFieldPubWithReadonlyStructs()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity =
                new(
                    "Pub Quiz", 2.0m, true, "A general knowledge pub quiz.", new[] { "One", "Two", "Three" }, DayOfWeek.Monday,
                    new(
                        "Clueless", new()
                        {
                            "Boris",
                            "David",
                            "Theresa"
                        }), new(
                        "ZZ", new()
                        {
                            "Has Beard",
                            "Has Beard",
                            "Is Called Beard"
                        })),
            EveningActivity =
                new(
                    "Music Quiz", 5.0m, true, "A music pub quiz.", Array.Empty<string>(), DayOfWeek.Friday,
                    new(
                        "Dazed and Confused", new()
                        {
                            "Robert",
                            "Jimmy",
                            "John",
                            "Jason"
                        }), new("Banksy", new())),
            FeaturedTeam = new("Not In This Lifetime", new() { "Slash", "Axl" })
        };

    protected class FieldPubWithReadonlyStructs
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivityReadonlyStruct LunchtimeActivity;
        public FieldActivityReadonlyStruct EveningActivity;
        public FieldTeamReadonlyStruct FeaturedTeam;
    }

    protected readonly struct FieldActivityReadonlyStruct(
        string name,
        decimal? coverCharge,
        bool isTeamBased,
        string? description,
        string[]? notes,
        DayOfWeek day,
        FieldTeamReadonlyStruct champions,
        FieldTeamReadonlyStruct runnersUp)
    {
        public readonly string Name = name;
        public readonly decimal? CoverCharge = coverCharge;
        public readonly bool IsTeamBased = isTeamBased;
        public readonly string? Description = description;
        public readonly string[]? Notes = notes;
        public readonly DayOfWeek Day = day;
        public readonly FieldTeamReadonlyStruct Champions = champions;
        public readonly FieldTeamReadonlyStruct RunnersUp = runnersUp;
    }

    protected readonly struct FieldTeamReadonlyStruct(string name, List<string> members)
    {
        public readonly string Name = name;
        public readonly List<string> Members = members;
    }

    protected static FieldPubWithRecords CreateFieldPubWithRecords()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new()
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = new[] { "One", "Two", "Three" },
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Clueless",
                    Members = new()
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = new()
                {
                    Name = "ZZ",
                    Members = new()
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                },
            },
            EveningActivity = new()
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = Array.Empty<string>(),
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new()
                {
                    Name = "Dazed and Confused",
                    Members = new()
                    {
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    }
                },
                RunnersUp = new() { Name = "Banksy", Members = new() }
            },
            FeaturedTeam = new() { Name = "Not In This Lifetime", Members = new() { "Slash", "Axl" } }
        };

    protected class FieldPubWithRecords
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivityRecord LunchtimeActivity = null!;
        public FieldActivityRecord EveningActivity = null!;
        public FieldTeamRecord FeaturedTeam = null!;
    }

    protected record FieldActivityRecord
    {
        public string Name = null!;
        public decimal? CoverCharge;
        public bool IsTeamBased;
        public string? Description;
        public string[]? Notes;
        public DayOfWeek Day;
        public FieldTeamRecord Champions = null!;
        public FieldTeamRecord RunnersUp = null!;
    }

    protected record FieldTeamRecord
    {
        public string Name = null!;
        public List<string> Members = null!;
    }

    protected class Yogurt
    {
        public Guid Id { get; set; }
        public Culture Culture { get; set; }
        public Milk Milk { get; set; } = null!;
    }

    protected struct Culture
    {
        public string Species { get; set; }
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation  { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public License License { get; set; }
    }

    protected class Milk
    {
        public string Species { get; set; } = null!;
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation  { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        public License License { get; set; }
    }

    protected class Manufacturer
    {
        public string? Name { get; set; }
        public int Rating { get; set; }
        public Tag Tag { get; set; } = null!;
        public Tog Tog { get; set; }
    }

    protected struct License
    {
        public string Title { get; set; }
        public decimal Charge { get; set; }
        public Tag Tag { get; set; }
        public Tog Tog { get; set; }
    }

    protected class Tag
    {
        public string? Text { get; set; }
    }

    protected struct Tog
    {
        public string? Text { get; set; }
    }

    protected static Yogurt CreateYogurt(bool nullMilk = false, bool nullManufacturer = false, bool nullTag = false)
        => new()
        {
            Id = Guid.NewGuid(),
            Culture = new()
            {
                License = new()
                {
                    Charge = 1.0m,
                    Tag = nullTag ? null! : new() { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new() { Text = "To1" }
                },
                Manufacturer = nullManufacturer
                    ? null!
                    : new()
                    {
                        Name = "M1",
                        Rating = 7,
                        Tag = nullTag ? null! : new() { Text = "Ta2" },
                        Tog = new() { Text = "To2" }
                    },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = nullMilk
                ? null!
                : new()
                {
                    License = new()
                    {
                        Charge = 1.0m,
                        Tag = nullTag ? null! : new() { Text = "Ta1" },
                        Title = "Ti1",
                        Tog = new() { Text = "To1" }
                    },
                    Manufacturer = nullManufacturer
                        ? null!
                        : new()
                        {
                            Name = "M1",
                            Rating = 7,
                            Tag = nullTag ? null! : new() { Text = "Ta2" },
                            Tog = new() { Text = "To2" }
                        },
                    Rating = 8,
                    Species = "S1",
                    Validation = false
                }
        };
}

