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
}
