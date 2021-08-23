// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class OwnedEntityQueryTestBase : NonSharedModelTestBase
    {
        public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

        protected override string StoreName => "OwnedEntityQueryTests";

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Multiple_single_result_in_projection_containing_owned_types(bool async)
        {
            var contextFactory = await InitializeAsync<Context20277>();

            using (var context = contextFactory.CreateContext())
            {
                var query = context.Entities.AsNoTracking().Select(e => new
                {
                    Id = e.Id,
                    FirstChild = e.Children
                    .Where(c => c.Type == 1)
                    .AsQueryable()
                    .Select(_project)
                    .FirstOrDefault(),

                    SecondChild = e.Children
                    .Where(c => c.Type == 2)
                    .AsQueryable()
                    .Select(_project)
                    .FirstOrDefault(),
                });

                var result = async
                    ? await query.ToListAsync()
                    : query.ToList();
            }
        }

        private static readonly Expression<Func<Child20277, object>> _project = x => new
        {
            x.Id,
            x.Owned, // Comment this line for success
            x.Type,
        };

        protected class Context20277 : DbContext
        {
            public Context20277(DbContextOptions options)
                   : base(options)
            {
            }

            public DbSet<Entity20277> Entities => Set<Entity20277>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Entity20277>(cfg =>
                {
                    cfg.OwnsMany(e => e.Children, inner =>
                    {
                        inner.OwnsOne(e => e.Owned);
                    });
                });
            }
        }

        protected class Entity20277
        {
            public int Id { get; set; }
            public List<Child20277> Children { get; set; }
        }

        protected class Child20277
        {
            public int Id { get; set; }
            public int Type { get; set; }
            public Owned20277 Owned { get; set; }
        }

        protected class Owned20277
        {
            public bool IsDeleted { get; set; }
            public string Value { get; set; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task OwnsMany_correlated_projection(bool async)
        {
            var contextFactory = await InitializeAsync<SomeDbContext22089>();

            using (var context = contextFactory.CreateContext())
            {
                var results = await context.Contacts.Select(contact => new ContactDto22089()
                {
                    Id = contact.Id,
                    Names = contact.Names.Select(name => new NameDto22089() { }).ToArray()
                }).ToListAsync();
            }
        }

        protected class Contact22089
        {
            public Guid Id { get; set; }
            public IReadOnlyList<Name22809> Names { get; protected set; } = new List<Name22809>();
        }

        protected class ContactDto22089
        {
            public Guid Id { get; set; }
            public IReadOnlyList<NameDto22089> Names { get; set; }
        }

        protected class Name22809
        {
            public Guid Id { get; set; }
            public Guid ContactId { get; set; }
        }

        protected class NameDto22089
        {
            public Guid Id { get; set; }
            public Guid ContactId { get; set; }
        }

        protected class SomeDbContext22089 : DbContext
        {
            public SomeDbContext22089(DbContextOptions options)
                      : base(options)
            {
            }

            public DbSet<Contact22089> Contacts { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Contact22089>().HasKey(c => c.Id);
                modelBuilder.Entity<Contact22089>().OwnsMany(c => c.Names, names => names.WithOwner().HasForeignKey(n => n.ContactId));
            }
        }
    }
}
