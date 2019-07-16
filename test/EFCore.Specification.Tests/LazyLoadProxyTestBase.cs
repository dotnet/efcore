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
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class LazyLoadProxyTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : LazyLoadProxyTestBase<TFixture>.LoadFixtureBase
    {
        protected LazyLoadProxyTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false, false)]
        [InlineData(EntityState.Modified, false, false)]
        [InlineData(EntityState.Deleted, false, false)]
        [InlineData(EntityState.Unchanged, true, false)]
        [InlineData(EntityState.Modified, true, false)]
        [InlineData(EntityState.Deleted, true, false)]
        [InlineData(EntityState.Unchanged, true, true)]
        [InlineData(EntityState.Modified, true, true)]
        [InlineData(EntityState.Deleted, true, true)]
        public virtual void Lazy_load_collection(EntityState state, bool useAttach, bool useDetach)
        {
            Parent parent = null;

            if (useAttach)
            {
                using (var context = CreateContext(lazyLoadingEnabled: true))
                {
                    parent = context.Set<Parent>().Single();

                    if (useDetach)
                    {
                        context.Entry(parent).State = EntityState.Detached;
                    }
                }

                if (useDetach)
                {
                    Assert.Null(parent.Children);
                }
                else
                {
                    Assert.Equal(
                        CoreStrings.WarningAsErrorTemplate(
                            CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                            CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("Children", "ParentProxy"),
                            "CoreEventId.LazyLoadOnDisposedContextWarning"),
                        Assert.Throws<InvalidOperationException>(
                            () => parent.Children).Message);
                }
            }

            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                parent ??= context.Set<Parent>().Single();

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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false, false)]
        [InlineData(EntityState.Modified, false, false)]
        [InlineData(EntityState.Deleted, false, false)]
        [InlineData(EntityState.Unchanged, true, false)]
        [InlineData(EntityState.Modified, true, false)]
        [InlineData(EntityState.Deleted, true, false)]
        [InlineData(EntityState.Unchanged, true, true)]
        [InlineData(EntityState.Modified, true, true)]
        [InlineData(EntityState.Deleted, true, true)]
        public virtual void Lazy_load_many_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
        {
            Child child = null;

            if (useAttach)
            {
                using (var context = CreateContext(lazyLoadingEnabled: true))
                {
                    child = context.Set<Child>().Single(e => e.Id == 12);

                    if (useDetach)
                    {
                        context.Entry(child).State = EntityState.Detached;
                    }
                }

                if (useDetach)
                {
                    Assert.Null(child.Parent);
                }
                else
                {
                    Assert.Equal(
                        CoreStrings.WarningAsErrorTemplate(
                            CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                            CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("Parent", "ChildProxy"),
                            "CoreEventId.LazyLoadOnDisposedContextWarning"),
                        Assert.Throws<InvalidOperationException>(
                            () => child.Parent).Message);
                }
            }

            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                child ??= context.Set<Child>().Single(e => e.Id == 12);

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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false, false)]
        [InlineData(EntityState.Modified, false, false)]
        [InlineData(EntityState.Deleted, false, false)]
        [InlineData(EntityState.Unchanged, true, false)]
        [InlineData(EntityState.Modified, true, false)]
        [InlineData(EntityState.Deleted, true, false)]
        [InlineData(EntityState.Unchanged, true, true)]
        [InlineData(EntityState.Modified, true, true)]
        [InlineData(EntityState.Deleted, true, true)]
        public virtual void Lazy_load_one_to_one_reference_to_principal(EntityState state, bool useAttach, bool useDetach)
        {
            Single single = null;

            if (useAttach)
            {
                using (var context = CreateContext(lazyLoadingEnabled: true))
                {
                    single = context.Set<Single>().Single();

                    if (useDetach)
                    {
                        context.Entry(single).State = EntityState.Detached;
                    }
                }

                if (useDetach)
                {
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.Equal(
                        CoreStrings.WarningAsErrorTemplate(
                            CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                            CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("Parent", "SingleProxy"),
                            "CoreEventId.LazyLoadOnDisposedContextWarning"),
                        Assert.Throws<InvalidOperationException>(
                            () => single.Parent).Message);
                }
            }

            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                single ??= context.Set<Single>().Single();

                ClearLog();

                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                context.Entry(single).State = state;

                Assert.False(referenceEntry.IsLoaded);

                changeDetector.DetectChangesCalled = false;

                Assert.NotNull(single.Parent);

                Assert.True(referenceEntry.IsLoaded);

                Assert.False(changeDetector.DetectChangesCalled);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, false, false)]
        [InlineData(EntityState.Modified, false, false)]
        [InlineData(EntityState.Deleted, false, false)]
        [InlineData(EntityState.Unchanged, true, false)]
        [InlineData(EntityState.Modified, true, false)]
        [InlineData(EntityState.Deleted, true, false)]
        [InlineData(EntityState.Unchanged, true, true)]
        [InlineData(EntityState.Modified, true, true)]
        [InlineData(EntityState.Deleted, true, true)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent(EntityState state, bool useAttach, bool useDetach)
        {
            Parent parent = null;

            if (useAttach)
            {
                using (var context = CreateContext(lazyLoadingEnabled: true))
                {
                    parent = context.Set<Parent>().Single();

                    if (useDetach)
                    {
                        context.Entry(parent).State = EntityState.Detached;
                    }
                }

                if (useDetach)
                {
                    Assert.Null(parent.Single);
                }
                else
                {
                    Assert.Equal(
                        CoreStrings.WarningAsErrorTemplate(
                            CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                            CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>()).GenerateMessage("Single", "ParentProxy"),
                            "CoreEventId.LazyLoadOnDisposedContextWarning"),
                        Assert.Throws<InvalidOperationException>(
                            () => parent.Single).Message);
                }
            }

            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                parent ??= context.Set<Parent>().Single();

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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var child = context.CreateProxy<Child>();
                child.Id = 767;

                context.Attach(child);

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

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var single = context.CreateProxy<Single>();
                single.Id = 767;

                context.Attach(single);

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

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_changed_non_found_FK(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var child = context.CreateProxy<Child>();
                child.Id = 767;
                child.ParentId = 797;

                context.Attach(child);

                ClearLog();

                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                context.Entry(child).State = state;

                referenceEntry.IsLoaded = true;

                changeDetector.DetectChangesCalled = false;

                child.ParentId = 707;

                context.ChangeTracker.DetectChanges();

                Assert.NotNull(child.Parent);

                Assert.True(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(child, parent.Children.Single());
                Assert.Same(parent, child.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_changed_found_FK(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var parent = context.CreateProxy<Parent>();
                parent.Id = 797;
                parent.AlternateId = "X";

                var child = context.CreateProxy<Child>();
                child.Id = 767;

                child.ParentId = 797;
                child.Parent = parent;
                parent.Children = new List<Child>
                {
                    child
                };

                context.Attach(child);
                context.Attach(parent);

                ClearLog();

                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                context.Entry(child).State = state;

                referenceEntry.IsLoaded = true;

                changeDetector.DetectChangesCalled = false;

                child.ParentId = 707;

                context.ChangeTracker.DetectChanges();

                Assert.NotNull(child.Parent);

                Assert.True(changeDetector.DetectChangesCalled);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                var newParent = context.ChangeTracker.Entries<Parent>().Single(e => e.Entity.Id != parent.Id).Entity;

                Assert.Same(child, newParent.Children.Single());
                Assert.Same(newParent, child.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var parent = context.CreateProxy<Parent>();
                parent.Id = 767;
                parent.AlternateId = "NewRoot";

                context.Attach(parent);

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

                Assert.Equal(0, parent.Children.Count());
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var child = context.CreateProxy<Child>();
                child.Id = 767;
                child.ParentId = 787;

                context.Attach(child);

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

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var single = context.CreateProxy<Single>();
                single.Id = 767;
                single.ParentId = 787;

                context.Attach(single);

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

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var parent = context.CreateProxy<Parent>();
                parent.Id = 767;
                parent.AlternateId = "NewRoot";

                context.Attach(parent);

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

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(parent.Single);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
        public virtual void Lazy_load_collection_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;

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
                    && cascadeDeleteTiming == CascadeTiming.Immediate)
                {
                    Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Null(c));
                }
                else
                {
                    Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));
                }

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, CascadeTiming.Never)]
        [InlineData(EntityState.Modified, CascadeTiming.Never)]
        [InlineData(EntityState.Deleted, CascadeTiming.Never)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_already_loaded(EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;

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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Modified, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Deleted, CascadeTiming.OnSaveChanges)]
        [InlineData(EntityState.Unchanged, CascadeTiming.Immediate)]
        [InlineData(EntityState.Modified, CascadeTiming.Immediate)]
        [InlineData(EntityState.Deleted, CascadeTiming.Immediate)]
        [InlineData(EntityState.Unchanged, CascadeTiming.Never)]
        [InlineData(EntityState.Modified, CascadeTiming.Never)]
        [InlineData(EntityState.Deleted, CascadeTiming.Never)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_already_loaded(
            EntityState state, CascadeTiming cascadeDeleteTiming)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;

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

                if (cascadeDeleteTiming == CascadeTiming.Immediate
                    && state == EntityState.Deleted)
                {
                    // No fixup to Deleted entity.
                    Assert.Null(single.Parent);
                }
                else
                {
                    Assert.Same(parent, single.Parent);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_alternate_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_alternate_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.CreateProxy<ChildAk>();
                child.Id = 767;

                context.Attach(child);

                ClearLog();

                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                context.Entry(child).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(child.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_alternate_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.CreateProxy<SingleAk>();
                single.Id = 767;

                context.Attach(single);

                ClearLog();

                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                context.Entry(single).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(single.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_shadow_fk(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_shadow_fk(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_shadow_fk(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.CreateProxy<ChildShadowFk>();
                child.Id = 767;

                context.Attach(child);

                ClearLog();

                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                context.Entry(child).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(child.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_shadow_fk(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.CreateProxy<SingleShadowFk>();
                single.Id = 767;

                context.Attach(single);

                ClearLog();

                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                context.Entry(single).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(single.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_composite_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_composite_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_composite_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_composite_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
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
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.CreateProxy<ChildCompositeKey>();
                child.Id = 767;
                child.ParentId = 567;

                context.Attach(child);

                ClearLog();

                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                context.Entry(child).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(child.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK_composite_key(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.CreateProxy<SingleCompositeKey>();
                single.Id = 767;
                single.ParentAlternateId = "Boot";

                context.Attach(single);

                ClearLog();

                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                context.Entry(single).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(single.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_collection_for_detached_is_no_op()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Single();

                context.Entry(parent).State = EntityState.Detached;

                Assert.Null(parent.Children);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_reference_to_principal_for_detached_is_no_op()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.Set<Child>().Single(e => e.Id == 12);

                context.Entry(child).State = EntityState.Detached;

                Assert.Null(child.Parent);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_reference_to_dependent_for_detached_is_no_op()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Single();

                context.Entry(parent).State = EntityState.Detached;

                Assert.Null(parent.Single);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_collection_for_no_tracking_throws()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().AsNoTracking().Single();

                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        CoreEventId.DetachedLazyLoadingWarning.ToString(),
                        CoreResources.LogDetachedLazyLoading(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(nameof(Parent.Children), "ParentProxy"),
                        "CoreEventId.DetachedLazyLoadingWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => parent.Children).Message);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_reference_to_principal_for_no_tracking_throws()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.Set<Child>().AsNoTracking().Single(e => e.Id == 12);

                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        CoreEventId.DetachedLazyLoadingWarning.ToString(),
                        CoreResources.LogDetachedLazyLoading(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(nameof(Child.Parent), "ChildProxy"),
                        "CoreEventId.DetachedLazyLoadingWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => child.Parent).Message);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_reference_to_dependent_for_no_tracking_throws()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().AsNoTracking().Single();

                Assert.Equal(
                    CoreStrings.WarningAsErrorTemplate(
                        CoreEventId.DetachedLazyLoadingWarning.ToString(),
                        CoreResources.LogDetachedLazyLoading(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(nameof(Parent.Single), "ParentProxy"),
                        "CoreEventId.DetachedLazyLoadingWarning"),
                    Assert.Throws<InvalidOperationException>(
                        () => parent.Single).Message);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_collection_for_no_tracking_does_not_throw_if_populated()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Include(e => e.Children).AsNoTracking().Single();

                Assert.Same(parent, parent.Children.First().Parent);

                ((ICollection<Child>)parent.Children).Clear();

                Assert.Empty(parent.Children);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_reference_to_principal_for_no_tracking_does_not_throw_if_populated()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.Set<Child>().Include(e => e.Parent).AsNoTracking().Single(e => e.Id == 12);

                Assert.NotNull(child.Parent);

                child.Parent = null;

                Assert.Null(child.Parent);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_load_reference_to_dependent_for_no_does_not_throw_if_populated()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Include(e => e.Single).AsNoTracking().Single();

                Assert.Same(parent, parent.Single.Parent);

                parent.Single = null;

                Assert.Null(parent.Single);
            }
        }

        [ConditionalTheory]
        [InlineData(EntityState.Unchanged, true)]
        [InlineData(EntityState.Unchanged, false)]
        [InlineData(EntityState.Modified, true)]
        [InlineData(EntityState.Modified, false)]
        [InlineData(EntityState.Deleted, true)]
        [InlineData(EntityState.Deleted, false)]
        public virtual async Task Load_collection(EntityState state, bool async)
        {
            using (var context = CreateContext())
            {
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
        }

        [ConditionalFact]
        public virtual void Lazy_loading_finds_correct_entity_type_with_already_loaded_owned_types()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var blogs = context.Set<Blog>().OrderBy(e => e.Host.HostName).ToList();

                VerifyBlogs(blogs);
            }
        }

        private static void VerifyBlogs(List<Blog> blogs)
        {
            Assert.Equal(3, blogs.Count);

            for (var i = 0; i < 3; i++)
            {
                Assert.Equal($"firstNameReader{i}", blogs[i].Reader.FirstName);
                Assert.Equal($"lastNameReader{i}", blogs[i].Reader.LastName);

                Assert.Equal($"firstNameWriter{i}", blogs[i].Writer.FirstName);
                Assert.Equal($"lastNameWriter{i}", blogs[i].Writer.LastName);

                Assert.Equal($"127.0.0.{i + 1}", blogs[i].Host.HostName);
            }
        }

        [ConditionalFact]
        public virtual void Lazy_loading_finds_correct_entity_type_with_multiple_queries()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var blogs = context.Set<Blog>().Where(_ => true);

                VerifyBlogs(blogs.ToList().OrderBy(e => e.Host.HostName).ToList());
                Assert.Equal(12, context.ChangeTracker.Entries().Count());

                VerifyBlogs(blogs.ToList().OrderBy(e => e.Host.HostName).ToList());
                Assert.Equal(12, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Lazy_loading_finds_correct_entity_type_with_opaque_predicate_and_multiple_queries()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                // ReSharper disable once ConvertToLocalFunction
                bool opaquePredicate(Blog _) => true;

                var blogs = context.Set<Blog>().Where(opaquePredicate);

                VerifyBlogs(blogs.ToList().OrderBy(e => e.Host.HostName).ToList());
                Assert.Equal(12, context.ChangeTracker.Entries().Count());

                VerifyBlogs(blogs.ToList().OrderBy(e => e.Host.HostName).ToList());
                Assert.Equal(12, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact(Skip = "Issue#16622")]
        public virtual void Lazy_loading_finds_correct_entity_type_with_multiple_queries_using_Count()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var blogs = context.Set<Blog>().Where(_ => true);

                Assert.Equal(3, blogs.Count());
                Assert.Empty(context.ChangeTracker.Entries());

                Assert.Equal(3, blogs.Count());
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [ConditionalFact]
        public virtual void Lazy_loading_shares_service__property_on_derived_types()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parson = context.Set<Parson>().Single();

                Assert.Equal(2, parson.ParsonNoses.Count);
                Assert.Equal(
                    new[] { "Large", "Medium" },
                    parson.ParsonNoses.Select(b => b.Size).OrderBy(h => h));

                var company = context.Set<Company>().Single();

                Assert.Equal(2, company.CompanyNoses.Count);
                Assert.Equal(
                    new[] { "Large", "Small" },
                    company.CompanyNoses.Select(b => b.Size).OrderBy(h => h));

                var entity = context.Set<Entity>().ToList().Except(new Entity[] { parson, company }).Single();

                Assert.Equal(3, entity.BaseNoses.Count);
                Assert.Equal(
                    new[] { "Large", "Medium", "Small" },
                    entity.BaseNoses.Select(b => b.Size).OrderBy(h => h));
            }
        }

        [ConditionalFact]
        public virtual void Lazy_loading_finds_correct_entity_type_with_alternate_model()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var person = context.Set<Pyrson>().Single();

                Assert.NotNull(person.Name.FirstName);
                Assert.NotNull(person.Name.LastName);
                Assert.NotNull(person.Address);

                var applicant = context.Set<Applicant>().Single();

                Assert.NotNull(applicant.Name);

                var address = context.Set<Address>().Single();

                Assert.NotNull(address.Line1);
                Assert.NotNull(address.Line2);

                Assert.Same(address, person.Address);
            }
        }

        public class Address
        {
            public int AddressId { get; set; }
            public string Line1 { get; set; }
            public string Line2 { get; set; }
            public int PyrsonId { get; set; }
        }

        public class Applicant
        {
            public int ApplicantId { get; set; }
            public virtual FullName Name { get; set; }

            protected Applicant()
            {
            }

            public Applicant(FullName name)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }
        }

        public class FirstName
        {
            private readonly string _value;

            protected FirstName()
            {
            }

            private FirstName(string value)
            {
                _value = value;
            }

            public static FirstName Create(string firstName)
            {
                return new FirstName(firstName);
            }
        }

        public class LastName
        {
            private readonly string _value;

            protected LastName()
            {
            }

            private LastName(string value)
            {
                _value = value;
            }

            public static LastName Create(string lastName)
            {
                return new LastName(lastName);
            }
        }

        public class Pyrson
        {
            public int PyrsonId { get; set; }
            public virtual FullName Name { get; set; }
            public virtual Address Address { get; set; }

            protected Pyrson()
            {
            }

            public Pyrson(FullName name)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }
        }

        public class FullName
        {
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public virtual FirstName FirstName { get; private set; }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public virtual LastName LastName { get; private set; }

            protected FullName()
            {
            }

            public FullName(FirstName firstName, LastName lastName)
            {
                FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
                LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            }
        }

        public class Parent
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string AlternateId { get; set; }

            public virtual IEnumerable<Child> Children { get; set; }
            public virtual SinglePkToPk SinglePkToPk { get; set; }
            public virtual Single Single { get; set; }
            public virtual IEnumerable<ChildAk> ChildrenAk { get; set; }
            public virtual SingleAk SingleAk { get; set; }
            public virtual IEnumerable<ChildShadowFk> ChildrenShadowFk { get; set; }
            public virtual SingleShadowFk SingleShadowFk { get; set; }
            public virtual IEnumerable<ChildCompositeKey> ChildrenCompositeKey { get; set; }
            public virtual SingleCompositeKey SingleCompositeKey { get; set; }
        }

        public class Child
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class SinglePkToPk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class Single
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class ChildAk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string ParentId { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class SingleAk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string ParentId { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class ChildShadowFk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class SingleShadowFk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class ChildCompositeKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public string ParentAlternateId { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class SingleCompositeKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public string ParentAlternateId { get; set; }

            public virtual Parent Parent { get; set; }
        }

        public class Blog
        {
            public int Id { get; set; }
            public virtual Person Writer { get; set; }
            public virtual Person Reader { get; set; }
            public virtual Host Host { get; set; }
        }

        public class Nose
        {
            public int Id { get; set; }
            public string Size { get; set; }
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class Entity
        {
            public int Id { get; set; }

            public virtual ICollection<Nose> BaseNoses { get; set; }
        }

        public class Company : Entity
        {
            public virtual ICollection<Nose> CompanyNoses { get; set; }
        }

        public class Parson : Entity
        {
            public virtual ICollection<Nose> ParsonNoses { get; set; }
        }

        public class Host
        {
            public string HostName { get; set; }
        }

        protected DbContext CreateContext(bool lazyLoadingEnabled = false)
        {
            var context = Fixture.CreateContext();
            context.ChangeTracker.LazyLoadingEnabled = lazyLoadingEnabled;

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

        public abstract class LoadFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "LazyLoadProxyTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(
                    serviceCollection
                        .AddScoped<IChangeDetector, ChangeDetectorProxy>()
                        .AddEntityFrameworkProxies());

            // By-design. Lazy loaders are not disposed when using pooling
            protected override bool UsePooling => false;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<Entity>();
                modelBuilder.Entity<Company>();
                modelBuilder.Entity<Parson>();

                modelBuilder.Entity<SingleShadowFk>()
                    .Property<int?>("ParentId");

                modelBuilder.Entity<Parent>(
                    b =>
                    {
                        b.Property(e => e.AlternateId).ValueGeneratedOnAdd();

                        b.HasMany(e => e.Children)
                            .WithOne(e => e.Parent)
                            .HasForeignKey(e => e.ParentId);

                        b.HasOne(e => e.SinglePkToPk)
                            .WithOne(e => e.Parent)
                            .HasForeignKey<SinglePkToPk>(e => e.Id)
                            .IsRequired();

                        b.HasOne(e => e.Single)
                            .WithOne(e => e.Parent)
                            .HasForeignKey<Single>(e => e.ParentId);

                        b.HasMany(e => e.ChildrenAk)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey(e => e.AlternateId)
                            .HasForeignKey(e => e.ParentId);

                        b.HasOne(e => e.SingleAk)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey<Parent>(e => e.AlternateId)
                            .HasForeignKey<SingleAk>(e => e.ParentId);

                        b.HasMany(e => e.ChildrenShadowFk)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey(e => e.Id)
                            .HasForeignKey("ParentId");

                        b.HasOne(e => e.SingleShadowFk)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey<Parent>(e => e.Id)
                            .HasForeignKey<SingleShadowFk>("ParentId");

                        b.HasMany(e => e.ChildrenCompositeKey)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey(
                                e => new
                                {
                                    e.AlternateId,
                                    e.Id
                                })
                            .HasForeignKey(
                                e => new
                                {
                                    e.ParentAlternateId,
                                    e.ParentId
                                });

                        b.HasOne(e => e.SingleCompositeKey)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey<Parent>(
                                e => new
                                {
                                    e.AlternateId,
                                    e.Id
                                })
                            .HasForeignKey<SingleCompositeKey>(
                                e => new
                                {
                                    e.ParentAlternateId,
                                    e.ParentId
                                });
                    });

                modelBuilder.Entity<Blog>(
                    e =>
                    {
                        e.OwnsOne(x => x.Writer);
                        e.OwnsOne(x => x.Reader);
                        e.OwnsOne(x => x.Host);
                    });

                modelBuilder.Entity<Blog>(
                    e =>
                    {
                        e.OwnsOne(x => x.Writer);
                        e.OwnsOne(x => x.Reader);
                        e.OwnsOne(x => x.Host);
                    });

                modelBuilder.Entity<Address>(
                    builder =>
                    {
                        builder.HasKey(prop => prop.AddressId);

                        builder.Property(prop => prop.Line1)
                            .IsRequired()
                            .HasMaxLength(50);

                        builder.Property(prop => prop.Line2)
                            .IsRequired(false)
                            .HasMaxLength(50);
                    });

                modelBuilder.Entity<Applicant>(
                    builder =>
                    {
                        builder.HasKey(prop => prop.ApplicantId);

                        builder.OwnsOne(
                            prop => prop.Name, name =>
                            {
                                name
                                    .OwnsOne(prop => prop.FirstName)
                                    .Property("_value")
                                    .HasMaxLength(50)
                                    .IsRequired();

                                name
                                    .OwnsOne(prop => prop.LastName)
                                    .Property("_value")
                                    .HasMaxLength(50)
                                    .IsRequired();
                            });
                    });

                modelBuilder.Entity<Pyrson>(
                    builder =>
                    {
                        builder.HasKey(prop => prop.PyrsonId);

                        builder.OwnsOne(
                            prop => prop.Name, name =>
                            {
                                name
                                    .OwnsOne(prop => prop.FirstName)
                                    .Property("_value")
                                    .HasMaxLength(50)
                                    .IsRequired();

                                name
                                    .OwnsOne(prop => prop.LastName)
                                    .Property("_value")
                                    .HasMaxLength(50)
                                    .IsRequired();
                            });

                        builder.HasOne(prop => prop.Address)
                            .WithOne()
                            .HasForeignKey<Address>(prop => prop.PyrsonId);
                    });
            }

            protected override void Seed(DbContext context)
            {
                context.Add(
                    new Parent
                    {
                        Id = 707,
                        AlternateId = "Root",
                        Children = new List<Child>
                        {
                            new Child
                            {
                                Id = 11
                            },
                            new Child
                            {
                                Id = 12
                            }
                        },
                        SinglePkToPk = new SinglePkToPk
                        {
                            Id = 707
                        },
                        Single = new Single
                        {
                            Id = 21
                        },
                        ChildrenAk = new List<ChildAk>
                        {
                            new ChildAk
                            {
                                Id = 31
                            },
                            new ChildAk
                            {
                                Id = 32
                            }
                        },
                        SingleAk = new SingleAk
                        {
                            Id = 42
                        },
                        ChildrenShadowFk = new List<ChildShadowFk>
                        {
                            new ChildShadowFk
                            {
                                Id = 51
                            },
                            new ChildShadowFk
                            {
                                Id = 52
                            }
                        },
                        SingleShadowFk = new SingleShadowFk
                        {
                            Id = 62
                        },
                        ChildrenCompositeKey = new List<ChildCompositeKey>
                        {
                            new ChildCompositeKey
                            {
                                Id = 51
                            },
                            new ChildCompositeKey
                            {
                                Id = 52
                            }
                        },
                        SingleCompositeKey = new SingleCompositeKey
                        {
                            Id = 62
                        }
                    });

                context.Add(
                    new Blog
                    {
                        Writer = new Person
                        {
                            FirstName = "firstNameWriter0",
                            LastName = "lastNameWriter0"
                        },
                        Reader = new Person
                        {
                            FirstName = "firstNameReader0",
                            LastName = "lastNameReader0"
                        },
                        Host = new Host
                        {
                            HostName = "127.0.0.1"
                        }
                    });

                context.Add(
                    new Blog
                    {
                        Writer = new Person
                        {
                            FirstName = "firstNameWriter1",
                            LastName = "lastNameWriter1"
                        },
                        Reader = new Person
                        {
                            FirstName = "firstNameReader1",
                            LastName = "lastNameReader1"
                        },
                        Host = new Host
                        {
                            HostName = "127.0.0.2"
                        }
                    });

                context.Add(
                    new Blog
                    {
                        Writer = new Person
                        {
                            FirstName = "firstNameWriter2",
                            LastName = "lastNameWriter2"
                        },
                        Reader = new Person
                        {
                            FirstName = "firstNameReader2",
                            LastName = "lastNameReader2"
                        },
                        Host = new Host
                        {
                            HostName = "127.0.0.3"
                        }
                    });

                var nose1 = new Nose
                {
                    Size = "Small"
                };

                var nose2 = new Nose
                {
                    Size = "Medium"
                };

                var nose3 = new Nose
                {
                    Size = "Large"
                };

                context.Add(
                    new Entity
                    {
                        BaseNoses = new List<Nose>
                        {
                            nose1,
                            nose2,
                            nose3
                        }
                    });

                context.Add(
                    new Parson
                    {
                        ParsonNoses = new List<Nose>
                        {
                            nose2,
                            nose3
                        }
                    });

                context.Add(
                    new Company
                    {
                        CompanyNoses = new List<Nose>
                        {
                            nose1,
                            nose3
                        }
                    });

                context.Add(
                    new Applicant(
                        new FullName(FirstName.Create("Amila"), LastName.Create("Udayanga"))));

                context.Add(
                    new Pyrson(new FullName(FirstName.Create("Amila"), LastName.Create("Udayanga")))
                    {
                        Address = new Address
                        {
                            Line1 = "Line1",
                            Line2 = "Line2"
                        }
                    });

                context.SaveChanges();
            }
        }
    }
}
