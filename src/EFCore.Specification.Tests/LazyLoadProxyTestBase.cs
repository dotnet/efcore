// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class LazyLoadProxyTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : LazyLoadProxyTestBase<TFixture>.LoadFixtureBase
    {
        protected LazyLoadProxyTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Single();

                ClearLog();

                var collectionEntry = context.Entry(parent).Collection(e => e.Children);

                context.Entry(parent).State = state;

                Assert.False(collectionEntry.IsLoaded);

                Assert.NotNull(parent.Children);

                Assert.True(collectionEntry.IsLoaded);

                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.Children.Count());
                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.Set<Child>().Single(e => e.Id == 12);

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
                Assert.Same(child, parent.Children.Single());
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.Set<Single>().Single();

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
                Assert.Same(single, parent.Single);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Single();

                ClearLog();

                var referenceEntry = context.Entry(parent).Reference(e => e.Single);

                context.Entry(parent).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.NotNull(parent.Single);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.Set<SinglePkToPk>().Single();

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
                Assert.Same(single, parent.SinglePkToPk);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Single();

                ClearLog();

                var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

                context.Entry(parent).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.NotNull(parent.SinglePkToPk);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

                Assert.Same(single, parent.SinglePkToPk);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_null_FK(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.CreateProxy<Child>();
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

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_null_FK(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.CreateProxy<Single>();
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

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.CreateProxy<Parent>();
                parent.Id = 767;
                parent.AlternateId = "NewRoot";

                context.Attach(parent);

                ClearLog();

                var collectionEntry = context.Entry(parent).Collection(e => e.Children);

                context.Entry(parent).State = state;

                Assert.False(collectionEntry.IsLoaded);

                Assert.Empty(parent.Children);

                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(0, parent.Children.Count());
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.CreateProxy<Child>();
                child.Id = 767;
                child.ParentId = 787;

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

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.CreateProxy<Single>();
                single.Id = 767;
                single.ParentId = 787;

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

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_not_found(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.CreateProxy<Parent>();
                parent.Id = 767;
                parent.AlternateId = "NewRoot";

                context.Attach(parent);

                ClearLog();

                var referenceEntry = context.Entry(parent).Reference(e => e.Single);

                context.Entry(parent).State = state;

                Assert.False(referenceEntry.IsLoaded);

                Assert.Null(parent.Single);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(parent.Single);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_collection_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Include(e => e.Children).Single();

                ClearLog();

                var collectionEntry = context.Entry(parent).Collection(e => e.Children);

                context.Entry(parent).State = state;

                Assert.True(collectionEntry.IsLoaded);

                Assert.NotNull(parent.Children);

                Assert.True(collectionEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, parent.Children.Count());
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_many_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.Set<Child>().Include(e => e.Parent).Single(e => e.Id == 12);

                ClearLog();

                var referenceEntry = context.Entry(child).Reference(e => e.Parent);

                context.Entry(child).State = state;

                Assert.True(referenceEntry.IsLoaded);

                Assert.NotNull(child.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_principal_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.Set<Single>().Include(e => e.Parent).Single();

                ClearLog();

                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                context.Entry(single).State = state;

                Assert.True(referenceEntry.IsLoaded);

                Assert.NotNull(single.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_reference_to_dependent_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Include(e => e.Single).Single();

                ClearLog();

                var referenceEntry = context.Entry(parent).Reference(e => e.Single);

                context.Entry(parent).State = state;

                Assert.True(referenceEntry.IsLoaded);

                Assert.NotNull(parent.Single);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var single = context.Set<SinglePkToPk>().Include(e => e.Parent).Single();

                ClearLog();

                var referenceEntry = context.Entry(single).Reference(e => e.Parent);

                context.Entry(single).State = state;

                Assert.True(referenceEntry.IsLoaded);

                Assert.NotNull(single.Parent);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SinglePkToPk);
            }
        }

        [Theory]
        [InlineData(EntityState.Unchanged)]
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public virtual void Lazy_load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(EntityState state)
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Include(e => e.SinglePkToPk).Single();

                ClearLog();

                var referenceEntry = context.Entry(parent).Reference(e => e.SinglePkToPk);

                context.Entry(parent).State = state;

                Assert.True(referenceEntry.IsLoaded);

                Assert.NotNull(parent.SinglePkToPk);

                Assert.True(referenceEntry.IsLoaded);

                RecordLog();
                context.ChangeTracker.LazyLoadingEnabled = false;

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

                Assert.Same(single, parent.SinglePkToPk);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Theory]
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

        [Fact]
        public virtual void Lazy_load_collection_for_detached_is_no_op()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Single();

                context.Entry(parent).State = EntityState.Detached;

                Assert.Null(parent.Children);
            }
        }

        [Fact]
        public virtual void Lazy_load_reference_to_principal_for_detached_is_no_op()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var child = context.Set<Child>().Single(e => e.Id == 12);

                context.Entry(child).State = EntityState.Detached;

                Assert.Null(child.Parent);
            }
        }

        [Fact]
        public virtual void Lazy_load_reference_to_dependent_for_detached_is_no_op()
        {
            using (var context = CreateContext(lazyLoadingEnabled: true))
            {
                var parent = context.Set<Parent>().Single();

                context.Entry(parent).State = EntityState.Detached;

                Assert.Null(parent.Single);
            }
        }

        [Theory]
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

        public abstract class LoadFixtureBase : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = "LazyLoadProxyTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
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
            }

            protected override void Seed(DbContext context)
            {
                context.Add(
                    (object)new Parent
                    {
                        Id = 707,
                        AlternateId = "Root",
                        Children = new List<Child>
                        {
                            new Child { Id = 11 },
                            new Child { Id = 12 }
                        },
                        SinglePkToPk = new SinglePkToPk { Id = 707 },
                        Single = new Single { Id = 21 },
                        ChildrenAk = new List<ChildAk>
                        {
                            new ChildAk { Id = 31 },
                            new ChildAk { Id = 32 }
                        },
                        SingleAk = new SingleAk { Id = 42 },
                        ChildrenShadowFk = new List<ChildShadowFk>
                        {
                            new ChildShadowFk { Id = 51 },
                            new ChildShadowFk { Id = 52 }
                        },
                        SingleShadowFk = new SingleShadowFk { Id = 62 },
                        ChildrenCompositeKey = new List<ChildCompositeKey>
                        {
                            new ChildCompositeKey { Id = 51 },
                            new ChildCompositeKey { Id = 52 }
                        },
                        SingleCompositeKey = new SingleCompositeKey { Id = 62 }
                    });
                context.SaveChanges();
            }
        }
    }
}
