// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class LoadTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : LoadTestBase<TFixture>.LoadFixtureBase
    {
        protected LoadTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Added, true)]
        public virtual void Attached_references_to_principal_are_marked_as_loaded(EntityState state, bool lazy)
        {
            using var context = CreateContext(lazy);
            var parent = new Parent
            {
                Id = 707,
                AlternateId = "Root",
                SinglePkToPk = new SinglePkToPk { Id = 707 },
                Single = new Single { Id = 21 },
                SingleAk = new SingleAk { Id = 42 },
                SingleShadowFk = new SingleShadowFk { Id = 62 },
                SingleCompositeKey = new SingleCompositeKey { Id = 62 }
            };

            context.Attach(parent);

            if (state != EntityState.Unchanged)
            {
                context.ChangeTracker.LazyLoadingEnabled = false;

                context.Entry(parent.SinglePkToPk).State = state;
                context.Entry(parent.Single).State = state;
                context.Entry(parent.SingleAk).State = state;
                context.Entry(parent.SingleShadowFk).State = state;
                context.Entry(parent.SingleCompositeKey).State = state;
                context.Entry(parent).State = state;

                context.ChangeTracker.LazyLoadingEnabled = true;
            }

            Assert.True(context.Entry(parent).Reference(e => e.SinglePkToPk).IsLoaded);
            Assert.True(context.Entry(parent).Reference(e => e.Single).IsLoaded);
            Assert.True(context.Entry(parent).Reference(e => e.SingleAk).IsLoaded);
            Assert.True(context.Entry(parent).Reference(e => e.SingleShadowFk).IsLoaded);
            Assert.True(context.Entry(parent).Reference(e => e.SingleCompositeKey).IsLoaded);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Added, true)]
        public virtual void Attached_references_to_dependents_are_marked_as_loaded(EntityState state, bool lazy)
        {
            using var context = CreateContext(lazy);
            var parent = new Parent
            {
                Id = 707,
                AlternateId = "Root",
                SinglePkToPk = new SinglePkToPk { Id = 707 },
                Single = new Single { Id = 21 },
                SingleAk = new SingleAk { Id = 42 },
                SingleShadowFk = new SingleShadowFk { Id = 62 },
                SingleCompositeKey = new SingleCompositeKey { Id = 62 }
            };

            context.Attach(parent);

            if (state != EntityState.Unchanged)
            {
                context.ChangeTracker.LazyLoadingEnabled = false;

                context.Entry(parent.SinglePkToPk).State = state;
                context.Entry(parent.Single).State = state;
                context.Entry(parent.SingleAk).State = state;
                context.Entry(parent.SingleShadowFk).State = state;
                context.Entry(parent.SingleCompositeKey).State = state;
                context.Entry(parent).State = state;

                context.ChangeTracker.LazyLoadingEnabled = true;
            }

            Assert.True(context.Entry(parent.SinglePkToPk).Reference(e => e.Parent).IsLoaded);
            Assert.True(context.Entry(parent.Single).Reference(e => e.Parent).IsLoaded);
            Assert.True(context.Entry(parent.SingleAk).Reference(e => e.Parent).IsLoaded);
            Assert.True(context.Entry(parent.SingleShadowFk).Reference(e => e.Parent).IsLoaded);
            Assert.True(context.Entry(parent.SingleCompositeKey).Reference(e => e.Parent).IsLoaded);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Added, false)]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Added, true)]
        public virtual void Attached_collections_are_not_marked_as_loaded(EntityState state, bool lazy)
        {
            using var context = CreateContext(lazy);
            var parent = new Parent
            {
                Id = 707,
                AlternateId = "Root",
                Children = new List<Child> { new Child { Id = 11 }, new Child { Id = 12 } },
                ChildrenAk = new List<ChildAk> { new ChildAk { Id = 31 }, new ChildAk { Id = 32 } },
                ChildrenShadowFk = new List<ChildShadowFk> { new ChildShadowFk { Id = 51 }, new ChildShadowFk { Id = 52 } },
                ChildrenCompositeKey = new List<ChildCompositeKey> { new ChildCompositeKey { Id = 51 }, new ChildCompositeKey { Id = 52 } }
            };

            context.Attach(parent);

            if (state != EntityState.Unchanged)
            {
                context.ChangeTracker.LazyLoadingEnabled = false;

                foreach (var child in parent.Children.Cast<object>()
                    .Concat(parent.ChildrenAk)
                    .Concat(parent.ChildrenShadowFk)
                    .Concat(parent.ChildrenCompositeKey))
                {
                    context.Entry(child).State = state;
                }

                context.Entry(parent).State = state;

                context.ChangeTracker.LazyLoadingEnabled = true;
            }

            Assert.False(context.Entry(parent).Collection(e => e.Children).IsLoaded);
            Assert.False(context.Entry(parent).Collection(e => e.ChildrenAk).IsLoaded);
            Assert.False(context.Entry(parent).Collection(e => e.ChildrenShadowFk).IsLoaded);
            Assert.False(context.Entry(parent).Collection(e => e.ChildrenCompositeKey).IsLoaded);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(parent.Children);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(collectionEntry.IsLoaded);

            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, parent.Children.Count());
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var child = context.Set<Child>().Single(e => e.Id == 12);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var single = context.Set<Single>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(parent.Single);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var single = context.Set<SinglePkToPk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(parent.SinglePkToPk);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var child = context.Attach(
                new Child(context.GetService<ILazyLoader>().Load) { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.Null(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var single = context.Attach(
                new Single(context.GetService<ILazyLoader>().Load) { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.Null(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_not_found(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Attach(
                new Parent(context.GetService<ILazyLoader>().Load) { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.Empty(parent.Children);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Empty(parent.Children);
            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var child = context.Attach(
                new Child(context.GetService<ILazyLoader>().Load) { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.Null(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var single = context.Attach(
                new Single(context.GetService<ILazyLoader>().Load) { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.Null(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Attach(
                new Parent(context.GetService<ILazyLoader>().Load) { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.Null(parent.Single);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges)]
        public virtual void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Set<Parent>().Include(e => e.Children).Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.True(collectionEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(parent.Children);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, parent.Children.Count());

            if (state == EntityState.Deleted
                && deleteOrphansTiming != CascadeTiming.Never)
            {
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
            }
            else
            {
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
            }

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(child.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var single = context.Set<Single>().Include(e => e.Parent).Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_already_loaded(EntityState state, CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Set<Parent>().Include(e => e.Single).Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(parent.Single);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);

            if (state == EntityState.Deleted
                && deleteOrphansTiming != CascadeTiming.Never)
            {
                Assert.Null(single.Parent);
            }
            else
            {
                Assert.Same(parent, single.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(single.Parent);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

            var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

            context.Entry(parent).State = state;

            Assert.True(referenceEntry.IsLoaded);

            changeDetector.DetectChangesCalled = false;

            Assert.NotNull(parent.SinglePkToPk);

            Assert.False(changeDetector.DetectChangesCalled);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var child = context.Set<ChildAk>().Single(e => e.Id == 32);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(child.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenAk.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var single = context.Set<SingleAk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(single.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleAk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_alternate_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleAk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(parent.SingleAk);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleAk>().Single().Entity;

            Assert.Same(single, parent.SingleAk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var child = context.Attach(
                new ChildAk(context.GetService<ILazyLoader>().Load) { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.Null(child.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var single = context.Attach(
                new SingleAk(context.GetService<ILazyLoader>().Load) { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.Null(single.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_shadow_fk(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenShadowFk);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            Assert.NotNull(parent.ChildrenShadowFk);

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, parent.ChildrenShadowFk.Count());
            Assert.All(parent.ChildrenShadowFk.Select(e => e.Parent), c => Assert.Same(parent, c));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(child.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenShadowFk.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var single = context.Set<SingleShadowFk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(single.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleShadowFk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleShadowFk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(parent.SingleShadowFk);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleShadowFk>().Single().Entity;

            Assert.Same(single, parent.SingleShadowFk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var child = context.Attach(
                new ChildShadowFk(context.GetService<ILazyLoader>().Load) { Id = 767 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.Null(child.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var single = context.Attach(
                new SingleShadowFk(context.GetService<ILazyLoader>().Load) { Id = 767 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.Null(single.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_composite_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenCompositeKey);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            Assert.NotNull(parent.ChildrenCompositeKey);

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, parent.ChildrenCompositeKey.Count());
            Assert.All(parent.ChildrenCompositeKey.Select(e => e.Parent), c => Assert.Same(parent, c));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_composite_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var child = context.Set<ChildCompositeKey>().Single(e => e.Id == 52);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(child.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenCompositeKey.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_composite_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var single = context.Set<SingleCompositeKey>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(single.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleCompositeKey);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_composite_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleCompositeKey);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.NotNull(parent.SingleCompositeKey);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleCompositeKey>().Single().Entity;

            Assert.Same(single, parent.SingleCompositeKey);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var child = context.Attach(
                new ChildCompositeKey(context.GetService<ILazyLoader>().Load) { Id = 767, ParentId = 567 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.Null(child.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var single = context.Attach(
                new SingleCompositeKey(context.GetService<ILazyLoader>().Load) { Id = 767, ParentAlternateId = "Boot" }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            Assert.Null(single.Parent);

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Lazy_load_collection_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(lazyLoadingEnabled: true, noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.DetachedLazyLoadingWarning.ToString(),
                    CoreResources.LogDetachedLazyLoading(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Parent.Children), "Parent"),
                    "CoreEventId.DetachedLazyLoadingWarning"),
                Assert.Throws<InvalidOperationException>(
                    () => parent.Children).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Lazy_load_reference_to_principal_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(lazyLoadingEnabled: true, noTracking: noTracking);
            var child = context.Set<Child>().Single(e => e.Id == 12);

            if (!noTracking)
            {
                context.Entry(child).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.DetachedLazyLoadingWarning.ToString(),
                    CoreResources.LogDetachedLazyLoading(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Child.Parent), "Child"),
                    "CoreEventId.DetachedLazyLoadingWarning"),
                Assert.Throws<InvalidOperationException>(
                    () => child.Parent).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Lazy_load_reference_to_dependent_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(lazyLoadingEnabled: true, noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.DetachedLazyLoadingWarning.ToString(),
                    CoreResources.LogDetachedLazyLoading(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Parent.Single), "Parent"),
                    "CoreEventId.DetachedLazyLoadingWarning"),
                Assert.Throws<InvalidOperationException>(
                    () => parent.Single).Message);
        }

        [ConditionalFact]
        public virtual void Lazy_loading_uses_field_access_when_abstract_base_class_navigation()
        {
            using var context = CreateContext(lazyLoadingEnabled: true);
            var product = context.Set<SimpleProduct>().Single();
            var deposit = product.Deposit;

            Assert.NotNull(deposit);
            Assert.Same(deposit, product.Deposit);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, true)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.TrackAll, false)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, true)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.TrackAll, false)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, true)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.TrackAll, false)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, true)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTracking, false)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, true)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTracking, false)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, true)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTracking, false)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
        [InlineData(EntityState.Unchanged, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
        [InlineData(EntityState.Modified, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, true)]
        [InlineData(EntityState.Deleted, QueryTrackingBehavior.NoTrackingWithIdentityResolution, false)]
        public virtual async Task Load_collection(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
        {
            using var context = CreateContext();

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            if (async)
            {
                await collectionEntry.LoadAsync();
            }
            else
            {
                collectionEntry.Load();
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();
            context.ChangeTracker.LazyLoadingEnabled = false;

            Assert.Equal(2, parent.Children.Count());
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Single(e => e.Id == 12);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<Single>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_when_NoTracking_behavior(EntityState state, bool async)
        {
            using var context = CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var single = context.Set<Single>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SinglePkToPk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, children.Count);
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Single(e => e.Id == 12);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<Single>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SinglePkToPk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new Child { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new Single { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new Child { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(child.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new Single { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(single.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            if (async)
            {
                await collectionEntry.LoadAsync();
            }
            else
            {
                collectionEntry.Load();
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Empty(parent.Children);
            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new Child { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new Single { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Empty(children);
            Assert.Empty(parent.Children);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new Child { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(child.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new Single { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(single.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(single);
            Assert.Null(parent.Single);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_collection_already_loaded(EntityState state, bool async, CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var parent = context.Set<Parent>().Include(e => e.Children).Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.True(collectionEntry.IsLoaded);

            if (async)
            {
                await collectionEntry.LoadAsync();
            }
            else
            {
                collectionEntry.Load();
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, parent.Children.Count());

            if (state == EntityState.Deleted
                && deleteOrphansTiming != CascadeTiming.Never)
            {
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
            }
            else
            {
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
            }

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_already_loaded(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.True(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_one_to_one_reference_to_principal_already_loaded(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var single = context.Set<Single>().Include(e => e.Parent).Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.True(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_one_to_one_reference_to_dependent_already_loaded(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var parent = context.Set<Parent>().Include(e => e.Single).Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.True(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);

            if (state == EntityState.Deleted
                && deleteOrphansTiming != CascadeTiming.Never)
            {
                Assert.Null(single.Parent);
            }
            else
            {
                Assert.Same(parent, single.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.True(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

            context.Entry(parent).State = state;

            Assert.True(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_collection_using_Query_already_loaded(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            var parent = context.Set<Parent>().Include(e => e.Children).Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            context.Entry(parent).State = state;

            Assert.True(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, children.Count);
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.True(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<Single>().Include(e => e.Parent).Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.True(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            var parent = context.Set<Parent>().Include(e => e.Single).Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            context.Entry(parent).State = state;

            Assert.True(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.True(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SinglePkToPk);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

            context.Entry(parent).State = state;

            Assert.True(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.SinglePkToPk);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Children");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, parent.Children.Count());
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Single(e => e.Id == 12);

            ClearLog();

            var navigationEntry = context.Entry(child).Navigation("Parent");

            context.Entry(child).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<Single>().Single();

            ClearLog();

            var navigationEntry = context.Entry(single).Navigation("Parent");

            context.Entry(single).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Single");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Children");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var children = async
                ? await navigationEntry.Query().ToListAsync<object>()
                : navigationEntry.Query().ToList<object>();

            Assert.False(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, children.Count);
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Single(e => e.Id == 12);

            ClearLog();

            var navigationEntry = context.Entry(child).Navigation("Parent");

            context.Entry(child).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, child.Parent);
            Assert.Same(child, ((Parent)parent).Children.Single());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<Single>().Single();

            ClearLog();

            var navigationEntry = context.Entry(single).Navigation("Parent");

            context.Entry(single).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, ((Parent)parent).Single);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Single");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var single = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.Single);
            Assert.Same(parent, ((Single)single).Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Children");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Empty(parent.Children);
            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new Child { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(child).Navigation("Parent");

            context.Entry(child).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new Single { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(single).Navigation("Parent");

            context.Entry(single).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Single");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Children");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var children = async
                ? await navigationEntry.Query().ToListAsync<object>()
                : navigationEntry.Query().ToList<object>();

            Assert.False(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Empty(children);
            Assert.Empty(parent.Children);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new Child { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(child).Navigation("Parent");

            context.Entry(child).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).SingleOrDefault()
                : navigationEntry.Query().ToList<object>().SingleOrDefault();

            Assert.False(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(child.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new Single { Id = 767, ParentId = 787 }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(single).Navigation("Parent");

            context.Entry(single).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).SingleOrDefault()
                : navigationEntry.Query().ToList<object>().SingleOrDefault();

            Assert.False(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(single.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Attach(
                new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Single");

            context.Entry(parent).State = state;

            Assert.False(navigationEntry.IsLoaded);

            // Issue #16429
            var single = async
                ? (await navigationEntry.Query().ToListAsync<object>()).SingleOrDefault()
                : navigationEntry.Query().ToList<object>().SingleOrDefault();

            Assert.False(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Null(single);
            Assert.Null(parent.Single);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_collection_already_loaded_untyped(EntityState state, bool async, CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var parent = context.Set<Parent>().Include(e => e.Children).Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Children");

            context.Entry(parent).State = state;

            Assert.True(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, parent.Children.Count());

            if (state == EntityState.Deleted
                && deleteOrphansTiming != CascadeTiming.Never)
            {
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
            }
            else
            {
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
            }

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

            ClearLog();

            var navigationEntry = context.Entry(child).Navigation("Parent");

            context.Entry(child).State = state;

            Assert.True(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.Children.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_already_loaded_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<Single>().Include(e => e.Parent).Single();

            ClearLog();

            var navigationEntry = context.Entry(single).Navigation("Parent");

            context.Entry(single).State = state;

            Assert.True(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.Single);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_one_to_one_reference_to_dependent_already_loaded_untyped(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;

            var parent = context.Set<Parent>().Include(e => e.Single).Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Single");

            context.Entry(parent).State = state;

            Assert.True(navigationEntry.IsLoaded);

            if (async)
            {
                await navigationEntry.LoadAsync();
            }
            else
            {
                navigationEntry.Load();
            }

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<Single>().Single().Entity;

            Assert.Same(single, parent.Single);

            if (state == EntityState.Deleted
                && deleteOrphansTiming != CascadeTiming.Never)
            {
                Assert.Null(single.Parent);
            }
            else
            {
                Assert.Same(parent, single.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_collection_using_Query_already_loaded_untyped(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            var parent = context.Set<Parent>().Include(e => e.Children).Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Children");

            context.Entry(parent).State = state;

            Assert.True(navigationEntry.IsLoaded);

            // Issue #16429
            var children = async
                ? await navigationEntry.Query().ToListAsync<object>()
                : navigationEntry.Query().ToList<object>();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, children.Count);
            Assert.Equal(2, parent.Children.Count());
            Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.Children));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

            ClearLog();

            var navigationEntry = context.Entry(child).Navigation("Parent");

            context.Entry(child).State = state;

            Assert.True(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, child.Parent);
            Assert.Same(child, ((Parent)parent).Children.Single());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<Single>().Include(e => e.Parent).Single();

            ClearLog();

            var navigationEntry = context.Entry(single).Navigation("Parent");

            context.Entry(single).State = state;

            Assert.True(navigationEntry.IsLoaded);

            // Issue #16429
            var parent = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, ((Parent)parent).Single);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, false, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, true, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, false, CascadeTiming.OnSaveChanges)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(
            EntityState state,
            bool async,
            CascadeTiming deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            var parent = context.Set<Parent>().Include(e => e.Single).Single();

            ClearLog();

            var navigationEntry = context.Entry(parent).Navigation("Single");

            context.Entry(parent).State = state;

            Assert.True(navigationEntry.IsLoaded);

            // Issue #16429
            var single = async
                ? (await navigationEntry.Query().ToListAsync<object>()).Single()
                : navigationEntry.Query().ToList<object>().Single();

            Assert.True(navigationEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.Single);

            Assert.Same(parent, ((Single)single).Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenAk);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            if (async)
            {
                await collectionEntry.LoadAsync();
            }
            else
            {
                collectionEntry.Load();
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, parent.ChildrenAk.Count());
            Assert.All(parent.ChildrenAk.Select(e => e.Parent), c => Assert.Same(parent, c));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<ChildAk>().Single(e => e.Id == 32);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenAk.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SingleAk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleAk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleAk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleAk>().Single().Entity;

            Assert.Same(single, parent.SingleAk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenAk);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, children.Count);
            Assert.Equal(2, parent.ChildrenAk.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.ChildrenAk));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<ChildAk>().Single(e => e.Id == 32);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenAk.Single());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SingleAk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleAk);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleAk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.SingleAk);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new ChildAk { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new SingleAk { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new ChildAk { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(child.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new SingleAk { Id = 767, ParentId = null }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(single.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenShadowFk);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            if (async)
            {
                await collectionEntry.LoadAsync();
            }
            else
            {
                collectionEntry.Load();
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, parent.ChildrenShadowFk.Count());
            Assert.All(parent.ChildrenShadowFk.Select(e => e.Parent), c => Assert.Same(parent, c));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenShadowFk.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SingleShadowFk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleShadowFk);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleShadowFk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleShadowFk>().Single().Entity;

            Assert.Same(single, parent.SingleShadowFk);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenShadowFk);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, children.Count);
            Assert.Equal(2, parent.ChildrenShadowFk.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.ChildrenShadowFk));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<ChildShadowFk>().Single(e => e.Id == 52);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenShadowFk.Single());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SingleShadowFk>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleShadowFk);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleShadowFk);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.SingleShadowFk);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new ChildShadowFk { Id = 767 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new SingleShadowFk { Id = 767 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new ChildShadowFk { Id = 767 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(child.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new SingleShadowFk { Id = 767 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(single.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenCompositeKey);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            if (async)
            {
                await collectionEntry.LoadAsync();
            }
            else
            {
                collectionEntry.Load();
            }

            Assert.True(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, parent.ChildrenCompositeKey.Count());
            Assert.All(parent.ChildrenCompositeKey.Select(e => e.Parent), c => Assert.Same(parent, c));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<ChildCompositeKey>().Single(e => e.Id == 52);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenCompositeKey.Single());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SingleCompositeKey>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleCompositeKey);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleCompositeKey);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            var single = context.ChangeTracker.Entries<SingleCompositeKey>().Single().Entity;

            Assert.Same(single, parent.SingleCompositeKey);
            Assert.Same(parent, single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection_using_Query_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var collectionEntry = context.Entry(parent).Collection(e => e.ChildrenCompositeKey);

            context.Entry(parent).State = state;

            Assert.False(collectionEntry.IsLoaded);

            var children = async
                ? await collectionEntry.Query().ToListAsync()
                : collectionEntry.Query().ToList();

            Assert.False(collectionEntry.IsLoaded);

            RecordLog();

            Assert.Equal(2, children.Count);
            Assert.Equal(2, parent.ChildrenCompositeKey.Count());
            Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.All(children, p => Assert.Contains(p, parent.ChildrenCompositeKey));

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Set<ChildCompositeKey>().Single(e => e.Id == 52);

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, child.Parent);
            Assert.Same(child, parent.ChildrenCompositeKey.Single());

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Set<SingleCompositeKey>().Single();

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(parent);
            Assert.Same(parent, single.Parent);
            Assert.Same(single, parent.SingleCompositeKey);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            ClearLog();

            var referenceEntry = context.Entry(parent).Reference(e => e.SingleCompositeKey);

            context.Entry(parent).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var single = async
                ? await referenceEntry.Query().SingleAsync()
                : referenceEntry.Query().Single();

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.NotNull(single);
            Assert.Same(single, parent.SingleCompositeKey);
            Assert.Same(parent, single.Parent);

            Assert.Equal(2, context.ChangeTracker.Entries().Count());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new ChildCompositeKey { Id = 767, ParentId = 567 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());
            Assert.Null(child.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new SingleCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            if (async)
            {
                await referenceEntry.LoadAsync();
            }
            else
            {
                referenceEntry.Load();
            }

            Assert.True(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Single(context.ChangeTracker.Entries());

            Assert.Null(single.Parent);
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var child = context.Attach(
                new ChildCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            context.Entry(child).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(child.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(EntityState state, bool async)
        {
            using var context = CreateContext();
            var single = context.Attach(
                new SingleCompositeKey { Id = 767, ParentId = 567 }).Entity;

            ClearLog();

            var referenceEntry = context.Entry(single).Reference(e => e.Parent);

            context.Entry(single).State = state;

            Assert.False(referenceEntry.IsLoaded);

            var parent = async
                ? await referenceEntry.Query().SingleOrDefaultAsync()
                : referenceEntry.Query().SingleOrDefault();

            Assert.False(referenceEntry.IsLoaded);

            RecordLog();

            Assert.Null(parent);
            Assert.Null(single.Parent);

            Assert.Single(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public virtual void Can_change_IsLoaded_flag_for_collection()
        {
            using var context = CreateContext();
            var parent = context.Set<Parent>().Single();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            Assert.False(collectionEntry.IsLoaded);

            collectionEntry.IsLoaded = true;

            Assert.True(collectionEntry.IsLoaded);

            collectionEntry.Load();

            Assert.Empty(parent.Children);
            Assert.Single(context.ChangeTracker.Entries());

            Assert.True(collectionEntry.IsLoaded);

            collectionEntry.IsLoaded = false;

            Assert.False(collectionEntry.IsLoaded);

            collectionEntry.Load();

            Assert.Equal(2, parent.Children.Count());
            Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
            Assert.Equal(3, context.ChangeTracker.Entries().Count());

            Assert.True(collectionEntry.IsLoaded);
        }

        [ConditionalFact]
        public virtual void Can_change_IsLoaded_flag_for_reference_only_if_null()
        {
            using var context = CreateContext();
            var child = context.Set<Child>().Single(e => e.Id == 12);

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            Assert.False(referenceEntry.IsLoaded);

            referenceEntry.IsLoaded = true;

            Assert.True(referenceEntry.IsLoaded);

            referenceEntry.Load();

            Assert.True(referenceEntry.IsLoaded);

            Assert.Single(context.ChangeTracker.Entries());

            referenceEntry.IsLoaded = true;

            referenceEntry.IsLoaded = false;

            referenceEntry.Load();

            Assert.Equal(2, context.ChangeTracker.Entries().Count());

            Assert.True(referenceEntry.IsLoaded);

            Assert.Equal(
                CoreStrings.ReferenceMustBeLoaded("Parent", typeof(Child).Name),
                Assert.Throws<InvalidOperationException>(() => referenceEntry.IsLoaded = false).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_collection_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Children), nameof(Parent)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await collectionEntry.LoadAsync();
                        }
                        else
                        {
                            collectionEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_collection_using_string_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var collectionEntry = context.Entry(parent).Collection(nameof(Parent.Children));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Children), nameof(Parent)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await collectionEntry.LoadAsync();
                        }
                        else
                        {
                            collectionEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_collection_with_navigation_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var collectionEntry = context.Entry(parent).Navigation(nameof(Parent.Children));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Children), nameof(Parent)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await collectionEntry.LoadAsync();
                        }
                        else
                        {
                            collectionEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_reference_to_principal_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var child = context.Set<Child>().Single(e => e.Id == 12);

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            if (!noTracking)
            {
                context.Entry(child).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Child.Parent), nameof(Child)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_reference_with_navigation_to_principal_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var child = context.Set<Child>().Single(e => e.Id == 12);

            var referenceEntry = context.Entry(child).Navigation(nameof(Child.Parent));

            if (!noTracking)
            {
                context.Entry(child).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Child.Parent), nameof(Child)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_reference_using_string_to_principal_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var child = context.Set<Child>().Single(e => e.Id == 12);

            var referenceEntry = context.Entry(child).Reference(nameof(Child.Parent));

            if (!noTracking)
            {
                context.Entry(child).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Child.Parent), nameof(Child)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_reference_to_dependent_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Single), nameof(Parent)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_reference_to_dependent_with_navigation_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var referenceEntry = context.Entry(parent).Navigation(nameof(Parent.Single));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Single), nameof(Parent)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Load_reference_to_dependent_using_string_for_detached_throws(bool async, bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var referenceEntry = context.Entry(parent).Reference(nameof(Parent.Single));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Single), nameof(Parent)),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await referenceEntry.LoadAsync();
                        }
                        else
                        {
                            referenceEntry.Load();
                        }
                    })).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_collection_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var collectionEntry = context.Entry(parent).Collection(e => e.Children);

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Children), nameof(Parent)),
                Assert.Throws<InvalidOperationException>(() => collectionEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_collection_using_string_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var collectionEntry = context.Entry(parent).Collection(nameof(Parent.Children));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Children), nameof(Parent)),
                Assert.Throws<InvalidOperationException>(() => collectionEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_collection_with_navigation_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var collectionEntry = context.Entry(parent).Navigation(nameof(Parent.Children));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Children), nameof(Parent)),
                Assert.Throws<InvalidOperationException>(() => collectionEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_reference_to_principal_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var child = context.Set<Child>().Single(e => e.Id == 12);

            var referenceEntry = context.Entry(child).Reference(e => e.Parent);

            if (!noTracking)
            {
                context.Entry(child).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Child.Parent), nameof(Child)),
                Assert.Throws<InvalidOperationException>(() => referenceEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_reference_with_navigation_to_principal_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var child = context.Set<Child>().Single(e => e.Id == 12);

            var referenceEntry = context.Entry(child).Navigation(nameof(Child.Parent));

            if (!noTracking)
            {
                context.Entry(child).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Child.Parent), nameof(Child)),
                Assert.Throws<InvalidOperationException>(() => referenceEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_reference_using_string_to_principal_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var child = context.Set<Child>().Single(e => e.Id == 12);

            var referenceEntry = context.Entry(child).Reference(nameof(Child.Parent));

            if (!noTracking)
            {
                context.Entry(child).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Child.Parent), nameof(Child)),
                Assert.Throws<InvalidOperationException>(() => referenceEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_reference_to_dependent_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var referenceEntry = context.Entry(parent).Reference(e => e.Single);

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Single), nameof(Parent)),
                Assert.Throws<InvalidOperationException>(() => referenceEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_reference_to_dependent_with_navigation_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var referenceEntry = context.Entry(parent).Navigation(nameof(Parent.Single));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Single), nameof(Parent)),
                Assert.Throws<InvalidOperationException>(() => referenceEntry.Query()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void Query_reference_to_dependent_using_string_for_detached_throws(bool noTracking)
        {
            using var context = CreateContext(noTracking: noTracking);
            var parent = context.Set<Parent>().Single();

            var referenceEntry = context.Entry(parent).Reference(nameof(Parent.Single));

            if (!noTracking)
            {
                context.Entry(parent).State = EntityState.Detached;
            }

            Assert.Equal(
                CoreStrings.CannotLoadDetached(nameof(Parent.Single), nameof(Parent)),
                Assert.Throws<InvalidOperationException>(() => referenceEntry.Query()).Message);
        }

        protected class Parent
        {
            private readonly Action<object, string> _loader;
            private IEnumerable<Child> _children;
            private SinglePkToPk _singlePkToPk;
            private Single _single;
            private IEnumerable<ChildAk> _childrenAk;
            private SingleAk _singleAk;
            private IEnumerable<ChildShadowFk> _childrenShadowFk;
            private SingleShadowFk _singleShadowFk;
            private IEnumerable<ChildCompositeKey> _childrenCompositeKey;
            private SingleCompositeKey _singleCompositeKey;

            public Parent()
            {
            }

            public Parent(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string AlternateId { get; set; }

            public IEnumerable<Child> Children
            {
                get => _loader.Load(this, ref _children);
                set => _children = value;
            }

            public SinglePkToPk SinglePkToPk
            {
                get => _loader.Load(this, ref _singlePkToPk);
                set => _singlePkToPk = value;
            }

            public Single Single
            {
                get => _loader.Load(this, ref _single);
                set => _single = value;
            }

            public IEnumerable<ChildAk> ChildrenAk
            {
                get => _loader.Load(this, ref _childrenAk);
                set => _childrenAk = value;
            }

            public SingleAk SingleAk
            {
                get => _loader.Load(this, ref _singleAk);
                set => _singleAk = value;
            }

            public IEnumerable<ChildShadowFk> ChildrenShadowFk
            {
                get => _loader.Load(this, ref _childrenShadowFk);
                set => _childrenShadowFk = value;
            }

            public SingleShadowFk SingleShadowFk
            {
                get => _loader.Load(this, ref _singleShadowFk);
                set => _singleShadowFk = value;
            }

            public IEnumerable<ChildCompositeKey> ChildrenCompositeKey
            {
                get => _loader.Load(this, ref _childrenCompositeKey);
                set => _childrenCompositeKey = value;
            }

            public SingleCompositeKey SingleCompositeKey
            {
                get => _loader.Load(this, ref _singleCompositeKey);
                set => _singleCompositeKey = value;
            }
        }

        protected class Child
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public Child()
            {
            }

            public Child(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class SinglePkToPk
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public SinglePkToPk()
            {
            }

            protected SinglePkToPk(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class Single
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public Single()
            {
            }

            public Single(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class ChildAk
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public ChildAk()
            {
            }

            public ChildAk(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string ParentId { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class SingleAk
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public SingleAk()
            {
            }

            public SingleAk(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string ParentId { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class ChildShadowFk
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public ChildShadowFk()
            {
            }

            public ChildShadowFk(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class SingleShadowFk
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public SingleShadowFk()
            {
            }

            public SingleShadowFk(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class ChildCompositeKey
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public ChildCompositeKey()
            {
            }

            public ChildCompositeKey(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public string ParentAlternateId { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected class SingleCompositeKey
        {
            private readonly Action<object, string> _loader;
            private Parent _parent;

            public SingleCompositeKey()
            {
            }

            public SingleCompositeKey(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public string ParentAlternateId { get; set; }

            public Parent Parent
            {
                get => _loader.Load(this, ref _parent);
                set => _parent = value;
            }
        }

        protected abstract class RootClass
        {
            protected RootClass(Action<object, string> lazyLoader)
            {
                LazyLoader = lazyLoader;
            }

            protected RootClass()
            {
            }

            public int Id { get; set; }

            protected Action<object, string> LazyLoader { get; }
        }

        protected class Deposit : RootClass
        {
            private Deposit(Action<object, string> lazyLoader)
                : base(lazyLoader)
            {
            }

            public Deposit()
            {
            }
        }

        protected abstract class Product : RootClass
        {
            protected Product(Action<object, string> lazyLoader)
                : base(lazyLoader)
            {
            }

            protected Product()
            {
            }

            public int? DepositID { get; set; }

            private Deposit _deposit;

            public Deposit Deposit
            {
                get => LazyLoader.Load(this, ref _deposit);
                set => _deposit = value;
            }
        }

        protected class SimpleProduct : Product
        {
            private SimpleProduct(Action<object, string> lazyLoader)
                : base(lazyLoader)
            {
            }

            public SimpleProduct()
            {
            }
        }

        protected class OptionalChildView
        {
            private readonly Action<object, string> _loader;
            private RootClass _root;

            public OptionalChildView()
            {
            }

            public OptionalChildView(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            public int? RootId { get; set; }

            public RootClass Root
            {
                get => _loader.Load(this, ref _root);
                set => _root = value;
            }
        }

        protected class RequiredChildView
        {
            private readonly Action<object, string> _loader;
            private RootClass _root;

            public RequiredChildView()
            {
            }

            public RequiredChildView(Action<object, string> lazyLoader)
            {
                _loader = lazyLoader;
            }

            public int RootId { get; set; }

            public RootClass Root
            {
                get => _loader.Load(this, ref _root);
                set => _root = value;
            }
        }

        protected DbContext CreateContext(bool lazyLoadingEnabled = false, bool noTracking = false)
        {
            var context = Fixture.CreateContext();
            context.ChangeTracker.LazyLoadingEnabled = lazyLoadingEnabled;

            context.ChangeTracker.QueryTrackingBehavior = noTracking
                ? QueryTrackingBehavior.NoTracking
                : QueryTrackingBehavior.TrackAll;

            return context;
        }

        protected virtual void ClearLog()
        {
        }

        protected virtual void RecordLog()
        {
        }

        protected class ChangeDetectorProxy : ChangeDetector
        {
            public ChangeDetectorProxy(
                IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
                ILoggingOptions loggingOptions)
                : base(logger, loggingOptions)
            {
            }

            public bool DetectChangesCalled { get; set; }

            public override void DetectChanges(IStateManager stateManager)
            {
                DetectChangesCalled = true;

                base.DetectChanges(stateManager);
            }
        }

        public abstract class LoadFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "LoadTest";

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddScoped<IChangeDetector, ChangeDetectorProxy>());

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<SingleShadowFk>()
                    .Property<int?>("ParentId");

                modelBuilder.Entity<Parent>(
                    b =>
                    {
                        b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                        b.HasMany<Child>(nameof(Parent.Children))
                            .WithOne(nameof(Child.Parent))
                            .HasForeignKey(e => e.ParentId);

                        b.HasOne<SinglePkToPk>(nameof(Parent.SinglePkToPk))
                            .WithOne(nameof(SinglePkToPk.Parent))
                            .HasForeignKey<SinglePkToPk>(e => e.Id)
                            .IsRequired();

                        b.HasOne<Single>(nameof(Parent.Single))
                            .WithOne(e => e.Parent)
                            .HasForeignKey<Single>(e => e.ParentId);

                        b.HasMany<ChildAk>(nameof(Parent.ChildrenAk))
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey(e => e.AlternateId)
                            .HasForeignKey(e => e.ParentId);

                        b.HasOne<SingleAk>(nameof(Parent.SingleAk))
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey<Parent>(e => e.AlternateId)
                            .HasForeignKey<SingleAk>(e => e.ParentId);

                        b.HasMany(e => e.ChildrenShadowFk)
                            .WithOne(nameof(ChildShadowFk.Parent))
                            .HasPrincipalKey(e => e.Id)
                            .HasForeignKey("ParentId");

                        b.HasOne<SingleShadowFk>(nameof(Parent.SingleShadowFk))
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey<Parent>(e => e.Id)
                            .HasForeignKey<SingleShadowFk>("ParentId");

                        b.HasMany(e => e.ChildrenCompositeKey)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey(
                                e => new { e.AlternateId, e.Id })
                            .HasForeignKey(
                                e => new { e.ParentAlternateId, e.ParentId });

                        b.HasOne<SingleCompositeKey>(nameof(Parent.SingleCompositeKey))
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey<Parent>(
                                e => new { e.AlternateId, e.Id })
                            .HasForeignKey<SingleCompositeKey>(
                                e => new { e.ParentAlternateId, e.ParentId });
                    });

                modelBuilder.Entity<RootClass>();
                modelBuilder.Entity<Product>();
                modelBuilder.Entity<Deposit>();
                modelBuilder.Entity<SimpleProduct>();

                modelBuilder.Entity<OptionalChildView>().HasNoKey();
                modelBuilder.Entity<RequiredChildView>().HasNoKey();
            }

            protected override void Seed(PoolableDbContext context)
            {
                context.Add(
                    new Parent
                    {
                        Id = 707,
                        AlternateId = "Root",
                        Children = new List<Child> { new Child { Id = 11 }, new Child { Id = 12 } },
                        SinglePkToPk = new SinglePkToPk { Id = 707 },
                        Single = new Single { Id = 21 },
                        ChildrenAk = new List<ChildAk> { new ChildAk { Id = 31 }, new ChildAk { Id = 32 } },
                        SingleAk = new SingleAk { Id = 42 },
                        ChildrenShadowFk = new List<ChildShadowFk> { new ChildShadowFk { Id = 51 }, new ChildShadowFk { Id = 52 } },
                        SingleShadowFk = new SingleShadowFk { Id = 62 },
                        ChildrenCompositeKey = new List<ChildCompositeKey>
                        {
                            new ChildCompositeKey { Id = 51 }, new ChildCompositeKey { Id = 52 }
                        },
                        SingleCompositeKey = new SingleCompositeKey { Id = 62 }
                    });

                context.Add(
                    new SimpleProduct { Deposit = new Deposit() });

                context.SaveChanges();
            }
        }
    }
}
