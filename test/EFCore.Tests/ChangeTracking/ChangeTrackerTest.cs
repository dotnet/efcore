// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    public class ChangeTrackerTest
    {
        [ConditionalFact]
        public void Change_tracker_can_be_cleared()
        {
            Seed();

            using var context = new LikeAZooContext();

            var cats = context.Cats.ToList();
            var hats = context.Set<Hat>().ToList();

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(cats[0]).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(hats[0]).State);

            context.ChangeTracker.Clear();

            Assert.Empty(context.ChangeTracker.Entries());
            Assert.Equal(EntityState.Detached, context.Entry(cats[0]).State);
            Assert.Equal(EntityState.Detached, context.Entry(hats[0]).State);

            var catsAgain = context.Cats.ToList();
            var hatsAgain = context.Set<Hat>().ToList();

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(catsAgain[0]).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(hatsAgain[0]).State);

            Assert.Equal(EntityState.Detached, context.Entry(cats[0]).State);
            Assert.Equal(EntityState.Detached, context.Entry(hats[0]).State);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Keys_generated_on_behalf_of_a_principal_are_not_saved(bool async)
        {
            using var context = new WeakHerosContext();

            var entity = new Weak { Id = Guid.NewGuid() };

            if (async)
            {
                await context.AddAsync(entity);
            }
            else
            {
                context.Add(entity);
            }

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(
                CoreStrings.UnknownKeyValue(nameof(Weak), nameof(Weak.HeroId)),
                Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
        }

        public class Hero
        {
            public Guid Id { get; set; }
            public ICollection<Weak> Weaks { get; set; }
        }

        public class Weak
        {
            public Guid Id { get; set; }
            public Guid HeroId { get; set; }

            public Hero Hero { get; set; }
        }

        public class WeakHerosContext : DbContext
        {
            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Weak>(
                    b =>
                    {
                        b.HasKey(e => new { e.Id, e.HeroId });
                        b.HasOne(e => e.Hero).WithMany(e => e.Weaks).HasForeignKey(e => e.HeroId);
                    });

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(nameof(WeakHerosContext));
        }

        [ConditionalFact]
        public void DetectChanges_is_logged()
        {
            Seed();

            using var context = new LikeAZooContext();
            _loggerFactory.Log.Clear();

            context.SaveChanges();

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.DetectChangesStarting.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                CoreResources.LogDetectChangesStarting(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(LikeAZooContext)), message);

            (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.DetectChangesCompleted.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                CoreResources.LogDetectChangesCompleted(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(LikeAZooContext)), message);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Detect_property_change_is_logged(bool sensitive, bool callDetectChangesTwice)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            var cat = context.Cats.Find(1);

            _loggerFactory.Log.Clear();

            cat.Name = "Smoke-a-doke";

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogPropertyChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(Cat), nameof(Cat.Name), "Smokey", "Smoke-a-doke", "{Id: 1}")
                    : CoreResources.LogPropertyChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Cat), nameof(Cat.Name)),
                message);

            _loggerFactory.Log.Clear();

            cat.Name = "Little Artichoke";

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));
        }

        [ConditionalTheory] // Issue #21896
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Property_changes_on_Deleted_entities_are_not_continually_detected(bool sensitive, bool callDetectChangesTwice)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            var cat = context.Cats.Find(1);

            _loggerFactory.Log.Clear();

            context.Entry(cat).State = EntityState.Deleted;

            cat.Name = "Smoke-a-doke";

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));

            _loggerFactory.Log.Clear();

            cat.Name = "Little Artichoke";

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Detect_foreign_key_property_change_is_logged(bool sensitive, bool callDetectChangesTwice)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

            _loggerFactory.Log.Clear();

            var hat = cat.Hats.Single(h => h.Id == 77);
            hat.CatId = 2;

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ForeignKeyChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogForeignKeyChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.CatId), 1, 2, "{Id: 77}")
                    : CoreResources.LogForeignKeyChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.CatId)),
                message);

            _loggerFactory.Log.Clear();

            hat.CatId = 1;

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ForeignKeyChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogForeignKeyChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.CatId), 2, 1, "{Id: 77}")
                    : CoreResources.LogForeignKeyChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.CatId)),
                message);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Detect_collection_change_is_logged(bool sensitive, bool callDetectChangesTwice)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);
            var hat = cat.Hats.Single(h => h.Id == 77);

            _loggerFactory.Log.Clear();

            cat.Hats.Clear();

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CollectionChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Hats), "{Id: 1}")
                    : CoreResources.LogCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Hats)),
                message);

            _loggerFactory.Log.Clear();

            cat.Hats.Add(hat);

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CollectionChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Hats), "{Id: 1}")
                    : CoreResources.LogCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Hats)),
                message);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Detect_skip_collection_change_is_logged(bool sensitive, bool callDetectChangesTwice)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            var cat = context.Cats.Include(e => e.Mats).Single(e => e.Id == 1);
            var mat = cat.Mats.Single(h => h.Id == 77);

            _loggerFactory.Log.Clear();

            cat.Mats.Clear();

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SkipCollectionChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogSkipCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Mats), "{Id: 1}")
                    : CoreResources.LogSkipCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Mats)),
                message);

            _loggerFactory.Log.Clear();

            cat.Mats.Add(mat);

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SkipCollectionChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogSkipCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Mats), "{Id: 1}")
                    : CoreResources.LogSkipCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Mats)),
                message);
        }

        [ConditionalTheory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void Detect_reference_change_is_logged(bool sensitive, bool callDetectChangesTwice)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);
            var hat = cat.Hats.Single(h => h.Id == 77);

            _loggerFactory.Log.Clear();

            hat.Cat = null;

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ReferenceChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogReferenceChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.Cat), "{Id: 77}")
                    : CoreResources.LogReferenceChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.Cat)),
                message);

            _loggerFactory.Log.Clear();

            hat.Cat = cat;

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ReferenceChangeDetected.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogReferenceChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.Cat), "{Id: 77}")
                    : CoreResources.LogReferenceChangeDetected(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), nameof(Hat.Cat)),
                message);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Start_tracking_is_logged_from_query(bool sensitive)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            _loggerFactory.Log.Clear();
            context.Cats.Find(1);

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StartedTracking.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogStartedTrackingSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(LikeAZooContextSensitive), nameof(Cat), "{Id: 1}")
                    : CoreResources.LogStartedTracking(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(LikeAZooContext), nameof(Cat)),
                message);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Start_tracking_is_logged_from_attach(bool sensitive)
        {
            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            _loggerFactory.Log.Clear();
            context.Attach(new Hat(88));

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StartedTracking.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogStartedTrackingSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(LikeAZooContextSensitive), nameof(Hat), "{Id: 88}")
                    : CoreResources.LogStartedTracking(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(LikeAZooContext), nameof(Hat)),
                message);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void State_change_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            var cat = context.Cats.Find(1);

            _loggerFactory.Log.Clear();

            context.Entry(cat).State = EntityState.Deleted;

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StateChanged.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogStateChangedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(Cat), "{Id: 1}", nameof(LikeAZooContextSensitive), EntityState.Unchanged, EntityState.Deleted)
                    : CoreResources.LogStateChanged(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(Cat), nameof(LikeAZooContext), EntityState.Unchanged, EntityState.Deleted),
                message);
        }

        [ConditionalTheory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public async Task Value_generation_is_logged(bool sensitive, bool async, bool temporary)
        {
            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            ResetValueGenerator(
                context,
                context.Model.FindEntityType(typeof(Hat)).FindProperty(nameof(Hat.Id)),
                temporary);

            _loggerFactory.Log.Clear();

            if (async)
            {
                context.Add(new Hat(0));
            }
            else
            {
                await context.AddAsync(new Hat(0));
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ValueGenerated.Id);
            Assert.Equal(LogLevel.Debug, level);

            if (temporary)
            {
                Assert.Equal(
                    sensitive
                        ? CoreResources.LogTempValueGeneratedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            nameof(LikeAZooContextSensitive), 1, nameof(Hat.Id), nameof(Hat))
                        : CoreResources.LogTempValueGenerated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            nameof(LikeAZooContext), nameof(Hat.Id), nameof(Hat)),
                    message);
            }
            else
            {
                Assert.Equal(
                    sensitive
                        ? CoreResources.LogValueGeneratedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            nameof(LikeAZooContextSensitive), 1, nameof(Hat.Id), nameof(Hat))
                        : CoreResources.LogValueGenerated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            nameof(LikeAZooContext), nameof(Hat.Id), nameof(Hat)),
                    message);
            }
        }

        private static void ResetValueGenerator(DbContext context, IProperty property, bool generateTemporaryValues)
        {
            var cache = context.GetService<IValueGeneratorCache>();

            var generator = (ResettableValueGenerator)cache.GetOrAdd(
                property,
                property.DeclaringEntityType,
                (p, e) => new ResettableValueGenerator());

            generator.Reset(generateTemporaryValues);
        }

        private class ResettableValueGenerator : ValueGenerator<int>
        {
            private int _current;
            private bool _generatesTemporaryValues;

            public override bool GeneratesTemporaryValues
                => _generatesTemporaryValues;

            public override int Next(EntityEntry entry)
                => Interlocked.Increment(ref _current);

            public void Reset(bool generateTemporaryValues)
            {
                _generatesTemporaryValues = generateTemporaryValues;
                _current = 0;
            }
        }

        [ConditionalTheory]
        [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
        [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
        [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
        [InlineData(false, CascadeTiming.OnSaveChanges, null)]
        [InlineData(false, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
        [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Immediate)]
        [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Never)]
        [InlineData(false, CascadeTiming.Immediate, null)]
        [InlineData(false, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
        [InlineData(false, CascadeTiming.Never, CascadeTiming.Immediate)]
        [InlineData(false, CascadeTiming.Never, CascadeTiming.Never)]
        [InlineData(false, CascadeTiming.Never, null)]
        [InlineData(false, null, CascadeTiming.OnSaveChanges)]
        [InlineData(false, null, CascadeTiming.Immediate)]
        [InlineData(false, null, CascadeTiming.Never)]
        [InlineData(false, null, null)]
        [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
        [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
        [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
        [InlineData(true, CascadeTiming.OnSaveChanges, null)]
        [InlineData(true, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
        [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Immediate)]
        [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Never)]
        [InlineData(true, CascadeTiming.Immediate, null)]
        [InlineData(true, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
        [InlineData(true, CascadeTiming.Never, CascadeTiming.Immediate)]
        [InlineData(true, CascadeTiming.Never, CascadeTiming.Never)]
        [InlineData(true, CascadeTiming.Never, null)]
        [InlineData(true, null, CascadeTiming.OnSaveChanges)]
        [InlineData(true, null, CascadeTiming.Immediate)]
        [InlineData(true, null, CascadeTiming.Never)]
        [InlineData(true, null, null)]
        public void Cascade_delete_is_logged(
            bool sensitive,
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            if (cascadeDeleteTiming.HasValue)
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming.Value;
            }

            if (deleteOrphansTiming.HasValue)
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming.Value;
            }

            var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

            LogLevel? cascadeDeleteLevel = null;
            string cascadeDeleteMessage = null;
            string deleteOrphansMessage = null;

            void CaptureMessages()
            {
                (cascadeDeleteLevel, _, cascadeDeleteMessage, _, _) =
                    _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDelete.Id);
                (_, _, deleteOrphansMessage, _, _) =
                    _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDeleteOrphan.Id);
            }

            void ClearMessages()
            {
                _loggerFactory.Log.Clear();
            }

            switch (cascadeDeleteTiming)
            {
                case CascadeTiming.Immediate:
                case null:
                    ClearMessages();

                    context.Entry(cat).State = EntityState.Deleted;

                    CaptureMessages();

                    context.SaveChanges();
                    break;
                case CascadeTiming.OnSaveChanges:
                    context.Entry(cat).State = EntityState.Deleted;

                    ClearMessages();

                    context.SaveChanges();

                    CaptureMessages();
                    break;
                case CascadeTiming.Never:
                    ClearMessages();

                    context.Entry(cat).State = EntityState.Deleted;

                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

                    CaptureMessages();
                    break;
            }

            Assert.Null(deleteOrphansMessage);

            if (cascadeDeleteTiming == CascadeTiming.Never)
            {
                Assert.Null(cascadeDeleteMessage);
            }
            else
            {
                Assert.Equal(LogLevel.Debug, cascadeDeleteLevel);
                Assert.Equal(
                    sensitive
                        ? CoreResources.LogCascadeDeleteSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            nameof(Hat), "{Id: 77}", EntityState.Deleted, nameof(Cat), "{Id: 1}")
                        : CoreResources.LogCascadeDelete(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            nameof(Hat), EntityState.Deleted, nameof(Cat)),
                    cascadeDeleteMessage);
            }
        }

        [ConditionalTheory]
        [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
        [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
        [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
        [InlineData(false, CascadeTiming.OnSaveChanges, null)]
        [InlineData(false, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
        [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Immediate)]
        [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Never)]
        [InlineData(false, CascadeTiming.Immediate, null)]
        [InlineData(false, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
        [InlineData(false, CascadeTiming.Never, CascadeTiming.Immediate)]
        [InlineData(false, CascadeTiming.Never, CascadeTiming.Never)]
        [InlineData(false, CascadeTiming.Never, null)]
        [InlineData(false, null, CascadeTiming.OnSaveChanges)]
        [InlineData(false, null, CascadeTiming.Immediate)]
        [InlineData(false, null, CascadeTiming.Never)]
        [InlineData(false, null, null)]
        [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
        [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
        [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
        [InlineData(true, CascadeTiming.OnSaveChanges, null)]
        [InlineData(true, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
        [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Immediate)]
        [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Never)]
        [InlineData(true, CascadeTiming.Immediate, null)]
        [InlineData(true, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
        [InlineData(true, CascadeTiming.Never, CascadeTiming.Immediate)]
        [InlineData(true, CascadeTiming.Never, CascadeTiming.Never)]
        [InlineData(true, CascadeTiming.Never, null)]
        [InlineData(true, null, CascadeTiming.OnSaveChanges)]
        [InlineData(true, null, CascadeTiming.Immediate)]
        [InlineData(true, null, CascadeTiming.Never)]
        [InlineData(true, null, null)]
        public void Cascade_delete_orphan_is_logged(
            bool sensitive,
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            Seed(sensitive);

            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            if (cascadeDeleteTiming.HasValue)
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming.Value;
            }

            if (deleteOrphansTiming.HasValue)
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming.Value;
            }

            var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

            LogLevel? deleteOrphansLevel = null;
            string cascadeDeleteMessage = null;
            string deleteOrphansMessage = null;

            void CaptureMessages()
            {
                (_, _, cascadeDeleteMessage, _, _) = _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDelete.Id);
                (deleteOrphansLevel, _, deleteOrphansMessage, _, _) =
                    _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDeleteOrphan.Id);
            }

            void ClearMessages()
            {
                _loggerFactory.Log.Clear();
            }

            switch (deleteOrphansTiming)
            {
                case CascadeTiming.Immediate:
                case null:
                    ClearMessages();

                    cat.Hats.Clear();
                    context.ChangeTracker.DetectChanges();

                    CaptureMessages();

                    context.SaveChanges();
                    break;
                case CascadeTiming.OnSaveChanges:
                    cat.Hats.Clear();
                    context.ChangeTracker.DetectChanges();

                    ClearMessages();

                    context.SaveChanges();

                    CaptureMessages();
                    break;
                case CascadeTiming.Never:
                    ClearMessages();

                    cat.Hats.Clear();
                    context.ChangeTracker.DetectChanges();

                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

                    CaptureMessages();
                    break;
            }

            Assert.Null(cascadeDeleteMessage);

            if (deleteOrphansTiming == CascadeTiming.Never)
            {
                Assert.Null(deleteOrphansMessage);
            }
            else
            {
                Assert.Equal(LogLevel.Debug, deleteOrphansLevel);
                Assert.Equal(
                    sensitive
                        ? CoreResources.LogCascadeDeleteOrphanSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                            nameof(Hat), "{Id: 77}", EntityState.Deleted, nameof(Cat))
                        : CoreResources.LogCascadeDeleteOrphan(new TestLogger<TestLoggingDefinitions>())
                            .GenerateMessage(nameof(Hat), EntityState.Deleted, nameof(Cat)),
                    deleteOrphansMessage);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SaveChanges_is_logged(bool async)
        {
            Seed();

            using var context = new LikeAZooContext();
            var cat = context.Cats.Find(1);

            context.Entry(cat).State = EntityState.Deleted;

            _loggerFactory.Log.Clear();

            if (async)
            {
                await context.SaveChangesAsync();
            }
            else
            {
                context.SaveChanges();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SaveChangesStarting.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                CoreResources.LogSaveChangesStarting(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(nameof(LikeAZooContext)),
                message);

            (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SaveChangesCompleted.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                CoreResources.LogSaveChangesCompleted(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(LikeAZooContext), 1), message);
        }

        [ConditionalFact]
        public void Context_Dispose_is_logged()
        {
            using (var context = new LikeAZooContext())
            {
                context.Cats.Find(1);

                _loggerFactory.Log.Clear();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ContextDisposed.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(
                CoreResources.LogContextDisposed(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(nameof(LikeAZooContext)),
                message);
        }

        [ConditionalFact]
        public void State_change_events_fire_from_query()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            Seed(usePool: true);

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

                Assert.Equal(2, tracked.Count);
                Assert.Empty(changed);

                AssertTrackedEvent(context, 1, EntityState.Unchanged, tracked[0], fromQuery: true);
                AssertTrackedEvent(context, 2, EntityState.Unchanged, tracked[1], fromQuery: true);
            }

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

                Assert.Equal(2, tracked.Count);
                Assert.Empty(changed);
            }
        }

        [ConditionalFact]
        public void State_change_events_fire_from_Attach()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using var scope = _poolProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

            RegisterEvents(context, tracked, changed);

            context.Attach(new Cat(1));

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Single(tracked);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 1, EntityState.Unchanged, tracked[0], fromQuery: false);

            context.Entry(new Cat(2)).State = EntityState.Unchanged;

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(2, tracked.Count);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 2, EntityState.Unchanged, tracked[1], fromQuery: false);
        }

        [ConditionalFact]
        public void State_change_events_fire_from_Add()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using var scope = _poolProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

            RegisterEvents(context, tracked, changed);

            context.Add(new Cat(1));

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Single(tracked);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 1, EntityState.Added, tracked[0], fromQuery: false);

            context.Entry(new Cat(2)).State = EntityState.Added;

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(2, tracked.Count);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 2, EntityState.Added, tracked[1], fromQuery: false);
        }

        [ConditionalFact]
        public void State_change_events_fire_from_Update()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using var scope = _poolProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

            RegisterEvents(context, tracked, changed);

            context.Update(new Cat(1));

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Single(tracked);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 1, EntityState.Modified, tracked[0], fromQuery: false);

            context.Entry(new Cat(2)).State = EntityState.Modified;

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(2, tracked.Count);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 2, EntityState.Modified, tracked[1], fromQuery: false);
        }

        [ConditionalFact]
        public void State_change_events_fire_for_tracked_state_changes()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                context.AddRange(new Cat(1), new Cat(2));

                Assert.True(context.ChangeTracker.HasChanges());

                Assert.Equal(2, tracked.Count);
                Assert.Empty(changed);

                AssertTrackedEvent(context, 1, EntityState.Added, tracked[0], fromQuery: false);
                AssertTrackedEvent(context, 2, EntityState.Added, tracked[1], fromQuery: false);

                context.Entry(context.Cats.Find(1)).State = EntityState.Unchanged;
                context.Entry(context.Cats.Find(2)).State = EntityState.Modified;

                Assert.Equal(2, tracked.Count);
                Assert.Equal(2, changed.Count);

                Assert.True(context.ChangeTracker.HasChanges());

                AssertChangedEvent(context, 1, EntityState.Added, EntityState.Unchanged, changed[0]);
                AssertChangedEvent(context, 2, EntityState.Added, EntityState.Modified, changed[1]);

                context.Entry(context.Cats.Find(1)).State = EntityState.Added;
                context.Entry(context.Cats.Find(2)).State = EntityState.Deleted;

                Assert.Equal(2, tracked.Count);
                Assert.Equal(4, changed.Count);

                AssertChangedEvent(context, 1, EntityState.Unchanged, EntityState.Added, changed[2]);
                AssertChangedEvent(context, 2, EntityState.Modified, EntityState.Deleted, changed[3]);

                context.Remove(context.Cats.Find(1));
                context.Entry(context.Cats.Find(2)).State = EntityState.Detached;

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(2, tracked.Count);
                Assert.Equal(6, changed.Count);

                AssertChangedEvent(context, null, EntityState.Added, EntityState.Detached, changed[4]);
                AssertChangedEvent(context, null, EntityState.Deleted, EntityState.Detached, changed[5]);
            }

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                context.AddRange(new Cat(1), new Cat(2));

                context.Entry(context.Cats.Find(1)).State = EntityState.Unchanged;
                context.Entry(context.Cats.Find(2)).State = EntityState.Modified;

                context.Entry(context.Cats.Find(1)).State = EntityState.Added;
                context.Entry(context.Cats.Find(2)).State = EntityState.Deleted;

                context.Remove(context.Cats.Find(1));
                context.Entry(context.Cats.Find(2)).State = EntityState.Detached;

                Assert.Equal(2, tracked.Count);
                Assert.Equal(6, changed.Count);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void State_change_events_fire_when_saving_changes(bool callDetectChangesTwice)
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            Seed(usePool: true);

            using var scope = _poolProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

            RegisterEvents(context, tracked, changed);

            var cat1 = context.Cats.Find(1);

            Assert.Single(tracked);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 1, EntityState.Unchanged, tracked[0], fromQuery: true);

            context.Add(new Cat(3));
            cat1.Name = "Clippy";

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            Assert.Equal(2, tracked.Count);
            Assert.Single(changed);

            AssertTrackedEvent(context, 3, EntityState.Added, tracked[1], fromQuery: false);
            AssertChangedEvent(context, 1, EntityState.Unchanged, EntityState.Modified, changed[0]);

            Assert.True(context.ChangeTracker.HasChanges());

            context.SaveChanges();

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(2, tracked.Count);
            Assert.Equal(3, changed.Count);

            AssertChangedEvent(context, 1, EntityState.Modified, EntityState.Unchanged, changed[2]);
            AssertChangedEvent(context, 3, EntityState.Added, EntityState.Unchanged, changed[1]);

            context.Database.EnsureDeleted();
        }

        [ConditionalFact]
        public void State_change_events_fire_when_property_modified_flags_cause_state_change()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using var scope = _poolProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

            RegisterEvents(context, tracked, changed);

            var cat = context.Attach(
                new Cat(3) { Name = "Achilles" }).Entity;

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Single(tracked);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 3, EntityState.Unchanged, tracked[0], fromQuery: false);

            context.Entry(cat).Property(e => e.Name).IsModified = true;

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Single(tracked);
            Assert.Single(changed);

            AssertChangedEvent(context, 3, EntityState.Unchanged, EntityState.Modified, changed[0]);

            context.Entry(cat).Property(e => e.Name).IsModified = false;

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Single(tracked);
            Assert.Equal(2, changed.Count);

            AssertChangedEvent(context, 3, EntityState.Modified, EntityState.Unchanged, changed[1]);
        }

        [ConditionalFact]
        public void State_change_events_are_limited_to_the_current_context()
        {
            var tracked1 = new List<EntityTrackedEventArgs>();
            var changed1 = new List<EntityStateChangedEventArgs>();
            var tracked2 = new List<EntityTrackedEventArgs>();
            var changed2 = new List<EntityStateChangedEventArgs>();

            Seed(usePool: true);

            using var scope = _poolProvider.CreateScope();
            var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

            RegisterEvents(context, tracked1, changed1);

            using (var scope2 = _poolProvider.CreateScope())
            {
                var context2 = scope2.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context2, tracked2, changed2);

                Assert.Equal(2, context2.Cats.OrderBy(e => e.Id).ToList().Count);

                Assert.Equal(2, tracked2.Count);
                Assert.Empty(changed2);

                context2.Entry(context2.Cats.Find(1)).State = EntityState.Modified;

                Assert.Equal(2, tracked2.Count);
                Assert.Single(changed2);

                Assert.Empty(tracked1);
                Assert.Empty(changed1);
            }

            Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

            Assert.Equal(2, tracked1.Count);
            Assert.Empty(changed1);

            context.Entry(context.Cats.Find(1)).State = EntityState.Modified;

            Assert.Equal(2, tracked1.Count);
            Assert.Single(changed1);

            Assert.Equal(2, tracked2.Count);
            Assert.Single(changed2);

            context.Database.EnsureDeleted();
        }

        private static void AssertTrackedEvent(
            LikeAZooContext context,
            int id,
            EntityState newState,
            EntityTrackedEventArgs tracked,
            bool fromQuery)
        {
            Assert.Equal(newState, tracked.Entry.State);
            Assert.Equal(fromQuery, tracked.FromQuery);
            Assert.Same(context.Cats.Find(id), tracked.Entry.Entity);
        }

        private static void AssertChangedEvent(
            LikeAZooContext context,
            int? id,
            EntityState oldState,
            EntityState newState,
            EntityStateChangedEventArgs changed)
        {
            Assert.Equal(oldState, changed.OldState);
            Assert.Equal(newState, changed.NewState);
            Assert.Equal(newState, changed.Entry.State);

            if (id != null)
            {
                Assert.Same(context.Cats.Find(id), changed.Entry.Entity);
            }
        }

        private static void RegisterEvents(
            LikeAZooContext context,
            IList<EntityTrackedEventArgs> tracked,
            IList<EntityStateChangedEventArgs> changed)
        {
            context.ChangeTracker.Tracked += (s, e) =>
            {
                Assert.Same(context.ChangeTracker, s);
                tracked.Add(e);
            };

            context.ChangeTracker.StateChanged += (s, e) =>
            {
                Assert.Same(context.ChangeTracker, s);
                Assert.Equal(e.NewState, e.Entry.State);
                changed.Add(e);
            };
        }

        private class Cat
        {
            public Cat(int id)
                => Id = id;

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public int Id { get; private set; }

            public string Name { get; set; }

            public ICollection<Hat> Hats { get; } = new List<Hat>();

            public ICollection<Mat> Mats { get; } = new List<Mat>();
        }

        private class Hat
        {
            public Hat(int id)
                => Id = id;

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public int Id { get; private set; }

            public string Color { get; set; }

            public int CatId { get; set; }
            public Cat Cat { get; set; }
        }

        private class Mat
        {
            public Mat(int id)
                => Id = id;

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public int Id { get; private set; }

            public ICollection<Cat> Cats { get; } = new List<Cat>();
        }

        private class CatMat
        {
            public int CatId { get; set; }
            public int MatId { get; set; }
        }

        private static readonly ListLoggerFactory _loggerFactory
            = new ListLoggerFactory();

        private static readonly IServiceProvider _serviceProvider
            = InMemoryFixture.BuildServiceProvider(_loggerFactory);

        private static readonly IServiceProvider _sensitiveProvider
            = InMemoryFixture.BuildServiceProvider(_loggerFactory);

        private static readonly IServiceProvider _poolProvider
            = new ServiceCollection()
                .AddDbContextPool<LikeAZooContextPooled>(
                    p => p.UseInMemoryDatabase(nameof(LikeAZooContextPooled))
                        .UseInternalServiceProvider(InMemoryFixture.BuildServiceProvider(_loggerFactory)))
                .BuildServiceProvider();

        private class LikeAZooContextPooled : LikeAZooContext
        {
            public LikeAZooContextPooled(DbContextOptions<LikeAZooContextPooled> options)
                : base(options)
            {
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
            }
        }

        private class LikeAZooContext : DbContext
        {
            public LikeAZooContext()
            {
            }

            protected LikeAZooContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Cat> Cats { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(LikeAZooContext));

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Cat>()
                    .Property(e => e.Id)
                    .HasValueGenerator<InMemoryIntegerValueGenerator<int>>();

                modelBuilder
                    .Entity<Hat>()
                    .Property(e => e.Id)
                    .HasValueGenerator<InMemoryIntegerValueGenerator<int>>();

                modelBuilder.Entity<Mat>(
                    b =>
                    {
                        b.Property(e => e.Id).HasValueGenerator<InMemoryIntegerValueGenerator<int>>();
                        b.HasMany(e => e.Cats)
                            .WithMany(e => e.Mats)
                            .UsingEntity<CatMat>(
                                ts => ts.HasOne<Cat>().WithMany(),
                                ts => ts.HasOne<Mat>().WithMany())
                            .HasKey(ts => new { ts.CatId, ts.MatId });
                    });
            }
        }

        private class LikeAZooContextSensitive : LikeAZooContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseInternalServiceProvider(_sensitiveProvider)
                    .UseInMemoryDatabase(nameof(LikeAZooContextSensitive));
        }

        private void Seed(bool sensitive = false, bool usePool = false)
        {
            void Seed(LikeAZooContext context)
            {
                context.Database.EnsureDeleted();

                var cat1 = new Cat(1) { Name = "Smokey" };
                var cat2 = new Cat(2) { Name = "Sid" };

                cat1.Hats.Add(new Hat(77) { Color = "Pine Green" });

                context.AddRange(cat1, cat2);

                var mat = new Mat(77);
                context.Add(mat);
                cat1.Mats.Add(mat);

                context.SaveChanges();
            }

            if (usePool)
            {
                using var scope = _poolProvider.CreateScope();
                Seed(scope.ServiceProvider.GetService<LikeAZooContextPooled>());
            }
            else
            {
                using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
                Seed(context);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_remove_dependent_identifying_one_to_many(bool saveEntities)
        {
            using var context = new EarlyLearningCenter();
            var product = new Product();
            var order = new Order();
            var orderDetails = new OrderDetails { Order = order, Product = product };

            context.Add(orderDetails);
            if (saveEntities)
            {
                context.SaveChanges();
            }

            var expectedState = saveEntities ? EntityState.Unchanged : EntityState.Added;

            Assert.Equal(expectedState, context.Entry(product).State);
            Assert.Equal(expectedState, context.Entry(order).State);
            Assert.Equal(expectedState, context.Entry(orderDetails).State);

            Assert.Same(orderDetails, product.OrderDetails.Single());
            Assert.Same(orderDetails, order.OrderDetails.Single());

            order.OrderDetails.Remove(orderDetails);

            Assert.Equal(expectedState, context.Entry(product).State);
            Assert.Equal(expectedState, context.Entry(order).State);
            Assert.Equal(saveEntities ? EntityState.Deleted : EntityState.Detached, context.Entry(orderDetails).State);

            Assert.Empty(product.OrderDetails);
            Assert.Empty(order.OrderDetails);

            context.SaveChanges();

            Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(order).State);
            Assert.Equal(EntityState.Detached, context.Entry(orderDetails).State);

            Assert.Empty(product.OrderDetails);
            Assert.Empty(order.OrderDetails);
        }

        [ConditionalFact]
        public void Keyless_type_negative_cases()
        {
            using var context = new EarlyLearningCenter();
            var whoAmI = new WhoAmI();

            Assert.Equal(
                CoreStrings.KeylessTypeTracked("WhoAmI"),
                Assert.Throws<InvalidOperationException>(() => context.Add(whoAmI)).Message);

            Assert.Equal(
                CoreStrings.KeylessTypeTracked("WhoAmI"),
                Assert.Throws<InvalidOperationException>(() => context.Remove(whoAmI)).Message);

            Assert.Equal(
                CoreStrings.KeylessTypeTracked("WhoAmI"),
                Assert.Throws<InvalidOperationException>(() => context.Attach(whoAmI)).Message);

            Assert.Equal(
                CoreStrings.KeylessTypeTracked("WhoAmI"),
                Assert.Throws<InvalidOperationException>(() => context.Update(whoAmI)).Message);

            Assert.Equal(
                CoreStrings.InvalidSetKeylessOperation("WhoAmI"),
                Assert.Throws<InvalidOperationException>(() => context.Find<WhoAmI>(1)).Message);

            Assert.Equal(
                CoreStrings.InvalidSetKeylessOperation("WhoAmI"),
                Assert.Throws<InvalidOperationException>(() => context.Set<WhoAmI>().Local).Message);

            Assert.Equal(
                CoreStrings.KeylessTypeTracked("WhoAmI"),
                Assert.Throws<InvalidOperationException>(() => context.Entry(whoAmI)).Message);
        }

        [ConditionalFact]
        public void Can_get_all_entries()
        {
            using var context = new EarlyLearningCenter();
            var category = context.Add(new Category()).Entity;
            var product = context.Add(new Product()).Entity;

            Assert.Equal(
                new object[] { category, product },
                context.ChangeTracker.Entries().Select(e => e.Entity).OrderBy(e => e.GetType().Name));
        }

        [ConditionalFact]
        public void Can_get_all_entities_for_an_entity_of_a_given_type()
        {
            using var context = new EarlyLearningCenter();
            var category = context.Add(new Category()).Entity;
            var product = context.Add(new Product()).Entity;

            Assert.Equal(
                new object[] { product },
                context.ChangeTracker.Entries<Product>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));

            Assert.Equal(
                new object[] { category },
                context.ChangeTracker.Entries<Category>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));

            Assert.Equal(
                new object[] { category, product },
                context.ChangeTracker.Entries<object>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));
        }

        [ConditionalFact]
        public void Can_get_Context()
        {
            using var context = new EarlyLearningCenter();
            Assert.Same(context, context.ChangeTracker.Context);
        }

        [ConditionalTheory] // Issue #17828
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void DetectChanges_reparents_even_when_immediate_cascade_enabled(bool delayCascade, bool callDetectChangesTwice)
        {
            using var context = new EarlyLearningCenter();

            // Construct initial state
            var parent1 = new Category { Id = 1 };
            var parent2 = new Category { Id = 2 };
            var child = new Product { Id = 3, Category = parent1 };

            context.AddRange(parent1, parent2, child);
            context.ChangeTracker.AcceptAllChanges();

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(parent1).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(child).State);

            if (delayCascade)
            {
                context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
            }

            child.Category = parent2;

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            context.Remove(parent1);

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Deleted, context.Entry(parent1).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
            Assert.Equal(EntityState.Modified, context.Entry(child).State);
        }

        [ConditionalTheory] // Issue #19203
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Dependent_FKs_are_not_nulled_when_principal_is_detached(bool delayCascade, bool trackNewDependents)
        {
            using var context = new EarlyLearningCenter();

            var category = new OptionalCategory
            {
                Id = 1,
                Products = new List<OptionalProduct>
                {
                    new OptionalProduct { Id = 1 },
                    new OptionalProduct { Id = 2 },
                    new OptionalProduct { Id = 3 }
                }
            };

            context.Attach(category);

            var categoryEntry = context.Entry(category);
            var product0Entry = context.Entry(category.Products[0]);
            var product1Entry = context.Entry(category.Products[1]);
            var product2Entry = context.Entry(category.Products[2]);

            Assert.Equal(EntityState.Unchanged, categoryEntry.State);
            Assert.Equal(EntityState.Unchanged, product0Entry.State);
            Assert.Equal(EntityState.Unchanged, product1Entry.State);
            Assert.Equal(EntityState.Unchanged, product2Entry.State);

            if (delayCascade)
            {
                context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
            }

            context.Entry(category).State = EntityState.Detached;

            Assert.Equal(EntityState.Detached, categoryEntry.State);

            Assert.Equal(EntityState.Unchanged, product0Entry.State);
            Assert.Equal(EntityState.Unchanged, product1Entry.State);
            Assert.Equal(EntityState.Unchanged, product2Entry.State);

            var newCategory = new OptionalCategory { Id = 1, };

            if (trackNewDependents)
            {
                newCategory.Products = new List<OptionalProduct>
                {
                    new OptionalProduct { Id = 1 },
                    new OptionalProduct { Id = 2 },
                    new OptionalProduct { Id = 3 }
                };
            }

            if (trackNewDependents)
            {
                Assert.Equal(
                    CoreStrings.IdentityConflict(nameof(OptionalProduct), "{'Id'}"),
                    Assert.Throws<InvalidOperationException>(() => context.Attach(newCategory)).Message);
            }
            else
            {
                context.Update(newCategory);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                categoryEntry = context.Entry(newCategory);
                product0Entry = context.Entry(newCategory.Products[0]);
                product1Entry = context.Entry(newCategory.Products[1]);
                product2Entry = context.Entry(newCategory.Products[2]);

                Assert.Equal(EntityState.Modified, categoryEntry.State);

                Assert.Equal(EntityState.Unchanged, product0Entry.State);
                Assert.Equal(EntityState.Unchanged, product1Entry.State);
                Assert.Equal(EntityState.Unchanged, product2Entry.State);

                Assert.Same(newCategory.Products[0], category.Products[0]);
                Assert.Same(newCategory.Products[1], category.Products[1]);
                Assert.Same(newCategory.Products[2], category.Products[2]);

                Assert.Same(newCategory, newCategory.Products[0].Category);
                Assert.Same(newCategory, newCategory.Products[1].Category);
                Assert.Same(newCategory, newCategory.Products[2].Category);

                Assert.Equal(newCategory.Id, product0Entry.Property("CategoryId").CurrentValue);
                Assert.Equal(newCategory.Id, product1Entry.Property("CategoryId").CurrentValue);
                Assert.Equal(newCategory.Id, product2Entry.Property("CategoryId").CurrentValue);
            }
        }

        [ConditionalTheory] // Issue #16546
        [InlineData(false)]
        [InlineData(true)]
        public void Optional_relationship_with_cascade_still_cascades(bool delayCascade)
        {
            Kontainer detachedContainer;
            var databaseName = "K" + delayCascade;
            using (var context = new KontainerContext(databaseName))
            {
                context.Add(
                    new Kontainer
                    {
                        Name = "C1",
                        Rooms = { new KontainerRoom { Number = 1, Troduct = new Troduct { Description = "Heavy Engine XT3" } } }
                    }
                );

                context.SaveChanges();

                detachedContainer = context.Set<Kontainer>()
                    .Include(container => container.Rooms)
                    .ThenInclude(room => room.Troduct)
                    .AsNoTracking()
                    .Single();
            }

            using (var context = new KontainerContext(databaseName))
            {
                var attachedContainer = context.Set<Kontainer>()
                    .Include(container => container.Rooms)
                    .ThenInclude(room => room.Troduct)
                    .Single();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer.Rooms.Single()).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer.Rooms.Single().Troduct).State);

                var detachedRoom = detachedContainer.Rooms.Single();
                detachedRoom.Troduct = null;
                detachedRoom.TroductId = null;

                var attachedRoom = attachedContainer.Rooms.Single();

                if (delayCascade)
                {
                    context.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
                }

                context.Entry(attachedRoom).CurrentValues.SetValues(detachedRoom);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer.Rooms.Single().Troduct).State);

                if (delayCascade)
                {
                    Assert.Equal(EntityState.Modified, context.Entry(attachedContainer.Rooms.Single()).State);
                }
                else
                {
                    // Deleted because FK with cascade has been set to null
                    Assert.Equal(EntityState.Deleted, context.Entry(attachedContainer.Rooms.Single()).State);
                }

                context.ChangeTracker.CascadeChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer.Rooms.Single().Troduct).State);
                Assert.Equal(EntityState.Deleted, context.Entry(attachedContainer.Rooms.Single()).State);

                context.SaveChanges();
            }
        }

        private class Kontainer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<KontainerRoom> Rooms { get; set; } = new List<KontainerRoom>();
        }

        private class KontainerRoom
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public int KontainerId { get; set; }
            public Kontainer Kontainer { get; set; }
            public int? TroductId { get; set; }
            public Troduct Troduct { get; set; }
        }

        private class Troduct
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public List<KontainerRoom> Rooms { get; set; } = new List<KontainerRoom>();
        }

        private class KontainerContext : DbContext
        {
            private readonly string _databaseName;

            public KontainerContext(string databaseName)
            {
                _databaseName = databaseName;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<KontainerRoom>()
                    .HasOne(room => room.Troduct)
                    .WithMany(product => product.Rooms)
                    .HasForeignKey(room => room.TroductId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(_databaseName);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Adding_derived_owned_throws(bool useAdd)
        {
            using var context = new EarlyLearningCenter();
            var dreams = new Dreams { Sweet = new Sweet { Id = 1 }, Are = new OfThis() };

            context.Entry(dreams.Sweet).State = EntityState.Unchanged;

            if (useAdd)
            {
                Assert.Equal(
                    CoreStrings.TrackingTypeMismatch(nameof(OfThis), "Dreams.Are#AreMade"),
                    Assert.Throws<InvalidOperationException>(() => context.Add(dreams)).Message);
            }
            else
            {
                Assert.Equal(
                    CoreStrings.TrackingTypeMismatch(nameof(OfThis), "Dreams.Are#AreMade"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            context.ChangeTracker.TrackGraph(
                                dreams, e =>
                                {
                                    e.Entry.State = e.Entry.IsKeySet && !e.Entry.Metadata.IsOwned()
                                        ? EntityState.Unchanged
                                        : EntityState.Added;
                                })).Message);
            }
        }

        [ConditionalFact]
        public void Moving_derived_owned_to_non_derived_reference_throws()
        {
            using var context = new EarlyLearningCenter();
            var dreams = new Dreams { Sweet = new Sweet { Id = 1 }, OfThis = new OfThis() };

            context.Entry(dreams.Sweet).State = EntityState.Unchanged;
            context.Add(dreams);

            dreams.Are = dreams.OfThis;
            dreams.OfThis = null;

            Assert.Equal(
                CoreStrings.TrackingTypeMismatch(nameof(OfThis), "Dreams.Are#AreMade"),
                Assert.Throws<InvalidOperationException>(() => context.Entry(dreams)).Message);
        }

        [ConditionalFact] // Issue #1207
        public void Can_add_principal_and_then_identifying_dependents_with_key_generation()
        {
            using var context = new EarlyLearningCenter();
            var product1 = new Product
            {
                Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
            };
            var product2 = new Product
            {
                Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
            };

            context.Add(product1);
            context.Add(product1.Details);
            context.Add(product1.Details.Tag);
            context.Add(product1.Details.Tag.TagDetails);
            context.Add(product2);
            context.Add(product2.Details);
            context.Add(product2.Details.Tag);
            context.Add(product2.Details.Tag.TagDetails);

            AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
        }

        [ConditionalFact] // Issue #1207
        public void Can_add_identifying_dependents_and_then_principal_with_key_generation()
        {
            using var context = new EarlyLearningCenter();
            var tagDetails1 = new ProductDetailsTagDetails
            {
                Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
            };

            var tagDetails2 = new ProductDetailsTagDetails
            {
                Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
            };

            context.Add(tagDetails1);
            context.Add(tagDetails1.Tag);
            context.Add(tagDetails1.Tag.Details);
            context.Add(tagDetails1.Tag.Details.Product);
            context.Add(tagDetails2);
            context.Add(tagDetails2.Tag);
            context.Add(tagDetails2.Tag.Details);
            context.Add(tagDetails2.Tag.Details.Product);

            AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
        }

        [ConditionalFact] // Issue #1207
        public void Can_add_identifying_dependents_and_then_principal_interleaved_with_key_generation()
        {
            using var context = new EarlyLearningCenter();
            var tagDetails1 = new ProductDetailsTagDetails
            {
                Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
            };

            var tagDetails2 = new ProductDetailsTagDetails
            {
                Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
            };

            context.Add(tagDetails1);
            context.Add(tagDetails2);
            context.Add(tagDetails1.Tag);
            context.Add(tagDetails2.Tag);
            context.Add(tagDetails2.Tag.Details);
            context.Add(tagDetails1.Tag.Details);
            context.Add(tagDetails1.Tag.Details.Product);
            context.Add(tagDetails2.Tag.Details.Product);

            AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
        }

        [ConditionalFact] // Issue #1207
        public void Can_add_identifying_dependents_and_principal_starting_in_the_middle_with_key_generation()
        {
            using var context = new EarlyLearningCenter();
            var tagDetails1 = new ProductDetailsTagDetails
            {
                Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
            };

            var tagDetails2 = new ProductDetailsTagDetails
            {
                Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
            };

            context.Add(tagDetails1.Tag);
            context.Add(tagDetails2.Tag);
            context.Add(tagDetails1);
            context.Add(tagDetails2);
            context.Add(tagDetails2.Tag.Details);
            context.Add(tagDetails1.Tag.Details);
            context.Add(tagDetails1.Tag.Details.Product);
            context.Add(tagDetails2.Tag.Details.Product);

            AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
        }

        [ConditionalFact] // Issue #1207
        public void Can_add_principal_and_identifying_dependents_starting_in_the_middle_with_key_generation()
        {
            using var context = new EarlyLearningCenter();
            var product1 = new Product
            {
                Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
            };
            var product2 = new Product
            {
                Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
            };

            context.Add(product1.Details);
            context.Add(product2.Details);
            context.Add(product1.Details.Tag.TagDetails);
            context.Add(product1);
            context.Add(product1.Details.Tag);
            context.Add(product2.Details.Tag);
            context.Add(product2.Details.Tag.TagDetails);
            context.Add(product2);

            AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
        }

        [ConditionalTheory] // Issue #1207
        [InlineData(false)]
        [InlineData(true)]
        public void Can_add_identifying_dependents_and_principal_with_post_nav_fixup_with_key_generation(bool callDetectChangesTwice)
        {
            using var context = new EarlyLearningCenter();
            var product1 = new Product();
            var details1 = new ProductDetails();
            var tag1 = new ProductDetailsTag();
            var tagDetails1 = new ProductDetailsTagDetails();

            var product2 = new Product();
            var details2 = new ProductDetails();
            var tag2 = new ProductDetailsTag();
            var tagDetails2 = new ProductDetailsTagDetails();

            context.Add(product1);
            context.Add(tagDetails2);
            context.Add(details1);
            context.Add(tag2);
            context.Add(details2);
            context.Add(tag1);
            context.Add(tagDetails1);
            context.Add(product2);

            product1.Details = details1;
            details1.Tag = tag1;
            tag1.TagDetails = tagDetails1;

            product2.Details = details2;
            details2.Tag = tag2;
            tag2.TagDetails = tagDetails2;

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
        }

        [ConditionalTheory] // Issue #1207
        [InlineData(false)]
        [InlineData(true)]
        public void Can_add_identifying_dependents_and_principal_with_reverse_post_nav_fixup_with_key_generation(bool callDetectChangesTwice)
        {
            using var context = new EarlyLearningCenter();
            var product1 = new Product();
            var details1 = new ProductDetails();
            var tag1 = new ProductDetailsTag();
            var tagDetails1 = new ProductDetailsTagDetails();

            var product2 = new Product();
            var details2 = new ProductDetails();
            var tag2 = new ProductDetailsTag();
            var tagDetails2 = new ProductDetailsTagDetails();

            context.Add(product1);
            context.Add(tagDetails2);
            context.Add(details1);
            context.Add(tag2);
            context.Add(details2);
            context.Add(tag1);
            context.Add(tagDetails1);
            context.Add(product2);

            tagDetails1.Tag = tag1;
            tag1.Details = details1;
            details1.Product = product1;

            tagDetails2.Tag = tag2;
            tag2.Details = details2;
            details2.Product = product2;

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
        }

        private static void AssertProductAndDetailsFixedUp(
            DbContext context,
            ProductDetailsTagDetails tagDetails1,
            ProductDetailsTagDetails tagDetails2)
        {
            Assert.Equal(8, context.ChangeTracker.Entries().Count());

            Assert.Equal(EntityState.Added, context.Entry(tagDetails1).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag.Details).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag.Details.Product).State);

            Assert.Equal(EntityState.Added, context.Entry(tagDetails2).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag.Details).State);
            Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag.Details.Product).State);

            Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Id);
            Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Details.Id);
            Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Details.Product.Id);
            Assert.True(tagDetails1.Id > 0);

            Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Id);
            Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Details.Id);
            Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Details.Product.Id);
            Assert.True(tagDetails2.Id > 0);

            Assert.Same(tagDetails1, tagDetails1.Tag.TagDetails);
            Assert.Same(tagDetails1.Tag, tagDetails1.Tag.Details.Tag);
            Assert.Same(tagDetails1.Tag.Details, tagDetails1.Tag.Details.Product.Details);

            Assert.Same(tagDetails2, tagDetails2.Tag.TagDetails);
            Assert.Same(tagDetails2.Tag, tagDetails2.Tag.Details.Tag);
            Assert.Same(tagDetails2.Tag.Details, tagDetails2.Tag.Details.Product.Details);

            var product1 = tagDetails1.Tag.Details.Product;
            Assert.Same(product1, product1.Details.Product);
            Assert.Same(product1.Details, product1.Details.Tag.Details);
            Assert.Same(product1.Details.Tag, product1.Details.Tag.TagDetails.Tag);

            var product2 = tagDetails2.Tag.Details.Product;
            Assert.Same(product2, product2.Details.Product);
            Assert.Same(product2.Details, product2.Details.Tag.Details);
            Assert.Same(product2.Details.Tag, product2.Details.Tag.TagDetails.Tag);
        }

        [ConditionalFact] // Issue #1207
        public void Can_add_identifying_one_to_many_via_principal_with_key_generation()
        {
            using var context = new EarlyLearningCenter();
            var product1 = new Product();
            var product2 = new Product();

            var order1 = new Order();
            var order2 = new Order();

            var orderDetails1a = new OrderDetails { Order = order1, Product = product1 };
            var orderDetails1b = new OrderDetails { Order = order1, Product = product2 };
            var orderDetails2a = new OrderDetails { Order = order2, Product = product1 };
            var orderDetails2b = new OrderDetails { Order = order2, Product = product2 };

            context.Add(product1);
            context.Add(order1);
            context.Add(orderDetails1a);
            context.Add(orderDetails1b);
            context.Add(product2);
            context.Add(order2);
            context.Add(orderDetails2a);
            context.Add(orderDetails2b);

            AssertOrderAndDetailsFixedUp(context, orderDetails1a, orderDetails1b, orderDetails2a, orderDetails2b);
        }

        [ConditionalFact] // Issue #1207
        public void Can_add_identifying_one_to_many_via_dependents_with_key_generation()
        {
            using var context = new EarlyLearningCenter();
            var product1 = new Product();
            var product2 = new Product();

            var order1 = new Order();
            var order2 = new Order();

            var orderDetails1a = new OrderDetails { Order = order1, Product = product1 };
            var orderDetails1b = new OrderDetails { Order = order1, Product = product2 };
            var orderDetails2a = new OrderDetails { Order = order2, Product = product1 };
            var orderDetails2b = new OrderDetails { Order = order2, Product = product2 };

            context.Add(orderDetails1a);
            context.Add(orderDetails2a);
            context.Add(orderDetails1b);
            context.Add(orderDetails2b);
            context.Add(order1);
            context.Add(product1);
            context.Add(order2);
            context.Add(product2);

            AssertOrderAndDetailsFixedUp(context, orderDetails1a, orderDetails1b, orderDetails2a, orderDetails2b);
        }

        private static void AssertOrderAndDetailsFixedUp(
            DbContext context,
            OrderDetails orderDetails1a,
            OrderDetails orderDetails1b,
            OrderDetails orderDetails2a,
            OrderDetails orderDetails2b)
        {
            Assert.Equal(8, context.ChangeTracker.Entries().Count());

            Assert.Equal(EntityState.Added, context.Entry(orderDetails1a).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails1b).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails1a.Order).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails1b.Product).State);

            Assert.Equal(EntityState.Added, context.Entry(orderDetails2a).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails2b).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails2a.Order).State);
            Assert.Equal(EntityState.Added, context.Entry(orderDetails2b.Product).State);

            Assert.Equal(orderDetails1a.OrderId, orderDetails1a.Order.Id);
            Assert.Equal(orderDetails1b.OrderId, orderDetails1b.Order.Id);
            Assert.Equal(orderDetails1a.ProductId, orderDetails1a.Product.Id);
            Assert.Equal(orderDetails1b.ProductId, orderDetails1b.Product.Id);
            Assert.True(orderDetails1a.OrderId > 0);
            Assert.True(orderDetails1b.OrderId > 0);
            Assert.True(orderDetails1a.ProductId > 0);
            Assert.True(orderDetails1b.ProductId > 0);

            Assert.Equal(orderDetails2a.OrderId, orderDetails2a.Order.Id);
            Assert.Equal(orderDetails2b.OrderId, orderDetails2b.Order.Id);
            Assert.Equal(orderDetails2a.ProductId, orderDetails2a.Product.Id);
            Assert.Equal(orderDetails2b.ProductId, orderDetails2b.Product.Id);
            Assert.True(orderDetails2a.OrderId > 0);
            Assert.True(orderDetails2b.OrderId > 0);
            Assert.True(orderDetails2a.ProductId > 0);
            Assert.True(orderDetails2b.ProductId > 0);

            Assert.Same(orderDetails1a.Order, orderDetails1b.Order);
            Assert.Same(orderDetails2a.Order, orderDetails2b.Order);

            Assert.Same(orderDetails1a.Product, orderDetails2a.Product);
            Assert.Same(orderDetails1b.Product, orderDetails2b.Product);

            Assert.Equal(2, orderDetails1a.Order.OrderDetails.Count);
            Assert.Equal(2, orderDetails2a.Order.OrderDetails.Count);

            Assert.Contains(orderDetails1a, orderDetails1a.Order.OrderDetails);
            Assert.Contains(orderDetails1b, orderDetails1a.Order.OrderDetails);
            Assert.Contains(orderDetails2a, orderDetails2a.Order.OrderDetails);
            Assert.Contains(orderDetails2b, orderDetails2a.Order.OrderDetails);

            Assert.Equal(2, orderDetails1a.Product.OrderDetails.Count);
            Assert.Equal(2, orderDetails1b.Product.OrderDetails.Count);

            Assert.Contains(orderDetails1a, orderDetails1a.Product.OrderDetails);
            Assert.Contains(orderDetails2a, orderDetails1a.Product.OrderDetails);
            Assert.Contains(orderDetails1b, orderDetails1b.Product.OrderDetails);
            Assert.Contains(orderDetails2b, orderDetails1b.Product.OrderDetails);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entries_calls_DetectChanges_by_default(bool useGenericOverload)
        {
            using var context = new EarlyLearningCenter();
            var entry = context.Attach(
                new Product { Id = 1, CategoryId = 66 });

            entry.Entity.CategoryId = 77;

            Assert.Equal(EntityState.Unchanged, entry.State);

            if (useGenericOverload)
            {
                context.ChangeTracker.Entries<Product>();
            }
            else
            {
                context.ChangeTracker.Entries();
            }

            Assert.Equal(EntityState.Modified, entry.State);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Auto_DetectChanges_for_Entries_can_be_switched_off(bool useGenericOverload)
        {
            using var context = new EarlyLearningCenter();
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            var entry = context.Attach(
                new Product { Id = 1, CategoryId = 66 });

            entry.Entity.CategoryId = 77;

            Assert.Equal(EntityState.Unchanged, entry.State);

            if (useGenericOverload)
            {
                context.ChangeTracker.Entries<Product>();
            }
            else
            {
                context.ChangeTracker.Entries();
            }

            Assert.Equal(EntityState.Unchanged, entry.State);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Explicitly_calling_DetectChanges_works_even_if_auto_DetectChanges_is_switched_off(bool callDetectChangesTwice)
        {
            using var context = new EarlyLearningCenter();
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            var entry = context.Attach(
                new Product { Id = 1, CategoryId = 66 });

            entry.Entity.CategoryId = 77;

            Assert.Equal(EntityState.Unchanged, entry.State);

            context.ChangeTracker.DetectChanges();

            if (callDetectChangesTwice)
            {
                context.ChangeTracker.DetectChanges();
            }

            Assert.Equal(EntityState.Modified, entry.State);
        }

        [ConditionalFact]
        public void Does_not_throw_when_instance_of_unmapped_derived_type_is_used()
        {
            using var context = new EarlyLearningCenter();
            Assert.Same(
                context.Model.FindEntityType(typeof(Product)),
                context.Add(new SpecialProduct()).Metadata);
        }

        [ConditionalFact]
        public void Shadow_properties_are_not_included_in_update_unless_value_explicitly_set()
        {
            int id;

            using (var context = new TheShadows())
            {
                var entry = context.Add(new Dark());

                Assert.NotEqual(0, id = entry.Property<int>("Id").CurrentValue);
                Assert.Equal(0, entry.Property<int>("SomeInt").CurrentValue);
                Assert.Null(entry.Property<string>("SomeString").CurrentValue);

                entry.Property<int>("SomeInt").CurrentValue = 77;
                entry.Property<string>("SomeString").CurrentValue = "Morden";

                context.SaveChanges();
            }

            AssertValuesSaved(id, 77, "Morden");

            using (var context = new TheShadows())
            {
                var entry = context.Entry(new Dark());
                entry.Property<int>("Id").CurrentValue = id;
                entry.State = EntityState.Modified;

                context.SaveChanges();
            }

            AssertValuesSaved(id, 77, "Morden");

            using (var context = new TheShadows())
            {
                var entry = context.Entry(new Dark());
                entry.Property<int>("Id").CurrentValue = id;
                entry.Property<int>("SomeInt").CurrentValue = 78;
                entry.Property<string>("SomeString").CurrentValue = "Mr";
                entry.State = EntityState.Modified;

                context.SaveChanges();
            }

            AssertValuesSaved(id, 78, "Mr");

            using (var context = new TheShadows())
            {
                var entry = context.Entry(new Dark());
                entry.Property<int>("Id").CurrentValue = id;
                entry.State = EntityState.Modified;
                entry.Property<int>("SomeInt").CurrentValue = 0;
                entry.Property<string>("SomeString").CurrentValue = null;

                context.SaveChanges();
            }

            AssertValuesSaved(id, 0, null);
        }

        private static void AssertValuesSaved(int id, int someInt, string someString)
        {
            using var context = new TheShadows();
            var entry = context.Entry(context.Set<Dark>().Single(e => EF.Property<int>(e, "Id") == id));

            Assert.Equal(id, entry.Property<int>("Id").CurrentValue);
            Assert.Equal(someInt, entry.Property<int>("SomeInt").CurrentValue);
            Assert.Equal(someString, entry.Property<string>("SomeString").CurrentValue);
        }

        private class TheShadows : DbContext
        {
            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Dark>(
                    b =>
                    {
                        b.Property<int>("Id").ValueGeneratedOnAdd();
                        b.Property<int>("SomeInt");
                        b.Property<string>("SomeString");
                    });

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase(nameof(TheShadows));
        }

        private class Dark
        {
        }

        private class Category
        {
            public int Id { get; set; }

            public List<Product> Products { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }

            public ProductDetails Details { get; set; }

            // ReSharper disable once CollectionNeverUpdated.Local
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public List<OrderDetails> OrderDetails { get; set; }
        }

        private class OptionalCategory
        {
            public int Id { get; set; }

            public List<OptionalProduct> Products { get; set; }
        }

        private class OptionalProduct
        {
            public int Id { get; set; }

            public int? CategoryId { get; set; }
            public OptionalCategory Category { get; set; }
        }

        private class SpecialProduct : Product
        {
        }

        private class ProductDetails
        {
            public int Id { get; set; }

            public Product Product { get; set; }

            public ProductDetailsTag Tag { get; set; }
        }

        private class ProductDetailsTag
        {
            public int Id { get; set; }

            public ProductDetails Details { get; set; }

            public ProductDetailsTagDetails TagDetails { get; set; }
        }

        private class ProductDetailsTagDetails
        {
            public int Id { get; set; }

            public ProductDetailsTag Tag { get; set; }
        }

        private class Order
        {
            public int Id { get; set; }

            // ReSharper disable once CollectionNeverUpdated.Local
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public List<OrderDetails> OrderDetails { get; set; }
        }

        private class OrderDetails
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }

            public Order Order { get; set; }
            public Product Product { get; set; }
        }

        private class Sweet
        {
            public int? Id { get; set; }
            public Dreams Dreams { get; set; }
        }

        private class Dreams
        {
            public Sweet Sweet { get; set; }
            public AreMade Are { get; set; }
            public AreMade Made { get; set; }
            public OfThis OfThis { get; set; }
        }

        private class AreMade
        {
        }

        private class OfThis : AreMade
        {
        }

        private class WhoAmI
        {
            public string ToDisagree { get; set; }
        }

        private class EarlyLearningCenter : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public EarlyLearningCenter()
            {
                _serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Sweet>().OwnsOne(
                    e => e.Dreams, b =>
                    {
                        b.WithOwner(e => e.Sweet);
                        b.OwnsOne(e => e.Are);
                        b.OwnsOne(e => e.Made);
                        b.OwnsOne(e => e.OfThis);
                    });

                modelBuilder.Entity<WhoAmI>().HasNoKey();

                modelBuilder
                    .Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

                modelBuilder
                    .Entity<ProductDetailsTag>().HasOne(e => e.TagDetails).WithOne(e => e.Tag)
                    .HasForeignKey<ProductDetailsTagDetails>(e => e.Id);

                modelBuilder
                    .Entity<ProductDetails>().HasOne(e => e.Tag).WithOne(e => e.Details)
                    .HasForeignKey<ProductDetailsTag>(e => e.Id);

                modelBuilder
                    .Entity<Product>().HasOne(e => e.Details).WithOne(e => e.Product)
                    .HasForeignKey<ProductDetails>(e => e.Id);

                modelBuilder.Entity<OrderDetails>(
                    b =>
                    {
                        b.HasKey(
                            e => new { e.OrderId, e.ProductId });
                        b.HasOne(e => e.Order).WithMany(e => e.OrderDetails).HasForeignKey(e => e.OrderId);
                        b.HasOne(e => e.Product).WithMany(e => e.OrderDetails).HasForeignKey(e => e.ProductId);
                    });

                modelBuilder.Entity<OptionalProduct>();
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(EarlyLearningCenter));
        }
    }
}
