// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class ComplexTypesTrackingTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ComplexTypesTrackingTestBase<TFixture>.FixtureBase
{
    protected ComplexTypesTrackingTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

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

        MarkModified(entry, "EveningActivity.RunnersUp.Members", true);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.RunnersUp.Members"));

        MarkModified(entry, "LunchtimeActivity.Day", true);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "LunchtimeActivity.Day"));

        MarkModified(entry, "EveningActivity.CoverCharge", true);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.CoverCharge"));

        Assert.False(IsModified(entry, "LunchtimeActivity.Name"));
        Assert.False(IsModified(entry, "LunchtimeActivity.Description"));
        Assert.False(IsModified(entry, "LunchtimeActivity.Notes"));
        Assert.False(IsModified(entry, "LunchtimeActivity.CoverCharge"));
        Assert.False(IsModified(entry, "LunchtimeActivity.IsTeamBased"));
        Assert.False(IsModified(entry, "LunchtimeActivity.Champions.Name"));
        Assert.False(IsModified(entry, "LunchtimeActivity.Champions.Members"));
        Assert.False(IsModified(entry, "LunchtimeActivity.RunnersUp.Name"));
        Assert.False(IsModified(entry, "LunchtimeActivity.RunnersUp.Members"));
        Assert.False(IsModified(entry, "EveningActivity.Name"));
        Assert.False(IsModified(entry, "EveningActivity.Day"));
        Assert.False(IsModified(entry, "EveningActivity.Description"));
        Assert.False(IsModified(entry, "EveningActivity.Notes"));
        Assert.False(IsModified(entry, "EveningActivity.IsTeamBased"));
        Assert.False(IsModified(entry, "EveningActivity.Champions.Name"));
        Assert.False(IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.False(IsModified(entry, "EveningActivity.RunnersUp.Name"));
        Assert.False(IsModified(entry, "FeaturedTeam.Name"));
        Assert.False(IsModified(entry, "FeaturedTeam.Members"));

        MarkModified(entry, "EveningActivity.RunnersUp.Members", false);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.False(IsModified(entry, "EveningActivity.RunnersUp.Members"));

        MarkModified(entry, "LunchtimeActivity.Day", false);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.False(IsModified(entry, "LunchtimeActivity.Day"));

        MarkModified(entry, "EveningActivity.CoverCharge", false);
        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(IsModified(entry, "EveningActivity.CoverCharge"));

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

        WriteCurrentValue(entry, "EveningActivity.Champions.Members", new List<string> { "1", "2", "3" });
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(new[] { "1", "2", "3" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(
            new[] { "Robert", "Jimmy", "John", "Jason" }, ReadOriginalValue<List<string>>(entry, "EveningActivity.Champions.Members"));

        WriteCurrentValue(entry, "LunchtimeActivity.Day", DayOfWeek.Wednesday);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Wednesday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Monday, ReadOriginalValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));

        WriteCurrentValue(entry, "EveningActivity.CoverCharge", 3.0m);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(3.0m, ReadCurrentValue<decimal?>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadOriginalValue<decimal?>(entry, "EveningActivity.CoverCharge"));
    }

    private void WriteOriginalValuesTest<TEntity>(bool trackFromQuery, TEntity pub)
        where TEntity : class
    {
        using var context = CreateContext();
        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);
        AssertPropertyValues(entry);
        AssertPropertiesModified(entry, false);

        WriteOriginalValue(entry, "EveningActivity.Champions.Members", new List<string> { "1", "2", "3" });
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(new[] { "1", "2", "3" }, ReadOriginalValue<List<string>>(entry, "EveningActivity.Champions.Members"));

        WriteOriginalValue(entry, "LunchtimeActivity.Day", DayOfWeek.Wednesday);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Monday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Wednesday, ReadOriginalValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));

        WriteOriginalValue(entry, "EveningActivity.CoverCharge", 3.0m);
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadCurrentValue<decimal?>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(3.0m, ReadOriginalValue<decimal?>(entry, "EveningActivity.CoverCharge"));
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

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(new[] { "1", "2", "3" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(
            new[] { "Robert", "Jimmy", "John", "Jason" }, ReadOriginalValue<List<string>>(entry, "EveningActivity.Champions.Members"));

        pub.LunchtimeActivity.Day = DayOfWeek.Wednesday;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Wednesday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Monday, ReadOriginalValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));

        pub.EveningActivity.CoverCharge = 3.0m;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(3.0m, ReadCurrentValue<decimal?>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadOriginalValue<decimal?>(entry, "EveningActivity.CoverCharge"));
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

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(new[] { "1", "2", "3" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(
            new[] { "Robert", "Jimmy", "John", "Jason" }, ReadOriginalValue<List<string>>(entry, "EveningActivity.Champions.Members"));

        var lunchtimeActivity = pub.LunchtimeActivity;
        lunchtimeActivity.Day = DayOfWeek.Wednesday;
        pub.LunchtimeActivity = lunchtimeActivity;

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Wednesday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Monday, ReadOriginalValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));

        eveningActivity = pub.EveningActivity;
        eveningActivity.CoverCharge = 3.0m;
        pub.EveningActivity = eveningActivity;

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(3.0m, ReadCurrentValue<decimal?>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadOriginalValue<decimal?>(entry, "EveningActivity.CoverCharge"));
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

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(new[] { "1", "2", "3" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(
            new[] { "Robert", "Jimmy", "John", "Jason" }, ReadOriginalValue<List<string>>(entry, "EveningActivity.Champions.Members"));

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

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Wednesday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Monday, ReadOriginalValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));

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

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(3.0m, ReadCurrentValue<decimal?>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadOriginalValue<decimal?>(entry, "EveningActivity.CoverCharge"));
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

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(new[] { "1", "2", "3" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(
            new[] { "Robert", "Jimmy", "John", "Jason" }, ReadOriginalValue<List<string>>(entry, "EveningActivity.Champions.Members"));

        pub.LunchtimeActivity = pub.LunchtimeActivity with { Day = DayOfWeek.Wednesday };
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Wednesday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal(DayOfWeek.Monday, ReadOriginalValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));

        pub.EveningActivity = pub.EveningActivity with { CoverCharge = 3.0m };
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(IsModified(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(3.0m, ReadCurrentValue<decimal?>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadOriginalValue<decimal?>(entry, "EveningActivity.CoverCharge"));
    }

    protected void AssertPropertyValues(EntityEntry entry)
    {
        Assert.Equal("The FBI", ReadCurrentValue<string>(entry, "Name"));
        Assert.Equal("Pub Quiz", ReadCurrentValue<string>(entry, "LunchtimeActivity.Name"));
        Assert.Equal(DayOfWeek.Monday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal("A general knowledge pub quiz.", ReadCurrentValue<string>(entry, "LunchtimeActivity.Description"));
        Assert.Equal(new[] { "One", "Two", "Three" }, ReadCurrentValue<string[]>(entry, "LunchtimeActivity.Notes"));
        Assert.Equal(2.0m, ReadCurrentValue<decimal?>(entry, "LunchtimeActivity.CoverCharge"));
        Assert.True(ReadCurrentValue<bool>(entry, "LunchtimeActivity.IsTeamBased"));
        Assert.Equal("Clueless", ReadCurrentValue<string>(entry, "LunchtimeActivity.Champions.Name"));
        Assert.Equal(new[] { "Boris", "David", "Theresa" }, ReadCurrentValue<List<string>>(entry, "LunchtimeActivity.Champions.Members"));
        Assert.Equal("ZZ", ReadCurrentValue<string>(entry, "LunchtimeActivity.RunnersUp.Name"));
        Assert.Equal(
            new[] { "Has Beard", "Has Beard", "Is Called Beard" },
            ReadCurrentValue<List<string>>(entry, "LunchtimeActivity.RunnersUp.Members"));
        Assert.Equal("Music Quiz", ReadCurrentValue<string>(entry, "EveningActivity.Name"));
        Assert.Equal(DayOfWeek.Friday, ReadCurrentValue<DayOfWeek>(entry, "EveningActivity.Day"));
        Assert.Equal("A music pub quiz.", ReadCurrentValue<string>(entry, "EveningActivity.Description"));
        Assert.Empty(ReadCurrentValue<string[]>(entry, "EveningActivity.Notes"));
        Assert.Equal(5.0m, ReadCurrentValue<decimal?>(entry, "EveningActivity.CoverCharge"));
        Assert.True(ReadCurrentValue<bool>(entry, "EveningActivity.IsTeamBased"));
        Assert.Equal("Dazed and Confused", ReadCurrentValue<string>(entry, "EveningActivity.Champions.Name"));
        Assert.Equal(
            new[] { "Robert", "Jimmy", "John", "Jason" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.Equal("Banksy", ReadCurrentValue<string>(entry, "EveningActivity.RunnersUp.Name"));
        Assert.Empty(ReadCurrentValue<List<string>>(entry, "EveningActivity.RunnersUp.Members"));
        Assert.Equal("Not In This Lifetime", ReadCurrentValue<string>(entry, "FeaturedTeam.Name"));
        Assert.Equal(new[] { "Slash", "Axl" }, ReadCurrentValue<List<string>>(entry, "FeaturedTeam.Members"));
    }

    protected void AssertPropertiesModified(EntityEntry entry, bool expected)
    {
        Assert.Equal(expected, IsModified(entry, "Name"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.Name"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.Day"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.Description"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.Notes"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.CoverCharge"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.IsTeamBased"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.Champions.Name"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.Champions.Members"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.RunnersUp.Name"));
        Assert.Equal(expected, IsModified(entry, "LunchtimeActivity.RunnersUp.Members"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.Name"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.Day"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.Description"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.Notes"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.IsTeamBased"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.Champions.Name"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.Champions.Members"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.RunnersUp.Name"));
        Assert.Equal(expected, IsModified(entry, "EveningActivity.RunnersUp.Members"));
        Assert.Equal(expected, IsModified(entry, "FeaturedTeam.Name"));
        Assert.Equal(expected, IsModified(entry, "FeaturedTeam.Members"));
    }

    protected static TValue ReadCurrentValue<TValue>(EntityEntry entry, string propertyChain)
        => entry.GetInfrastructure().GetCurrentValue<TValue>(FindProperty(entry, propertyChain));

    protected static TValue ReadOriginalValue<TValue>(EntityEntry entry, string propertyChain)
        => entry.GetInfrastructure().GetOriginalValue<TValue>((IProperty)FindProperty(entry, propertyChain));

    protected static void WriteCurrentValue(EntityEntry entry, string propertyChain, object? value)
        => entry.GetInfrastructure().SetProperty(FindProperty(entry, propertyChain), value, isMaterialization: false);

    protected static void WriteOriginalValue(EntityEntry entry, string propertyChain, object? value)
        => entry.GetInfrastructure().SetOriginalValue(FindProperty(entry, propertyChain), value);

    protected static bool IsModified(EntityEntry entry, string propertyChain)
        => entry.GetInfrastructure().IsModified((IProperty)FindProperty(entry, propertyChain));

    protected static EntityEntry<TEntity> TrackFromQuery<TEntity>(DbContext context, TEntity pub)
        where TEntity : class
        => new(
            context.GetService<IStateManager>().StartTrackingFromQuery(
                context.Model.FindEntityType(typeof(TEntity))!, pub, new ValueBuffer()));

    protected static void MarkModified(EntityEntry entry, string propertyChain, bool modified)
        => entry.GetInfrastructure().SetPropertyModified((IProperty)FindProperty(entry, propertyChain), isModified: modified);

    protected static IPropertyBase FindProperty(EntityEntry entry, string propertyChain)
    {
        var internalEntry = entry.GetInfrastructure();
        var names = propertyChain.Split(".");
        var currentType = (ITypeBase)internalEntry.EntityType;

        IPropertyBase property = null!;
        foreach (var name in names)
        {
            var complexProperty = currentType.FindComplexProperty(name);
            if (complexProperty != null)
            {
                currentType = complexProperty.ComplexType;
                property = complexProperty;
            }
            else
            {
                property = currentType.FindProperty(name)!;
            }
        }

        return property;
    }

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
        }
    }

    protected static Pub CreatePub()
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
}
