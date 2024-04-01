// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        => TrackAndSaveTest(state, async, c => CreatePub(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_type_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreatePub(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_types(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreatePub(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_types(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreatePub(c));

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
        => TrackAndSaveTest(state, async, c => CreatePubWithStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreatePubWithStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_structs(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreatePubWithStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_structs(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreatePubWithStructs(c));

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
        => TrackAndSaveTest(state, async, c => CreatePubWithReadonlyStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_readonly_struct_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreatePubWithReadonlyStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_readonly_structs(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreatePubWithReadonlyStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_readonly_structs(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreatePubWithReadonlyStructs(c));

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
        => TrackAndSaveTest(state, async, c => CreatePubWithRecords(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_type_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreatePubWithRecords(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_record_complex_types(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreatePubWithRecords(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_record_complex_types(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreatePubWithRecords(c));

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
        => TrackAndSaveTest(state, async, c => CreateFieldPub(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_type_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreateFieldPub(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreateFieldPub(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreateFieldPub(c));

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
        => TrackAndSaveTest(state, async, c => CreateFieldPubWithStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreateFieldPubWithStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreateFieldPubWithStructs(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreateFieldPubWithStructs(c));

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
        => TrackAndSaveTest(state, async, c => CreateFieldPubWithReadonlyStructs(c));

    [ConditionalTheory(Skip = "Constructor binding")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreateFieldPubWithReadonlyStructs(c));

    [ConditionalTheory(Skip = "Constructor binding")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreateFieldPubWithReadonlyStructs(c));

    [ConditionalTheory(Skip = "Constructor binding")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreateFieldPubWithReadonlyStructs(c));

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
        => TrackAndSaveTest(state, async, c => CreateFieldPubWithRecords(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_type_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, c => CreateFieldPubWithRecords(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, c => CreateFieldPubWithRecords(c));

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, c => CreateFieldPubWithRecords(c));

    private async Task TrackAndSaveTest<TEntity>(EntityState state, bool async, Func<DbContext, TEntity> createPub)
        where TEntity : class
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var pub = createPub(context);
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

    private void MarkModifiedTest<TEntity>(bool trackFromQuery, Func<DbContext, TEntity> createPub)
        where TEntity : class
    {
        using var context = CreateContext();

        var pub = createPub(context);
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

    private void ReadOriginalValuesTest<TEntity>(bool trackFromQuery, Func<DbContext, TEntity> createPub)
        where TEntity : class
    {
        using var context = CreateContext();

        var pub = createPub(context);
        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        var membersEntry = entry.ComplexProperty("LunchtimeActivity").ComplexProperty("Champions").Property("Members");
        membersEntry.CurrentValue = new List<string>
        {
            "1",
            "2",
            "3"
        };
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);

        if (Fixture.UseProxies)
        {
            Assert.Equal(
                CoreStrings.OriginalValueNotTracked(membersEntry.Metadata.Name, membersEntry.Metadata.DeclaringType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => membersEntry.OriginalValue).Message);
        }
        else
        {
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
    }

    private void WriteOriginalValuesTest<TEntity>(bool trackFromQuery, Func<DbContext, TEntity> createPub)
        where TEntity : class
    {
        using var context = CreateContext();
        var pub = createPub(context);
        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        var membersEntry = entry.ComplexProperty("EveningActivity").ComplexProperty("Champions").Property("Members");

        if (Fixture.UseProxies)
        {
            Assert.Equal(
                CoreStrings.OriginalValueNotTracked(membersEntry.Metadata.Name, membersEntry.Metadata.DeclaringType.DisplayName()),
                Assert.Throws<InvalidOperationException>(() => membersEntry.OriginalValue = new List<string>()).Message);
        }
        else
        {
            membersEntry.OriginalValue = new List<string>
            {
                "1",
                "2",
                "3"
            };
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
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detect_changes_in_complex_type_properties(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePub(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        pub.EveningActivity.Champions.Members =
        [
            "1",
            "2",
            "3"
        ];
        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            Assert.Equal(EntityState.Unchanged, entry.State);
            membersEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);
        }

        pub.LunchtimeActivity.Day = DayOfWeek.Wednesday;
        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            dayEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);
        }

        pub.EveningActivity.CoverCharge = 3.0m;
        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            coverChargeEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Throws_only_when_saving_with_null_top_level_complex_property(bool async)
    {
        using var context = CreateContext();

        var yogurt = CreateYogurt(context, nullMilk: true);
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

        var yogurt = CreateYogurt(context, nullManufacturer: true);
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

        var yogurt = CreateYogurt(context, nullTag: true);
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
        var pub = CreatePubWithStructs(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        var eveningActivity = pub.EveningActivity;
        var champions = eveningActivity.Champions;
        champions.Members =
        [
            "1",
            "2",
            "3"
        ];
        eveningActivity.Champions = champions;
        pub.EveningActivity = eveningActivity;

        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            Assert.Equal(EntityState.Unchanged, entry.State);
            membersEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);
        }

        var lunchtimeActivity = pub.LunchtimeActivity;
        lunchtimeActivity.Day = DayOfWeek.Wednesday;
        pub.LunchtimeActivity = lunchtimeActivity;

        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            dayEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);
        }

        eveningActivity = pub.EveningActivity;
        eveningActivity.CoverCharge = 3.0m;
        pub.EveningActivity = eveningActivity;

        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            coverChargeEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);
        if (!Fixture.UseProxies)
        {
            Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detects_changes_in_complex_readonly_struct_type_properties(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithReadonlyStructs(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        pub.EveningActivity = new ActivityReadonlyStruct
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Champions = new TeamReadonlyStruct
            {
                Name = "Dazed and Confused",
                Members =
                [
                    "1",
                    "2",
                    "3"
                ]
            },
            RunnersUp = new TeamReadonlyStruct { Name = "Banksy", Members = [] }
        };

        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            Assert.Equal(EntityState.Unchanged, entry.State);
            membersEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);
        }

        pub.LunchtimeActivity = new ActivityReadonlyStruct
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Wednesday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Champions = new TeamReadonlyStruct
            {
                Name = "Clueless",
                Members =
                [
                    "Boris",
                    "David",
                    "Theresa"
                ]
            },
            RunnersUp = new TeamReadonlyStruct
            {
                Name = "ZZ",
                Members =
                [
                    "Has Beard",
                    "Has Beard",
                    "Is Called Beard"
                ]
            },
        };

        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            dayEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);
        }

        pub.EveningActivity = new ActivityReadonlyStruct
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 3.0m,
            IsTeamBased = true,
            Champions = new TeamReadonlyStruct
            {
                Name = "Dazed and Confused",
                Members =
                [
                    "1",
                    "2",
                    "3"
                ]
            },
            RunnersUp = new TeamReadonlyStruct { Name = "Banksy", Members = [] }
        };

        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            coverChargeEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detects_changes_in_complex_record_type_properties(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithRecords(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        pub.EveningActivity = pub.EveningActivity with
        {
            Champions = pub.EveningActivity.Champions with
            {
                Members =
                [
                    "1",
                    "2",
                    "3"
                ]
            }
        };

        context.ChangeTracker.DetectChanges();

        var membersEntry = entry.ComplexProperty(e => e.EveningActivity).ComplexProperty(e => e.Champions).Property(e => e.Members);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            Assert.Equal(EntityState.Unchanged, entry.State);
            membersEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);
        Assert.Equal(new[] { "1", "2", "3" }, membersEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, membersEntry.OriginalValue);
        }

        pub.LunchtimeActivity = pub.LunchtimeActivity with { Day = DayOfWeek.Wednesday };
        context.ChangeTracker.DetectChanges();

        var dayEntry = entry.ComplexProperty(e => e.LunchtimeActivity).Property(e => e.Day);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            dayEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);
        Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);
        }

        pub.EveningActivity = pub.EveningActivity with { CoverCharge = 3.0m };
        context.ChangeTracker.DetectChanges();

        var coverChargeEntry = entry.ComplexProperty(e => e.EveningActivity).Property(e => e.CoverCharge);

        if (Fixture.UseProxies)
        {
            // Mutating complex types does not result in notifications
            coverChargeEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        Assert.Equal(3.0m, coverChargeEntry.CurrentValue);

        if (!Fixture.UseProxies)
        {
            Assert.Equal(5.0m, coverChargeEntry.OriginalValue);
        }
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
        => new(context.GetService<IStateManager>().StartTrackingFromQuery(
            context.Model.FindEntityType(typeof(TEntity))!, pub, Snapshot.Empty));

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

        public virtual bool UseProxies
            => false;

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

            if (!UseProxies)
            {
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
            }

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

    protected Pub CreatePub(DbContext context, bool nullActivity = false, bool nullChampions = false, bool nullRunnersUp = false)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<Pub>()
            : new Pub();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.LunchtimeActivity = nullActivity
            ? null!
            : new Activity
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = ["One", "Two", "Three"],
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = nullChampions
                    ? null!
                    : new Team
                    {
                        Name = "Clueless",
                        Members =
                        {
                            "Boris",
                            "David",
                            "Theresa"
                        }
                    },
                RunnersUp = nullRunnersUp
                    ? null!
                    : new Team
                    {
                        Name = "ZZ",
                        Members =
                        {
                            "Has Beard",
                            "Has Beard",
                            "Is Called Beard"
                        }
                    },
            };

        pub.EveningActivity = new Activity
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Champions = new Team
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
            RunnersUp = new Team { Name = "Banksy", Members = [] },
        };

        pub.FeaturedTeam = new Team { Name = "Not In This Lifetime", Members = { "Slash", "Axl" } };

        return pub;
    }

    public class Pub
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual Activity LunchtimeActivity { get; set; } = null!;
        public virtual Activity EveningActivity { get; set; } = null!;
        public virtual Team FeaturedTeam { get; set; } = null!;
    }

    public class Activity
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

    public class Team
    {
        public string Name { get; set; } = null!;
        public List<string> Members { get; set; } = [];
    }

    protected PubWithStructs CreatePubWithStructs(DbContext context)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<PubWithStructs>()
            : new PubWithStructs();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.LunchtimeActivity = new ActivityStruct
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Monday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Champions = new TeamStruct
            {
                Name = "Clueless",
                Members =
                [
                    "Boris",
                    "David",
                    "Theresa"
                ]
            },
            RunnersUp = new TeamStruct
            {
                Name = "ZZ",
                Members =
                [
                    "Has Beard",
                    "Has Beard",
                    "Is Called Beard"
                ]
            },
        };

        pub.EveningActivity = new ActivityStruct
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Champions = new TeamStruct
            {
                Name = "Dazed and Confused",
                Members =
                [
                    "Robert",
                    "Jimmy",
                    "John",
                    "Jason"
                ]
            },
            RunnersUp = new TeamStruct { Name = "Banksy", Members = [] }
        };

        pub.FeaturedTeam = new TeamStruct { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] };

        return pub;
    }

    public class PubWithStructs
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual ActivityStruct LunchtimeActivity { get; set; }
        public virtual ActivityStruct EveningActivity { get; set; }
        public virtual TeamStruct FeaturedTeam { get; set; }
    }

    public struct ActivityStruct
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

    public struct TeamStruct
    {
        public string Name { get; set; }
        public List<string> Members { get; set; }
    }

    protected PubWithReadonlyStructs CreatePubWithReadonlyStructs(DbContext context)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<PubWithReadonlyStructs>()
            : new PubWithReadonlyStructs();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.LunchtimeActivity = new ActivityReadonlyStruct
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Monday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Champions = new TeamReadonlyStruct
            {
                Name = "Clueless",
                Members =
                [
                    "Boris",
                    "David",
                    "Theresa"
                ]
            },
            RunnersUp = new TeamReadonlyStruct
            {
                Name = "ZZ",
                Members =
                [
                    "Has Beard",
                    "Has Beard",
                    "Is Called Beard"
                ]
            },
        };

        pub.EveningActivity = new ActivityReadonlyStruct
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Champions = new TeamReadonlyStruct
            {
                Name = "Dazed and Confused",
                Members =
                [
                    "Robert",
                    "Jimmy",
                    "John",
                    "Jason"
                ]
            },
            RunnersUp = new TeamReadonlyStruct { Name = "Banksy", Members = [] }
        };

        pub.FeaturedTeam = new TeamReadonlyStruct { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] };

        return pub;
    }

    public class PubWithReadonlyStructs
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual ActivityReadonlyStruct LunchtimeActivity { get; set; }
        public virtual ActivityReadonlyStruct EveningActivity { get; set; }
        public virtual TeamReadonlyStruct FeaturedTeam { get; set; }
    }

    public readonly struct ActivityReadonlyStruct
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

    public readonly struct TeamReadonlyStruct
    {
        public string Name { get; init; }
        public List<string> Members { get; init; }
    }

    public PubWithRecords CreatePubWithRecords(DbContext context)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<PubWithRecords>()
            : new PubWithRecords();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.LunchtimeActivity = new ActivityRecord
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Monday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Champions = new TeamRecord
            {
                Name = "Clueless",
                Members =
                [
                    "Boris",
                    "David",
                    "Theresa"
                ]
            },
            RunnersUp = new TeamRecord
            {
                Name = "ZZ",
                Members =
                [
                    "Has Beard",
                    "Has Beard",
                    "Is Called Beard"
                ]
            },
        };

        pub.EveningActivity = new ActivityRecord
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Champions = new TeamRecord
            {
                Name = "Dazed and Confused",
                Members =
                [
                    "Robert",
                    "Jimmy",
                    "John",
                    "Jason"
                ]
            },
            RunnersUp = new TeamRecord { Name = "Banksy", Members = [] }
        };

        pub.FeaturedTeam = new TeamRecord { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] };

        return pub;
    }

    public class PubWithRecords
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual ActivityRecord LunchtimeActivity { get; set; } = null!;
        public virtual ActivityRecord EveningActivity { get; set; } = null!;
        public virtual TeamRecord FeaturedTeam { get; set; } = null!;
    }

    public record ActivityRecord
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

    public record TeamRecord
    {
        public string Name { get; init; } = null!;
        public List<string> Members { get; init; } = null!;
    }

    protected static FieldPub CreateFieldPub(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new FieldActivity
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = ["One", "Two", "Three"],
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new Team
                {
                    Name = "Clueless",
                    Members =
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                RunnersUp = new Team
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
            EveningActivity = new FieldActivity
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = [],
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new Team
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
                RunnersUp = new Team { Name = "Banksy", Members = [] },
            },
            FeaturedTeam = new FieldTeam { Name = "Not In This Lifetime", Members = { "Slash", "Axl" } }
        };

    public class FieldPub
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivity LunchtimeActivity = null!;
        public FieldActivity EveningActivity = null!;
        public FieldTeam FeaturedTeam = null!;
    }

    public class FieldActivity
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

    public class FieldTeam
    {
        public string Name = null!;
        public List<string> Members = [];
    }

    protected static FieldPubWithStructs CreateFieldPubWithStructs(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new FieldActivityStruct
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = ["One", "Two", "Three"],
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new FieldTeamStruct
                {
                    Name = "Clueless",
                    Members =
                    [
                        "Boris",
                        "David",
                        "Theresa"
                    ]
                },
                RunnersUp = new FieldTeamStruct
                {
                    Name = "ZZ",
                    Members =
                    [
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    ]
                },
            },
            EveningActivity = new FieldActivityStruct
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = [],
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new FieldTeamStruct
                {
                    Name = "Dazed and Confused",
                    Members =
                    [
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    ]
                },
                RunnersUp = new FieldTeamStruct { Name = "Banksy", Members = [] }
            },
            FeaturedTeam = new FieldTeamStruct { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] }
        };

    public class FieldPubWithStructs
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivityStruct LunchtimeActivity;
        public FieldActivityStruct EveningActivity;
        public FieldTeamStruct FeaturedTeam;
    }

    public struct FieldActivityStruct
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

    public struct FieldTeamStruct
    {
        public string Name;
        public List<string> Members;
    }

    protected static FieldPubWithReadonlyStructs CreateFieldPubWithReadonlyStructs(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity =
                new FieldActivityReadonlyStruct(
                    "Pub Quiz", 2.0m, true, "A general knowledge pub quiz.", ["One", "Two", "Three"], DayOfWeek.Monday,
                    new FieldTeamReadonlyStruct(
                        "Clueless", [
                            "Boris",
                            "David",
                            "Theresa"
                        ]), new FieldTeamReadonlyStruct(
                        "ZZ", [
                            "Has Beard",
                            "Has Beard",
                            "Is Called Beard"
                        ])),
            EveningActivity =
                new FieldActivityReadonlyStruct(
                    "Music Quiz", 5.0m, true, "A music pub quiz.", [], DayOfWeek.Friday,
                    new FieldTeamReadonlyStruct(
                        "Dazed and Confused", [
                            "Robert",
                            "Jimmy",
                            "John",
                            "Jason"
                        ]), new FieldTeamReadonlyStruct("Banksy", [])),
            FeaturedTeam = new FieldTeamReadonlyStruct("Not In This Lifetime", ["Slash", "Axl"])
        };

    public class FieldPubWithReadonlyStructs
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivityReadonlyStruct LunchtimeActivity;
        public FieldActivityReadonlyStruct EveningActivity;
        public FieldTeamReadonlyStruct FeaturedTeam;
    }

    public readonly struct FieldActivityReadonlyStruct(
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

    public readonly struct FieldTeamReadonlyStruct(string name, List<string> members)
    {
        public readonly string Name = name;
        public readonly List<string> Members = members;
    }

    protected static FieldPubWithRecords CreateFieldPubWithRecords(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            LunchtimeActivity = new FieldActivityRecord
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = ["One", "Two", "Three"],
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Champions = new FieldTeamRecord
                {
                    Name = "Clueless",
                    Members =
                    [
                        "Boris",
                        "David",
                        "Theresa"
                    ]
                },
                RunnersUp = new FieldTeamRecord
                {
                    Name = "ZZ",
                    Members =
                    [
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    ]
                },
            },
            EveningActivity = new FieldActivityRecord
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = [],
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Champions = new FieldTeamRecord
                {
                    Name = "Dazed and Confused",
                    Members =
                    [
                        "Robert",
                        "Jimmy",
                        "John",
                        "Jason"
                    ]
                },
                RunnersUp = new FieldTeamRecord { Name = "Banksy", Members = [] }
            },
            FeaturedTeam = new FieldTeamRecord { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] }
        };

    public class FieldPubWithRecords
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public FieldActivityRecord LunchtimeActivity = null!;
        public FieldActivityRecord EveningActivity = null!;
        public FieldTeamRecord FeaturedTeam = null!;
    }

    public record FieldActivityRecord
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

    public record FieldTeamRecord
    {
        public string Name = null!;
        public List<string> Members = null!;
    }

    public class Yogurt
    {
        public virtual Guid Id { get; set; }
        public virtual Culture Culture { get; set; }
        public virtual Milk Milk { get; set; } = null!;
    }

    public struct Culture
    {
        public string Species { get; set; }
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public License License { get; set; }
    }

    public class Milk
    {
        public string Species { get; set; } = null!;
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        public License License { get; set; }
    }

    public class Manufacturer
    {
        public string? Name { get; set; }
        public int Rating { get; set; }
        public Tag Tag { get; set; } = null!;
        public Tog Tog { get; set; }
    }

    public struct License
    {
        public string Title { get; set; }
        public decimal Charge { get; set; }
        public Tag Tag { get; set; }
        public Tog Tog { get; set; }
    }

    public class Tag
    {
        public string? Text { get; set; }
    }

    public struct Tog
    {
        public string? Text { get; set; }
    }

    protected Yogurt CreateYogurt(DbContext context, bool nullMilk = false, bool nullManufacturer = false, bool nullTag = false)
    {
        var yogurt = Fixture.UseProxies
            ? context.CreateProxy<Yogurt>()
            : new Yogurt();

        yogurt.Id = Guid.NewGuid();

        yogurt.Culture = new Culture
        {
            License = new License
            {
                Charge = 1.0m,
                Tag = nullTag ? null! : new Tag { Text = "Ta1" },
                Title = "Ti1",
                Tog = new Tog { Text = "To1" }
            },
            Manufacturer = nullManufacturer
                ? null!
                : new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = nullTag ? null! : new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
            Rating = 8,
            Species = "S1",
            Validation = false
        };

        yogurt.Milk = nullMilk
            ? null!
            : new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = nullTag ? null! : new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = nullManufacturer
                    ? null!
                    : new Manufacturer
                    {
                        Name = "M1",
                        Rating = 7,
                        Tag = nullTag ? null! : new Tag { Text = "Ta2" },
                        Tog = new Tog { Text = "To2" }
                    },
                Rating = 8,
                Species = "S1",
                Validation = false
            };

        return yogurt;
    }
}
