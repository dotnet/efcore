namespace Microsoft.EntityFrameworkCore;

public abstract class OwnedEntityQueryTestBase(NonSharedFixture fixture)
    : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName => "OwnedEntityQueryTests";

    protected virtual void ClearLog()
    {
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Owned_reference_projects_scalar(bool async)
    {
        var factory = await InitializeNonSharedTest<ReferenceContext>(seed: context => context.SeedAsync());
        using var context = factory.CreateDbContext();
        ClearLog();
        var query = context.Owners.OrderBy(owner => owner.Id).Select(owner => owner.Details.Name);
        var names = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { "existing" }, names);
    }

    protected class ReferenceContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Owner> Owners => Set<Owner>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Owner>().OwnsOne(owner => owner.Details);

        public async Task SeedAsync()
        {
            Add(new Owner { Id = 1, Details = new Details { Name = "existing" } });
            await SaveChangesAsync();
        }

        public class Owner
        {
            public int Id { get; set; }
            public Details Details { get; set; } = null!;
        }

        public class Details
        {
            public string Name { get; set; } = null!;
        }
    }
}