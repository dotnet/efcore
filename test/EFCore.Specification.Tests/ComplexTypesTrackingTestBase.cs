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
    public virtual async Task Can_track_entity_with_complex_objects(EntityState state, bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var pub = CreatePub();

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

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_mark_complex_type_properties_modified(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePub();

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

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_read_original_values_for_properties_of_complex_types(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePub();

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
        Assert.Equal(3.0m, ReadCurrentValue<decimal>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadOriginalValue<decimal>(entry, "EveningActivity.CoverCharge"));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Can_write_original_values_for_properties_of_complex_types(bool trackFromQuery)
    {
        using var context = CreateContext();
        var pub = CreatePub();

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
        Assert.Equal(5.0m, ReadCurrentValue<decimal>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(3.0m, ReadOriginalValue<decimal>(entry, "EveningActivity.CoverCharge"));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Detect_changes_detects_changes_in_complex_type_properties(bool trackFromQuery)
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
        Assert.Equal(3.0m, ReadCurrentValue<decimal>(entry, "EveningActivity.CoverCharge"));
        Assert.Equal(5.0m, ReadOriginalValue<decimal>(entry, "EveningActivity.CoverCharge"));
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
                    Name = "Banksy"
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

    protected void AssertPropertyValues(EntityEntry<Pub> entry)
    {
        Assert.Equal("The FBI", ReadCurrentValue<string>(entry, "Name"));
        Assert.NotNull(ReadCurrentValue<object?>(entry, "LunchtimeActivity"));
        Assert.Equal("Pub Quiz", ReadCurrentValue<string>(entry, "LunchtimeActivity.Name"));
        Assert.Equal(DayOfWeek.Monday, ReadCurrentValue<DayOfWeek>(entry, "LunchtimeActivity.Day"));
        Assert.Equal("A general knowledge pub quiz.", ReadCurrentValue<string>(entry, "LunchtimeActivity.Description"));
        Assert.Equal(new[] { "One", "Two", "Three" }, ReadCurrentValue<string[]>(entry, "LunchtimeActivity.Notes"));
        Assert.Equal(2.0m, ReadCurrentValue<decimal>(entry, "LunchtimeActivity.CoverCharge"));
        Assert.True(ReadCurrentValue<bool>(entry, "LunchtimeActivity.IsTeamBased"));
        Assert.NotNull(ReadCurrentValue<object?>(entry, "LunchtimeActivity.Champions"));
        Assert.Equal("Clueless", ReadCurrentValue<string>(entry, "LunchtimeActivity.Champions.Name"));
        Assert.Equal(new[] { "Boris", "David", "Theresa" }, ReadCurrentValue<List<string>>(entry, "LunchtimeActivity.Champions.Members"));
        Assert.NotNull(ReadCurrentValue<object?>(entry, "LunchtimeActivity.RunnersUp"));
        Assert.Equal("ZZ", ReadCurrentValue<string>(entry, "LunchtimeActivity.RunnersUp.Name"));
        Assert.Equal(
            new[] { "Has Beard", "Has Beard", "Is Called Beard" },
            ReadCurrentValue<List<string>>(entry, "LunchtimeActivity.RunnersUp.Members"));
        Assert.NotNull(ReadCurrentValue<object?>(entry, "EveningActivity"));
        Assert.Equal("Music Quiz", ReadCurrentValue<string>(entry, "EveningActivity.Name"));
        Assert.Equal(DayOfWeek.Friday, ReadCurrentValue<DayOfWeek>(entry, "EveningActivity.Day"));
        Assert.Equal("A music pub quiz.", ReadCurrentValue<string>(entry, "EveningActivity.Description"));
        Assert.Empty(ReadCurrentValue<string[]>(entry, "EveningActivity.Notes"));
        Assert.Equal(5.0m, ReadCurrentValue<decimal>(entry, "EveningActivity.CoverCharge"));
        Assert.True(ReadCurrentValue<bool>(entry, "EveningActivity.IsTeamBased"));
        Assert.NotNull(ReadCurrentValue<object?>(entry, "EveningActivity.Champions"));
        Assert.Equal("Dazed and Confused", ReadCurrentValue<string>(entry, "EveningActivity.Champions.Name"));
        Assert.Equal(
            new[] { "Robert", "Jimmy", "John", "Jason" }, ReadCurrentValue<List<string>>(entry, "EveningActivity.Champions.Members"));
        Assert.NotNull(ReadCurrentValue<object?>(entry, "EveningActivity.RunnersUp"));
        Assert.Equal("Banksy", ReadCurrentValue<string>(entry, "EveningActivity.RunnersUp.Name"));
        Assert.Empty(ReadCurrentValue<List<string>>(entry, "EveningActivity.RunnersUp.Members"));
        Assert.NotNull(ReadCurrentValue<object?>(entry, "FeaturedTeam"));
        Assert.Equal("Not In This Lifetime", ReadCurrentValue<string>(entry, "FeaturedTeam.Name"));
        Assert.Equal(new[] { "Slash", "Axl" }, ReadCurrentValue<List<string>>(entry, "FeaturedTeam.Members"));
    }

    protected void AssertPropertiesModified(EntityEntry<Pub> entry, bool expected)
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

    protected static EntityEntry<Pub> TrackFromQuery(DbContext context, Pub pub)
        => new(
            context.GetService<IStateManager>().StartTrackingFromQuery(
                context.Model.FindEntityType(typeof(Pub))!, pub, new ValueBuffer()));

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
        }
    }

    protected class Pub
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Activity LunchtimeActivity { get; set; } = null!;
        public Activity EveningActivity { get; set; } = null!;
        public Team FeaturedTeam { get; set; } = null!;
        // Complex collections:
        // public List<Activity> Activities { get; set; } = null!;
        // public List<Team>? Teams { get; set; }
    }

    protected class Activity
    {
        public string Name { get; set; } = null!;
        public decimal CoverCharge { get; set; }
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
}
