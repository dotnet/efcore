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
        => TrackAndSaveTest(state, async, CreatePubWithStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_structs(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_structs(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithStructs);

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
        => TrackAndSaveTest(state, async, CreatePubWithReadonlyStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_readonly_struct_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithReadonlyStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_readonly_structs(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithReadonlyStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_readonly_structs(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithReadonlyStructs);

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
        => TrackAndSaveTest(state, async, CreatePubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_type_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_record_complex_types(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_record_complex_types(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithRecords);

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
        => TrackAndSaveTest(state, async, CreateFieldPub);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_type_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPub);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPub);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_types_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPub);

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
        => TrackAndSaveTest(state, async, CreateFieldPubWithStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPubWithStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPubWithStructs);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_structs_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPubWithStructs);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_readonly_structs_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldPubWithReadonlyStructs);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_readonly_struct_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPubWithReadonlyStructs);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPubWithReadonlyStructs);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_readonly_structs_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPubWithReadonlyStructs);

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
        => TrackAndSaveTest(state, async, CreateFieldPubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_type_properties_modified_with_fields(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldPubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldPubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_record_complex_types_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldPubWithRecords);

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_type_collections(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePubWithCollections);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_type_collection_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithCollections);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_type_collections(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithCollections);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_type_collections(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithCollections);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_struct_collections(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePubWithStructCollections);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_struct_collection_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithStructCollections);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_struct_collections(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithStructCollections);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_struct_collections(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithStructCollections);

    [ConditionalTheory(Skip = "Issue #31621")]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_readonly_struct_collections(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePubWithReadonlyStructCollections);

    [ConditionalTheory(Skip = "Issue #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_collection_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithReadonlyStructCollections);

    [ConditionalTheory(Skip = "Issue #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_readonly_struct_collections(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithReadonlyStructCollections);

    [ConditionalTheory(Skip = "Issue #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_readonly_struct_collections(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithReadonlyStructCollections);

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_record_collections(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreatePubWithRecordCollections);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_collection_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreatePubWithRecordCollections);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_record_collections(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreatePubWithRecordCollections);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_record_collections(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreatePubWithRecordCollections);

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_field_collections(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldCollectionPub);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_field_collection_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldCollectionPub);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_field_collections(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldCollectionPub);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_field_collections(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldCollectionPub);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_struct_collections_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldCollectionPubWithStructs);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_struct_collections_with_fields_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldCollectionPubWithStructs);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_struct_collections_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldCollectionPubWithStructs);

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_struct_collections_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldCollectionPubWithStructs);

    [ConditionalTheory]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_record_collections_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldCollectionPubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_record_collections_with_fields_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldCollectionPubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_record_collections_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldCollectionPubWithRecords);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_record_collections_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldCollectionPubWithRecords);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Unchanged, false)]
    [InlineData(EntityState.Unchanged, true)]
    [InlineData(EntityState.Modified, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Deleted, true)]
    public virtual Task Can_track_entity_with_complex_readonly_struct_collections_with_fields(EntityState state, bool async)
        => TrackAndSaveTest(state, async, CreateFieldCollectionPubWithReadonlyStructs);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_readonly_struct_collections_with_fields_properties_modified(bool trackFromQuery)
        => MarkModifiedTest(trackFromQuery, CreateFieldCollectionPubWithReadonlyStructs);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_readonly_struct_collections_with_fields(bool trackFromQuery)
        => ReadOriginalValuesTest(trackFromQuery, CreateFieldCollectionPubWithReadonlyStructs);

    [ConditionalTheory(Skip = "Constructor binding #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_readonly_struct_collections_with_fields(bool trackFromQuery)
        => WriteOriginalValuesTest(trackFromQuery, CreateFieldCollectionPubWithReadonlyStructs);

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

                var hasCollections = entry.Metadata.GetComplexProperties().Any(p => p.Name == "Activities");
                if (hasCollections)
                {
                    AssertCollectionPropertyValues(entry);
                    AssertCollectionPropertiesModified(entry, state);
                }
                else
                {
                    AssertPropertyValues(entry);
                    AssertPropertiesModified(entry, state == EntityState.Modified);
                }

                //TODO: SaveChanges support #31237
                if (!hasCollections
                    && (state == EntityState.Added || state == EntityState.Unchanged))
                {
                    _ = async ? await context.SaveChangesAsync() : context.SaveChanges();

                    Assert.Equal(EntityState.Unchanged, entry.State);

                    if (hasCollections)
                    {
                        AssertCollectionPropertyValues(entry);
                    }
                    else
                    {
                        AssertPropertyValues(entry);
                    }
                }
            });

    private void MarkModifiedTest<TEntity>(bool trackFromQuery, Func<DbContext, TEntity> createPub)
        where TEntity : class
    {
        using var context = CreateContext();

        var pub = createPub(context);
        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);

        var hasCollections = entry.Metadata.GetComplexProperties().Any(p => p.Name == "Activities");
        if (hasCollections)
        {
            AssertCollectionPropertyValues(entry);
            AssertCollectionPropertiesModified(entry, EntityState.Unchanged);
        }
        else
        {
            AssertPropertyValues(entry);
            AssertPropertiesModified(entry, false);
        }

        var membersEntry = hasCollections
            ? entry.ComplexCollection("Activities")[0].ComplexCollection("Teams")[1].Property("Members")
            : entry.ComplexProperty("LunchtimeActivity").ComplexProperty("RunnersUp").Property("Members");
        membersEntry.IsModified = true;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(membersEntry.IsModified);

        var dayEntry = hasCollections
            ? entry.ComplexCollection("Activities")[0].Property("Day")
            : entry.ComplexProperty("LunchtimeActivity").Property("Day");
        dayEntry.IsModified = true;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(dayEntry.IsModified);

        var coverChargeEntry = hasCollections
            ? entry.ComplexCollection("Activities")[1].Property("CoverCharge")
            : entry.ComplexProperty("EveningActivity").Property("CoverCharge");
        coverChargeEntry.IsModified = true;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(coverChargeEntry.IsModified);
        if (hasCollections)
        {
            var activitiesCollection = entry.ComplexCollection("Activities");
            var firstActivityEntry = activitiesCollection[0];
            var secondActivityEntry = activitiesCollection[1];
            var firstTeamOfFirstActivity = firstActivityEntry.ComplexCollection("Teams")[0];
            var lastTeamOfFirstActivity = firstActivityEntry.ComplexCollection("Teams")[1];
            var firstTeamOfSecondActivity = secondActivityEntry.ComplexCollection("Teams")[0];
            var lastTeamOfSecondActivity = secondActivityEntry.ComplexCollection("Teams")[1];
            var featuredTeamEntry = entry.ComplexProperty("FeaturedTeam");

            Assert.False(firstActivityEntry.Property("Name").IsModified);
            Assert.False(firstActivityEntry.Property("Description").IsModified);
            Assert.True(firstActivityEntry.Property("Day").IsModified);
            Assert.False(firstActivityEntry.Property("Notes").IsModified);
            Assert.False(firstActivityEntry.Property("CoverCharge").IsModified);
            Assert.False(firstActivityEntry.Property("IsTeamBased").IsModified);
            Assert.False(firstTeamOfFirstActivity.Property("Name").IsModified);
            Assert.False(firstTeamOfFirstActivity.Property("Members").IsModified);
            Assert.False(lastTeamOfFirstActivity.Property("Name").IsModified);
            Assert.True(lastTeamOfFirstActivity.Property("Members").IsModified);

            Assert.False(secondActivityEntry.Property("Name").IsModified);
            Assert.False(secondActivityEntry.Property("Description").IsModified);
            Assert.False(secondActivityEntry.Property("Day").IsModified);
            Assert.False(secondActivityEntry.Property("Notes").IsModified);
            Assert.True(secondActivityEntry.Property("CoverCharge").IsModified);
            Assert.False(secondActivityEntry.Property("IsTeamBased").IsModified);
            Assert.False(firstTeamOfSecondActivity.Property("Name").IsModified);
            Assert.False(firstTeamOfSecondActivity.Property("Members").IsModified);
            Assert.False(lastTeamOfSecondActivity.Property("Name").IsModified);
            Assert.False(lastTeamOfSecondActivity.Property("Members").IsModified);

            Assert.False(featuredTeamEntry.Property("Name").IsModified);
            Assert.False(featuredTeamEntry.Property("Members").IsModified);
        }
        else
        {
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
        }

        membersEntry.IsModified = false;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.False(membersEntry.IsModified);

        dayEntry.IsModified = false;
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.False(dayEntry.IsModified);

        coverChargeEntry.IsModified = false;
        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(coverChargeEntry.IsModified);

        if (hasCollections)
        {
            AssertCollectionPropertyValues(entry);
            AssertCollectionPropertiesModified(entry, EntityState.Unchanged);
        }
        else
        {
            AssertPropertyValues(entry);
            AssertPropertiesModified(entry, false);
        }
    }

    private void ReadOriginalValuesTest<TEntity>(bool trackFromQuery, Func<DbContext, TEntity> createPub)
        where TEntity : class
    {
        using var context = CreateContext();

        var pub = createPub(context);
        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);

        var hasCollections = entry.Metadata.GetComplexProperties().Any(p => p.Name == "Activities");
        if (hasCollections)
        {
            AssertCollectionPropertyValues(entry);
            AssertCollectionPropertiesModified(entry, EntityState.Unchanged);
        }
        else
        {
            AssertPropertyValues(entry);
            AssertPropertiesModified(entry, false);
        }

        var membersEntry = hasCollections
            ? entry.ComplexCollection("Activities")[0].ComplexCollection("Teams")[0].Property("Members")
            : entry.ComplexProperty("LunchtimeActivity").ComplexProperty("Champions").Property("Members");
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

            var dayEntry = hasCollections
                ? entry.ComplexCollection("Activities")[0].Property("Day")
                : entry.ComplexProperty("LunchtimeActivity").Property("Day");
            dayEntry.CurrentValue = DayOfWeek.Wednesday;
            Assert.Equal(EntityState.Modified, entry.State);
            Assert.True(dayEntry.IsModified);
            Assert.Equal(DayOfWeek.Wednesday, dayEntry.CurrentValue);
            Assert.Equal(DayOfWeek.Monday, dayEntry.OriginalValue);

            var coverChargeEntry = hasCollections
                ? entry.ComplexCollection("Activities")[1].Property("CoverCharge")
                : entry.ComplexProperty("EveningActivity").Property("CoverCharge");
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

        var hasCollections = entry.Metadata.GetComplexProperties().Any(p => p.Name == "Activities");
        if (hasCollections)
        {
            AssertCollectionPropertyValues(entry);
            AssertCollectionPropertiesModified(entry, EntityState.Unchanged);
        }
        else
        {
            AssertPropertyValues(entry);
            AssertPropertiesModified(entry, false);
        }

        var membersEntry = hasCollections
            ? entry.ComplexCollection("Activities")[1].ComplexCollection("Teams")[0].Property("Members")
            : entry.ComplexProperty("EveningActivity").ComplexProperty("Champions").Property("Members");

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

            var dayEntry = hasCollections
                ? entry.ComplexCollection("Activities")[0].Property("Day")
                : entry.ComplexProperty("LunchtimeActivity").Property("Day");
            dayEntry.OriginalValue = DayOfWeek.Wednesday;
            Assert.Equal(EntityState.Modified, entry.State);
            Assert.True(dayEntry.IsModified);
            Assert.Equal(DayOfWeek.Monday, dayEntry.CurrentValue);
            Assert.Equal(DayOfWeek.Wednesday, dayEntry.OriginalValue);

            var coverChargeEntry = hasCollections
                ? entry.ComplexCollection("Activities")[1].Property("CoverCharge")
                : entry.ComplexProperty("EveningActivity").Property("CoverCharge");
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

        Assert.Equal(
            CoreStrings.NullRequiredComplexProperty("Culture", "Manufacturer"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => async ? context.SaveChangesAsync() : Task.FromResult(context.SaveChanges()))).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_save_null_third_level_complex_property_with_all_optional_properties(bool async)
    {
        using var context = CreateContext();

        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            context, async context =>
            {
                using var transaction = context.Database.BeginTransaction();

                var yogurt = CreateYogurt(context, nullTag: true);
                var entry = async ? await context.AddAsync(yogurt) : context.Add(yogurt);

                context.SaveChanges();
            });
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

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_reordered_elements_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);

        var activities = pub.Activities;
        activities.Reverse();
        pub.Activities = activities;

        context.ChangeTracker.DetectChanges();

        var collectionEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            collectionEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(collectionEntry.IsModified);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_added_elements_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities.Add(new ActivityWithCollection
        {
            Name = "New Activity",
            Day = DayOfWeek.Saturday,
            CoverCharge = 10.0m,
            Description = "A new activity",
            IsTeamBased = false,
            Notes = ["New", "Notes"],
            Teams =
            [
                new Team { Name = "New Champions", Members = ["A", "B", "C"] },
                new Team { Name = "New Runners Up", Members = ["X", "Y", "Z"] }
            ]
        });

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal(3, pub.Activities.Count);
        var newActivity = pub.Activities.Last();
        Assert.Equal("New Activity", newActivity.Name);
        Assert.Equal(2, newActivity.Teams.Count);
        Assert.Equal("New Champions", newActivity.Teams[0].Name);
        Assert.Equal("New Runners Up", newActivity.Teams[1].Name);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_removed_elements_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities.RemoveAt(0);

        context.ChangeTracker.DetectChanges();

        var collectionEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            collectionEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(collectionEntry.IsModified);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_replaced_elements_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities[0] = new ActivityWithCollection
        {
            Name = "Replaced Activity",
            Day = DayOfWeek.Saturday,
            CoverCharge = 10.0m,
            Description = "A replaced activity",
            IsTeamBased = false,
            Notes = ["Replaced", "Notes"],
            Teams =
            [
                new Team { Name = "Replaced Champions", Members = ["A", "B", "C"] },
                new Team { Name = "Replaced Runners Up", Members = ["X", "Y", "Z"] }
            ]
        };

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        var activityEntry = activitiesEntry[0];
        Assert.Equal(EntityState.Modified, activityEntry.State);
        Assert.Equal("Replaced Activity", activityEntry.Property(nameof(ActivityWithCollection.Name)).CurrentValue);

        var championsEntry = activityEntry.ComplexCollection("Teams")[0];
        Assert.Equal(EntityState.Modified, championsEntry.State);
        Assert.Equal("Replaced Champions", championsEntry.Property(nameof(Team.Name)).CurrentValue);

        var runnersUpEntry = activityEntry.ComplexCollection("Teams")[1];
        Assert.Equal(EntityState.Modified, runnersUpEntry.State);
        Assert.Equal("Replaced Runners Up", runnersUpEntry.Property(nameof(Team.Name)).CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_duplicates_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities.Add(pub.Activities[0]);

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal(3, pub.Activities.Count);
        Assert.Equal(2, pub.Activities.Count(a => a.Name == pub.Activities[0].Name));
        Assert.Equal(EntityState.Unchanged, activitiesEntry[0].State);
        Assert.Equal(EntityState.Unchanged, activitiesEntry[1].State);
        Assert.Equal(EntityState.Added, activitiesEntry[2].State);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_handle_null_elements_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);
        pub.Activities.Clear();
        pub.Activities.Add(new ActivityWithCollection
        {
            Name = "Activity with null Teams",
            Day = DayOfWeek.Sunday,
            CoverCharge = 0.0m,
            Description = "Added after null",
            IsTeamBased = false,
            Notes = ["Note after null"],
            Teams = null!
        });

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities.Add(null!);

        pub.Activities.Add(new ActivityWithCollection
        {
            Name = "Activity with empty Teams",
            Day = DayOfWeek.Sunday,
            CoverCharge = 0.0m,
            Description = "Added with empty teams",
            IsTeamBased = false,
            Notes = ["Empty teams note"],
            Teams = []
        });

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal(3, pub.Activities.Count);
        Assert.Contains(null, pub.Activities);

        var activityWithNullTeams = pub.Activities.FirstOrDefault(a => a?.Name == "Activity with null Teams");
        Assert.NotNull(activityWithNullTeams);
        Assert.Null(activityWithNullTeams.Teams);

        var activityWithEmptyTeams = pub.Activities.FirstOrDefault(a => a?.Name == "Activity with empty Teams");
        Assert.NotNull(activityWithEmptyTeams);
        Assert.NotNull(activityWithEmptyTeams.Teams);
        Assert.Empty(activityWithEmptyTeams.Teams);

        var nullActivityEntry = activitiesEntry[1];
        Assert.Null(nullActivityEntry.CurrentValue);
        Assert.Equal(EntityState.Added, nullActivityEntry.State);
        var nullCoverChargeEntry = nullActivityEntry.Property(e => e.CoverCharge);
        Assert.Null(nullCoverChargeEntry.CurrentValue);
        Assert.Equal(
            CoreStrings.ComplexCollectionNullElementSetter(
                "PubWithCollections.Activities#ActivityWithCollection",
                nameof(ActivityWithCollection.CoverCharge),
                nameof(PubWithCollections),
                nameof(PubWithCollections.Activities),
                1),
            Assert.Throws<InvalidOperationException>(() => nullCoverChargeEntry.CurrentValue = 3.0m).Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_swapped_complex_objects_in_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        (pub.Activities[1], pub.Activities[0]) = (pub.Activities[0], pub.Activities[1]);

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }
        else
        {
            Assert.Equal(EntityState.Unchanged, activitiesEntry[0].State);
            Assert.Equal(EntityState.Unchanged, activitiesEntry[1].State);
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal("Music Quiz", activitiesEntry[0].Property("Name").CurrentValue);
        Assert.Equal("Pub Quiz", activitiesEntry[1].Property("Name").CurrentValue);

        (pub.Activities[1].Teams[1], pub.Activities[1].Teams[0]) = (pub.Activities[1].Teams[0], pub.Activities[1].Teams[1]);
        context.ChangeTracker.DetectChanges();

        var team1Entry = activitiesEntry[1].ComplexCollection("Teams")[0];
        var team2Entry = activitiesEntry[1].ComplexCollection("Teams")[1];
        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);
        Assert.Equal(EntityState.Unchanged, team1Entry.State);
        Assert.Equal(EntityState.Unchanged, team2Entry.State);
        Assert.Equal("ZZ", team1Entry.Property("Name").CurrentValue);
        Assert.Equal("Clueless", team2Entry.Property("Name").CurrentValue);
        Assert.Equal([1, 0], team1Entry.GetInfrastructure().GetOrdinals());
        Assert.Equal([1, 1], team2Entry.GetInfrastructure().GetOrdinals());
    }

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_changes_to_struct_collection_elements(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithStructCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        var activity = pub.Activities[0];
        activity.CoverCharge = 12.5m;
        pub.Activities[0] = activity;

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexProperty(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal(12.5m, pub.Activities[0].CoverCharge);
    }

    [ConditionalTheory(Skip = "Issue #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_changes_to_readonly_struct_collection_elements(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithReadonlyStructCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        var newTeams = new List<TeamReadonlyStruct>
        {
            pub.Activities[0].Teams[0],
            new TeamReadonlyStruct
            {
                Name = "New Readonly Team",
                Members = ["X", "Y", "Z"]
            }
        };

        pub.Activities[0] = new ActivityReadonlyStructWithCollection
        {
            Name = pub.Activities[0].Name,
            Day = pub.Activities[0].Day,
            Description = pub.Activities[0].Description,
            Notes = pub.Activities[0].Notes,
            CoverCharge = 15.0m,
            IsTeamBased = pub.Activities[0].IsTeamBased,
            Teams = newTeams
        };

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal(15.0m, pub.Activities[0].CoverCharge);
        Assert.Equal(2, pub.Activities[0].Teams.Count);
        Assert.Equal("New Readonly Team", pub.Activities[0].Teams[1].Name);
        Assert.Equal(new[] { "X", "Y", "Z" }, pub.Activities[0].Teams[1].Members);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_handle_collection_with_mixed_null_and_duplicate_elements(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        pub.Activities.Add(null!);
        pub.Activities.Add(new ActivityWithCollection
        {
            Name = pub.Activities[0].Name,
            Day = pub.Activities[0].Day,
            Description = pub.Activities[0].Description,
            Notes = pub.Activities[0].Notes?.ToArray(),
            CoverCharge = pub.Activities[0].CoverCharge,
            IsTeamBased = pub.Activities[0].IsTeamBased,
            Teams = null!
        });

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities[^1].CoverCharge = 99.99m;

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal(4, pub.Activities.Count);
        Assert.Contains(null, pub.Activities);
        Assert.Equal(2, pub.Activities.Count(a => a?.Name == pub.Activities[0].Name));

        var duplicate = pub.Activities.LastOrDefault(a => a?.Name == pub.Activities[0].Name);
        Assert.NotNull(duplicate);
        Assert.Equal(99.99m, duplicate.CoverCharge);
        Assert.Null(duplicate.Teams);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_changes_to_record_collection_elements(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithRecordCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);

        pub.Activities[0] = pub.Activities[0] with { CoverCharge = 20.0m };

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        var activityEntry = activitiesEntry[0];
        Assert.Equal(EntityState.Modified, activityEntry.State);
        Assert.Equal(20.0m, activityEntry.Property("CoverCharge").CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_nested_collection_changes_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities[0].Teams.Add(new Team { Name = "New Team", Members = { "New1", "New2" } });

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        var activityEntry = activitiesEntry[0];
        Assert.Equal(EntityState.Modified, activityEntry.State);

        var teamEntry = activityEntry.ComplexCollection("Teams")[2];
        Assert.Equal(EntityState.Added, teamEntry.State);
        Assert.Equal("New Team", teamEntry.Property("Name").CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_changes_to_nested_teams_members_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities[0].Teams[0].Members.Add("New Member");

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Contains("New Member", pub.Activities[0].Teams[0].Members);
    }

    [ConditionalTheory(Skip = "Issue #31411")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_changes_to_nested_struct_teams_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithStructCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        var teams = pub.Activities[0].Teams.ToList();
        teams[0] = new TeamStruct
        {
            Name = teams[0].Name,
            Members = [.. teams[0].Members, "Additional Member"]
        };
        var activity = pub.Activities[0];
        activity.Teams = teams;
        pub.Activities[0] = activity;

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexProperty(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Contains("Additional Member", pub.Activities[0].Teams[0].Members);
    }

    [ConditionalTheory(Skip = "Issue #31621")]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_changes_to_nested_readonly_struct_teams_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithReadonlyStructCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        var teams = pub.Activities[0].Teams.ToList();
        teams[0] = new TeamReadonlyStruct
        {
            Name = teams[0].Name,
            Members = [.. teams[0].Members, "Additional Member"]
        };

        pub.Activities[0] = new ActivityReadonlyStructWithCollection
        {
            Name = pub.Activities[0].Name,
            Day = pub.Activities[0].Day,
            Description = pub.Activities[0].Description,
            Notes = pub.Activities[0].Notes,
            CoverCharge = 15.0m,
            IsTeamBased = pub.Activities[0].IsTeamBased,
            Teams = teams
        };

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Contains("Additional Member", pub.Activities[0].Teams[0].Members);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_detect_changes_to_record_teams_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithRecordCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        var teams = pub.Activities[0].Teams.ToList();
        teams[0] = teams[0] with { Members = [.. teams[0].Members, "Additional Member"] };

        pub.Activities[0] = pub.Activities[0] with { Teams = teams };

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Contains("Additional Member", pub.Activities[0].Teams[0].Members);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_handle_empty_nested_teams_in_complex_type_collections(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = trackFromQuery ? TrackFromQuery(context, pub) : context.Attach(pub);
        Assert.Equal(EntityState.Unchanged, entry.State);

        pub.Activities.Add(new ActivityWithCollection
        {
            Name = "Activity with empty Teams",
            Day = DayOfWeek.Thursday,
            CoverCharge = 1.0m,
            Description = "Testing empty nested collections",
            IsTeamBased = false,
            Notes = ["Note"],
            Teams = []
        });

        context.ChangeTracker.DetectChanges();

        var activitiesEntry = entry.ComplexCollection(e => e.Activities);

        if (Fixture.UseProxies)
        {
            Assert.Equal(EntityState.Unchanged, entry.State);
            activitiesEntry.IsModified = true;
        }

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(activitiesEntry.IsModified);

        Assert.Equal(3, pub.Activities.Count);
        var emptyTeamsActivity = pub.Activities.FirstOrDefault(a => a.Name == "Activity with empty Teams");
        Assert.NotNull(emptyTeamsActivity);
        Assert.Empty(emptyTeamsActivity.Teams);
    }

    [ConditionalFact]
    public virtual void Throws_when_accessing_complex_entries_using_incorrect_cardinality()
    {
        using var context = CreateContext();
        var pub = CreatePubWithCollections(context);

        var entry = context.Attach(pub);

        Assert.Equal(CoreStrings.ComplexReferenceIsCollection(entry.Metadata.DisplayName(), nameof(PubWithCollections.Activities), "ComplexProperty", "ComplexCollection"),
            Assert.Throws<InvalidOperationException>(() => entry.ComplexProperty(e => e.Activities)).Message);

        Assert.Equal(CoreStrings.ComplexCollectionIsReference(entry.Metadata.DisplayName(), nameof(PubWithCollections.FeaturedTeam), "ComplexCollection", "ComplexProperty"),
            Assert.Throws<InvalidOperationException>(() => entry.ComplexCollection(e => (IList<Team>)e.FeaturedTeam)).Message);
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

    protected void AssertCollectionPropertyValues(EntityEntry entry)
    {
        Assert.Equal("The FBI", entry.Property("Name").CurrentValue);

        if (entry.State == EntityState.Deleted)
        {
            return;
        }

        var activitiesEntry = entry.ComplexCollection("Activities");
        var pubQuizActivity = activitiesEntry[0];
        var musicQuizActivity = activitiesEntry[1];

        Assert.NotNull(pubQuizActivity);
        Assert.Equal("Pub Quiz", pubQuizActivity.Property("Name").CurrentValue);
        Assert.Equal(DayOfWeek.Monday, pubQuizActivity.Property("Day").CurrentValue);
        Assert.Equal("A general knowledge pub quiz.", pubQuizActivity.Property("Description").CurrentValue);
        Assert.Equal(new[] { "One", "Two", "Three" }, pubQuizActivity.Property("Notes").CurrentValue);
        Assert.Equal(2.0m, pubQuizActivity.Property("CoverCharge").CurrentValue);
        Assert.True((bool)pubQuizActivity.Property("IsTeamBased").CurrentValue!);

        var teamsEntry = pubQuizActivity.ComplexCollection("Teams");
        var championsEntry = teamsEntry[0];
        Assert.Equal("Clueless", championsEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Boris", "David", "Theresa" }, championsEntry.Property("Members").CurrentValue);

        var runnersUpEntry = teamsEntry[1];
        Assert.Equal("ZZ", runnersUpEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Has Beard", "Has Beard", "Is Called Beard" }, runnersUpEntry.Property("Members").CurrentValue);

        Assert.NotNull(musicQuizActivity);
        Assert.Equal("Music Quiz", musicQuizActivity.Property("Name").CurrentValue);
        Assert.Equal(DayOfWeek.Friday, musicQuizActivity.Property("Day").CurrentValue);
        Assert.Equal("A music pub quiz.", musicQuizActivity.Property("Description").CurrentValue);
        Assert.Empty((IEnumerable<string>)musicQuizActivity.Property("Notes").CurrentValue!);
        Assert.Equal(5.0m, musicQuizActivity.Property("CoverCharge").CurrentValue);
        Assert.True((bool)musicQuizActivity.Property("IsTeamBased").CurrentValue!);

        teamsEntry = musicQuizActivity.ComplexCollection("Teams");
        championsEntry = teamsEntry[0];
        Assert.Equal("Dazed and Confused", championsEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Robert", "Jimmy", "John", "Jason" }, championsEntry.Property("Members").CurrentValue);

        runnersUpEntry = teamsEntry[1];
        Assert.Equal("Banksy", runnersUpEntry.Property("Name").CurrentValue);
        Assert.Empty((IEnumerable<string>)runnersUpEntry.Property("Members").CurrentValue!);

        var featuredTeamEntry = entry.ComplexProperty("FeaturedTeam");
        Assert.NotNull(featuredTeamEntry);
        Assert.Equal("Not In This Lifetime", featuredTeamEntry.Property("Name").CurrentValue);
        Assert.Equal(new[] { "Slash", "Axl" }, featuredTeamEntry.Property("Members").CurrentValue);
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

    protected void AssertCollectionPropertiesModified(EntityEntry entry, EntityState expectedState)
    {
        Assert.Equal("The FBI", entry.Property("Name").CurrentValue);

        var activitiesEntry = entry.ComplexCollection("Activities");
        Assert.NotNull(activitiesEntry);
        Assert.Equal(expectedState != EntityState.Unchanged, activitiesEntry.IsModified);

        if (entry.State == EntityState.Deleted)
        {
            return;
        }

        var pubQuizActivity = activitiesEntry[0];
        var musicQuizActivity = activitiesEntry[1];

        var expected = expectedState == EntityState.Modified;
        Assert.NotNull(pubQuizActivity);
        Assert.Equal(expected, pubQuizActivity.Property("Name").IsModified);
        Assert.Equal(expected, pubQuizActivity.Property("Day").IsModified);
        Assert.Equal(expected, pubQuizActivity.Property("Description").IsModified);
        Assert.Equal(expected, pubQuizActivity.Property("Notes").IsModified);
        Assert.Equal(expected, pubQuizActivity.Property("CoverCharge").IsModified);
        Assert.Equal(expected, pubQuizActivity.Property("IsTeamBased").IsModified);

        var teamsEntry = pubQuizActivity.ComplexCollection("Teams");
        Assert.Equal(expectedState != EntityState.Unchanged, teamsEntry.IsModified);

        var firstTeamEntry = teamsEntry[0];
        Assert.Equal(expected, firstTeamEntry.Property("Name").IsModified);
        Assert.Equal(expected, firstTeamEntry.Property("Members").IsModified);

        var secondTeamEntry = teamsEntry[1];
        Assert.Equal(expected, secondTeamEntry.Property("Name").IsModified);
        Assert.Equal(expected, secondTeamEntry.Property("Members").IsModified);

        Assert.NotNull(musicQuizActivity);
        Assert.Equal(expected, musicQuizActivity.Property("Name").IsModified);
        Assert.Equal(expected, musicQuizActivity.Property("Day").IsModified);
        Assert.Equal(expected, musicQuizActivity.Property("Description").IsModified);
        Assert.Equal(expected, musicQuizActivity.Property("Notes").IsModified);
        Assert.Equal(expected, musicQuizActivity.Property("CoverCharge").IsModified);
        Assert.Equal(expected, musicQuizActivity.Property("IsTeamBased").IsModified);

        teamsEntry = musicQuizActivity.ComplexCollection("Teams");
        Assert.Equal(expectedState != EntityState.Unchanged, teamsEntry.IsModified);

        firstTeamEntry = teamsEntry[0];
        Assert.Equal(expected, firstTeamEntry.Property("Name").IsModified);
        Assert.Equal(expected, firstTeamEntry.Property("Members").IsModified);

        secondTeamEntry = teamsEntry[1];
        Assert.Equal(expected, secondTeamEntry.Property("Name").IsModified);
        Assert.Equal(expected, secondTeamEntry.Property("Members").IsModified);

        var featuredTeamEntry = entry.ComplexProperty("FeaturedTeam");
        Assert.NotNull(featuredTeamEntry);
        Assert.Equal(expected, featuredTeamEntry.Property("Name").IsModified);
        Assert.Equal(expected, featuredTeamEntry.Property("Members").IsModified);
    }

    protected static EntityEntry<TEntity> TrackFromQuery<TEntity>(DbContext context, TEntity pub)
        where TEntity : class
        => new(
            context.GetService<IStateManager>().StartTrackingFromQuery(
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
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<PubWithStructs>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<PubWithReadonlyStructs>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<PubWithRecords>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.LunchtimeActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(
                        e => e.EveningActivity, b =>
                        {
                            b.ComplexProperty(e => e.Champions);
                            b.ComplexProperty(e => e.RunnersUp);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            modelBuilder.Entity<PubWithCollections>(
                b =>
                {
                    b.ComplexCollection(
                        e => e.Activities, b =>
                        {
                            b.ComplexCollection(e => e.Teams);
                        });
                    b.ComplexProperty(e => e.FeaturedTeam);
                });

            // TODO: Issue #31411
            //modelBuilder.Entity<PubWithStructCollections>(
            //    b =>
            //    {
            //        b.ComplexCollection(
            //            e => e.Activities, b =>
            //            {
            //                b.ComplexCollection(e => e.Teams);
            //            });
            //        b.ComplexProperty(e => e.FeaturedTeam);
            //    });

            // TODO: Allow binding of complex properties to constructors #31621
            //modelBuilder.Entity<PubWithReadonlyStructCollections>(
            //    b =>
            //    {
            //        b.ComplexCollection(
            //            e => e.Activities, b =>
            //            {
            //                b.ComplexCollection(e => e.Teams);
            //            });
            //        b.ComplexProperty(e => e.FeaturedTeam);
            //    });

            modelBuilder.Entity<PubWithRecordCollections>(
                b =>
                {
                    b.ComplexCollection(
                        e => e.Activities, b =>
                        {
                            b.ComplexCollection(e => e.Teams);
                        });
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
                                b.ComplexProperty(e => e.Champions);
                                b.ComplexProperty(e => e.RunnersUp);
                            });
                        b.ComplexProperty(
                            e => e.EveningActivity, b =>
                            {
                                b.ComplexProperty(e => e.Champions);
                                b.ComplexProperty(e => e.RunnersUp);
                            });
                        b.ComplexProperty(e => e.FeaturedTeam);
                    });

                modelBuilder.Entity<FieldPubWithStructs>(
                    b =>
                    {
                        b.ComplexProperty(
                            e => e.LunchtimeActivity, b =>
                            {
                                b.ComplexProperty(e => e.Champions);
                                b.ComplexProperty(e => e.RunnersUp);
                            });
                        b.ComplexProperty(
                            e => e.EveningActivity, b =>
                            {
                                b.ComplexProperty(e => e.Champions);
                                b.ComplexProperty(e => e.RunnersUp);
                            });
                        b.ComplexProperty(e => e.FeaturedTeam);
                    });

                // TODO: Allow binding of complex properties to constructors #31621
                //modelBuilder.Entity<FieldPubWithReadonlyStructs>(
                //    b =>
                //    {
                //        b.ComplexProperty(
                //            e => e.LunchtimeActivity, b =>
                //            {
                //                b.ComplexProperty(e => e!.Champions);
                //                b.ComplexProperty(e => e!.RunnersUp);
                //            });
                //        b.ComplexProperty(
                //            e => e.EveningActivity, b =>
                //            {
                //                b.ComplexProperty(e => e.Champions);
                //                b.ComplexProperty(e => e.RunnersUp);
                //            });
                //        b.ComplexProperty(e => e.FeaturedTeam);
                //    });

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
                    });

                modelBuilder.Entity<FieldPubWithCollections>(
                    b =>
                    {
                        b.ComplexCollection(
                            e => e.Activities, b =>
                            {
                                b.ComplexCollection(e => e.Teams);
                            });
                        b.ComplexProperty(e => e.FeaturedTeam);
                    });

                // TODO: Issue #31411
                //modelBuilder.Entity<FieldPubWithStructCollections>(
                //    b =>
                //    {
                //        b.ComplexCollection(
                //            e => e.Activities, b =>
                //            {
                //                b.ComplexCollection(e => e.Teams);
                //            });
                //        b.ComplexProperty(e => e.FeaturedTeam);
                //    });

                // TODO: Allow binding of complex properties to constructors #31621
                //modelBuilder.Entity<FieldPubWithReadonlyStructCollections>(
                //    b =>
                //    {
                //        b.ComplexCollection(
                //            e => e.Activities, b =>
                //            {
                //                b.ComplexCollection(e => e.Teams);
                //            });
                //        b.ComplexProperty(e => e.FeaturedTeam);
                //    });

                modelBuilder.Entity<FieldPubWithRecordCollections>(
                    b =>
                    {
                        b.ComplexCollection(
                            e => e.Activities, b =>
                            {
                                b.ComplexCollection(e => e.Teams);
                            });
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

    public class ActivityWithCollection
    {
        public string Name { get; set; } = null!;
        public decimal? CoverCharge { get; set; }
        public bool IsTeamBased { get; set; }
        public string? Description { get; set; }
        public string[]? Notes { get; set; }
        public DayOfWeek Day { get; set; }
        public List<Team> Teams { get; set; } = [];
    }

    public struct ActivityStructWithCollection
    {
        public string Name { get; set; }
        public decimal? CoverCharge { get; set; }
        public bool IsTeamBased { get; set; }
        public string? Description { get; set; }
        public string[]? Notes { get; set; }
        public DayOfWeek Day { get; set; }
        public List<TeamStruct> Teams { get; set; }
    }

    public readonly struct ActivityReadonlyStructWithCollection
    {
        public string Name { get; init; }
        public decimal? CoverCharge { get; init; }
        public bool IsTeamBased { get; init; }
        public string? Description { get; init; }
        public string[]? Notes { get; init; }
        public DayOfWeek Day { get; init; }
        public List<TeamReadonlyStruct> Teams { get; init; }
    }

    public record ActivityRecordWithCollection
    {
        public string Name { get; init; } = null!;
        public decimal? CoverCharge { get; init; }
        public bool IsTeamBased { get; init; }
        public string? Description { get; init; }
        public string[]? Notes { get; init; }
        public DayOfWeek Day { get; init; }
        public List<TeamRecord> Teams { get; init; } = null!;
    }

    public record FieldActivityRecordWithCollection
    {
        public string Name = null!;
        public decimal? CoverCharge;
        public bool IsTeamBased;
        public string? Description;
        public string[]? Notes;
        public DayOfWeek Day;
        public List<FieldTeamRecord> Teams = null!;
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
        public TeamReadonlyStruct(string name, List<string> members)
        {
            Name = name;
            Members = members;
        }

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

    public class PubWithCollections
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual List<ActivityWithCollection> Activities { get; set; } = [];
        public virtual Team FeaturedTeam { get; set; } = null!;
    }

    public class PubWithStructCollections
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        // TODO: Use ObservableList<T> #31621
        public virtual List<ActivityStructWithCollection> Activities { get; set; } = [];
        public virtual TeamStruct FeaturedTeam { get; set; }
    }

    public class PubWithReadonlyStructCollections
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        // TODO: Use ObservableList<T> #31621
        public virtual List<ActivityReadonlyStructWithCollection> Activities { get; set; } = [];
        public virtual TeamReadonlyStruct FeaturedTeam { get; set; }
    }

    public class PubWithRecordCollections
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public virtual List<ActivityRecordWithCollection> Activities { get; set; } = [];
        public virtual TeamRecord FeaturedTeam { get; set; } = null!;
    }

    public class FieldPubWithCollections
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public List<ActivityWithCollection> Activities = [];
        public Team FeaturedTeam = null!;
    }

    public class FieldPubWithStructCollections
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        // TODO: Use ObservableList<T> #31621
        public List<ActivityStructWithCollection> Activities = [];
        public TeamStruct FeaturedTeam;
    }

    public class FieldPubWithReadonlyStructCollections
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        // TODO: Use ObservableList<T> #31621
        public List<ActivityReadonlyStructWithCollection> Activities = [];
        public TeamReadonlyStruct FeaturedTeam;
    }

    public class FieldPubWithRecordCollections
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public List<FieldActivityRecordWithCollection> Activities = [];
        public FieldTeamRecord FeaturedTeam = null!;
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

    protected PubWithCollections CreatePubWithCollections(DbContext context)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<PubWithCollections>()
            : new PubWithCollections();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.Activities =
        [
            new ActivityWithCollection
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Monday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Teams =
            [
                new Team
                {
                    Name = "Clueless",
                    Members =
                    {
                        "Boris",
                        "David",
                        "Theresa"
                    }
                },
                new Team
                {
                    Name = "ZZ",
                    Members =
                    {
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    }
                }
            ]
        },
        new ActivityWithCollection
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Teams =
            [
                new Team
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
                new Team { Name = "Banksy", Members = [] }
            ]
        }
        ];

        pub.FeaturedTeam = new Team { Name = "Not In This Lifetime", Members = { "Slash", "Axl" } };

        return pub;
    }

    protected PubWithStructCollections CreatePubWithStructCollections(DbContext context)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<PubWithStructCollections>()
            : new PubWithStructCollections();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.Activities =
        [
            new ActivityStructWithCollection
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Monday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Teams =
            [
                new TeamStruct
                {
                    Name = "Clueless",
                    Members =
                    [
                        "Boris",
                        "David",
                        "Theresa"
                    ]
                },
                new TeamStruct
                {
                    Name = "ZZ",
                    Members =
                    [
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    ]
                }
            ]
        },
        new ActivityStructWithCollection(),
        new ActivityStructWithCollection
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Teams =
            [
                new TeamStruct
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
                new TeamStruct { Name = "Banksy", Members = [] }
            ]
        }
        ];

        pub.FeaturedTeam = new TeamStruct { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] };

        return pub;
    }

    protected PubWithReadonlyStructCollections CreatePubWithReadonlyStructCollections(DbContext context)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<PubWithReadonlyStructCollections>()
            : new PubWithReadonlyStructCollections();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.Activities =
        [
            new ActivityReadonlyStructWithCollection
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Monday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Teams =
            [
                new TeamReadonlyStruct
                {
                    Name = "Clueless",
                    Members =
                    [
                        "Boris",
                        "David",
                        "Theresa"
                    ]
                },
                new TeamReadonlyStruct
                {
                    Name = "ZZ",
                    Members =
                    [
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    ]
                }
            ]
        },
        new ActivityReadonlyStructWithCollection(),
        new ActivityReadonlyStructWithCollection
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Teams =
            [
                new TeamReadonlyStruct
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
                new TeamReadonlyStruct { Name = "Banksy", Members = [] }
            ]
        }
        ];

        pub.FeaturedTeam = new TeamReadonlyStruct { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] };

        return pub;
    }

    protected PubWithRecordCollections CreatePubWithRecordCollections(DbContext context)
    {
        var pub = Fixture.UseProxies
            ? context.CreateProxy<PubWithRecordCollections>()
            : new PubWithRecordCollections();

        pub.Id = Guid.NewGuid();
        pub.Name = "The FBI";

        pub.Activities =
        [
            new ActivityRecordWithCollection
        {
            Name = "Pub Quiz",
            Day = DayOfWeek.Monday,
            Description = "A general knowledge pub quiz.",
            Notes = ["One", "Two", "Three"],
            CoverCharge = 2.0m,
            IsTeamBased = true,
            Teams =
            [
                new TeamRecord
                {
                    Name = "Clueless",
                    Members =
                    [
                        "Boris",
                        "David",
                        "Theresa"
                    ]
                },
                new TeamRecord
                {
                    Name = "ZZ",
                    Members =
                    [
                        "Has Beard",
                        "Has Beard",
                        "Is Called Beard"
                    ]
                }
            ]
        },
        new ActivityRecordWithCollection
        {
            Name = "Music Quiz",
            Day = DayOfWeek.Friday,
            Description = "A music pub quiz.",
            Notes = [],
            CoverCharge = 5.0m,
            IsTeamBased = true,
            Teams =
            [
                new TeamRecord
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
                new TeamRecord { Name = "Banksy", Members = [] }
            ]
        }
        ];

        pub.FeaturedTeam = new TeamRecord { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] };

        return pub;
    }

    protected static FieldPubWithCollections CreateFieldCollectionPub(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            Activities =
            [
                new ActivityWithCollection
                {
                    Name = "Pub Quiz",
                    Day = DayOfWeek.Monday,
                    Description = "A general knowledge pub quiz.",
                    Notes = ["One", "Two", "Three"],
                    CoverCharge = 2.0m,
                    IsTeamBased = true,
                    Teams =
                    [
                        new Team
                        {
                            Name = "Clueless",
                            Members =
                            {
                                "Boris",
                                "David",
                                "Theresa"
                            }
                        },
                        new Team
                        {
                            Name = "ZZ",
                            Members =
                            {
                                "Has Beard",
                                "Has Beard",
                                "Is Called Beard"
                            }
                        }
                    ]
                },
                new ActivityWithCollection
                {
                    Name = "Music Quiz",
                    Day = DayOfWeek.Friday,
                    Description = "A music pub quiz.",
                    Notes = [],
                    CoverCharge = 5.0m,
                    IsTeamBased = true,
                    Teams =
                    [
                        new Team
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
                        new Team { Name = "Banksy", Members = [] }
                    ]
                }
            ],
            FeaturedTeam = new Team
            {
                Name = "Not In This Lifetime",
                Members = { "Slash", "Axl" }
            }
        };

    protected static FieldPubWithStructCollections CreateFieldCollectionPubWithStructs(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            Activities =
            [
                new ActivityStructWithCollection
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = ["One", "Two", "Three"],
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Teams =
                [
                    new TeamStruct
                    {
                        Name = "Clueless",
                        Members =
                        [
                            "Boris",
                            "David",
                            "Theresa"
                        ]
                    },
                    new TeamStruct
                    {
                        Name = "ZZ",
                        Members =
                        [
                            "Has Beard",
                            "Has Beard",
                            "Is Called Beard"
                        ]
                    }
                ]
            },
            new ActivityStructWithCollection(),
            new ActivityStructWithCollection
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = [],
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Teams =
                [
                    new TeamStruct
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
                    new TeamStruct { Name = "Banksy", Members = [] }
                ]
            }
            ],
            FeaturedTeam = new TeamStruct { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] }
        };

    protected static FieldPubWithRecordCollections CreateFieldCollectionPubWithRecords(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            Activities =
            [
                new FieldActivityRecordWithCollection
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = ["One", "Two", "Three"],
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Teams =
                [
                    new FieldTeamRecord
                    {
                        Name = "Clueless",
                        Members =
                        [
                            "Boris",
                            "David",
                            "Theresa"
                        ]
                    },
                    new FieldTeamRecord
                    {
                        Name = "ZZ",
                        Members =
                        [
                            "Has Beard",
                            "Has Beard",
                            "Is Called Beard"
                        ]
                    }
                ]
            },
            new FieldActivityRecordWithCollection
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = [],
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Teams =
                [
                    new FieldTeamRecord
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
                    new FieldTeamRecord { Name = "Banksy", Members = [] }
                ]
            }
            ],
            FeaturedTeam = new FieldTeamRecord { Name = "Not In This Lifetime", Members = ["Slash", "Axl"] }
        };

    protected static FieldPubWithReadonlyStructCollections CreateFieldCollectionPubWithReadonlyStructs(DbContext context)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "The FBI",
            Activities =
            [
                new ActivityReadonlyStructWithCollection
            {
                Name = "Pub Quiz",
                Day = DayOfWeek.Monday,
                Description = "A general knowledge pub quiz.",
                Notes = ["One", "Two", "Three"],
                CoverCharge = 2.0m,
                IsTeamBased = true,
                Teams =
                [
                    new TeamReadonlyStruct("Clueless", ["Boris", "David", "Theresa"]),
                    new TeamReadonlyStruct("ZZ", ["Has Beard", "Has Beard", "Is Called Beard"])
                ]
            },
            new ActivityReadonlyStructWithCollection(),
            new ActivityReadonlyStructWithCollection
            {
                Name = "Music Quiz",
                Day = DayOfWeek.Friday,
                Description = "A music pub quiz.",
                Notes = [],
                CoverCharge = 5.0m,
                IsTeamBased = true,
                Teams =
                [
                    new TeamReadonlyStruct("Dazed and Confused", ["Robert", "Jimmy", "John", "Jason"]),
                    new TeamReadonlyStruct("Banksy", [])
                ]
            }
            ],
            FeaturedTeam = new TeamReadonlyStruct("Not In This Lifetime", ["Slash", "Axl"])
        };
}
