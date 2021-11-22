// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class OwnedEntityQueryTestBase : NonSharedModelTestBase
    {
        public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

        protected override string StoreName
            => "OwnedEntityQueryTests";

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Multiple_single_result_in_projection_containing_owned_types(bool async)
        {
            var contextFactory = await InitializeAsync<Context20277>();

            using (var context = contextFactory.CreateContext())
            {
                var query = context.Entities.AsNoTracking().Select(
                    e => new
                    {
                        e.Id,
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

            public DbSet<Entity20277> Entities
                => Set<Entity20277>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Entity20277>(
                    cfg =>
                    {
                        cfg.OwnsMany(
                            e => e.Children, inner =>
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
                var results = await context.Contacts.Select(
                        contact => new ContactDto22089
                        {
                            Id = contact.Id, Names = contact.Names.Select(name => new NameDto22089()).ToArray()
                        })
                    .ToListAsync();
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Projecting_owned_collection_and_aggregate(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext24133>();

            using var context = contextFactory.CreateContext();
            var query = context.Set<Blog24133>()
                .Select(
                    b => new BlogDto24133
                    {
                        Id = b.Id,
                        TotalComments = b.Posts.Sum(p => p.CommentsCount),
                        Posts = b.Posts.Select(p => new PostDto24133 { Title = p.Title, CommentsCount = p.CommentsCount })
                    });

            var result = async
                ? await query.ToListAsync()
                : query.ToList();
        }

        protected class MyContext24133 : DbContext
        {
            public MyContext24133(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Blog24133>(
                    blog =>
                    {
                        blog.OwnsMany(
                            b => b.Posts, p =>
                            {
                                p.WithOwner().HasForeignKey("BlogId");
                                p.Property("BlogId").HasMaxLength(40);
                            });
                    });
        }

        protected class Blog24133
        {
            public int Id { get; private set; }

            private List<Post24133> _posts = new();

            public static Blog24133 Create(IEnumerable<Post24133> posts)
                => new() { _posts = posts.ToList() };

            public IReadOnlyCollection<Post24133> Posts
                => new ReadOnlyCollection<Post24133>(_posts);
        }

        protected class Post24133
        {
            public string Title { get; set; }
            public int CommentsCount { get; set; }
        }

        protected class BlogDto24133
        {
            public int Id { get; set; }
            public int TotalComments { get; set; }
            public IEnumerable<PostDto24133> Posts { get; set; }
        }

        protected class PostDto24133
        {
            public string Title { get; set; }
            public int CommentsCount { get; set; }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Projecting_correlated_collection_property_for_owned_entity(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext18582>(seed: c => c.Seed());

            using var context = contextFactory.CreateContext();
            var query = context.Warehouses.Select(
                x => new WarehouseModel
                {
                    WarehouseCode = x.WarehouseCode,
                    DestinationCountryCodes = x.DestinationCountries.Select(c => c.CountryCode).ToArray()
                }).AsNoTracking();

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            var warehouseModel = Assert.Single(result);
            Assert.Equal("W001", warehouseModel.WarehouseCode);
            Assert.True(new[] { "US", "CA" }.SequenceEqual(warehouseModel.DestinationCountryCodes));
        }

        protected class MyContext18582 : DbContext
        {
            public MyContext18582(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Warehouse> Warehouses { get; set; }

            public void Seed()
            {
                Add(
                    new Warehouse
                    {
                        WarehouseCode = "W001",
                        DestinationCountries =
                        {
                            new WarehouseDestinationCountry { Id = "1", CountryCode = "US" },
                            new WarehouseDestinationCountry { Id = "2", CountryCode = "CA" }
                        }
                    });

                SaveChanges();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Warehouse>()
                    .OwnsMany(x => x.DestinationCountries)
                    .WithOwner()
                    .HasForeignKey(x => x.WarehouseCode)
                    .HasPrincipalKey(x => x.WarehouseCode);
        }

        protected class Warehouse
        {
            public int Id { get; set; }
            public string WarehouseCode { get; set; }
            public ICollection<WarehouseDestinationCountry> DestinationCountries { get; set; } = new HashSet<WarehouseDestinationCountry>();
        }

        protected class WarehouseDestinationCountry
        {
            public string Id { get; set; }
            public string WarehouseCode { get; set; }
            public string CountryCode { get; set; }
        }

        protected class WarehouseModel
        {
            public string WarehouseCode { get; set; }

            public ICollection<string> DestinationCountryCodes { get; set; }
        }

        protected virtual async Task Owned_references_on_same_level_expanded_at_different_times_around_take_helper(
            MyContext26592Base context,
            bool async)
        {
            var query = context.Companies.Where(e => e.CustomerData != null).OrderBy(e => e.Id).Take(10);
            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            var company = Assert.Single(result);
            Assert.Equal("Acme Inc.", company.Name);
            Assert.Equal("Regular", company.CustomerData.AdditionalCustomerData);
            Assert.Equal("Free shipping", company.SupplierData.AdditionalSupplierData);
        }

        protected virtual async Task Owned_references_on_same_level_nested_expanded_at_different_times_around_take_helper(
            MyContext26592Base context,
            bool async)
        {
            var query = context.Owners.Where(e => e.OwnedEntity.CustomerData != null).OrderBy(e => e.Id).Take(10);
            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            var owner = Assert.Single(result);
            Assert.Equal("Owner1", owner.Name);
            Assert.Equal("Intermediate1", owner.OwnedEntity.Name);
            Assert.Equal("IM Regular", owner.OwnedEntity.CustomerData.AdditionalCustomerData);
            Assert.Equal("IM Free shipping", owner.OwnedEntity.SupplierData.AdditionalSupplierData);
        }

        protected abstract class MyContext26592Base : DbContext
        {
            protected MyContext26592Base(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Company> Companies { get; set; }
            public DbSet<Owner> Owners { get; set; }

            public void Seed()
            {
                Add(
                    new Company
                    {
                        Name = "Acme Inc.",
                        CustomerData = new CustomerData { AdditionalCustomerData = "Regular" },
                        SupplierData = new SupplierData { AdditionalSupplierData = "Free shipping" }
                    });

                Add(
                    new Owner
                    {
                        Name = "Owner1",
                        OwnedEntity = new IntermediateOwnedEntity
                        {
                            Name = "Intermediate1",
                            CustomerData = new CustomerData { AdditionalCustomerData = "IM Regular" },
                            SupplierData = new SupplierData { AdditionalSupplierData = "IM Free shipping" }
                        }
                    });

                SaveChanges();
            }
        }

        protected class Company
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public CustomerData CustomerData { get; set; }
            public SupplierData SupplierData { get; set; }
        }

        [Owned]
        protected class CustomerData
        {
            public int Id { get; set; }
            public string AdditionalCustomerData { get; set; }
        }

        [Owned]
        protected class SupplierData
        {
            public int Id { get; set; }
            public string AdditionalSupplierData { get; set; }
        }

        protected class Owner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IntermediateOwnedEntity OwnedEntity { get; set; }
        }

        [Owned]
        protected class IntermediateOwnedEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public CustomerData CustomerData { get; set; }
            public SupplierData SupplierData { get; set; }
        }
    }
}
