// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class LoadTestBase<TFixture> : IClassFixture<TFixture>, IDisposable
        where TFixture : LoadTestBase<TFixture>.LoadFixtureBase, new()
    {
        protected LoadTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.Initialize();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Collection(e => e.Children).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Collection(e => e.Children).Load();
                }

                RecordLog();

                Assert.Equal(2, parent.Children.Count());
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Single(e => e.Id == 12);

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.Single).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.Single).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SinglePkToPks.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SinglePkToPk);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.SinglePkToPk).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.SinglePkToPk).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

                Assert.Same(single, parent.SinglePkToPk);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Collection(e => e.Children).Query().ToListAsync()
                    : context.Entry(parent).Collection(e => e.Children).Query().ToList();

                RecordLog();

                Assert.Equal(2, children.Count);
                Assert.Equal(2, parent.Children.Count());
                Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
                Assert.All(children, p => Assert.Contains(p, parent.Children));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Single(e => e.Id == 12);

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.Single).Query().SingleAsync()
                    : context.Entry(parent).Reference(e => e.Single).Query().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SinglePkToPks.Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SinglePkToPk);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.SinglePkToPk).Query().SingleAsync()
                    : context.Entry(parent).Reference(e => e.SinglePkToPk).Query().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.SinglePkToPk);
                Assert.Same(parent, single.Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new Child { Id = 767, ParentId = null }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new Single { Id = 767, ParentId = null }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new Child { Id = 767, ParentId = null }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(child.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new Single { Id = 767, ParentId = null }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(single.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Collection(e => e.Children).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Collection(e => e.Children).Load();
                }

                RecordLog();

                Assert.Equal(0, parent.Children.Count());
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.Single).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.Single).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(parent.Single);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Collection(e => e.Children).Query().ToListAsync()
                    : context.Entry(parent).Collection(e => e.Children).Query().ToList();

                RecordLog();

                Assert.Equal(0, children.Count);
                Assert.Equal(0, parent.Children.Count());

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(child.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(single.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.Single).Query().SingleOrDefaultAsync()
                    : context.Entry(parent).Reference(e => e.Single).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(single);
                Assert.Null(parent.Single);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Children).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Collection(e => e.Children).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Collection(e => e.Children).Load();
                }

                RecordLog();

                Assert.Equal(2, parent.Children.Count());
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Include(e => e.Parent).Single(e => e.Id == 12);

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Include(e => e.Parent).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Single).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.Single).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.Single).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SinglePkToPks.Include(e => e.Parent).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SinglePkToPk);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.SinglePkToPk).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.SinglePkToPk).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.SinglePkToPk).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<SinglePkToPk>().Single().Entity;

                Assert.Same(single, parent.SinglePkToPk);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Children).Single();

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Collection(e => e.Children).Query().ToListAsync()
                    : context.Entry(parent).Collection(e => e.Children).Query().ToList();

                RecordLog();

                Assert.Equal(2, children.Count);
                Assert.Equal(2, parent.Children.Count());
                Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
                Assert.All(children, p => Assert.Contains(p, parent.Children));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Include(e => e.Parent).Single(e => e.Id == 12);

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Include(e => e.Parent).Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Single).Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.Single).Query().SingleAsync()
                    : context.Entry(parent).Reference(e => e.Single).Query().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_principal_using_Query_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SinglePkToPks.Include(e => e.Parent).Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SinglePkToPk);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_PK_to_PK_reference_to_dependent_using_Query_already_loaded(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.SinglePkToPk).Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.SinglePkToPk).Query().SingleAsync()
                    : context.Entry(parent).Reference(e => e.SinglePkToPk).Query().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.SinglePkToPk);
                Assert.Same(parent, single.Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Navigation("Children").LoadAsync();
                }
                else
                {
                    context.Entry(parent).Navigation("Children").Load();
                }

                RecordLog();

                Assert.Equal(2, parent.Children.Count());
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Single(e => e.Id == 12);

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Navigation("Parent").LoadAsync();
                }
                else
                {
                    context.Entry(child).Navigation("Parent").Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Navigation("Parent").LoadAsync();
                }
                else
                {
                    context.Entry(single).Navigation("Parent").Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Navigation("Single").LoadAsync();
                }
                else
                {
                    context.Entry(parent).Navigation("Single").Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Navigation("Children").Query().OfType<object>().ToListAsync()
                    : context.Entry(parent).Navigation("Children").Query().OfType<object>().ToList();

                RecordLog();

                Assert.Equal(2, children.Count);
                Assert.Equal(2, parent.Children.Count());
                Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
                Assert.All(children, p => Assert.Contains(p, parent.Children));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Single(e => e.Id == 12);

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Navigation("Parent").Query().OfType<object>().SingleAsync()
                    : context.Entry(child).Navigation("Parent").Query().OfType<object>().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, ((Parent)parent).Children.Single());

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Navigation("Parent").Query().OfType<object>().SingleAsync()
                    : context.Entry(single).Navigation("Parent").Query().OfType<object>().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, ((Parent)parent).Single);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Navigation("Single").Query().OfType<object>().SingleAsync()
                    : context.Entry(parent).Navigation("Single").Query().OfType<object>().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.Single);
                Assert.Same(parent, ((Single)single).Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Navigation("Children").LoadAsync();
                }
                else
                {
                    context.Entry(parent).Navigation("Children").Load();
                }

                RecordLog();

                Assert.Equal(0, parent.Children.Count());
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Navigation("Parent").LoadAsync();
                }
                else
                {
                    context.Entry(child).Navigation("Parent").Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Navigation("Parent").LoadAsync();
                }
                else
                {
                    context.Entry(single).Navigation("Parent").Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Navigation("Single").LoadAsync();
                }
                else
                {
                    context.Entry(parent).Navigation("Single").Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(parent.Single);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Navigation("Children").Query().OfType<object>().ToListAsync()
                    : context.Entry(parent).Navigation("Children").Query().OfType<object>().ToList();

                RecordLog();

                Assert.Equal(0, children.Count);
                Assert.Equal(0, parent.Children.Count());

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new Child { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Navigation("Parent").Query().OfType<object>().SingleOrDefaultAsync()
                    : context.Entry(child).Navigation("Parent").Query().OfType<object>().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(child.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new Single { Id = 767, ParentId = 787 }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Navigation("Parent").Query().OfType<object>().SingleOrDefaultAsync()
                    : context.Entry(single).Navigation("Parent").Query().OfType<object>().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(single.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_not_found_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Attach(new Parent { Id = 767, AlternateId = "NewRoot" }).Entity;

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Navigation("Single").Query().OfType<object>().SingleOrDefaultAsync()
                    : context.Entry(parent).Navigation("Single").Query().OfType<object>().SingleOrDefault();

                RecordLog();

                Assert.Null(single);
                Assert.Null(parent.Single);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Children).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Navigation("Children").LoadAsync();
                }
                else
                {
                    context.Entry(parent).Navigation("Children").Load();
                }

                RecordLog();

                Assert.Equal(2, parent.Children.Count());
                Assert.All(parent.Children.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Include(e => e.Parent).Single(e => e.Id == 12);

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Navigation("Parent").LoadAsync();
                }
                else
                {
                    context.Entry(child).Navigation("Parent").Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.Children.Single());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Include(e => e.Parent).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Navigation("Parent").LoadAsync();
                }
                else
                {
                    context.Entry(single).Navigation("Parent").Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.Single);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Single).Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Navigation("Single").LoadAsync();
                }
                else
                {
                    context.Entry(parent).Navigation("Single").Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<Single>().Single().Entity;

                Assert.Same(single, parent.Single);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Children).Single();

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Navigation("Children").Query().OfType<object>().ToListAsync()
                    : context.Entry(parent).Navigation("Children").Query().OfType<object>().ToList();

                RecordLog();

                Assert.Equal(2, children.Count);
                Assert.Equal(2, parent.Children.Count());
                Assert.All(children.Select(e => ((Child)e).Parent), c => Assert.Same(parent, c));
                Assert.All(children, p => Assert.Contains(p, parent.Children));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Children.Include(e => e.Parent).Single(e => e.Id == 12);

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Navigation("Parent").Query().OfType<object>().SingleAsync()
                    : context.Entry(child).Navigation("Parent").Query().OfType<object>().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, ((Parent)parent).Children.Single());

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Singles.Include(e => e.Parent).Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Navigation("Parent").Query().OfType<object>().SingleAsync()
                    : context.Entry(single).Navigation("Parent").Query().OfType<object>().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, ((Parent)parent).Single);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_already_loaded_untyped(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Include(e => e.Single).Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Navigation("Single").Query().OfType<object>().SingleAsync()
                    : context.Entry(parent).Navigation("Single").Query().OfType<object>().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.Single);
                Assert.Same(parent, ((Single)single).Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Collection(e => e.ChildrenAk).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Collection(e => e.ChildrenAk).Load();
                }

                RecordLog();

                Assert.Equal(2, parent.ChildrenAk.Count());
                Assert.All(parent.ChildrenAk.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.ChildrenAks.Single(e => e.Id == 32);

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenAk.Single());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SingleAks.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleAk);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.SingleAk).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.SingleAk).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<SingleAk>().Single().Entity;

                Assert.Same(single, parent.SingleAk);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Collection(e => e.ChildrenAk).Query().ToListAsync()
                    : context.Entry(parent).Collection(e => e.ChildrenAk).Query().ToList();

                RecordLog();

                Assert.Equal(2, children.Count);
                Assert.Equal(2, parent.ChildrenAk.Count());
                Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
                Assert.All(children, p => Assert.Contains(p, parent.ChildrenAk));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.ChildrenAks.Single(e => e.Id == 32);

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenAk.Single());

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SingleAks.Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleAk);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.SingleAk).Query().SingleAsync()
                    : context.Entry(parent).Reference(e => e.SingleAk).Query().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.SingleAk);
                Assert.Same(parent, single.Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new ChildAk { Id = 767, ParentId = null }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new SingleAk { Id = 767, ParentId = null }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new ChildAk { Id = 767, ParentId = null }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(child.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_alternate_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new SingleAk { Id = 767, ParentId = null }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(single.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Collection(e => e.ChildrenShadowFk).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Collection(e => e.ChildrenShadowFk).Load();
                }

                RecordLog();

                Assert.Equal(2, parent.ChildrenShadowFk.Count());
                Assert.All(parent.ChildrenShadowFk.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.ChildrenShadowFks.Single(e => e.Id == 52);

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenShadowFk.Single());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SingleShadowFks.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleShadowFk);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.SingleShadowFk).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.SingleShadowFk).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<SingleShadowFk>().Single().Entity;

                Assert.Same(single, parent.SingleShadowFk);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Collection(e => e.ChildrenShadowFk).Query().ToListAsync()
                    : context.Entry(parent).Collection(e => e.ChildrenShadowFk).Query().ToList();

                RecordLog();

                Assert.Equal(2, children.Count);
                Assert.Equal(2, parent.ChildrenShadowFk.Count());
                Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
                Assert.All(children, p => Assert.Contains(p, parent.ChildrenShadowFk));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.ChildrenShadowFks.Single(e => e.Id == 52);

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenShadowFk.Single());

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SingleShadowFks.Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleShadowFk);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.SingleShadowFk).Query().SingleAsync()
                    : context.Entry(parent).Reference(e => e.SingleShadowFk).Query().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.SingleShadowFk);
                Assert.Same(parent, single.Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new ChildShadowFk { Id = 767 }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new SingleShadowFk { Id = 767 }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new ChildShadowFk { Id = 767 }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(child.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_shadow_fk(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new SingleShadowFk { Id = 767 }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(single.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Collection(e => e.ChildrenCompositeKey).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Collection(e => e.ChildrenCompositeKey).Load();
                }

                RecordLog();

                Assert.Equal(2, parent.ChildrenCompositeKey.Count());
                Assert.All(parent.ChildrenCompositeKey.Select(e => e.Parent), c => Assert.Same(parent, c));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.ChildrenCompositeKeys.Single(e => e.Id == 52);

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenCompositeKey.Single());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SingleCompositeKeys.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var parent = context.ChangeTracker.Entries<Parent>().Single().Entity;

                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleCompositeKey);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                if (async)
                {
                    await context.Entry(parent).Reference(e => e.SingleCompositeKey).LoadAsync();
                }
                else
                {
                    context.Entry(parent).Reference(e => e.SingleCompositeKey).Load();
                }

                RecordLog();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                var single = context.ChangeTracker.Entries<SingleCompositeKey>().Single().Entity;

                Assert.Same(single, parent.SingleCompositeKey);
                Assert.Same(parent, single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_collection_using_Query_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var children = async
                    ? await context.Entry(parent).Collection(e => e.ChildrenCompositeKey).Query().ToListAsync()
                    : context.Entry(parent).Collection(e => e.ChildrenCompositeKey).Query().ToList();

                RecordLog();

                Assert.Equal(2, children.Count);
                Assert.Equal(2, parent.ChildrenCompositeKey.Count());
                Assert.All(children.Select(e => e.Parent), c => Assert.Same(parent, c));
                Assert.All(children, p => Assert.Contains(p, parent.ChildrenCompositeKey));

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.ChildrenCompositeKeys.Single(e => e.Id == 52);

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, child.Parent);
                Assert.Same(child, parent.ChildrenCompositeKey.Single());

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.SingleCompositeKeys.Single();

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().Single();

                RecordLog();

                Assert.NotNull(parent);
                Assert.Same(parent, single.Parent);
                Assert.Same(single, parent.SingleCompositeKey);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_dependent_using_Query_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var parent = context.Parents.Single();

                ClearLog();

                var single = async
                    ? await context.Entry(parent).Reference(e => e.SingleCompositeKey).Query().SingleAsync()
                    : context.Entry(parent).Reference(e => e.SingleCompositeKey).Query().Single();

                RecordLog();

                Assert.NotNull(single);
                Assert.Same(single, parent.SingleCompositeKey);
                Assert.Same(parent, single.Parent);

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_null_FK_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new ChildCompositeKey { Id = 767, ParentId = 567 }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(child).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(child).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Null(child.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_null_FK_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new SingleCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

                ClearLog();

                if (async)
                {
                    await context.Entry(single).Reference(e => e.Parent).LoadAsync();
                }
                else
                {
                    context.Entry(single).Reference(e => e.Parent).Load();
                }

                RecordLog();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());

                Assert.Null(single.Parent);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_many_to_one_reference_to_principal_using_Query_null_FK_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var child = context.Attach(new ChildCompositeKey { Id = 767, ParentAlternateId = "Boot" }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(child).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(child.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual async Task Load_one_to_one_reference_to_principal_using_Query_null_FK_composite_key(bool async)
        {
            using (var context = CreateContext())
            {
                var single = context.Attach(new SingleCompositeKey { Id = 767, ParentId = 567 }).Entity;

                ClearLog();

                var parent = async
                    ? await context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefaultAsync()
                    : context.Entry(single).Reference(e => e.Parent).Query().SingleOrDefault();

                RecordLog();

                Assert.Null(parent);
                Assert.Null(single.Parent);

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        protected class Parent
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string AlternateId { get; set; }

            public IEnumerable<Child> Children { get; set; }
            public SinglePkToPk SinglePkToPk { get; set; }
            public Single Single { get; set; }

            public IEnumerable<ChildAk> ChildrenAk { get; set; }
            public SingleAk SingleAk { get; set; }

            public IEnumerable<ChildShadowFk> ChildrenShadowFk { get; set; }
            public SingleShadowFk SingleShadowFk { get; set; }

            public IEnumerable<ChildCompositeKey> ChildrenCompositeKey { get; set; }
            public SingleCompositeKey SingleCompositeKey { get; set; }
        }

        protected class Child
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public Parent Parent { get; set; }
        }

        protected class SinglePkToPk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public Parent Parent { get; set; }
        }

        protected class Single
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public Parent Parent { get; set; }
        }

        protected class ChildAk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string ParentId { get; set; }
            public Parent Parent { get; set; }
        }

        protected class SingleAk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string ParentId { get; set; }
            public Parent Parent { get; set; }
        }

        protected class ChildShadowFk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public Parent Parent { get; set; }
        }

        protected class SingleShadowFk
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public Parent Parent { get; set; }
        }

        protected class ChildCompositeKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public string ParentAlternateId { get; set; }
            public Parent Parent { get; set; }
        }

        protected class SingleCompositeKey
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int? ParentId { get; set; }
            public string ParentAlternateId { get; set; }
            public Parent Parent { get; set; }
        }

        protected class LoadContext : DbContext
        {
            public LoadContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Parent> Parents { get; set; }
            public DbSet<SinglePkToPk> SinglePkToPks { get; set; }
            public DbSet<Single> Singles { get; set; }
            public DbSet<Child> Children { get; set; }
            public DbSet<SingleAk> SingleAks { get; set; }
            public DbSet<ChildAk> ChildrenAks { get; set; }
            public DbSet<SingleShadowFk> SingleShadowFks { get; set; }
            public DbSet<ChildShadowFk> ChildrenShadowFks { get; set; }
            public DbSet<SingleCompositeKey> SingleCompositeKeys { get; set; }
            public DbSet<ChildCompositeKey> ChildrenCompositeKeys { get; set; }
        }

        protected LoadContext CreateContext() => (LoadContext)Fixture.CreateContext();

        protected TFixture Fixture { get; }

        public virtual void Dispose()
        {
        }

        public virtual void ClearLog()
        {
        }

        public virtual void RecordLog()
        {
        }

        public abstract class LoadFixtureBase
        {
            private static readonly object _lock = new object();
            private static bool _initialized;

            public virtual void Initialize()
            {
                lock (_lock)
                {
                    if (!_initialized)
                    {
                        CreateTestStore();
                        _initialized = true;
                    }
                }
            }

            public abstract void CreateTestStore();

            public abstract DbContext CreateContext();

            protected virtual void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Parent>(b =>
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
                            .HasPrincipalKey(e => new { e.AlternateId, e.Id })
                            .HasForeignKey(e => new { e.ParentAlternateId, e.ParentId });

                        b.HasOne(e => e.SingleCompositeKey)
                            .WithOne(e => e.Parent)
                            .HasPrincipalKey<Parent>(e => new { e.AlternateId, e.Id })
                            .HasForeignKey<SingleCompositeKey>(e => new { e.ParentAlternateId, e.ParentId });
                    });
            }

            protected virtual object CreateFullGraph()
                => new Parent
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
                };

            protected virtual void Seed(DbContext context)
            {
                context.Add(CreateFullGraph());
                context.SaveChanges();
            }
        }
    }
}
