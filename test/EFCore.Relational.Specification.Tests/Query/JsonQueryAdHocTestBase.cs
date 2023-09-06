// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class JsonQueryAdHocTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "JsonQueryAdHocTest";

    #region 29219

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Optional_json_properties_materialized_as_null_when_the_element_in_json_is_not_present(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext29219>(
            seed: Seed29219);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(x => x.Id == 3);

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(3, result.Id);
            Assert.Equal(null, result.Reference.NullableScalar);
            Assert.Equal(null, result.Collection[0].NullableScalar);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_project_nullable_json_property_when_the_element_in_json_is_not_present(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext29219>(
            seed: Seed29219);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.OrderBy(x => x.Id).Select(x => x.Reference.NullableScalar);

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(11, result[0]);
            Assert.Equal(null, result[1]);
            Assert.Equal(null, result[2]);
        }
    }

    protected abstract void Seed29219(MyContext29219 ctx);

    protected class MyContext29219 : DbContext
    {
        public MyContext29219(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MyEntity29219> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity29219>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<MyEntity29219>().OwnsOne(x => x.Reference).ToJson();
            modelBuilder.Entity<MyEntity29219>().OwnsMany(x => x.Collection).ToJson();
        }
    }

    public class MyEntity29219
    {
        public int Id { get; set; }
        public MyJsonEntity29219 Reference { get; set; }
        public List<MyJsonEntity29219> Collection { get; set; }
    }

    public class MyJsonEntity29219
    {
        public int NonNullableScalar { get; set; }
        public int? NullableScalar { get; set; }
    }

    #endregion

    #region 30028

    protected abstract void Seed30028(MyContext30028 ctx);

    protected class MyContext30028 : DbContext
    {
        public MyContext30028(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MyEntity30028> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntity30028>(b =>
            {
                b.Property(x => x.Id).ValueGeneratedNever();
                b.OwnsOne(x => x.Json, nb =>
                {
                    nb.ToJson();
                    nb.OwnsMany(x => x.Collection, nnb => nnb.OwnsOne(x => x.Nested));
                    nb.OwnsOne(x => x.OptionalReference, nnb => nnb.OwnsOne(x => x.Nested));
                    nb.OwnsOne(x => x.RequiredReference, nnb => nnb.OwnsOne(x => x.Nested));
                    nb.Navigation(x => x.RequiredReference).IsRequired();
                });
            });
        }
    }

    public class MyEntity30028
    {
        public int Id { get; set; }
        public MyJsonRootEntity30028 Json { get; set; }
    }

    public class MyJsonRootEntity30028
    {
        public string RootName { get; set; }
        public MyJsonBranchEntity30028 RequiredReference { get; set; }
        public MyJsonBranchEntity30028 OptionalReference { get; set; }
        public List<MyJsonBranchEntity30028> Collection { get; set; }
    }

    public class MyJsonBranchEntity30028
    {
        public string BranchName { get; set; }
        public MyJsonLeafEntity30028 Nested { get; set; }
    }

    public class MyJsonLeafEntity30028
    {
        public string LeafName { get; set; }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Accessing_missing_navigation_works(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext30028>(seed: Seed30028);
        using (var context = contextFactory.CreateContext())
        {
            var result = context.Entities.OrderBy(x => x.Id).ToList();
            Assert.Equal(4, result.Count);
            Assert.NotNull(result[0].Json.Collection);
            Assert.NotNull(result[0].Json.OptionalReference);
            Assert.NotNull(result[0].Json.RequiredReference);

            Assert.Null(result[1].Json.Collection);
            Assert.NotNull(result[1].Json.OptionalReference);
            Assert.NotNull(result[1].Json.RequiredReference);

            Assert.NotNull(result[2].Json.Collection);
            Assert.Null(result[2].Json.OptionalReference);
            Assert.NotNull(result[2].Json.RequiredReference);

            Assert.NotNull(result[3].Json.Collection);
            Assert.NotNull(result[3].Json.OptionalReference);
            Assert.Null(result[3].Json.RequiredReference);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Missing_navigation_works_with_deduplication(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext30028>(seed: Seed30028);
        using (var context = contextFactory.CreateContext())
        {
            var result = context.Entities.OrderBy(x => x.Id).Select(x => new
            {
                x,
                x.Json,
                x.Json.OptionalReference,
                x.Json.RequiredReference,
                NestedOptional = x.Json.OptionalReference.Nested,
                NestedRequired = x.Json.RequiredReference.Nested,
                x.Json.Collection,
            }).AsNoTracking().ToList();

            Assert.Equal(4, result.Count);
            Assert.NotNull(result[0].OptionalReference);
            Assert.NotNull(result[0].RequiredReference);
            Assert.NotNull(result[0].NestedOptional);
            Assert.NotNull(result[0].NestedRequired);
            Assert.NotNull(result[0].Collection);

            Assert.NotNull(result[1].OptionalReference);
            Assert.NotNull(result[1].RequiredReference);
            Assert.NotNull(result[1].NestedOptional);
            Assert.NotNull(result[1].NestedRequired);
            Assert.Null(result[1].Collection);

            Assert.Null(result[2].OptionalReference);
            Assert.NotNull(result[2].RequiredReference);
            Assert.Null(result[2].NestedOptional);
            Assert.NotNull(result[2].NestedRequired);
            Assert.NotNull(result[2].Collection);

            Assert.NotNull(result[3].OptionalReference);
            Assert.Null(result[3].RequiredReference);
            Assert.NotNull(result[3].NestedOptional);
            Assert.Null(result[3].NestedRequired);
            Assert.NotNull(result[3].Collection);
        }
    }

    #endregion

    #region ArrayOfPrimitives

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_json_array_of_primitives_on_reference(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextArrayOfPrimitives>(
            seed: SeedArrayOfPrimitives);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.OrderBy(x => x.Id).Select(x => new { x.Reference.IntArray, x.Reference.ListOfString });

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(3, result[0].IntArray.Length);
            Assert.Equal(3, result[0].ListOfString.Count);
            Assert.Equal(3, result[1].IntArray.Length);
            Assert.Equal(3, result[1].ListOfString.Count);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_json_array_of_primitives_on_collection(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextArrayOfPrimitives>(
            seed: SeedArrayOfPrimitives);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.OrderBy(x => x.Id).Select(x => new { x.Collection[0].IntArray, x.Collection[1].ListOfString });

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(3, result[0].IntArray.Length);
            Assert.Equal(2, result[0].ListOfString.Count);
            Assert.Equal(3, result[1].IntArray.Length);
            Assert.Equal(2, result[1].ListOfString.Count);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_element_of_json_array_of_primitives(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextArrayOfPrimitives>(
            seed: SeedArrayOfPrimitives);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.OrderBy(x => x.Id).Select(x => new
            {
                ArrayElement = x.Reference.IntArray[0],
                ListElement = x.Reference.ListOfString[1]
            });

            var result = async
                ? await query.ToListAsync()
                : query.ToList();
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives1(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextArrayOfPrimitives>(
            seed: SeedArrayOfPrimitives);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(x => x.Reference.IntArray[0] == 1);

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(1, result[0].Reference.IntArray[0]);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives2(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextArrayOfPrimitives>(
            seed: SeedArrayOfPrimitives);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(x => x.Reference.ListOfString[1] == "Bar");

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal("Bar", result[0].Reference.ListOfString[1]);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Predicate_based_on_element_of_json_array_of_primitives3(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextArrayOfPrimitives>(
            seed: SeedArrayOfPrimitives);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Where(
                    x => x.Reference.IntArray.AsQueryable().ElementAt(0) == 1
                        || x.Reference.ListOfString.AsQueryable().ElementAt(1) == "Bar")
                .OrderBy(e => e.Id);

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(1, result[0].Reference.IntArray[0]);
            Assert.Equal("Bar", result[0].Reference.ListOfString[1]);
        }
    }

    protected abstract void SeedArrayOfPrimitives(MyContextArrayOfPrimitives ctx);

    protected class MyContextArrayOfPrimitives : DbContext
    {
        public MyContextArrayOfPrimitives(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MyEntityArrayOfPrimitives> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntityArrayOfPrimitives>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<MyEntityArrayOfPrimitives>().OwnsOne(x => x.Reference, b =>
            {
                b.ToJson();
                b.Property(x => x.IntArray).HasConversion(
                     x => string.Join(" ", x),
                     x => x.Split(" ", StringSplitOptions.None).Select(v => int.Parse(v)).ToArray(),
                     new ValueComparer<int[]>(true));

                b.Property(x => x.ListOfString).HasConversion(
                    x => string.Join(" ", x),
                    x => x.Split(" ", StringSplitOptions.None).ToList(),
                    new ValueComparer<List<string>>(true));
            });

            modelBuilder.Entity<MyEntityArrayOfPrimitives>().OwnsMany(x => x.Collection, b =>
            {
                b.ToJson();
                b.Property(x => x.IntArray).HasConversion(
                     x => string.Join(" ", x),
                     x => x.Split(" ", StringSplitOptions.None).Select(v => int.Parse(v)).ToArray(),
                     new ValueComparer<int[]>(true));
                b.Property(x => x.ListOfString).HasConversion(
                    x => string.Join(" ", x),
                    x => x.Split(" ", StringSplitOptions.None).ToList(),
                    new ValueComparer<List<string>>(true));
            });
        }
    }

    public class MyEntityArrayOfPrimitives
    {
        public int Id { get; set; }
        public MyJsonEntityArrayOfPrimitives Reference { get; set; }
        public List<MyJsonEntityArrayOfPrimitives> Collection { get; set; }
    }

    public class MyJsonEntityArrayOfPrimitives
    {
        public int[] IntArray { get; set; }
        public List<string> ListOfString { get; set; }
    }

    #endregion

    #region JunkInJson

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Junk_in_json_basic_tracking(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextJunkInJson>(
            seed: SeedJunkInJson);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities;

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(2, result[0].Collection.Count);
            Assert.Equal(2, result[0].CollectionWithCtor.Count);
            Assert.Equal(2, result[0].Reference.NestedCollection.Count);
            Assert.NotNull(result[0].Reference.NestedReference);
            Assert.Equal(2, result[0].ReferenceWithCtor.NestedCollection.Count);
            Assert.NotNull(result[0].ReferenceWithCtor.NestedReference);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Junk_in_json_basic_no_tracking(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextJunkInJson>(
            seed: SeedJunkInJson);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.AsNoTracking();

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(2, result[0].Collection.Count);
            Assert.Equal(2, result[0].CollectionWithCtor.Count);
            Assert.Equal(2, result[0].Reference.NestedCollection.Count);
            Assert.NotNull(result[0].Reference.NestedReference);
            Assert.Equal(2, result[0].ReferenceWithCtor.NestedCollection.Count);
            Assert.NotNull(result[0].ReferenceWithCtor.NestedReference);
        }
    }

    protected abstract void SeedJunkInJson(MyContextJunkInJson ctx);

    protected class MyContextJunkInJson : DbContext
    {
        public MyContextJunkInJson(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MyEntityJunkInJson> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntityJunkInJson>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<MyEntityJunkInJson>().OwnsOne(x => x.Reference, b =>
            {
                b.ToJson();
                b.OwnsOne(x => x.NestedReference);
                b.OwnsMany(x => x.NestedCollection);
            });
            modelBuilder.Entity<MyEntityJunkInJson>().OwnsOne(x => x.ReferenceWithCtor, b =>
            {
                b.ToJson();
                b.OwnsOne(x => x.NestedReference);
                b.OwnsMany(x => x.NestedCollection);
            });
            modelBuilder.Entity<MyEntityJunkInJson>().OwnsMany(x => x.Collection, b =>
            {
                b.ToJson();
                b.OwnsOne(x => x.NestedReference);
                b.OwnsMany(x => x.NestedCollection);
            });
            modelBuilder.Entity<MyEntityJunkInJson>().OwnsMany(x => x.CollectionWithCtor, b =>
            {
                b.ToJson();
                b.OwnsOne(x => x.NestedReference);
                b.OwnsMany(x => x.NestedCollection);
            });
        }
    }

    public class MyEntityJunkInJson
    {
        public int Id { get; set; }
        public MyJsonEntityJunkInJson Reference { get; set; }
        public MyJsonEntityJunkInJsonWithCtor ReferenceWithCtor { get; set; }
        public List<MyJsonEntityJunkInJson> Collection { get; set; }
        public List<MyJsonEntityJunkInJsonWithCtor> CollectionWithCtor { get; set; }
    }

    public class MyJsonEntityJunkInJson
    {
        public string Name { get; set; }
        public double Number { get; set; }

        public MyJsonEntityJunkInJsonNested NestedReference { get; set; }
        public List<MyJsonEntityJunkInJsonNested> NestedCollection { get; set; }
    }

    public class MyJsonEntityJunkInJsonNested
    {
        public DateTime DoB { get; set; }
    }

    public class MyJsonEntityJunkInJsonWithCtor
    {
        public MyJsonEntityJunkInJsonWithCtor(bool myBool, string name)
        {
            MyBool = myBool;
            Name = name;
        }

        public bool MyBool { get; set; }
        public string Name { get; set; }

        public MyJsonEntityJunkInJsonWithCtorNested NestedReference { get; set; }
        public List<MyJsonEntityJunkInJsonWithCtorNested> NestedCollection { get; set; }
    }

    public class MyJsonEntityJunkInJsonWithCtorNested
    {
        public MyJsonEntityJunkInJsonWithCtorNested(DateTime doB)
        {
            DoB = doB;
        }

        public DateTime DoB { get; set; }
    }

    #endregion

    #region ShadowProperties

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Shadow_properties_basic_tracking(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextShadowProperties>(
            seed: SeedShadowProperties);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities;

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(2, result[0].Collection.Count);
            Assert.Equal(2, result[0].CollectionWithCtor.Count);
            Assert.NotNull(result[0].Reference);
            Assert.NotNull(result[0].ReferenceWithCtor);

            var referenceEntry = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Reference);
            Assert.Equal("Foo", referenceEntry.Property("ShadowString").CurrentValue);

            var referenceCtorEntry = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].ReferenceWithCtor);
            Assert.Equal(143, referenceCtorEntry.Property("Shadow_Int").CurrentValue);

            var collectionEntry1 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Collection[0]);
            var collectionEntry2 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].Collection[1]);
            Assert.Equal(5.5, collectionEntry1.Property("ShadowDouble").CurrentValue);
            Assert.Equal(20.5, collectionEntry2.Property("ShadowDouble").CurrentValue);

            var collectionCtorEntry1 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].CollectionWithCtor[0]);
            var collectionCtorEntry2 = context.ChangeTracker.Entries().Single(x => x.Entity == result[0].CollectionWithCtor[1]);
            Assert.Equal((byte)6, collectionCtorEntry1.Property("ShadowNullableByte").CurrentValue);
            Assert.Null(collectionCtorEntry2.Property("ShadowNullableByte").CurrentValue);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Shadow_properties_basic_no_tracking(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextShadowProperties>(
            seed: SeedShadowProperties);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.AsNoTracking();

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal(2, result[0].Collection.Count);
            Assert.Equal(2, result[0].CollectionWithCtor.Count);
            Assert.NotNull(result[0].Reference);
            Assert.NotNull(result[0].ReferenceWithCtor);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_shadow_properties_from_json_entity(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextShadowProperties>(
            seed: SeedShadowProperties);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities.Select(x => new
            {
                ShadowString = EF.Property<string>(x.Reference, "ShadowString"),
                ShadowInt = EF.Property<int>(x.ReferenceWithCtor, "Shadow_Int"),
            });

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal("Foo", result[0].ShadowString);
            Assert.Equal(143, result[0].ShadowInt);
        }
    }

    protected abstract void SeedShadowProperties(MyContextShadowProperties ctx);

    protected class MyContextShadowProperties : DbContext
    {
        public MyContextShadowProperties(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MyEntityShadowProperties> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntityShadowProperties>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<MyEntityShadowProperties>().OwnsOne(x => x.Reference, b =>
            {
                b.ToJson();
                b.Property<string>("ShadowString");
            });
            modelBuilder.Entity<MyEntityShadowProperties>().OwnsOne(x => x.ReferenceWithCtor, b =>
            {
                b.ToJson();
                b.Property<int>("Shadow_Int").HasJsonPropertyName("ShadowInt");
            });
            modelBuilder.Entity<MyEntityShadowProperties>().OwnsMany(x => x.Collection, b =>
            {
                b.ToJson();
                b.Property<double>("ShadowDouble");
            });
            modelBuilder.Entity<MyEntityShadowProperties>().OwnsMany(x => x.CollectionWithCtor, b =>
            {
                b.ToJson();
                b.Property<byte?>("ShadowNullableByte");
            });
        }
    }

    public class MyEntityShadowProperties
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public MyJsonEntityShadowProperties Reference { get; set; }
        public List<MyJsonEntityShadowProperties> Collection { get; set; }
        public MyJsonEntityShadowPropertiesWithCtor ReferenceWithCtor { get; set; }
        public List<MyJsonEntityShadowPropertiesWithCtor> CollectionWithCtor { get; set; }
    }

    public class MyJsonEntityShadowProperties
    {
        public string Name { get; set; }
    }

    public class MyJsonEntityShadowPropertiesWithCtor
    {
        public MyJsonEntityShadowPropertiesWithCtor(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    #endregion

    #region LazyLoadingProxies

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Project_proxies_entity_with_json(bool async)
    {
        var contextFactory = await InitializeAsync<MyContextLazyLoadingProxies>(
            seed: SeedLazyLoadingProxies,
            onConfiguring: OnConfiguringLazyLoadingProxies,
            addServices: AddServicesLazyLoadingProxies);

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Entities;

            var result = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Equal(2, result.Count);
        }
    }

    protected void OnConfiguringLazyLoadingProxies(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseLazyLoadingProxies();

    protected void AddServicesLazyLoadingProxies(IServiceCollection addServices)
        => addServices.AddEntityFrameworkProxies();

    private void SeedLazyLoadingProxies(MyContextLazyLoadingProxies ctx)
    {
        var r1 = new MyJsonEntityLazyLoadingProxiesWithCtor("r1", 1);
        var c11 = new MyJsonEntityLazyLoadingProxies { Name = "c11", Number = 11 };
        var c12 = new MyJsonEntityLazyLoadingProxies { Name = "c12", Number = 12 };
        var c13 = new MyJsonEntityLazyLoadingProxies { Name = "c13", Number = 13 };

        var r2 = new MyJsonEntityLazyLoadingProxiesWithCtor("r2", 2);
        var c21 = new MyJsonEntityLazyLoadingProxies { Name = "c21", Number = 21 };
        var c22 = new MyJsonEntityLazyLoadingProxies { Name = "c22", Number = 22 };

        var e1 = new MyEntityLazyLoadingProxies
        {
            Id = 1,
            Name = "e1",
            Reference = r1,
            Collection = new List<MyJsonEntityLazyLoadingProxies> { c11, c12, c13 }
        };

        var e2 = new MyEntityLazyLoadingProxies
        {
            Id = 2,
            Name = "e2",
            Reference = r2,
            Collection = new List<MyJsonEntityLazyLoadingProxies> { c21, c22 }
        };

        ctx.Entities.AddRange(e1, e2);
        ctx.SaveChanges();
    }

    protected class MyContextLazyLoadingProxies : DbContext
    {
        public MyContextLazyLoadingProxies(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MyEntityLazyLoadingProxies> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyEntityLazyLoadingProxies>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<MyEntityLazyLoadingProxies>().OwnsOne(x => x.Reference, b => b.ToJson());
            modelBuilder.Entity<MyEntityLazyLoadingProxies>().OwnsMany(x => x.Collection, b => b.ToJson());
        }
    }

    public class MyEntityLazyLoadingProxies
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual MyJsonEntityLazyLoadingProxiesWithCtor Reference { get; set; }
        public virtual List<MyJsonEntityLazyLoadingProxies> Collection { get; set; }
    }

    public class MyJsonEntityLazyLoadingProxiesWithCtor
    {
        public MyJsonEntityLazyLoadingProxiesWithCtor(string name, int number)
        {
            Name = name;
            Number = number;
        }

        public string Name { get; set; }
        public int Number { get; set; }
    }

    public class MyJsonEntityLazyLoadingProxies
    {
        public string Name { get; set; }
        public int Number { get; set; }
    }

    #endregion

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
