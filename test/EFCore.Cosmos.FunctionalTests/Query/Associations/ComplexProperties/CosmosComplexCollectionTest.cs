// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties.CosmosComplexCollectionTest;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public class CosmosComplexCollectionTest(CosmosComplexCollectionTestFixture fixture) : IClassFixture<CosmosComplexCollectionTestFixture>
{
    [ConditionalFact]
    public async Task CanAddAndQuery()
    {
        //var entity = new Entity()
        //{
        //    Id = Guid.NewGuid(),
        //    ComplexType = new ComplexType { Name = "One" },
        //};

        using (var context = fixture.CreateContext())
        {
            //context.Add(entity);
            //await context.SaveChangesAsync();

            var result = await context.Entities.Where(x => x.ComplexType != null).ToListAsync();
        }
    }

    public class CosmosComplexCollectionTestFixture : SharedStoreFixtureBase<CosmosComplexCollectionDbContext>
    {
        protected override string StoreName
            => nameof(CosmosComplexCollectionTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }

    public class CosmosComplexCollectionDbContext : DbContext
    {
        public CosmosComplexCollectionDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().HasPartitionKey(x => x.PartitionKey).OwnsOne(e => e.ComplexType);
        }

        public DbSet<Entity> Entities { get; set; } = null!;
    }

    public class Entity
    {
        public Guid Id { get; set; }

        public string PartitionKey { get; set; } = "";

        public ComplexType? ComplexType { get; set; } = new();
    }

    public class ComplexType
    {
        public string Name { get; set; } = null!;
    }
}
