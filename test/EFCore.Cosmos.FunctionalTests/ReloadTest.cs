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

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class ReloadTest
    {
        public static IEnumerable<object[]> IsAsyncData = new[]
        {
            new object[] { true },
            new object[] { false }
        };

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Entity_reference_can_be_reloaded(bool async)
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized("Database");

            using var context = new BloggingContext(testDatabase);
            await context.Database.EnsureCreatedAsync();

            var entry = context.Add(new Blog { Id = 1337 });

            await context.SaveChangesAsync();

            if (async)
            {
                await entry.ReloadAsync();
            }
            else
            {
                entry.Reload();
            }
        }
    }
}
