using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class IdentifierShadowValuePresenceTest
    {
        [ConditionalFact]
        public async Task Newly_attached_entity_has_id_shadow_property_populated_and_is_in_unchanged_state()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized("IdentifierShadowValuePresenceTest");

            using var context = new IdentifierShadowValuePresenceTestContext(testDatabase);
            var entry = context.Attach(new Item { Id = 1337 });

            Assert.True(entry.Property("__id") is { CurrentValue: "Item|1337", EntityEntry: { State: EntityState.Unchanged } });

            entry.State = EntityState.Detached;
            entry = context.Update(new Item { Id = 70 });

            Assert.True(entry.Property("__id") is { CurrentValue: "Item|70", EntityEntry: { State: EntityState.Modified } });
        }
    }

    public class IdentifierShadowValuePresenceTestContext : DbContext
    {
        private readonly string _connectionUri;
        private readonly string _authToken;
        private readonly string _name;

        public IdentifierShadowValuePresenceTestContext(CosmosTestStore testStore)
        {
            _connectionUri = testStore.ConnectionUri;
            _authToken = testStore.AuthToken;
            _name = testStore.Name;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseCosmos(
                    _connectionUri,
                    _authToken,
                    _name,
                    b => b.ApplyConfiguration());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
    }
}
