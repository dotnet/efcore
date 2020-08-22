using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class IdentifierShadowValuePresenceTest
    {
        [ConditionalFact]
        public async Task Entities_can_be_tracked_with_normal_use_of_DbContext_methods_and_have_correct_resultant_state_and_id_shadow_value()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized("IdentifierShadowValuePresenceTest");

            using var context = new IdentifierShadowValuePresenceTestContext(testDatabase);
            var entry = context.Attach(new Item { Id = 1337 });

            Assert.True(entry.Property("__id") is { EntityEntry: { State: EntityState.Unchanged }, CurrentValue: "Item|1337" });

            entry.State = EntityState.Detached;
            entry = context.Update(new Item { Id = 71 });

            Assert.True(entry.Property("__id") is { EntityEntry: { State: EntityState.Modified }, CurrentValue: "Item|71" });

            entry.State = EntityState.Detached;
            entry = context.Remove(new Item { Id = 33 });

            Assert.True(entry.Property("__id") is { EntityEntry: { State: EntityState.Deleted }, CurrentValue: "Item|33" });
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
