// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class OwnedEntityQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "OwnedEntityQueryTests";

    #region 9202

    [ConditionalFact]
    public virtual async Task Include_collection_for_entity_with_owned_type_works()
    {
        var contextFactory = await InitializeAsync<Context9202>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Movies.Include(m => m.Cast);
            var result = query.ToList();

            Assert.Single(result);
            Assert.Equal(3, result[0].Cast.Count);
            Assert.NotNull(result[0].Details);
            Assert.True(result[0].Cast.All(a => a.Details != null));
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Movies.Include("Cast");
            var result = query.ToList();

            Assert.Single(result);
            Assert.Equal(3, result[0].Cast.Count);
            Assert.NotNull(result[0].Details);
            Assert.True(result[0].Cast.All(a => a.Details != null));
        }
    }

    private class Context9202(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>().HasMany(m => m.Cast).WithOne();
            modelBuilder.Entity<Movie>().OwnsOne(m => m.Details);
            modelBuilder.Entity<Actor>().OwnsOne(m => m.Details);
        }

        public Task SeedAsync()
        {
            var av = new Actor { Name = "Alicia Vikander", Details = new Details { Info = "Best actress ever" } };
            var oi = new Actor { Name = "Oscar Isaac", Details = new Details { Info = "Best actor ever made" } };
            var dg = new Actor { Name = "Domhnall Gleeson", Details = new Details { Info = "Second best actor ever" } };
            var em = new Movie
            {
                Title = "Ex Machina",
                Cast =
                [
                    av,
                    oi,
                    dg
                ],
                Details = new Details { Info = "Best movie ever made" }
            };

            Actors.AddRange(av, oi, dg);
            Movies.Add(em);
            return SaveChangesAsync();
        }

        public class Movie
        {
            public int Id { get; set; }
            public string Title { get; set; }

            public List<Actor> Cast { get; set; }

            public Details Details { get; set; }
        }

        public class Actor
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Details Details { get; set; }
        }

        public class Details
        {
            public string Info { get; set; }
            public int Rating { get; set; }
        }
    }

    #endregion

    #region 13079

    [ConditionalFact]
    public virtual async Task Multilevel_owned_entities_determine_correct_nullability()
    {
        var contextFactory = await InitializeAsync<Context13079>();
        using var context = contextFactory.CreateContext();
        await context.AddAsync(new Context13079.BaseEntity());
        await context.SaveChangesAsync();
    }

    private class Context13079(DbContextOptions options) : DbContext(options)
    {
        public virtual DbSet<BaseEntity> BaseEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<DerivedEntity>().OwnsOne(e => e.Data, b => b.OwnsOne(e => e.SubData));

        public class BaseEntity
        {
            public int Id { get; set; }
        }

        public class DerivedEntity : BaseEntity
        {
            public int Property { get; set; }
            public OwnedData Data { get; set; }
        }

        public class OwnedData
        {
            public int Property { get; set; }
            public OwnedSubData SubData { get; set; }
        }

        public class OwnedSubData
        {
            public int Property { get; set; }
        }
    }

    #endregion

    #region 13157

    [ConditionalFact]
    public virtual async Task Correlated_subquery_with_owned_navigation_being_compared_to_null_works()
    {
        var contextFactory = await InitializeAsync<Context13157>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var partners = context.Partners
                .Select(
                    x => new
                    {
                        Addresses = x.Addresses.Select(
                            y => new
                            {
                                Turnovers = y.Turnovers == null
                                    ? null
                                    : new { y.Turnovers.AmountIn }
                            }).ToList()
                    }).ToList();

            Assert.Single(partners);
            Assert.Collection(
                partners[0].Addresses,
                t =>
                {
                    Assert.NotNull(t.Turnovers);
                    Assert.Equal(10, t.Turnovers.AmountIn);
                },
                t =>
                {
                    Assert.Null(t.Turnovers);
                });
        }
    }

    private class Context13157(DbContextOptions options) : DbContext(options)
    {
        public virtual DbSet<Partner> Partners { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Address>().OwnsOne(x => x.Turnovers);

        public Task SeedAsync()
        {
            AddRange(
                new Partner
                {
                    Addresses = new List<Address>
                    {
                        new() { Turnovers = new AddressTurnovers { AmountIn = 10 } }, new() { Turnovers = null },
                    }
                }
            );

            return SaveChangesAsync();
        }

        public class Partner
        {
            public int Id { get; set; }
            public ICollection<Address> Addresses { get; set; }
        }

        public class Address
        {
            public int Id { get; set; }
            public AddressTurnovers Turnovers { get; set; }
        }

        public class AddressTurnovers
        {
            public int AmountIn { get; set; }
        }
    }

    #endregion

    #region 14911

    [ConditionalFact]
    public virtual async Task Owned_entity_multiple_level_in_aggregate()
    {
        var contextFactory = await InitializeAsync<Context14911>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var aggregate = context.Set<Context14911.Aggregate>().OrderByDescending(e => e.Id).FirstOrDefault();
        Assert.Equal(10, aggregate.FirstValueObject.SecondValueObjects[0].FourthValueObject.FifthValueObjects[0].AnyValue);
        Assert.Equal(
            20, aggregate.FirstValueObject.SecondValueObjects[0].ThirdValueObjects[0].FourthValueObject.FifthValueObjects[0].AnyValue);
    }

    protected class Context14911(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Aggregate>(
                builder =>
                {
                    builder.HasKey(e => e.Id);
                    builder.OwnsOne(
                        e => e.FirstValueObject, dr =>
                        {
                            dr.OwnsMany(
                                d => d.SecondValueObjects, c =>
                                {
                                    c.Property<int>("Id").IsRequired();
                                    c.HasKey("Id");
                                    c.OwnsOne(
                                        b => b.FourthValueObject, b =>
                                        {
                                            b.OwnsMany(
                                                t => t.FifthValueObjects, sp =>
                                                {
                                                    sp.Property<int>("Id").IsRequired();
                                                    sp.HasKey("Id");
                                                    sp.Property(e => e.AnyValue).IsRequired();
                                                    sp.WithOwner().HasForeignKey("SecondValueObjectId");
                                                });
                                        });
                                    c.OwnsMany(
                                        b => b.ThirdValueObjects, b =>
                                        {
                                            b.Property<int>("Id").IsRequired();
                                            b.HasKey("Id");

                                            b.OwnsOne(
                                                d => d.FourthValueObject, dpd =>
                                                {
                                                    dpd.OwnsMany(
                                                        d => d.FifthValueObjects, sp =>
                                                        {
                                                            sp.Property<int>("Id").IsRequired();
                                                            sp.HasKey("Id");
                                                            sp.Property(e => e.AnyValue).IsRequired();
                                                            sp.WithOwner().HasForeignKey("ThirdValueObjectId");
                                                        });
                                                });
                                            b.WithOwner().HasForeignKey("SecondValueObjectId");
                                        });
                                    c.WithOwner().HasForeignKey("AggregateId");
                                });
                        });
                });

        public Task SeedAsync()
        {
            var aggregate = new Aggregate
            {
                FirstValueObject = new FirstValueObject
                {
                    SecondValueObjects =
                    [
                        new()
                        {
                            FourthValueObject =
                                new FourthValueObject { FifthValueObjects = [new() { AnyValue = 10 }] },
                            ThirdValueObjects =
                            [
                                new() { FourthValueObject = new FourthValueObject { FifthValueObjects = [new() { AnyValue = 20 }] } }
                            ]
                        }
                    ]
                }
            };

            Set<Aggregate>().Add(aggregate);
            return SaveChangesAsync();
        }

        public class Aggregate
        {
            public int Id { get; set; }
            public FirstValueObject FirstValueObject { get; set; }
        }

        public class FirstValueObject
        {
            public int Value { get; set; }
            public List<SecondValueObject> SecondValueObjects { get; set; }
        }

        public class SecondValueObject
        {
            public FourthValueObject FourthValueObject { get; set; }
            public List<ThirdValueObject> ThirdValueObjects { get; set; }
        }

        public class ThirdValueObject
        {
            public FourthValueObject FourthValueObject { get; set; }
        }

        public class FourthValueObject
        {
            public int Value { get; set; }
            public List<FifthValueObject> FifthValueObjects { get; set; }
        }

        public class FifthValueObject
        {
            public int AnyValue { get; set; }
        }
    }

    #endregion

    #region 18582

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Projecting_correlated_collection_property_for_owned_entity(bool async)
    {
        var contextFactory = await InitializeAsync<Context18582>(seed: c => c.SeedAsync());

        using var context = contextFactory.CreateContext();
        var query = context.Warehouses.Select(
            x => new Context18582.WarehouseModel
            {
                WarehouseCode = x.WarehouseCode, DestinationCountryCodes = x.DestinationCountries.Select(c => c.CountryCode).ToArray()
            }).AsNoTracking();

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        var warehouseModel = Assert.Single(result);
        Assert.Equal("W001", warehouseModel.WarehouseCode);
        Assert.True(new[] { "US", "CA" }.SequenceEqual(warehouseModel.DestinationCountryCodes));
    }

    private class Context18582(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Warehouse> Warehouses { get; set; }

        public Task SeedAsync()
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

            return SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Warehouse>()
                .OwnsMany(x => x.DestinationCountries)
                .WithOwner()
                .HasForeignKey(x => x.WarehouseCode)
                .HasPrincipalKey(x => x.WarehouseCode);

        public class Warehouse
        {
            public int Id { get; set; }
            public string WarehouseCode { get; set; }
            public ICollection<WarehouseDestinationCountry> DestinationCountries { get; set; } = new HashSet<WarehouseDestinationCountry>();
        }

        public class WarehouseDestinationCountry
        {
            public string Id { get; set; }
            public string WarehouseCode { get; set; }
            public string CountryCode { get; set; }
        }

        public class WarehouseModel
        {
            public string WarehouseCode { get; set; }

            public ICollection<string> DestinationCountryCodes { get; set; }
        }
    }

    #endregion

    #region 19138

    [ConditionalFact]
    public virtual async Task Accessing_scalar_property_in_derived_type_projection_does_not_load_owned_navigations()
    {
        var contextFactory = await InitializeAsync<Context19138>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var result = context.BaseEntities
            .Select(b => context.OtherEntities.Where(o => o.OtherEntityData == ((Context19138.SubEntity)b).Data).FirstOrDefault())
            .ToList();

        Assert.Equal("A", Assert.Single(result).OtherEntityData);
    }

    private class Context19138(DbContextOptions options) : DbContext(options)
    {
        public DbSet<BaseEntity> BaseEntities { get; set; }
        public DbSet<OtherEntity> OtherEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseEntity>();
            modelBuilder.Entity<SubEntity>().OwnsOne(se => se.Owned);
            modelBuilder.Entity<OtherEntity>();
        }

        public Task SeedAsync()
        {
            Add(new OtherEntity { OtherEntityData = "A" });
            Add(new SubEntity { Data = "A" });

            return SaveChangesAsync();
        }

        public class BaseEntity
        {
            public int Id { get; set; }
        }

        public class SubEntity : BaseEntity
        {
            public string Data { get; set; }
            public Owned Owned { get; set; }
        }

        public class Owned
        {
            public string OwnedData { get; set; }
            public int Value { get; set; }
        }

        public class OtherEntity
        {
            public int Id { get; set; }
            public string OtherEntityData { get; set; }
        }
    }

    #endregion

    #region 20277

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiple_single_result_in_projection_containing_owned_types(bool async)
    {
        var contextFactory = await InitializeAsync<Context20277>();
        using var context = contextFactory.CreateContext();
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

    private static readonly Expression<Func<Context20277.Child, object>> _project = x => new
    {
        x.Id,
        x.Owned, // Comment this line for success
        x.Type,
    };

    private class Context20277(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities
            => Set<Entity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entity>(
                cfg =>
                {
                    cfg.OwnsMany(
                        e => e.Children, inner =>
                        {
                            inner.OwnsOne(e => e.Owned);
                        });
                });
        }

        public class Entity
        {
            public int Id { get; set; }
            public List<Child> Children { get; set; }
        }

        public class Child
        {
            public int Id { get; set; }
            public int Type { get; set; }
            public Owned Owned { get; set; }
        }

        public class Owned
        {
            public bool IsDeleted { get; set; }
            public string Value { get; set; }
        }
    }

    #endregion

    #region 21540

    [ConditionalFact]
    public virtual async Task Can_auto_include_navigation_from_model()
    {
        var contextFactory = await InitializeAsync<Context21540>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Parents.AsNoTracking().ToList();
            var result = Assert.Single(query);
            Assert.NotNull(result.OwnedReference);
            Assert.NotNull(result.Reference);
            Assert.NotNull(result.Collection);
            Assert.Equal(2, result.Collection.Count);
            Assert.NotNull(result.SkipOtherSide);
            Assert.Single(result.SkipOtherSide);
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Parents.AsNoTracking().IgnoreAutoIncludes().ToList();
            var result = Assert.Single(query);
            Assert.NotNull(result.OwnedReference);
            Assert.Null(result.Reference);
            Assert.Null(result.Collection);
            Assert.Null(result.SkipOtherSide);
        }
    }

    private class Context21540(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Parent> Parents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>().HasMany(e => e.SkipOtherSide).WithMany(e => e.SkipParent)
                .UsingEntity<JoinEntity>(
                    e => e.HasOne(i => i.OtherSide).WithMany().HasForeignKey(e => e.OtherSideId),
                    e => e.HasOne(i => i.Parent).WithMany().HasForeignKey(e => e.ParentId))
                .HasKey(e => new { e.ParentId, e.OtherSideId });
            modelBuilder.Entity<Parent>().OwnsOne(e => e.OwnedReference);

            modelBuilder.Entity<Parent>().Navigation(e => e.Reference).AutoInclude();
            modelBuilder.Entity<Parent>().Navigation(e => e.Collection).AutoInclude();
            modelBuilder.Entity<Parent>().Navigation(e => e.SkipOtherSide).AutoInclude();
        }

        public Task SeedAsync()
        {
            var joinEntity = new JoinEntity
            {
                OtherSide = new OtherSide(),
                Parent = new Parent
                {
                    Reference = new Reference(),
                    OwnedReference = new Owned(),
                    Collection =
                    [
                        new(), new()
                    ]
                }
            };

            AddRange(joinEntity);

            return SaveChangesAsync();
        }

        public class Parent
        {
            public int Id { get; set; }
            public Reference Reference { get; set; }
            public Owned OwnedReference { get; set; }
            public List<Collection> Collection { get; set; }
            public List<OtherSide> SkipOtherSide { get; set; }
        }

        public class JoinEntity
        {
            public int ParentId { get; set; }
            public Parent Parent { get; set; }
            public int OtherSideId { get; set; }
            public OtherSide OtherSide { get; set; }
        }

        public class OtherSide
        {
            public int Id { get; set; }
            public List<Parent> SkipParent { get; set; }
        }

        public class Reference
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public Parent Parent { get; set; }
        }

        public class Owned
        {
            public int Id { get; set; }
        }

        public class Collection
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public Parent Parent { get; set; }
        }
    }

    #endregion

    #region 21807

    [ConditionalFact]
    public virtual async Task Nested_owned_required_dependents_are_materialized()
    {
        var contextFactory = await InitializeAsync<Context21807>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = context.Set<Context21807.Entity>().ToList();
        var result = Assert.Single(query);
        Assert.NotNull(result.Contact);
        Assert.NotNull(result.Contact.Address);
        Assert.Equal(12345, result.Contact.Address.Zip);
    }

    private class Context21807(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(
                builder =>
                {
                    builder.HasKey(x => x.Id);

                    builder.OwnsOne(
                        x => x.Contact, contact =>
                        {
                            contact.OwnsOne(c => c.Address);
                        });

                    builder.Navigation(x => x.Contact).IsRequired();
                });

        public Task SeedAsync()
        {
            Add(new Entity { Id = "1", Contact = new Contact { Address = new Address { Zip = 12345 } } });

            return SaveChangesAsync();
        }

        public class Entity
        {
            public string Id { get; set; }
            public Contact Contact { get; set; }
        }

        public class Contact
        {
            public string Name { get; set; }
            public Address Address { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public int Zip { get; set; }
        }
    }

    #endregion

    #region 22090

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task OwnsMany_correlated_projection(bool async)
    {
        var contextFactory = await InitializeAsync<Context22089>();
        using var context = contextFactory.CreateContext();
        var results = await context.Contacts.Select(
                contact => new Context22089.ContactDto
                {
                    Id = contact.Id, Names = contact.Names.Select(name => new Context22089.NameDto()).ToArray()
                })
            .ToListAsync();
    }

    private class Context22089(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contact>().HasKey(c => c.Id);
            modelBuilder.Entity<Contact>().OwnsMany(c => c.Names, names => names.WithOwner().HasForeignKey(n => n.ContactId));
        }

        public class Contact
        {
            public Guid Id { get; set; }
            public IReadOnlyList<Name> Names { get; protected set; } = new List<Name>();
        }

        public class ContactDto
        {
            public Guid Id { get; set; }
            public IReadOnlyList<NameDto> Names { get; set; }
        }

        public class Name
        {
            public Guid Id { get; set; }
            public Guid ContactId { get; set; }
        }

        public class NameDto
        {
            public Guid Id { get; set; }
            public Guid ContactId { get; set; }
        }
    }

    #endregion

    #region 24133

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Projecting_owned_collection_and_aggregate(bool async)
    {
        var contextFactory = await InitializeAsync<Context24133>();
        using var context = contextFactory.CreateContext();
        var query = context.Set<Context24133.Blog>()
            .Select(
                b => new Context24133.BlogDto
                {
                    Id = b.Id,
                    TotalComments = b.Posts.Sum(p => p.CommentsCount),
                    Posts = b.Posts.Select(p => new Context24133.PostDto { Title = p.Title, CommentsCount = p.CommentsCount })
                });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    private class Context24133(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Blog>(
                blog =>
                {
                    blog.OwnsMany(
                        b => b.Posts, p =>
                        {
                            p.WithOwner().HasForeignKey("BlogId");
                            p.Property("BlogId").HasMaxLength(40);
                        });
                });

        public class Blog
        {
            public int Id { get; private set; }

            private List<Post> _posts = [];

            public static Blog Create(IEnumerable<Post> posts)
                => new() { _posts = posts.ToList() };

            public IReadOnlyCollection<Post> Posts
                => new ReadOnlyCollection<Post>(_posts);
        }

        public class Post
        {
            public string Title { get; set; }
            public int CommentsCount { get; set; }
        }

        public class BlogDto
        {
            public int Id { get; set; }
            public int TotalComments { get; set; }
            public IEnumerable<PostDto> Posts { get; set; }
        }

        public class PostDto
        {
            public string Title { get; set; }
            public int CommentsCount { get; set; }
        }
    }

    #endregion

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

        public Task SeedAsync()
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

            return SaveChangesAsync();
        }

        public class Company
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public CustomerData CustomerData { get; set; }
            public SupplierData SupplierData { get; set; }
        }

        [Owned]
        public class CustomerData
        {
            public int Id { get; set; }
            public string AdditionalCustomerData { get; set; }
        }

        [Owned]
        public class SupplierData
        {
            public int Id { get; set; }
            public string AdditionalSupplierData { get; set; }
        }

        public class Owner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IntermediateOwnedEntity OwnedEntity { get; set; }
        }

        [Owned]
        public class IntermediateOwnedEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public CustomerData CustomerData { get; set; }
            public SupplierData SupplierData { get; set; }
        }
    }
}
