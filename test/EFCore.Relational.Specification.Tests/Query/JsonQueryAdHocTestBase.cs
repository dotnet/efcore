// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class JsonQueryAdHocTestBase : NonSharedModelTestBase
{
    protected JsonQueryAdHocTestBase(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override string StoreName
        => "JsonQueryAdHocTest";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Optional_json_properties_materialized_as_null_when_the_element_in_json_is_not_present(bool async)
    {
        var contextFactory = await InitializeAsync<MyContext29219>(seed: Seed29219);
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
        var contextFactory = await InitializeAsync<MyContext29219>(seed: Seed29219);
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
            }).ToList();

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
}
