// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;

namespace Microsoft.EntityFrameworkCore;

public class LoadExistingEntityStateSqliteTest
{
    [Fact] // Issue #35762
    public void Load_collection_marks_tracked_Added_member_that_exists_in_store_as_Unchanged()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        int municipalityId;
        using (var context = new LoadContext(connection))
        {
            context.Database.EnsureCreated();
            var municipality = new Municipality
            {
                Name = "M1",
                Residences = [new ChildResidence { Id = 1 }, new ChildResidence { Id = 2 }]
            };
            context.Add(municipality);
            context.SaveChanges();
            municipalityId = municipality.Id;
        }

        using (var context = new LoadContext(connection))
        {
            var municipality = context.Set<Municipality>().Single();

            // A residence that already exists in the store is incorrectly tracked as Added.
            var existing = new ChildResidence { Id = 1, MunicipalityId = municipalityId };
            context.Add(existing);

            var collectionEntry = context.Entry(municipality).Collection(m => m.Residences);
            Assert.False(collectionEntry.IsLoaded);
            Assert.Equal(EntityState.Added, context.Entry(existing).State);

            collectionEntry.Load();

            // The load query returned the residence, proving it exists in the store, so it is now Unchanged.
            Assert.Equal(EntityState.Unchanged, context.Entry(existing).State);
        }
    }

    private class LoadContext(SqliteConnection connection) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(connection);

        public DbSet<Municipality> Municipalities
            => Set<Municipality>();

        public DbSet<ChildResidence> ChildResidences
            => Set<ChildResidence>();
    }

    private class Municipality
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<ChildResidence> Residences { get; set; } = null!;
    }

    private class ChildResidence
    {
        public int Id { get; set; }
        public int MunicipalityId { get; set; }
        public Municipality Municipality { get; set; } = null!;
    }
}
