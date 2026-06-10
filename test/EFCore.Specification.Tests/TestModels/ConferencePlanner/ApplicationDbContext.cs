// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner;

#nullable disable

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendee>()
            .HasIndex(a => a.UserName)
            .IsUnique();

        // Many-to-many: Session <-> Attendee
        modelBuilder.Entity<SessionAttendee>()
            .HasKey(ca => new { ca.SessionId, ca.AttendeeId });

        // Many-to-many: Speaker <-> Session
        modelBuilder.Entity<SessionSpeaker>()
            .HasKey(ss => new { ss.SessionId, ss.SpeakerId });
    }

    public DbSet<Session> Sessions { get; set; }

    public DbSet<Track> Tracks { get; set; }

    public DbSet<Speaker> Speakers { get; set; }

    public DbSet<Attendee> Attendees { get; set; }
}
