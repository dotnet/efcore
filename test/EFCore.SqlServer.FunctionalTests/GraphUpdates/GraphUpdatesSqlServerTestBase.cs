// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class GraphUpdatesSqlServerTestBase<TFixture>(TFixture fixture) : GraphUpdatesTestBase<TFixture>(fixture)
    where TFixture : GraphUpdatesSqlServerTestBase<TFixture>.GraphUpdatesSqlServerFixtureBase, new()
{
    [ConditionalFact] // Issue #32638
    public virtual void Key_and_index_properties_use_appropriate_comparer()
    {
        var parent = new StringKeyAndIndexParent
        {
            Id = "Parent",
            AlternateId = "Parent",
            Index = "Index",
            UniqueIndex = "UniqueIndex"
        };

        var child = new StringKeyAndIndexChild { Id = "Child", ParentId = "parent" };

        using var context = CreateContext();
        context.AttachRange(parent, child);

        Assert.Same(child, parent.Child);
        Assert.Same(parent, child.Parent);

        parent.Id = "parent";
        parent.AlternateId = "parent";
        parent.Index = "index";
        parent.UniqueIndex = "uniqueIndex";
        child.Id = "child";
        child.ParentId = "Parent";

        context.ChangeTracker.DetectChanges();

        var parentEntry = context.Entry(parent);
        Assert.Equal(EntityState.Modified, parentEntry.State);
        Assert.False(parentEntry.Property(e => e.Id).IsModified);
        Assert.False(parentEntry.Property(e => e.AlternateId).IsModified);
        Assert.True(parentEntry.Property(e => e.Index).IsModified);
        Assert.True(parentEntry.Property(e => e.UniqueIndex).IsModified);

        var childEntry = context.Entry(child);

        if (childEntry.Metadata.IsOwned())
        {
            Assert.Equal(EntityState.Modified, childEntry.State);
            Assert.True(childEntry.Property(e => e.Id).IsModified); // Not a key for the owned type
            Assert.False(childEntry.Property(e => e.ParentId).IsModified);
        }
        else
        {
            Assert.Equal(EntityState.Unchanged, childEntry.State);
            Assert.False(childEntry.Property(e => e.Id).IsModified);
            Assert.False(childEntry.Property(e => e.ParentId).IsModified);
        }
    }

    protected class StringKeyAndIndexParent : NotifyingEntity
    {
        private string _id;
        private string _alternateId;
        private string _uniqueIndex;
        private string _index;
        private StringKeyAndIndexChild _child;

        public string Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public string AlternateId
        {
            get => _alternateId;
            set => SetWithNotify(value, ref _alternateId);
        }

        public string Index
        {
            get => _index;
            set => SetWithNotify(value, ref _index);
        }

        public string UniqueIndex
        {
            get => _uniqueIndex;
            set => SetWithNotify(value, ref _uniqueIndex);
        }

        public StringKeyAndIndexChild Child
        {
            get => _child;
            set => SetWithNotify(value, ref _child);
        }
    }

    protected class StringKeyAndIndexChild : NotifyingEntity
    {
        private string _id;
        private string _parentId;
        private int _foo;
        private StringKeyAndIndexParent _parent;

        public string Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public string ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public int Foo
        {
            get => _foo;
            set => SetWithNotify(value, ref _foo);
        }

        public StringKeyAndIndexParent Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }
    }

    protected override IQueryable<Root> ModifyQueryRoot(IQueryable<Root> query)
        => query.AsSplitQuery();

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public abstract class GraphUpdatesSqlServerFixtureBase : GraphUpdatesFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<AccessState>(b =>
            {
                b.Property(e => e.AccessStateId).ValueGeneratedNever();
                b.HasData(new AccessState { AccessStateId = 1 });
            });

            modelBuilder.Entity<Cruiser>(b =>
            {
                b.Property(e => e.IdUserState).HasDefaultValue(1);
                b.HasOne(e => e.UserState).WithMany(e => e.Users).HasForeignKey(e => e.IdUserState);
            });

            modelBuilder.Entity<AccessStateWithSentinel>(b =>
            {
                b.Property(e => e.AccessStateWithSentinelId).ValueGeneratedNever();
                b.HasData(new AccessStateWithSentinel { AccessStateWithSentinelId = 1 });
            });

            modelBuilder.Entity<CruiserWithSentinel>(b =>
            {
                b.Property(e => e.IdUserState).HasDefaultValue(1).HasSentinel(667);
                b.HasOne(e => e.UserState).WithMany(e => e.Users).HasForeignKey(e => e.IdUserState);
            });

            modelBuilder.Entity<SomethingOfCategoryA>().Property<int>("CategoryId").HasDefaultValue(1);
            modelBuilder.Entity<SomethingOfCategoryB>().Property(e => e.CategoryId).HasDefaultValue(2);

            modelBuilder.Entity<StringKeyAndIndexParent>(b =>
            {
                b.HasOne(e => e.Child)
                    .WithOne(e => e.Parent)
                    .HasForeignKey<StringKeyAndIndexChild>(e => e.ParentId)
                    .HasPrincipalKey<StringKeyAndIndexParent>(e => e.AlternateId);
            });

            modelBuilder.Entity<CompositeKeyWith<int>>(b =>
            {
                b.Property(e => e.PrimaryGroup).HasDefaultValue(1).HasSentinel(1);
            });

            modelBuilder.Entity<CompositeKeyWith<bool>>(b =>
            {
                b.Property(e => e.PrimaryGroup).HasDefaultValue(true);
            });

            modelBuilder.Entity<CompositeKeyWith<bool?>>(b =>
            {
                b.Property(e => e.PrimaryGroup).HasDefaultValue(true);
            });

            modelBuilder.Entity<ParentWithSetDefault>(b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<ChildWithSetDefault>(b =>
            {
                b.Property(e => e.ParentId).HasDefaultValue(667).HasSentinel(667);
                b.HasOne(e => e.Parent)
                    .WithMany(e => e.Children)
                    .HasForeignKey(e => e.ParentId)
                    .OnDelete(DeleteBehavior.SetDefault);
            });
        }
    }

    protected class ParentWithSetDefault : NotifyingEntity
    {
        private int _id;
        private ICollection<ChildWithSetDefault> _children = new ObservableHashSet<ChildWithSetDefault>();

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public virtual ICollection<ChildWithSetDefault> Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }
    }

    protected class ChildWithSetDefault : NotifyingEntity
    {
        private int _id;
        private int _parentId;
        private ParentWithSetDefault _parent;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int ParentId
        {
            get => _parentId;
            set => SetWithNotify(value, ref _parentId);
        }

        public virtual ParentWithSetDefault Parent
        {
            get => _parent;
            set => SetWithNotify(value, ref _parent);
        }
    }

    [ConditionalTheory, InlineData(false), InlineData(true)]
    public virtual async Task SetDefault_with_default_value_sets_FK_to_default_on_delete(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                // Create a "default" parent that orphaned children will reference
                var defaultParent = new ParentWithSetDefault { Id = 667 };
                // Create the actual parent with a different Id
                var parent = new ParentWithSetDefault { Id = 1 };
                var child = new ChildWithSetDefault { ParentId = 1, Parent = parent };
                parent.Children.Add(child);

                if (async)
                {
                    await context.AddRangeAsync(defaultParent, parent);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.AddRange(defaultParent, parent);
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var parent = async
                    ? await context.Set<ParentWithSetDefault>().Include(e => e.Children).SingleAsync(e => e.Id == 1)
                    : context.Set<ParentWithSetDefault>().Include(e => e.Children).Single(e => e.Id == 1);

                var child = parent.Children.Single();
                Assert.Equal(1, child.ParentId);

                context.Remove(parent);

                Assert.Equal(EntityState.Deleted, context.Entry(parent).State);
                Assert.Equal(EntityState.Modified, context.Entry(child).State);
                Assert.Equal(667, child.ParentId); // FK should be set to default value (the default parent)
                Assert.Null(child.Parent);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var child = async
                    ? await context.Set<ChildWithSetDefault>().SingleAsync()
                    : context.Set<ChildWithSetDefault>().Single();

                Assert.Equal(667, child.ParentId); // Verify FK was persisted with default value
            });
}
