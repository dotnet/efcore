using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;
using static Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal.CosmosDatabaseCreatorTest;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class IdentifierShadowValuePresenceTest
    {
        [ConditionalFact]
        public async Task Newly_attached_entity_has_id_shadow_property_populated_and_is_in_unchanged_state()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized("Database");

            using var context = new BloggingContext(testDatabase);
            var entry = context.Attach(new Blog { Id = 1337 });
            
            Assert.True(entry.Property("__id") is { CurrentValue: "Blog|1337", EntityEntry: { State: EntityState.Unchanged } });
        }
    }
}
