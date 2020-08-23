using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class IdentifierShadowValuePresenceTest
    {
        [ConditionalFact]
        public async Task Entities_with_null_PK_can_be_added_with_normal_use_of_DbContext_methods_and_have_id_shadow_value_and_PK_created()
        {
            await using var testDatabase = CosmosTestStore.Create("IdentifierShadowValuePresenceTest");
            using var context = new IdentifierShadowValuePresenceTestContext(testDatabase);

            var item = new GItem { };

            Assert.Null(item.Id);

            var entry = context.Add(item);

            var id = entry.Property("__id").CurrentValue;

            Assert.NotNull(item.Id);
            Assert.NotNull(id);

            Assert.Equal($"GItem|{item.Id}", id);
            Assert.Equal(EntityState.Added, entry.State);
        }

        [ConditionalFact]
        public async Task Entities_can_be_tracked_with_normal_use_of_DbContext_methods_and_have_correct_resultant_state_and_id_shadow_value()
        {
            await using var testDatabase = CosmosTestStore.Create("IdentifierShadowValuePresenceTest");
            using var context = new IdentifierShadowValuePresenceTestContext(testDatabase);

            var item = new Item { Id = 1337 };
            var entry = context.Attach(item);

            Assert.Equal($"Item|{item.Id}", entry.Property("__id").CurrentValue);
            Assert.Equal(EntityState.Unchanged, entry.State);

            entry.State = EntityState.Detached;
            entry = context.Update(item = new Item { Id = 71 });

            Assert.Equal($"Item|{item.Id}", entry.Property("__id").CurrentValue);
            Assert.Equal(EntityState.Modified, entry.State);

            entry.State = EntityState.Detached;
            entry = context.Remove(item = new Item { Id = 33 });

            Assert.Equal($"Item|{item.Id}", entry.Property("__id").CurrentValue);
            Assert.Equal(EntityState.Deleted, entry.State);
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

        public DbSet<GItem> GItems { get; set; }

        public DbSet<Item> Items { get; set; }
    }

    public class GItem
    {
        public Guid? Id { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
    }
}
