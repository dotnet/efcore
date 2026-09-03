// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.ConferencePlanner.ConferenceDTO;

#nullable disable

public class Session
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(4000)]
    public virtual string Abstract { get; set; }

    public virtual DateTimeOffset? StartTime { get; set; }

    public virtual DateTimeOffset? EndTime { get; set; }

    // Bonus points to those who can figure out why this is written this way
    public TimeSpan Duration
        => EndTime?.Subtract(StartTime ?? EndTime ?? DateTimeOffset.MinValue) ?? TimeSpan.Zero;

    public int? TrackId { get; set; }
}
