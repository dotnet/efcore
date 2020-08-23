// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class OverzealousInitializationTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : OverzealousInitializationTestBase<TFixture>.OverzealousInitializationFixtureBase, new()
    {
        protected OverzealousInitializationTestBase(TFixture fixture)
            => Fixture = fixture;

        [ConditionalFact]
        public virtual void Fixup_does_not_ignore_eagerly_initialized_reference_navs()
        {
            using var context = CreateContext();

            var albums = context.Set<Album>()
                .Include(e => e.Tracks)
                .Include(e => e.Artist)
                .OrderBy(e => e.Artist)
                .ToList();

            foreach (var album in albums)
            {
                Assert.Equal(0, album.Artist.Id);
                Assert.Null(album.Artist.Name);
            }
        }

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

        public class AlbumViewerContext : PoolableDbContext
        {
            public AlbumViewerContext(DbContextOptions<AlbumViewerContext> options)
                : base(options)
            {
            }

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

            protected override string StoreName { get; } = "OverzealousInitialization";

            protected override void Seed(AlbumViewerContext context)
            {
                var artists = new[]
                {
                    new Artist { Id = 1, Name = "Freddie" },
                    new Artist { Id = 2, Name = "Kendrick" },
                    new Artist { Id = 3, Name = "Jarvis" }
                };

                for (var i = 1; i <= 10; i++)
                {
                    context.Add(
                        new Album
                        {
                            Id = i,
                            Artist = artists[i % 3],
                            Tracks = new List<Track> { new Track { Id = i * 2 }, new Track { Id = i * 2 + 1 } }
                        });
                }

                context.SaveChanges();
            }
        }
    }
}
