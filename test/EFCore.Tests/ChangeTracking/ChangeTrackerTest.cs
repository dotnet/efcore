// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        [Fact]
        public void DetectChanges_is_logged()
        {
            Seed();

            using (var context = new LikeAZooContext())
            {
                _loggerFactory.Log.Clear();

                context.SaveChanges();

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.DetectChangesStarting.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(CoreStrings.LogDetectChangesStarting.GenerateMessage(nameof(LikeAZooContext)), message);

                (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.DetectChangesCompleted.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(CoreStrings.LogDetectChangesCompleted.GenerateMessage(nameof(LikeAZooContext)), message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detect_property_change_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                var cat = context.Cats.Find(1);

                _loggerFactory.Log.Clear();

                cat.Name = "Smoke-a-doke";
                context.ChangeTracker.DetectChanges();

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogPropertyChangeDetectedSensitive.GenerateMessage(nameof(Cat), nameof(Cat.Name), "Smokey", "Smoke-a-doke", "{Id: 1}")
                        : CoreStrings.LogPropertyChangeDetected.GenerateMessage(nameof(Cat), nameof(Cat.Name)),
                    message);

                _loggerFactory.Log.Clear();

                cat.Name = "Little Artichoke";
                context.ChangeTracker.DetectChanges();

                Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detect_foreign_key_property_change_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

                _loggerFactory.Log.Clear();

                var hat = cat.Hats.Single(h => h.Id == 77);
                hat.CatId = 2;
                context.ChangeTracker.DetectChanges();

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ForeignKeyChangeDetected.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogForeignKeyChangeDetectedSensitive.GenerateMessage(nameof(Hat), nameof(Hat.CatId), 1, 2, "{Id: 77}")
                        : CoreStrings.LogForeignKeyChangeDetected.GenerateMessage(nameof(Hat), nameof(Hat.CatId)),
                    message);

                _loggerFactory.Log.Clear();

                hat.CatId = 1;
                context.ChangeTracker.DetectChanges();

                (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ForeignKeyChangeDetected.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogForeignKeyChangeDetectedSensitive.GenerateMessage(nameof(Hat), nameof(Hat.CatId), 2, 1, "{Id: 77}")
                        : CoreStrings.LogForeignKeyChangeDetected.GenerateMessage(nameof(Hat), nameof(Hat.CatId)),
                    message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detect_collection_change_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);
                var hat = cat.Hats.Single(h => h.Id == 77);

                _loggerFactory.Log.Clear();

                cat.Hats.Clear();
                context.ChangeTracker.DetectChanges();

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CollectionChangeDetected.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogCollectionChangeDetectedSensitive.GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Hats), "{Id: 1}")
                        : CoreStrings.LogCollectionChangeDetected.GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Hats)),
                    message);

                _loggerFactory.Log.Clear();

                cat.Hats.Add(hat);
                context.ChangeTracker.DetectChanges();

                (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CollectionChangeDetected.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogCollectionChangeDetectedSensitive.GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Hats), "{Id: 1}")
                        : CoreStrings.LogCollectionChangeDetected.GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Hats)),
                    message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detect_reference_change_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);
                var hat = cat.Hats.Single(h => h.Id == 77);

                _loggerFactory.Log.Clear();

                hat.Cat = null;
                context.ChangeTracker.DetectChanges();

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ReferenceChangeDetected.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogReferenceChangeDetectedSensitive.GenerateMessage(nameof(Hat), nameof(Hat.Cat), "{Id: 77}")
                        : CoreStrings.LogReferenceChangeDetected.GenerateMessage(nameof(Hat), nameof(Hat.Cat)),
                    message);

                _loggerFactory.Log.Clear();

                hat.Cat = cat;
                context.ChangeTracker.DetectChanges();

                (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ReferenceChangeDetected.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogReferenceChangeDetectedSensitive.GenerateMessage(nameof(Hat), nameof(Hat.Cat), "{Id: 77}")
                        : CoreStrings.LogReferenceChangeDetected.GenerateMessage(nameof(Hat), nameof(Hat.Cat)),
                    message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Start_tracking_is_logged_from_query(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                _loggerFactory.Log.Clear();
                context.Cats.Find(1);

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StartedTracking.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogStartedTrackingSensitive.GenerateMessage(nameof(LikeAZooContextSensitive), nameof(Cat), "{Id: 1}")
                        : CoreStrings.LogStartedTracking.GenerateMessage(nameof(LikeAZooContext), nameof(Cat)),
                    message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Start_tracking_is_logged_from_attach(bool sensitive)
        {
            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                _loggerFactory.Log.Clear();
                context.Attach(new Hat(88));

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StartedTracking.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogStartedTrackingSensitive.GenerateMessage(nameof(LikeAZooContextSensitive), nameof(Hat), "{Id: 88}")
                        : CoreStrings.LogStartedTracking.GenerateMessage(nameof(LikeAZooContext), nameof(Hat)),
                    message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void State_change_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                var cat = context.Cats.Find(1);

                _loggerFactory.Log.Clear();

                context.Entry(cat).State = EntityState.Deleted;

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StateChanged.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogStateChangedSensitive.GenerateMessage(nameof(Cat), "{Id: 1}", nameof(LikeAZooContextSensitive), EntityState.Unchanged, EntityState.Deleted)
                        : CoreStrings.LogStateChanged.GenerateMessage(nameof(Cat), nameof(LikeAZooContext), EntityState.Unchanged, EntityState.Deleted),
                    message);
            }
        }

        [Theory]
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
            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
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
                            ? CoreStrings.LogTempValueGeneratedSensitive.GenerateMessage(nameof(LikeAZooContextSensitive), 1, nameof(Hat.Id), nameof(Hat))
                            : CoreStrings.LogTempValueGenerated.GenerateMessage(nameof(LikeAZooContext), nameof(Hat.Id), nameof(Hat)),
                        message);
                }
                else
                {
                    Assert.Equal(
                        sensitive
                            ? CoreStrings.LogValueGeneratedSensitive.GenerateMessage(nameof(LikeAZooContextSensitive), 1, nameof(Hat.Id), nameof(Hat))
                            : CoreStrings.LogValueGenerated.GenerateMessage(nameof(LikeAZooContext), nameof(Hat.Id), nameof(Hat)),
                        message);
                }
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

            public override bool GeneratesTemporaryValues => _generatesTemporaryValues;

            public override int Next(EntityEntry entry)
                => Interlocked.Increment(ref _current);

            public void Reset(bool generateTemporaryValues)
            {
                _generatesTemporaryValues = generateTemporaryValues;
                _current = 0;
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Cascade_delete_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

                context.Entry(cat).State = EntityState.Deleted;

                _loggerFactory.Log.Clear();

                context.SaveChanges();

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CascadeDelete.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogCascadeDeleteSensitive.GenerateMessage(nameof(Hat), "{Id: 77}", EntityState.Deleted, nameof(Cat), "{Id: 1}")
                        : CoreStrings.LogCascadeDelete.GenerateMessage(nameof(Hat), EntityState.Deleted, nameof(Cat)),
                    message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Cascade_delete_orphan_is_logged(bool sensitive)
        {
            Seed(sensitive);

            using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
            {
                var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

                cat.Hats.Clear();

                _loggerFactory.Log.Clear();

                context.SaveChanges();

                var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CascadeDeleteOrphan.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(
                    sensitive
                        ? CoreStrings.LogCascadeDeleteOrphanSensitive.GenerateMessage(nameof(Hat), "{Id: 77}", EntityState.Deleted, nameof(Cat))
                        : CoreStrings.LogCascadeDeleteOrphan.GenerateMessage(nameof(Hat), EntityState.Deleted, nameof(Cat)),
                    message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SaveChanges_is_logged(bool async)
        {
            Seed();

            using (var context = new LikeAZooContext())
            {
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
                Assert.Equal(CoreStrings.LogSaveChangesStarting.GenerateMessage(nameof(LikeAZooContext)), message);

                (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SaveChangesCompleted.Id);
                Assert.Equal(LogLevel.Debug, level);
                Assert.Equal(CoreStrings.LogSaveChangesCompleted.GenerateMessage(nameof(LikeAZooContext), 1), message);
            }
        }

        [Fact]
        public void Context_Dispose_is_logged()
        {
            using (var context = new LikeAZooContext())
            {
                context.Cats.Find(1);

                _loggerFactory.Log.Clear();
            }

            var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ContextDisposed.Id);
            Assert.Equal(LogLevel.Debug, level);
            Assert.Equal(CoreStrings.LogContextDisposed.GenerateMessage(nameof(LikeAZooContext)), message);
        }

        [Fact]
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
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 1, EntityState.Unchanged, tracked[0], fromQuery: true);
                AssertTrackedEvent(context, 2, EntityState.Unchanged, tracked[1], fromQuery: true);
            }

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

                Assert.Equal(2, tracked.Count);
                Assert.Equal(0, changed.Count);
            }
        }

        [Fact]
        public void State_change_events_fire_from_Attach()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                context.Attach(new Cat(1));

                Assert.Equal(1, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 1, EntityState.Unchanged, tracked[0], fromQuery: false);

                context.Entry(new Cat(2)).State = EntityState.Unchanged;

                Assert.Equal(2, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 2, EntityState.Unchanged, tracked[1], fromQuery: false);
            }
        }

        [Fact]
        public void State_change_events_fire_from_Add()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                context.Add(new Cat(1));

                Assert.Equal(1, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 1, EntityState.Added, tracked[0], fromQuery: false);

                context.Entry(new Cat(2)).State = EntityState.Added;

                Assert.Equal(2, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 2, EntityState.Added, tracked[1], fromQuery: false);
            }
        }

        [Fact]
        public void State_change_events_fire_from_Update()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                context.Update(new Cat(1));

                Assert.Equal(1, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 1, EntityState.Modified, tracked[0], fromQuery: false);

                context.Entry(new Cat(2)).State = EntityState.Modified;

                Assert.Equal(2, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 2, EntityState.Modified, tracked[1], fromQuery: false);
            }
        }

        [Fact]
        public void State_change_events_fire_for_tracked_state_changes()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                context.AddRange(new Cat(1), new Cat(2));

                Assert.Equal(2, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 1, EntityState.Added, tracked[0], fromQuery: false);
                AssertTrackedEvent(context, 2, EntityState.Added, tracked[1], fromQuery: false);

                context.Entry(context.Cats.Find(1)).State = EntityState.Unchanged;
                context.Entry(context.Cats.Find(2)).State = EntityState.Modified;

                Assert.Equal(2, tracked.Count);
                Assert.Equal(2, changed.Count);

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

        [Fact]
        public void State_change_events_fire_when_saving_changes()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            Seed(usePool: true);

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                var cat1 = context.Cats.Find(1);

                Assert.Equal(1, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 1, EntityState.Unchanged, tracked[0], fromQuery: true);

                context.Add(new Cat(3));
                cat1.Name = "Clippy";

                context.ChangeTracker.DetectChanges();

                Assert.Equal(2, tracked.Count);
                Assert.Equal(1, changed.Count);

                AssertTrackedEvent(context, 3, EntityState.Added, tracked[1], fromQuery: false);
                AssertChangedEvent(context, 1, EntityState.Unchanged, EntityState.Modified, changed[0]);

                context.SaveChanges();

                Assert.Equal(2, tracked.Count);
                Assert.Equal(3, changed.Count);

                AssertChangedEvent(context, 1, EntityState.Modified, EntityState.Unchanged, changed[1]);
                AssertChangedEvent(context, 3, EntityState.Added, EntityState.Unchanged, changed[2]);

                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void State_change_events_fire_when_property_modified_flags_cause_state_change()
        {
            var tracked = new List<EntityTrackedEventArgs>();
            var changed = new List<EntityStateChangedEventArgs>();

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked, changed);

                var cat = context.Attach(
                    new Cat(3)
                    {
                        Name = "Achilles"
                    }).Entity;

                Assert.Equal(1, tracked.Count);
                Assert.Equal(0, changed.Count);

                AssertTrackedEvent(context, 3, EntityState.Unchanged, tracked[0], fromQuery: false);

                context.Entry(cat).Property(e => e.Name).IsModified = true;

                Assert.Equal(1, tracked.Count);
                Assert.Equal(1, changed.Count);

                AssertChangedEvent(context, 3, EntityState.Unchanged, EntityState.Modified, changed[0]);

                context.Entry(cat).Property(e => e.Name).IsModified = false;

                Assert.Equal(1, tracked.Count);
                Assert.Equal(2, changed.Count);

                AssertChangedEvent(context, 3, EntityState.Modified, EntityState.Unchanged, changed[1]);
            }
        }

        [Fact]
        public void State_change_events_are_limited_to_the_current_context()
        {
            var tracked1 = new List<EntityTrackedEventArgs>();
            var changed1 = new List<EntityStateChangedEventArgs>();
            var tracked2 = new List<EntityTrackedEventArgs>();
            var changed2 = new List<EntityStateChangedEventArgs>();

            Seed(usePool: true);

            using (var scope = _poolProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<LikeAZooContextPooled>();

                RegisterEvents(context, tracked1, changed1);

                using (var scope2 = _poolProvider.CreateScope())
                {
                    var context2 = scope2.ServiceProvider.GetService<LikeAZooContextPooled>();

                    RegisterEvents(context2, tracked2, changed2);

                    Assert.Equal(2, context2.Cats.OrderBy(e => e.Id).ToList().Count);

                    Assert.Equal(2, tracked2.Count);
                    Assert.Equal(0, changed2.Count);

                    context2.Entry(context2.Cats.Find(1)).State = EntityState.Modified;

                    Assert.Equal(2, tracked2.Count);
                    Assert.Equal(1, changed2.Count);

                    Assert.Equal(0, tracked1.Count);
                    Assert.Equal(0, changed1.Count);
                }

                Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

                Assert.Equal(2, tracked1.Count);
                Assert.Equal(0, changed1.Count);

                context.Entry(context.Cats.Find(1)).State = EntityState.Modified;

                Assert.Equal(2, tracked1.Count);
                Assert.Equal(1, changed1.Count);

                Assert.Equal(2, tracked2.Count);
                Assert.Equal(1, changed2.Count);

                context.Database.EnsureDeleted();
            }
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
            public Cat(int id) => Id = id;

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public int Id { get; private set; }

            public string Name { get; set; }

            public ICollection<Hat> Hats { get; } = new List<Hat>();
        }

        private class Hat
        {
            public Hat(int id) => Id = id;

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public int Id { get; private set; }

            public string Color { get; set; }

            public int CatId { get; set; }
            public Cat Cat { get; set; }
        }

        private static readonly ListLoggerFactory _loggerFactory
            = new ListLoggerFactory();

        private static readonly IServiceProvider _poolProvider
            = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContextPool<LikeAZooContextPooled>(
                    p => p.UseInMemoryDatabase(nameof(LikeAZooContextPooled))
                        .UseLoggerFactory(_loggerFactory))
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
                    .UseLoggerFactory(_loggerFactory)
                    .UseInMemoryDatabase(nameof(LikeAZooContext));
        }

        private class LikeAZooContextSensitive : LikeAZooContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(_loggerFactory)
                    .UseInMemoryDatabase(nameof(LikeAZooContextSensitive));
        }

        private void Seed(bool sensitive = false, bool usePool = false)
        {
            void Seed(LikeAZooContext context)
            {
                context.Database.EnsureDeleted();

                var cat1 = new Cat(1) { Name = "Smokey" };
                var cat2 = new Cat(2) { Name = "Sid" };

                cat1.Hats.Add(
                    new Hat(77) { Color = "Pine Green" });

                context.AddRange(cat1, cat2);

                context.SaveChanges();
            }

            if (usePool)
            {
                using (var scope = _poolProvider.CreateScope())
                {
                    Seed(scope.ServiceProvider.GetService<LikeAZooContextPooled>());
                }
            }
            else
            {
                using (var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext())
                {
                    Seed(context);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_remove_dependent_identifying_one_to_many(bool saveEntities)
        {
            using (var context = new EarlyLearningCenter())
            {
                var product = new Product();
                var order = new Order();
                var orderDetails = new OrderDetails
                {
                    Order = order,
                    Product = product
                };

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
        }

        [Fact]
        public void View_type_negative_cases()
        {
            using (var context = new EarlyLearningCenter())
            {
                var whoAmI = new WhoAmI();

                Assert.Equal(
                    CoreStrings.QueryTypeNotValid("WhoAmI"),
                    Assert.Throws<InvalidOperationException>(() => context.Add(whoAmI)).Message);

                Assert.Equal(
                    CoreStrings.QueryTypeNotValid("WhoAmI"),
                    Assert.Throws<InvalidOperationException>(() => context.Remove(whoAmI)).Message);

                Assert.Equal(
                    CoreStrings.QueryTypeNotValid("WhoAmI"),
                    Assert.Throws<InvalidOperationException>(() => context.Attach(whoAmI)).Message);

                Assert.Equal(
                    CoreStrings.QueryTypeNotValid("WhoAmI"),
                    Assert.Throws<InvalidOperationException>(() => context.Update(whoAmI)).Message);

                Assert.Equal(
                    CoreStrings.InvalidSetTypeQuery("WhoAmI"),
                    Assert.Throws<InvalidOperationException>(() => context.Find<WhoAmI>(1)).Message);

                Assert.Equal(
                    CoreStrings.InvalidSetTypeQuery("WhoAmI"),
                    Assert.Throws<InvalidOperationException>(() => context.Set<WhoAmI>().ToList()).Message);

                Assert.Equal(
                    CoreStrings.InvalidSetTypeEntity("Sweet"),
                    Assert.Throws<InvalidOperationException>(() => context.Query<Sweet>().ToList()).Message);

                Assert.Equal(
                    CoreStrings.QueryTypeNotValid("WhoAmI"),
                    Assert.Throws<InvalidOperationException>(() => context.Entry(whoAmI)).Message);
            }
        }

        [Fact]
        public void Can_get_all_entries()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = context.Add(new Category()).Entity;
                var product = context.Add(new Product()).Entity;

                Assert.Equal(
                    new object[] { category, product },
                    context.ChangeTracker.Entries().Select(e => e.Entity).OrderBy(e => e.GetType().Name));
            }
        }

        [Fact]
        public void Can_get_all_entities_for_an_entity_of_a_given_type()
        {
            using (var context = new EarlyLearningCenter())
            {
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
        }

        [Fact]
        public void Can_get_state_manager()
        {
            using (var context = new EarlyLearningCenter())
            {
                var stateManger = context.GetService<IStateManager>();

                Assert.Same(stateManger, context.ChangeTracker.GetInfrastructure());
            }
        }

        [Fact]
        public void Can_get_Context()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Same(context, context.ChangeTracker.Context);
            }
        }

        private static string NodeString(EntityEntryGraphNode node)
            => EntryString(node.SourceEntry)
               + " ---" + node.InboundNavigation?.Name + "--> "
               + EntryString(node.Entry);

        private static string EntryString(EntityEntry entry)
            => entry == null
                ? "<None>"
                : entry.Metadata.DisplayName()
                  + ":" + entry.Property(entry.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue;

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Can_attach_nullable_PK_parent_with_child_collection(bool useAttach, bool setKeys)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new NullbileCategory
                {
                    Products = new List<NullbileProduct>
                    {
                        new NullbileProduct(),
                        new NullbileProduct(),
                        new NullbileProduct()
                    }
                };

                if (setKeys)
                {
                    context.Entry(category).Property("Id").CurrentValue = 1;
                    context.Entry(category.Products[0]).Property("Id").CurrentValue = 1;
                    context.Entry(category.Products[1]).Property("Id").CurrentValue = 2;
                    context.Entry(category.Products[2]).Property("Id").CurrentValue = 3;
                }

                if (useAttach)
                {
                    context.Attach(category);
                }
                else
                {
                    var traversal = new List<string>();

                    context.ChangeTracker.TrackGraph(
                        category, e =>
                        {
                            e.Entry.State = e.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
                            traversal.Add(NodeString(e));
                        });

                    Assert.Equal(
                        new List<string>
                        {
                            "<None> -----> NullbileCategory:1",
                            "NullbileCategory:1 ---Products--> NullbileProduct:1",
                            "NullbileCategory:1 ---Products--> NullbileProduct:2",
                            "NullbileCategory:1 ---Products--> NullbileProduct:3"
                        },
                        traversal);
                }

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var categoryEntry = context.Entry(category);
                var product0Entry = context.Entry(category.Products[0]);
                var product1Entry = context.Entry(category.Products[1]);
                var product2Entry = context.Entry(category.Products[2]);

                var expectedState = setKeys ? EntityState.Unchanged : EntityState.Added;
                Assert.Equal(expectedState, categoryEntry.State);
                Assert.Equal(expectedState, product0Entry.State);
                Assert.Equal(expectedState, product1Entry.State);
                Assert.Equal(expectedState, product2Entry.State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                var categoryId = categoryEntry.Property("Id").CurrentValue;
                Assert.NotNull(categoryId);

                Assert.Equal(categoryId, product0Entry.Property("CategoryId").CurrentValue);
                Assert.Equal(categoryId, product1Entry.Property("CategoryId").CurrentValue);
                Assert.Equal(categoryId, product2Entry.Property("CategoryId").CurrentValue);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Can_attach_nullable_PK_parent_with_one_to_one_children(bool useAttach, bool setKeys)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new NullbileCategory
                {
                    Info = new NullbileCategoryInfo()
                };

                if (setKeys)
                {
                    context.Entry(category).Property("Id").CurrentValue = 1;
                    context.Entry(category.Info).Property("Id").CurrentValue = 1;
                }

                if (useAttach)
                {
                    context.Attach(category);
                }
                else
                {
                    var traversal = new List<string>();

                    context.ChangeTracker.TrackGraph(
                        category, e =>
                        {
                            e.Entry.State = e.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
                            traversal.Add(NodeString(e));
                        });

                    Assert.Equal(
                        new List<string>
                        {
                            "<None> -----> NullbileCategory:1",
                            "NullbileCategory:1 ---Info--> NullbileCategoryInfo:1"
                        },
                        traversal);
                }

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var expectedState = setKeys ? EntityState.Unchanged : EntityState.Added;
                Assert.Equal(expectedState, context.Entry(category).State);
                Assert.Equal(expectedState, context.Entry(category.Info).State);

                Assert.Same(category, category.Info.Category);
            }
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void Can_attach_parent_with_owned_dependent(bool useAttach, bool setPrincipalKey, bool setDependentKey)
        {
            using (var context = new EarlyLearningCenter())
            {
                var sweet = new Sweet
                {
                    Dreams = new Dreams
                    {
                        AreMade = new AreMadeOfThis(),
                        OfThis = new AreMadeOfThis()
                    }
                };

                if (setPrincipalKey)
                {
                    sweet.Id = 1;
                }

                if (setDependentKey)
                {
                    var dreamsEntry = context.Entry(sweet).Reference(e => e.Dreams).TargetEntry;
                    dreamsEntry.Property("SweetId").CurrentValue = 1;
                    dreamsEntry.Reference(e => e.AreMade).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
                    dreamsEntry.Reference(e => e.OfThis).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
                }

                if (useAttach)
                {
                    context.Attach(sweet);
                }
                else
                {
                    var traversal = new List<string>();

                    context.ChangeTracker.TrackGraph(
                        sweet, e =>
                        {
                            if (e.Entry.Metadata.IsOwned())
                            {
                                e.Entry.State = e.SourceEntry.State;
                            }
                            else
                            {
                                e.Entry.State = e.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;
                            }

                            traversal.Add(NodeString(e));
                        });

                    Assert.Equal(
                        new List<string>
                        {
                            "<None> -----> Sweet:1",
                            "Sweet:1 ---Dreams--> Dreams:1",
                            "Dreams:1 ---AreMade--> Dreams.AreMade#AreMadeOfThis:1",
                            "Dreams:1 ---OfThis--> Dreams.OfThis#AreMadeOfThis:1"
                        },
                        traversal);
                }

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var dependentEntry = context.Entry(sweet.Dreams);
                var dependentEntry2a = context.Entry(sweet.Dreams.AreMade);
                var dependentEntry2b = context.Entry(sweet.Dreams.OfThis);

                var expectedPrincipalState = setPrincipalKey ? EntityState.Unchanged : EntityState.Added;
                var expectedDependentState = setPrincipalKey || (setDependentKey && useAttach) ? EntityState.Unchanged : EntityState.Added;

                Assert.Equal(expectedPrincipalState, context.Entry(sweet).State);
                Assert.Equal(expectedDependentState, dependentEntry.State);
                Assert.Equal(expectedDependentState, dependentEntry2a.State);
                Assert.Equal(expectedDependentState, dependentEntry2b.State);

                Assert.Equal(1, sweet.Id);
                Assert.Equal(1, dependentEntry.Property(dependentEntry.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
                Assert.Equal(1, dependentEntry2a.Property(dependentEntry2a.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
                Assert.Equal(1, dependentEntry2b.Property(dependentEntry2b.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Can_attach_owned_dependent_with_reference_to_parent(bool useAttach, bool setDependentKey)
        {
            using (var context = new EarlyLearningCenter())
            {
                var dreams = new Dreams
                {
                    Sweet = new Sweet
                    {
                        Id = 1
                    },
                    AreMade = new AreMadeOfThis(),
                    OfThis = new AreMadeOfThis()
                };

                if (setDependentKey)
                {
                    var dreamsEntry = context.Entry(dreams);
                    dreamsEntry.Property("SweetId").CurrentValue = 1;
                    dreamsEntry.Reference(e => e.AreMade).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
                    dreamsEntry.Reference(e => e.OfThis).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
                }

                if (useAttach)
                {
                    context.Attach(dreams);
                }
                else
                {
                    var traversal = new List<string>();

                    context.ChangeTracker.TrackGraph(
                        dreams, e =>
                        {
                            e.Entry.State = e.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;

                            traversal.Add(NodeString(e));
                        });

                    Assert.Equal(
                        new List<string>
                        {
                            "<None> -----> Dreams:1",
                            "Dreams:1 ---AreMade--> Dreams.AreMade#AreMadeOfThis:1",
                            "Dreams:1 ---OfThis--> Dreams.OfThis#AreMadeOfThis:1",
                            "Dreams:1 ---Sweet--> Sweet:1"
                        },
                        traversal);
                }

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var dependentEntry = context.Entry(dreams);
                var dependentEntry2a = context.Entry(dreams.AreMade);
                var dependentEntry2b = context.Entry(dreams.OfThis);

                var expectedPrincipalState = EntityState.Unchanged;
                var expectedDependentState = setDependentKey ? EntityState.Unchanged : EntityState.Added;

                Assert.Equal(expectedPrincipalState, context.Entry(dreams.Sweet).State);
                Assert.Equal(expectedDependentState, dependentEntry.State);
                Assert.Equal(expectedDependentState, dependentEntry2a.State);
                Assert.Equal(expectedDependentState, dependentEntry2b.State);

                Assert.Equal(1, dreams.Sweet.Id);
                Assert.Equal(1, dependentEntry.Property(dependentEntry.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
                Assert.Equal(1, dependentEntry2a.Property(dependentEntry2a.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
                Assert.Equal(1, dependentEntry2b.Property(dependentEntry2b.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
            }
        }

        [Fact]
        public void Can_attach_parent_with_child_collection()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                {
                    Id = 1,
                    Products = new List<Product>
                    {
                        new Product
                        {
                            Id = 1
                        },
                        new Product
                        {
                            Id = 2
                        },
                        new Product
                        {
                            Id = 3
                        }
                    }
                };

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    category, e =>
                    {
                        traversal.Add(NodeString(e));
                        e.Entry.State = EntityState.Modified;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Category:1",
                        "Category:1 ---Products--> Product:1",
                        "Category:1 ---Products--> Product:2",
                        "Category:1 ---Products--> Product:3"
                    },
                    traversal);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Modified, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[2]).State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        [Fact]
        public void Can_attach_child_with_reference_to_parent()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product = new Product
                {
                    Id = 1,
                    Category = new Category
                    {
                        Id = 1
                    }
                };

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    product, e =>
                    {
                        traversal.Add(NodeString(e));
                        e.Entry.State = EntityState.Modified;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Product:1",
                        "Product:1 ---Category--> Category:1"
                    },
                    traversal);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Modified, context.Entry(product).State);
                Assert.Equal(EntityState.Modified, context.Entry(product.Category).State);

                Assert.Same(product, product.Category.Products[0]);
                Assert.Equal(product.Category.Id, product.CategoryId);
            }
        }

        [Fact]
        public void Can_attach_parent_with_one_to_one_children()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product = new Product
                {
                    Id = 1,
                    Details = new ProductDetails
                    {
                        Id = 1,
                        Tag = new ProductDetailsTag
                        {
                            Id = 1
                        }
                    }
                };

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    product, e =>
                    {
                        traversal.Add(NodeString(e));
                        e.Entry.State = EntityState.Unchanged;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Product:1",
                        "Product:1 ---Details--> ProductDetails:1",
                        "ProductDetails:1 ---Tag--> ProductDetailsTag:1"
                    },
                    traversal);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product.Details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product.Details.Tag).State);

                Assert.Same(product, product.Details.Product);
                Assert.Same(product.Details, product.Details.Tag.Details);
            }
        }

        [Fact]
        public void Can_attach_child_with_one_to_one_parents()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tag = new ProductDetailsTag
                {
                    Id = 1,
                    Details = new ProductDetails
                    {
                        Id = 1,
                        Product = new Product
                        {
                            Id = 1
                        }
                    }
                };

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    tag, e =>
                    {
                        traversal.Add(NodeString(e));
                        e.Entry.State = EntityState.Unchanged;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> ProductDetailsTag:1",
                        "ProductDetailsTag:1 ---Details--> ProductDetails:1",
                        "ProductDetails:1 ---Product--> Product:1"
                    },
                    traversal);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(tag).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(tag.Details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(tag.Details.Product).State);

                Assert.Same(tag, tag.Details.Tag);
                Assert.Same(tag.Details, tag.Details.Product.Details);
            }
        }

        [Fact]
        public void Can_attach_entity_with_one_to_one_parent_and_child()
        {
            using (var context = new EarlyLearningCenter())
            {
                var details = new ProductDetails
                {
                    Id = 1,
                    Product = new Product
                    {
                        Id = 1
                    },
                    Tag = new ProductDetailsTag
                    {
                        Id = 1
                    }
                };

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    details, e =>
                    {
                        traversal.Add(NodeString(e));
                        e.Entry.State = EntityState.Unchanged;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> ProductDetails:1",
                        "ProductDetails:1 ---Product--> Product:1",
                        "ProductDetails:1 ---Tag--> ProductDetailsTag:1"
                    },
                    traversal);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(details.Product).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(details.Tag).State);

                Assert.Same(details, details.Tag.Details);
                Assert.Same(details, details.Product.Details);
            }
        }

        [Fact]
        public void Entities_that_are_already_tracked_will_not_get_attached()
        {
            using (var context = new EarlyLearningCenter())
            {
                var existingProduct = context.Attach(
                    new Product
                    {
                        Id = 2,
                        CategoryId = 1
                    }).Entity;

                var category = new Category
                {
                    Id = 1,
                    Products = new List<Product>
                    {
                        new Product
                        {
                            Id = 1
                        },
                        existingProduct,
                        new Product
                        {
                            Id = 3
                        }
                    }
                };

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    category, e =>
                    {
                        traversal.Add(NodeString(e));
                        e.Entry.State = EntityState.Modified;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Category:1",
                        "Category:1 ---Products--> Product:1",
                        "Category:1 ---Products--> Product:3"
                    },
                    traversal);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Modified, context.Entry(category).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Modified, context.Entry(category.Products[2]).State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        [Fact]
        public void Further_graph_traversal_stops_if_an_entity_is_not_attached()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                {
                    Id = 1,
                    Products = new List<Product>
                    {
                        new Product
                        {
                            Id = 1,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 1
                            }
                        },
                        new Product
                        {
                            Id = 2,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 2
                            }
                        },
                        new Product
                        {
                            Id = 3,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 3
                            }
                        }
                    }
                };

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    category, e =>
                    {
                        traversal.Add(NodeString(e));
                        if (!(e.Entry.Entity is Product product)
                            || product.Id != 2)
                        {
                            e.Entry.State = EntityState.Unchanged;
                        }
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Category:1",
                        "Category:1 ---Products--> Product:1",
                        "Product:1 ---Details--> ProductDetails:1",
                        "Category:1 ---Products--> Product:2",
                        "Category:1 ---Products--> Product:3",
                        "Product:3 ---Details--> ProductDetails:3"
                    },
                    traversal);

                Assert.Equal(5, context.ChangeTracker.Entries().Count(e => e.State != EntityState.Detached));

                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0].Details).State);
                Assert.Equal(EntityState.Detached, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Detached, context.Entry(category.Products[1].Details).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2].Details).State);

                Assert.Same(category, category.Products[0].Category);
                Assert.Null(category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);

                Assert.Same(category.Products[0], category.Products[0].Details.Product);
                Assert.Null(category.Products[1].Details.Product);
                Assert.Same(category.Products[2], category.Products[2].Details.Product);
            }
        }

        [Fact]
        public void Graph_iterator_does_not_go_visit_Apple()
        {
            using (var context = new EarlyLearningCenter())
            {
                var details = new ProductDetails
                {
                    Id = 1,
                    Product = new Product
                    {
                        Id = 1
                    }
                };
                details.Product.Details = details;

                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(details, e => traversal.Add(NodeString(e)));

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> ProductDetails:1"
                    },
                    traversal);

                Assert.Equal(0, context.ChangeTracker.Entries().Count(e => e.State != EntityState.Detached));
            }
        }

        [Fact]
        public void Can_attach_parent_with_some_new_and_some_existing_entities()
        {
            KeyValueAttachTest(
                (category, changeTracker) =>
                {
                    var traversal = new List<string>();

                    changeTracker.TrackGraph(
                        category,
                        e =>
                        {
                            traversal.Add(NodeString(e));
                            e.Entry.State = e.Entry.Entity is Product product && product.Id == 0
                                ? EntityState.Added
                                : EntityState.Unchanged;
                        });

                    Assert.Equal(
                        new List<string>
                        {
                            "<None> -----> Category:77",
                            "Category:77 ---Products--> Product:77",
                            "Category:77 ---Products--> Product:0",
                            "Category:77 ---Products--> Product:78"
                        },
                        traversal);
                });
        }

        [Fact]
        public void Can_attach_graph_using_built_in_tracker()
        {
            var tracker = new KeyValueEntityTracker(updateExistingEntities: false);

            KeyValueAttachTest((category, changeTracker) => changeTracker.TrackGraph(category, tracker.TrackEntity));
        }

        [Fact]
        public void Can_update_graph_using_built_in_tracker()
        {
            var tracker = new KeyValueEntityTracker(updateExistingEntities: true);

            KeyValueAttachTest((category, changeTracker) => changeTracker.TrackGraph(category, tracker.TrackEntity), expectModified: true);
        }

        private static void KeyValueAttachTest(Action<Category, ChangeTracker> tracker, bool expectModified = false)
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                {
                    Id = 77,
                    Products = new List<Product>
                    {
                        new Product
                        {
                            Id = 77,
                            CategoryId = expectModified ? 0 : 77
                        },
                        new Product
                        {
                            Id = 0,
                            CategoryId = expectModified ? 0 : 77
                        },
                        new Product
                        {
                            Id = 78,
                            CategoryId = expectModified ? 0 : 77
                        }
                    }
                };

                tracker(category, context.ChangeTracker);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var nonAddedState = expectModified ? EntityState.Modified : EntityState.Unchanged;

                Assert.Equal(nonAddedState, context.Entry(category).State);
                Assert.Equal(nonAddedState, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Added, context.Entry(category.Products[1]).State);
                Assert.Equal(nonAddedState, context.Entry(category.Products[2]).State);

                Assert.Equal(77, category.Products[0].Id);
                Assert.Equal(1, category.Products[1].Id);
                Assert.Equal(78, category.Products[2].Id);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        [Fact]
        public void Can_attach_graph_using_custom_delegate()
        {
            var tracker = new MyTracker(updateExistingEntities: false);

            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                {
                    Id = 77,
                    Products = new List<Product>
                    {
                        new Product
                        {
                            Id = 77,
                            CategoryId = 77
                        },
                        new Product
                        {
                            Id = 0,
                            CategoryId = 77
                        },
                        new Product
                        {
                            Id = 78,
                            CategoryId = 77
                        }
                    }
                };

                context.ChangeTracker.TrackGraph(category, tracker.TrackEntity);

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[0]).State);
                Assert.Equal(EntityState.Added, context.Entry(category.Products[1]).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(category.Products[2]).State);

                Assert.Equal(77, category.Products[0].Id);
                Assert.Equal(777, category.Products[1].Id);
                Assert.Equal(78, category.Products[2].Id);

                Assert.Same(category, category.Products[0].Category);
                Assert.Same(category, category.Products[1].Category);
                Assert.Same(category, category.Products[2].Category);

                Assert.Equal(category.Id, category.Products[0].CategoryId);
                Assert.Equal(category.Id, category.Products[1].CategoryId);
                Assert.Equal(category.Id, category.Products[2].CategoryId);
            }
        }

        private class MyTracker : KeyValueEntityTracker
        {
            public MyTracker(bool updateExistingEntities)
                : base(updateExistingEntities)
            {
            }

            public override EntityState DetermineState(EntityEntry entry)
            {
                if (!entry.IsKeySet)
                {
                    entry.GetInfrastructure()[entry.Metadata.FindPrimaryKey().Properties.Single()] = 777;
                    return EntityState.Added;
                }

                return base.DetermineState(entry);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void Can_add_owned_dependent_with_reference_to_parent(bool useAdd, bool setDependentKey)
        {
            using (var context = new EarlyLearningCenter())
            {
                var dreams = new Dreams
                {
                    Sweet = new Sweet
                    {
                        Id = 1
                    },
                    AreMade = new AreMadeOfThis(),
                    OfThis = new AreMadeOfThis()
                };

                context.Entry(dreams.Sweet).State = EntityState.Unchanged;

                if (setDependentKey)
                {
                    var dreamsEntry = context.Entry(dreams);
                    dreamsEntry.Property("SweetId").CurrentValue = 1;
                    dreamsEntry.Reference(e => e.AreMade).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
                    dreamsEntry.Reference(e => e.OfThis).TargetEntry.Property("DreamsSweetId").CurrentValue = 1;
                }

                if (useAdd)
                {
                    context.Add(dreams);
                }
                else
                {
                    var traversal = new List<string>();

                    context.ChangeTracker.TrackGraph(
                        dreams, e =>
                        {
                            e.Entry.State = e.Entry.IsKeySet && !e.Entry.Metadata.IsOwned()
                                ? EntityState.Unchanged
                                : EntityState.Added;

                            traversal.Add(NodeString(e));
                        });

                    Assert.Equal(
                        new List<string>
                        {
                            "<None> -----> Dreams:1",
                            "Dreams:1 ---AreMade--> Dreams.AreMade#AreMadeOfThis:1",
                            "Dreams:1 ---OfThis--> Dreams.OfThis#AreMadeOfThis:1"
                        },
                        traversal);
                }

                Assert.Equal(4, context.ChangeTracker.Entries().Count());

                var dependentEntry = context.Entry(dreams);
                var dependentEntry2a = context.Entry(dreams.AreMade);
                var dependentEntry2b = context.Entry(dreams.OfThis);

                var expectedPrincipalState = EntityState.Unchanged;
                var expectedDependentState = EntityState.Added;

                Assert.Equal(expectedPrincipalState, context.Entry(dreams.Sweet).State);
                Assert.Equal(expectedDependentState, dependentEntry.State);
                Assert.Equal(expectedDependentState, dependentEntry2a.State);
                Assert.Equal(expectedDependentState, dependentEntry2b.State);

                Assert.Equal(1, dreams.Sweet.Id);
                Assert.Equal(1, dependentEntry.Property(dependentEntry.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
                Assert.Equal(1, dependentEntry2a.Property(dependentEntry2a.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
                Assert.Equal(1, dependentEntry2b.Property(dependentEntry2b.Metadata.FindPrimaryKey().Properties[0].Name).CurrentValue);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_principal_and_then_identifying_dependents_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product
                {
                    Details = new ProductDetails
                    {
                        Tag = new ProductDetailsTag
                        {
                            TagDetails = new ProductDetailsTagDetails()
                        }
                    }
                };
                var product2 = new Product
                {
                    Details = new ProductDetails
                    {
                        Tag = new ProductDetailsTag
                        {
                            TagDetails = new ProductDetailsTagDetails()
                        }
                    }
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
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_then_principal_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tagDetails1 = new ProductDetailsTagDetails
                {
                    Tag = new ProductDetailsTag
                    {
                        Details = new ProductDetails
                        {
                            Product = new Product()
                        }
                    }
                };

                var tagDetails2 = new ProductDetailsTagDetails
                {
                    Tag = new ProductDetailsTag
                    {
                        Details = new ProductDetails
                        {
                            Product = new Product()
                        }
                    }
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
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_then_principal_interleaved_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tagDetails1 = new ProductDetailsTagDetails
                {
                    Tag = new ProductDetailsTag
                    {
                        Details = new ProductDetails
                        {
                            Product = new Product()
                        }
                    }
                };

                var tagDetails2 = new ProductDetailsTagDetails
                {
                    Tag = new ProductDetailsTag
                    {
                        Details = new ProductDetails
                        {
                            Product = new Product()
                        }
                    }
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
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_principal_starting_in_the_middle_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var tagDetails1 = new ProductDetailsTagDetails
                {
                    Tag = new ProductDetailsTag
                    {
                        Details = new ProductDetails
                        {
                            Product = new Product()
                        }
                    }
                };

                var tagDetails2 = new ProductDetailsTagDetails
                {
                    Tag = new ProductDetailsTag
                    {
                        Details = new ProductDetails
                        {
                            Product = new Product()
                        }
                    }
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
        }

        [Fact] // Issue #1207
        public void Can_add_principal_and_identifying_dependents_starting_in_the_middle_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product
                {
                    Details = new ProductDetails
                    {
                        Tag = new ProductDetailsTag
                        {
                            TagDetails = new ProductDetailsTagDetails()
                        }
                    }
                };
                var product2 = new Product
                {
                    Details = new ProductDetails
                    {
                        Tag = new ProductDetailsTag
                        {
                            TagDetails = new ProductDetailsTagDetails()
                        }
                    }
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
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_principal_with_post_nav_fixup_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
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

                AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
            }
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_dependents_and_principal_with_reverse_post_nav_fixup_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
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

                AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
            }
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

        [Fact] // Issue #1207
        public void Can_add_identifying_one_to_many_via_principal_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product();
                var product2 = new Product();

                var order1 = new Order();
                var order2 = new Order();

                var orderDetails1a = new OrderDetails
                {
                    Order = order1,
                    Product = product1
                };
                var orderDetails1b = new OrderDetails
                {
                    Order = order1,
                    Product = product2
                };
                var orderDetails2a = new OrderDetails
                {
                    Order = order2,
                    Product = product1
                };
                var orderDetails2b = new OrderDetails
                {
                    Order = order2,
                    Product = product2
                };

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
        }

        [Fact] // Issue #1207
        public void Can_add_identifying_one_to_many_via_dependents_with_key_generation()
        {
            using (var context = new EarlyLearningCenter())
            {
                var product1 = new Product();
                var product2 = new Product();

                var order1 = new Order();
                var order2 = new Order();

                var orderDetails1a = new OrderDetails
                {
                    Order = order1,
                    Product = product1
                };
                var orderDetails1b = new OrderDetails
                {
                    Order = order1,
                    Product = product2
                };
                var orderDetails2a = new OrderDetails
                {
                    Order = order2,
                    Product = product1
                };
                var orderDetails2b = new OrderDetails
                {
                    Order = order2,
                    Product = product2
                };

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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entries_calls_DetectChanges_by_default(bool useGenericOverload)
        {
            using (var context = new EarlyLearningCenter())
            {
                var entry = context.Attach(
                    new Product
                    {
                        Id = 1,
                        CategoryId = 66
                    });

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
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Auto_DetectChanges_for_Entries_can_be_switched_off(bool useGenericOverload)
        {
            using (var context = new EarlyLearningCenter())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var entry = context.Attach(
                    new Product
                    {
                        Id = 1,
                        CategoryId = 66
                    });

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
        }

        [Fact]
        public void Explicitly_calling_DetectChanges_works_even_if_auto_DetectChanges_is_switched_off()
        {
            using (var context = new EarlyLearningCenter())
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var entry = context.Attach(
                    new Product
                    {
                        Id = 1,
                        CategoryId = 66
                    });

                entry.Entity.CategoryId = 77;

                Assert.Equal(EntityState.Unchanged, entry.State);

                context.ChangeTracker.DetectChanges();

                Assert.Equal(EntityState.Modified, entry.State);
            }
        }

        [Fact]
        public void TrackGraph_does_not_call_DetectChanges()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>());
            using (var context = new EarlyLearningCenter(provider))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                changeDetector.DetectChangesCalled = false;

                context.ChangeTracker.TrackGraph(CreateSimpleGraph(2), e => e.Entry.State = EntityState.Unchanged);

                Assert.False(changeDetector.DetectChangesCalled);

                context.ChangeTracker.DetectChanges();

                Assert.True(changeDetector.DetectChangesCalled);
            }
        }

        [Fact]
        public void TrackGraph_overload_can_visit_an_already_attached_graph()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                {
                    Id = 1,
                    Products = new List<Product>
                    {
                        new Product
                        {
                            Id = 1,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 1
                            }
                        },
                        new Product
                        {
                            Id = 2,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 2
                            }
                        },
                        new Product
                        {
                            Id = 3,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 3
                            }
                        }
                    }
                };

                context.Attach(category);

                var visited = new HashSet<object>();
                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    category, visited, (e, v) =>
                    {
                        if (v.Contains(e.Entry.Entity))
                        {
                            return false;
                        }

                        v.Add(e.Entry.Entity);

                        traversal.Add(NodeString(e));

                        return true;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Category:1",
                        "Category:1 ---Products--> Product:1",
                        "Product:1 ---Details--> ProductDetails:1",
                        "Category:1 ---Products--> Product:2",
                        "Product:2 ---Details--> ProductDetails:2",
                        "Category:1 ---Products--> Product:3",
                        "Product:3 ---Details--> ProductDetails:3"
                    },
                    traversal);

                Assert.Equal(7, visited.Count);
            }
        }

        [Fact]
        public void TrackGraph_overload_can_visit_a_graph_without_attaching()
        {
            using (var context = new EarlyLearningCenter())
            {
                var category = new Category
                {
                    Id = 1,
                    Products = new List<Product>
                    {
                        new Product
                        {
                            Id = 1,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 1
                            }
                        },
                        new Product
                        {
                            Id = 2,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 2
                            }
                        },
                        new Product
                        {
                            Id = 3,
                            CategoryId = 1,
                            Details = new ProductDetails
                            {
                                Id = 3
                            }
                        }
                    }
                };

                var visited = new HashSet<object>();
                var traversal = new List<string>();

                context.ChangeTracker.TrackGraph(
                    category, visited, (e, v) =>
                    {
                        if (v.Contains(e.Entry.Entity))
                        {
                            return false;
                        }

                        v.Add(e.Entry.Entity);

                        traversal.Add(NodeString(e));

                        return true;
                    });

                Assert.Equal(
                    new List<string>
                    {
                        "<None> -----> Category:1",
                        "Category:1 ---Products--> Product:1",
                        "Product:1 ---Details--> ProductDetails:1",
                        "Category:1 ---Products--> Product:2",
                        "Product:2 ---Details--> ProductDetails:2",
                        "Category:1 ---Products--> Product:3",
                        "Product:3 ---Details--> ProductDetails:3"
                    },
                    traversal);

                Assert.Equal(7, visited.Count);

                foreach (var entity in new object[] { category }
                    .Concat(category.Products)
                    .Concat(category.Products.Select(e => e.Details)))
                {
                    Assert.Equal(EntityState.Detached, context.Entry(entity).State);
                }
            }
        }

        [Fact]
        public void Does_not_throw_when_instance_of_unmapped_derived_type_is_used()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.Same(
                    context.Model.FindEntityType(typeof(Product)),
                    context.Add(new SpecialProduct()).Metadata);
            }
        }

        [Fact]
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
            using (var context = new TheShadows())
            {
                var entry = context.Entry(context.Set<Dark>().Single(e => EF.Property<int>(e, "Id") == id));

                Assert.Equal(id, entry.Property<int>("Id").CurrentValue);
                Assert.Equal(someInt, entry.Property<int>("SomeInt").CurrentValue);
                Assert.Equal(someString, entry.Property<string>("SomeString").CurrentValue);
            }
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
                => optionsBuilder.UseInMemoryDatabase(nameof(TheShadows));
        }

        private class Dark
        {
        }

        private static Product CreateSimpleGraph(int id)
            => new Product
            {
                Id = id,
                Category = new Category
                {
                    Id = id
                }
            };

        private class ChangeDetectorProxy : ChangeDetector
        {
            public ChangeDetectorProxy(
                IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
                ILoggingOptions loggingOptions)
                : base(logger, loggingOptions)
            {
            }

            public bool DetectChangesCalled { get; set; }

            public override void DetectChanges(InternalEntityEntry entry)
            {
                DetectChangesCalled = true;

                base.DetectChanges(entry);
            }

            public override void DetectChanges(IStateManager stateManager)
            {
                DetectChangesCalled = true;

                base.DetectChanges(stateManager);
            }
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

        private class NullbileCategory
        {
            public List<NullbileProduct> Products { get; set; }
            public NullbileCategoryInfo Info { get; set; }
        }

        private class NullbileCategoryInfo
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public NullbileCategory Category { get; set; }
        }

        private class NullbileProduct
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public NullbileCategory Category { get; set; }
        }

        private class Sweet
        {
            public int? Id { get; set; }
            public Dreams Dreams { get; set; }
        }

        private class Dreams
        {
            public Sweet Sweet { get; set; }
            public AreMadeOfThis AreMade { get; set; }
            public AreMadeOfThis OfThis { get; set; }
        }

        private class AreMadeOfThis
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

            public EarlyLearningCenter(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<NullbileProduct>(
                        b =>
                        {
                            b.Property<int?>("Id");
                            b.Property<int?>("CategoryId");
                            b.HasKey("Id");
                        });

                modelBuilder
                    .Entity<NullbileCategoryInfo>(
                        b =>
                        {
                            b.Property<int?>("Id");
                            b.Property<int?>("CategoryId");
                            b.HasKey("Id");
                        });

                modelBuilder
                    .Entity<NullbileCategory>(
                        b =>
                        {
                            b.Property<int?>("Id");
                            b.HasKey("Id");
                            b.HasMany(e => e.Products).WithOne(e => e.Category).HasForeignKey("CategoryId");
                            b.HasOne(e => e.Info).WithOne(e => e.Category).HasForeignKey<NullbileCategoryInfo>("CategoryId");
                        });

                modelBuilder.Entity<Sweet>().OwnsOne(
                    e => e.Dreams, b =>
                    {
                        b.HasOne(e => e.Sweet).WithOne(e => e.Dreams);
                        b.OwnsOne(e => e.AreMade);
                        b.OwnsOne(e => e.OfThis);
                    });

                modelBuilder.Query<WhoAmI>();

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
                            e => new
                            {
                                e.OrderId,
                                e.ProductId
                            });
                        b.HasOne(e => e.Order).WithMany(e => e.OrderDetails).HasForeignKey(e => e.OrderId);
                        b.HasOne(e => e.Product).WithMany(e => e.OrderDetails).HasForeignKey(e => e.ProductId);
                    });
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(nameof(EarlyLearningCenter));
        }

        public class KeyValueEntityTracker
        {
            private readonly bool _updateExistingEntities;

            public KeyValueEntityTracker(bool updateExistingEntities)
            {
                _updateExistingEntities = updateExistingEntities;
            }

            public virtual void TrackEntity(EntityEntryGraphNode node)
                => node.Entry.GetInfrastructure().SetEntityState(DetermineState(node.Entry), acceptChanges: true);

            public virtual EntityState DetermineState(EntityEntry entry)
                => entry.IsKeySet
                    ? (_updateExistingEntities ? EntityState.Modified : EntityState.Unchanged)
                    : EntityState.Added;
        }
    }
}
