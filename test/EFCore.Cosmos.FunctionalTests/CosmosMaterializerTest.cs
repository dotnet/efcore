namespace Microsoft.EntityFrameworkCore;

public class CosmosMaterializerTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName  { get; } = nameof(CosmosMaterializerTest);

    protected override ITestStoreFactory NonSharedTestStoreFactory { get; } = CosmosTestStoreFactory.Instance;

    [ConditionalFact]
    public async Task Materialize_entity_with_shadow_key()
    {
        var factory = await InitializeNonSharedTest<MaterializerContext>();

        using (var context = factory.CreateDbContext())
        {
            var entry = context.Entry(new ShadowKeyEntity { Id = 1 });
            entry.Property<string>("Name").CurrentValue = "Name";
            entry.Property<int>("Id2").CurrentValue = 1;
            entry.State = EntityState.Added;
            await context.SaveChangesAsync();
        }

        using (var context = factory.CreateDbContext())
        {
            var entities = await context.Entities.ToListAsync();
            Assert.Equal(1, entities.Count);
        }
    }

    public class ShadowKeyEntity
    {
        public int Id { get; set; }

    }

    public class MaterializerContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<ShadowKeyEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ShadowKeyEntity>(e =>
            {
                e.Property<string>("Name");
                e.Property<int>("Id2");
                e.HasKey("Id", "Id2");
                e.HasPartitionKey(x => x.Id);
            });
        }
    }
}
