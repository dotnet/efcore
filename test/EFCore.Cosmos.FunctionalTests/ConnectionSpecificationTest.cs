// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConnectionSpecificationTest
{
    [ConditionalFact]
    public async Task Can_specify_connection_string_in_OnConfiguring()
    {
        await using var testDatabase = CosmosTestStore.Create("NonExisting");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        Assert.False(await creator.EnsureDeletedAsync());
    }

    public class BloggingContext(CosmosTestStore testStore) : DbContext
    {
        private readonly string _connectionString = testStore.ConnectionString;
        private readonly string _name = testStore.Name;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(_connectionString, _name, b => b.ApplyConfiguration())
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));

        public DbSet<Blog> Blogs { get; set; }
    }

    [ConditionalFact]
    public async Task Throws_for_missing_connection_info()
    {
        using var context = new NoConnectionContext();
        var creator = context.GetService<IDatabaseCreator>();

        Assert.Equal(CosmosStrings.ConnectionInfoMissing,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => creator.EnsureDeletedAsync())).Message);
    }

    public class NoConnectionContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseCosmos(b => b.ApplyConfiguration())
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
    }


    public class Blog
    {
        public int Id { get; set; }
    }
}
