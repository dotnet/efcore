// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class OverzealousInitializationTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : OverzealousInitializationTestBase<TFixture>.OverzealousInitializationFixtureBase, new()
{
    protected OverzealousInitializationTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    [ConditionalFact]
    public virtual void Fixup_ignores_eagerly_initialized_reference_navs()
    {
        using var context = CreateContext();

        var albums = context.Set<Album>()
            .Include(e => e.Tracks)
            .Include(e => e.Artist)
            .OrderBy(e => e.Id)
            .ToList();

        var i = 0;
        foreach (var album in albums)
        {
            var artist = _artists[i++ % 3];

            Assert.Equal(artist.Id, album.Artist.Id);
            Assert.Equal(artist.Name, album.Artist.Name);
        }
    }

    private static readonly Artist[] _artists =
    [
        new() { Id = 1, Name = "Freddie" }, new() { Id = 2, Name = "Kendrick" }, new() { Id = 3, Name = "Jarvis" }
    ];

    protected class Album
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int ArtistId { get; set; }

        public virtual Artist Artist { get; set; }
        public virtual IList<Track> Tracks { get; set; }

        public Album()
        {
            Artist = new Artist();
            Tracks = new List<Track>();
        }
    }

    protected class Artist
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Track
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int AlbumId { get; set; }
    }

    public class AlbumViewerContext(DbContextOptions<AlbumViewerContext> options) : PoolableDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Album>();
            modelBuilder.Entity<Artist>();
            modelBuilder.Entity<Track>();
        }
    }

    protected TFixture Fixture { get; }

    protected AlbumViewerContext CreateContext()
        => Fixture.CreateContext();

    public abstract class OverzealousInitializationFixtureBase : SharedStoreFixtureBase<AlbumViewerContext>
    {
        public virtual IDisposable BeginTransaction(DbContext context)
            => context.Database.BeginTransaction();

        protected override string StoreName
            => "OverzealousInitialization";

        protected override Task SeedAsync(AlbumViewerContext context)
        {
            for (var i = 1; i <= 10; i++)
            {
                context.Add(
                    new Album
                    {
                        Id = i,
                        Artist = _artists[(i - 1) % 3],
                        Tracks = new List<Track> { new() { Id = i * 2 }, new() { Id = i * 2 + 1 } }
                    });
            }

            return context.SaveChangesAsync();
        }
    }
}
